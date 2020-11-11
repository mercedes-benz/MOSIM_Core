// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Adam Klodowski

using MahApps.Metro.Controls;
using MMICSharp.Common.Communication;
using MMILauncher.Core;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Communication;
using System.Windows.Data;
using System.ComponentModel;
using Encryption;
using System.Net.Http;
using System.Net;
using MMIULibrary;

namespace MMILauncher
{

    /// <summary>
    /// Central class which represents the main window of the MMU Server Launcher application.
    /// The application is responsible for managing the processes of the services and adapters and serves as a central accessing point to gather the required information.
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        /// <summary>
        /// Specifies whether the service windows are hidden
        /// </summary>
        public bool HideServiceWindows
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Specifies whether the services are automatically started
        /// </summary>
        public bool StartServices
        {
            get;
            set;
        } = true;

        public Encrypt encryptionService;

        private MMIRegisterServiceImplementation registerService;

        #region private variables

        private const string version = "5.1.2";

        /// <summary>
        /// The instance of the register server
        /// </summary>
        private MMIRegisterThriftServer registerServer;
        
        /// <summary>
        /// The settings class instance which contains all relevant settings for the launcher
        /// </summary>
        public ServerSettings settings = new ServerSettings();

        private bool running = false;
        private bool showPerformance = true;
        private PerformanceCounter cpuCounter;
        private int port;

        private HttpClientHandler httpClientHandler;
        private WebProxy proxy;
        private HttpClient client;
        private MMULibrary mmus = new MMULibrary();

        #endregion

        public NetworkAdapters NetworkAdapters;


        /// <summary>
        /// Main constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.StopButton.IsEnabled = false;
            //Important assign the dispatcher at the beginning
            UIData.Initialize(this.Dispatcher);

            //Register for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Initialize gui related things
            this.adapterListView.ItemsSource = UIData.AdapterCollection;
            this.serviceListView.ItemsSource = UIData.ServiceCollection;
            this.mmuView.ItemsSource = UIData.MMUCollection;
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.mmuView.ItemsSource);

            //PropertyGroupDescription groupDescription = new PropertyGroupDescription("Language");
            //view.GroupDescriptions.Add(groupDescription);
            view.SortDescriptions.Add(new SortDescription("MotionType", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("priority", ListSortDirection.Ascending));

            //Create an instance of the register service and register at events
            this.registerService = new MMIRegisterServiceImplementation();
            this.registerService.OnAdapterRegistered += RegisterService_OnAdapterRegistered;
            this.registerService.OnAdapterUnregistered += RegisterService_OnAdapterUnregistered;
            this.registerService.OnServiceRegistered += RegisterService_OnServiceRegistered;
            this.registerService.OnServiceUnregistered += RegisterService_OnServiceUnregistered;

            //Parse the settings from file
            try
            {
                //Load the settings file from json
                settings = Serialization.FromJsonString<ServerSettings>(File.ReadAllText("settings.json"));

                //Check if directory exists -> if not user must provide input
                if (!Directory.Exists(settings.DataPath))
                {
                    ShowFolderSelectionDialog("Invalid path: ");
                }
                
            }
            catch (Exception)
            {
                //Create a new settings file
                settings = new ServerSettings();

                //Show the folder selection dialog
                ShowFolderSelectionDialog("No settings file found: ");
                SaveSettings(); //if settings did not exist before save the default settings
            }

            //Read settings from system registry
            encryptionService = new Encrypt();
            LoadRegistrySettings();

            //Assign the port (common for loaded settings from file or settings from default settings file.
            RuntimeData.MMIRegisterAddress.Port = settings.RegisterPort;
            RuntimeData.MMIRegisterAddress.Address = settings.RegisterAddress;

            //Sets up the performance bar which visualizes performance stats within the main window
            SetupPerformanceBar();

            //check if last selected adapter has the same ip address and update ip if needed
            NetworkAdapters = new NetworkAdapters();
            if (NetworkAdapters.updatedCurrentIp(settings.RegisterInterface, settings.RegisterAddress))
            { //update settings and save only if the interface or IP address has changed
                settings.RegisterAddress = NetworkAdapters.AvailableIp[NetworkAdapters.currentIp].IP;
                settings.RegisterInterface = NetworkAdapters.AvailableIp[NetworkAdapters.currentIp].Name;
                RuntimeData.MMIRegisterAddress.Address = settings.RegisterAddress;
                SaveSettings(); 
            }

            UpdateTitleBar();

            //Directly start all processes if autostart is enabled
            if (this.settings.AutoStart)
            {
                this.StartButton_Click(this, new RoutedEventArgs());
            }
        }

        /// <summary>
        /// Updates the title bar
        /// </summary>
        public void UpdateTitleBar()
        {
            if (this.registerServer==null)
                this.Title = $"MMI Launcher {version}: register thrift server settings: {RuntimeData.MMIRegisterAddress.Address}:{RuntimeData.MMIRegisterAddress.Port}";
            else
                this.Title = $"MMI Launcher {version}: hosting register thrift server on: {RuntimeData.MMIRegisterAddress.Address}:{RuntimeData.MMIRegisterAddress.Port}";
        }

        /// <summary>
        /// Method to start the register server
        /// </summary>
        public void Start()
        {
            StopButton.IsEnabled = true;
            StartButton.Header = "_Restart";

            ///Start the register server
            this.registerServer = new MMIRegisterThriftServer(RuntimeData.MMIRegisterAddress.Port, this.registerService);
            this.registerServer.Start();

            //Update the title text of the window
            UpdateTitleBar();

            //Get the datapath  
            string datapath = this.settings.DataPath;

            //Setup the folder structure if not available
            this.CheckFolderStructure(datapath);

            //Set up the environment
            this.SetupEnvironment(datapath + "Adapters/", datapath + "MMUs/", datapath + "Services/");
        }

        public void Stop()
        {
            if (running)
            {
                //Dispose the present server if available and running
                Dispose();

                //Just wait a second
                Thread.Sleep(1000);
                StopButton.IsEnabled = false;
                StartButton.Header = "_Start";
            }
        }

        public void Restart()
        {
            Stop();

            try
            { //Start a new server with the specific configuration
                Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Problem at starting server using specified port: " + ex.Message,"Register server failure",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        public void LoadRegistrySettings()
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\MOSIM\\TaskEditor\\Unity"); //key with proxy settings that are same for task editor and launcher
            if (key != null)
            {
                object val;
                val = key.GetValue("ProxyUser");
                if (val != null)
                {
                    try
                    {
                        settings.ProxyUser = encryptionService.DecryptString(val.ToString());
                    }
                    catch
                    {
                        settings.ProxyUser = "";
                    }
                }
                val = key.GetValue("ProxyPass");
                if (val != null)
                {
                    try
                    {
                        settings.ProxyPass = encryptionService.DecryptString(val.ToString());
                    }
                    catch
                    {
                        settings.ProxyPass = "";
                    }
                }
                val = key.GetValue("ProxyAddress");
                if (val != null)
                    settings.ProxyHost = val.ToString();
                val = key.GetValue("ProxyPort");
                if (val != null)
                    settings.ProxyPort = val.ToString();
                val = key.GetValue("UseProxy");
                if (val != null)
                    settings.ProxyEnable = (val.ToString() == "True");
                val = key.GetValue("UseProxyAuthentication");
                if (val != null)
                    settings.ProxyAuthenticate = (val.ToString() == "True");
                val = key.GetValue("UseHTTPS");
                if (val != null)
                    settings.ProxyUseHTTPS = (val.ToString() == "True");
                key.Close();
            }
            proxyClient(); //update client to use proxy settings
        }

        public void SaveRegistrySettings()
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\MOSIM\\TaskEditor\\Unity"); //key with proxy settings that are same for task editor and launcher
            if (key != null)
            {
                key.SetValue("ProxyUser", encryptionService.EncryptString(settings.ProxyUser));
                key.SetValue("ProxyPass", encryptionService.EncryptString(settings.ProxyPass));
                key.SetValue("ProxyAddress", settings.ProxyHost);
                key.SetValue("ProxyPort", settings.ProxyPort);
                key.SetValue("UseProxy", settings.ProxyEnable);
                key.SetValue("UseHTTPS", settings.ProxyUseHTTPS);
                key.SetValue("UseProxyAuthentication", settings.ProxyAuthenticate);
                key.Close();
            }
            proxyClient(); //update client settings to use proxy
        }

        public void SaveSettings()
        {
            File.WriteAllText("settings.json", Serialization.ToJsonString(settings));
            UpdateTitleBar();
        }

        /// <summary>
        /// Disposes all connections and processes
        /// </summary>
        public void Dispose()
        {
            try
            {
                //Dispose every executed instance
                foreach (ExecutableController executableController in RuntimeData.ExecutableControllers)
                {
                    executableController.Dispose();
                }

                //Dispose every adapter
                foreach (RemoteAdapter remoteAdapter in RuntimeData.AdapterInstances.Values)
                {
                    remoteAdapter.Dispose();
                }
            }
            catch (Exception)
            {
            }

            try
            {
                //Dispose every service connection
                foreach (RemoteService service in RuntimeData.ServiceInstances.Values)
                {
                    service.Dispose();
                }
            }
            catch (Exception)
            {

            }

            try
            {
                this.registerServer.Dispose();
            }
            catch (Exception)
            {

            }

            //Clear all the data
            RuntimeData.AdapterInstances.Clear();
            RuntimeData.ExecutableControllers.Clear();
            RuntimeData.MMUDescriptions.Clear();
            RuntimeData.ServiceInstances.Clear();
            RuntimeData.SessionIds.Clear();
        }



        #region events


        private void RegisterService_OnServiceUnregistered(object sender, RemoteService e)
        {
            e.OnInactive += OnRemoteInactive;
            UIData.SynchronizeServices();
        }

        private void RegisterService_OnServiceRegistered(object sender, RemoteService e)
        {
            e.OnInactive += OnRemoteInactive;

            UIData.SynchronizeServices();
        }

        private void RegisterService_OnAdapterUnregistered(object sender, RemoteAdapter e)
        {
            e.OnInactive += OnRemoteInactive;
            UIData.SynchronizeAdapters();
        }

        private void RegisterService_OnAdapterRegistered(object sender, RemoteAdapter e)
        {
            e.OnInactive += OnRemoteInactive;
            UIData.SynchronizeAdapters();
        }

        /// <summary>
        /// Method is called if a remote service switches to inactive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoteInactive(object sender, RemoteService e)
        {
            UIData.SynchronizeServices();
        }

        /// <summary>
        /// Method is called if a remote adapter switches to inactive
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRemoteInactive(object sender, RemoteAdapter e)
        {
            UIData.SynchronizeAdapters();
        }

        /// <summary>
        /// Callback for unhandled exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = (Exception)e.ExceptionObject;
            System.Windows.MessageBox.Show("Unhandled Exception occured!", exception.Message + " " + exception.StackTrace);
        }

        #endregion


        #region button interaction

        /// <summary>
        /// Method is called whenever the start button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            //Start the register server (if server is running it will restart it if not it will simply start the server
            this.Restart();
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            //Start the register server (if server is running it will restart it if not it will simply start the server
            this.Stop();
        }

        private void proxyClient()
        {
            if (settings.ProxyEnable)
            {
                proxy = new WebProxy(new Uri($"http{(settings.ProxyUseHTTPS ? "s" : "")}://{settings.ProxyHost}:{settings.ProxyPort}"), true);

                if (settings.ProxyAuthenticate)
                {
                    if (settings.ProxyUser == "")
                        proxy.UseDefaultCredentials = true;
                    else
                    {
                        proxy.UseDefaultCredentials = false;
                        proxy.Credentials = new NetworkCredential(settings.ProxyUser, settings.ProxyPass);
                    }
                }

                // Now create a client handler which uses that proxy
                httpClientHandler = new HttpClientHandler();
                httpClientHandler.Proxy = proxy;

                // Omit this part if you don't need to authenticate with the web server:
                /* if (needServerAuthentication)
                 {
                     httpClientHandler.PreAuthenticate = true;
                     httpClientHandler.UseDefaultCredentials = false;

                     // *** These creds are given to the web server, not the proxy server ***
                     httpClientHandler.Credentials = new NetworkCredential(
                         userName: serverUserName,
                         password: serverPassword);
                 }*/

                // Finally, create the HTTP client object
                client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            }
            else
                client = new HttpClient();
        }

        //my new experimental integration to task list editor
        private async void TaskEditorButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> PostData = new Dictionary<string, string>();

            PostData.Add("token", settings.TaskEditorToken);
            PostData.Add("action", "getTaskList");
            
            if (settings.TaskEditorApiUrl == "")
                System.Windows.MessageBox.Show("Task editor connection details are missing, go to settings to configure task list editor connectivity.", "Task Editor connection", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                string html = "Connection succesfull: ";
                var PostForm = new FormUrlEncodedContent(PostData);
                try
                {
                    var content = await client.PostAsync(settings.TaskEditorApiUrl, PostForm);
                    html += content.Content.ReadAsStringAsync().Result;
                }
                catch(Exception err)
                {
                    if (err.InnerException!=null)
                    html = "Connection error: " + err.InnerException.Message;
                    else
                    html = "Connection error: " + err.Message;
                }
                System.Windows.MessageBox.Show(html, "Task Editor connection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void MMULibraryBrowse_Click(object sender, RoutedEventArgs e)
        {
            mmus.ConfigureConnection(client, settings.DataPath + "\\MMUs\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            await mmus.GetMMUListFromServer();
            string mmuList = "";
            for (int i = 0; i < mmus.RemoteMMUs.Length; i++)
                mmuList += mmus.RemoteMMUs[i].name + " " + mmus.RemoteMMUs[i].version + "\r\n";
            System.Windows.MessageBox.Show("Remote library contains:\r\n"+mmuList, "Remote MMU library content",MessageBoxButton.OK,MessageBoxImage.Information);
        }

        private void MMULibraryAdd_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "MMU zip archives (*.zip)|*.zip|All files|*.*";
            openDialog.FilterIndex = 0;
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mmus.ConfigureConnection(client, settings.DataPath + "\\MMUs\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
                int importedCount = mmus.ImportMMUs(openDialog.FileNames);
                System.Windows.MessageBox.Show("Successfully imported " + importedCount.ToString() + " out of " + openDialog.FileNames.Length.ToString() + ".", "MMU import",MessageBoxButton.OK,MessageBoxImage.Information);
            }

        }

        private async void MMULibrarySyncDown_Click(object sender, RoutedEventArgs e)
        {
            mmus.ConfigureConnection(client, settings.DataPath + "\\MMUs\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            await mmus.GetMMUListFromServer();
            mmus.ScanLibrary(settings.DataPath + "MMUs/");
            if (!Directory.Exists(settings.DataPath + "MMU-zip/"))
                Directory.CreateDirectory(settings.DataPath + "MMU-zip/");
            mmus.CompareRemoteAndLocal(settings.DataPath + "MMU-zip/");
            if (mmus.SyncDownCount() == 0)
                System.Windows.MessageBox.Show("All remote MMUs are already available locally", "MMU library synchronization");
            else
                System.Windows.MessageBox.Show("There are " + mmus.SyncDownCount().ToString() + " MMUs to load from server", "MMU library synchronization");
            if (mmus.SyncDownCount() > 0)
                await mmus.GetFromServer(settings.DataPath + "MMU-zip/");
        }

        private async void MMULibrarySyncUp_Click(object sender, RoutedEventArgs e)
        {
            mmus.ConfigureConnection(client, settings.DataPath + "\\MMUs\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            await mmus.GetMMUListFromServer();
            mmus.ScanLibrary(settings.DataPath + "MMUs/");
            if (!Directory.Exists(settings.DataPath + "MMU-zip/"))
                Directory.CreateDirectory(settings.DataPath + "MMU-zip/");
            mmus.CompareRemoteAndLocal(settings.DataPath + "MMU-zip/");
            if (mmus.SyncUpCount() + mmus.SyncDownCount() == 0)
                System.Windows.MessageBox.Show("ALL local MMUs are already available on the server", "MMU library synchronization");
            else
                System.Windows.MessageBox.Show("There are " + mmus.SyncUpCount().ToString() + " MMUs to send to server.", "MMU library synchronization");
            if (mmus.SyncUpCount() > 0)
                await mmus.SendToServer(settings.DataPath + "MMU-zip/");
        }

        private async void MMULibrarySync_Click(object sender, RoutedEventArgs e)
        {
            mmus.ConfigureConnection(client,settings.DataPath+"\\MMUs\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            await mmus.GetMMUListFromServer();
            mmus.ScanLibrary(settings.DataPath + "MMUs/");
            if (!Directory.Exists(settings.DataPath + "MMU-zip/"))
                Directory.CreateDirectory(settings.DataPath + "MMU-zip/");
            mmus.CompareRemoteAndLocal(settings.DataPath + "MMU-zip/");
            if (mmus.SyncUpCount()+ mmus.SyncDownCount() == 0)
                System.Windows.MessageBox.Show("MMU libraries are in sync", "MMU library synchronization");
            else
                System.Windows.MessageBox.Show("There are " + mmus.SyncUpCount().ToString() + " MMUs to send to server.\r\nThere are " + mmus.SyncDownCount().ToString() + " MMUs to load from server", "MMU library synchronization");
            if (mmus.SyncUpCount()>0)
            await mmus.SendToServer(settings.DataPath + "MMU-zip/");
            if (mmus.SyncDownCount() > 0)
            await mmus.GetFromServer(settings.DataPath + "MMU-zip/");

            /*
            Dictionary<string, string> PostData = new Dictionary<string, string>();

            PostData.Add("token", settings.TaskEditorToken);
            PostData.Add("action", "getMMUList");

            if (settings.TaskEditorApiUrl == "")
                System.Windows.MessageBox.Show("Task editor connection details are missing, go to settings to configure task list editor connectivity.", "Task Editor connection", MessageBoxButton.OK, MessageBoxImage.Information);
            else
            {
                string html = "Synchronization succesfull: \r\n";
                var PostForm = new FormUrlEncodedContent(PostData);
                try
                {
                    var content = await client.PostAsync(settings.TaskEditorApiUrl, PostForm);
                    html += content.Content.ReadAsStringAsync().Result;
                }
                catch (Exception err)
                {
                    if (err.InnerException != null)
                        html = "Connection error: \r\n" + err.InnerException.Message;
                    else
                        html = "Connection error: \r\n" + err.Message;
                }

                System.Windows.MessageBox.Show(html, "MMU list synchronization", MessageBoxButton.OK, MessageBoxImage.Information);
            }*/
        }

        /// <summary>
        /// Method is called if the exit button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// Method is called if the settings button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            //Create and open a new settings window
            SettingsWindow window = new SettingsWindow(this);
            window.ShowDialog(); //showing window in modal form, then it cannot be hidden somwehere below the main window
            //window.Show();
        }


        /// <summary>
        /// Method is called if the arrange window button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrangeWindows_Click(object sender, RoutedEventArgs e)
        {
            this.AutoArrangeWindows();
        }


        /// <summary>
        /// Method is called if the main window is closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.showPerformance = false;
            this.running = false;

            this.Dispose();
        }


        private void adapterListView_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                RemoteAdapter adapter = UIData.AdapterCollection.ElementAt(this.adapterListView.SelectedIndex);


                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

                stringBuilder.AppendLine("Name" + " : " + adapter.Description.Name);
                stringBuilder.AppendLine("ID" + " : " + adapter.Description.ID);
                stringBuilder.AppendLine("Language" + " : " + adapter.Description.Language);


                if (adapter.Description.Parameters != null && adapter.Description.Parameters.Count > 0)
                {
                    stringBuilder.AppendLine("Parameters:");

                    foreach (var entry in adapter.Description.Parameters)
                    {
                        stringBuilder.AppendLine(entry.Name + " , " + entry.Type + " , required:" + entry.Required);
                    }
                }

                System.Windows.MessageBox.Show(stringBuilder.ToString(), adapter.Name);


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void serviceListView_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                RemoteService service = UIData.ServiceCollection.ElementAt(this.serviceListView.SelectedIndex);


                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();


                stringBuilder.AppendLine("Name" + " : " + service.Description.Name);
                stringBuilder.AppendLine("ID" + " : " + service.Description.ID);
                stringBuilder.AppendLine("Language" + " : " + service.Description.Language);


                if (service.Description.Parameters != null && service.Description.Parameters.Count > 0)
                {
                    stringBuilder.AppendLine("Parameters:");

                    foreach (var entry in service.Description.Parameters)
                    {
                        stringBuilder.AppendLine(entry.Name + " , " + entry.Type + " , required:" + entry.Required);
                    }
                }
                System.Windows.MessageBox.Show(stringBuilder.ToString(), service.Name);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void  mmuView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var item = e.Source as System.Windows.Controls.TextBlock;
            if (item != null)
            {
                (item.DataContext as MMUOrderAndDescriptionData).Priority++;
                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.mmuView.ItemsSource);
                view.Refresh();
                
                //mmuView.RaiseEvent(new RoutedEventArgs(mmuView.SourceUpdated));

            }

            //e.Source.DataContext.Priority++;
        }

        private void mmuView_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var ob = sender as FrameworkElement;
            if (ob == null)
                return;
            try
            {
                var mmu = ob.DataContext as MMUOrderAndDescriptionData; //this way we take data directly from the clicked listview item

                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

                stringBuilder.AppendLine("Name" + " : " + mmu.Name);
                stringBuilder.AppendLine("ID" + " : " + mmu.ID);
                stringBuilder.AppendLine("MotionType" + " : " + mmu.MotionType);
                stringBuilder.AppendLine("Language" + " : " + mmu.Language);
                stringBuilder.AppendLine("Author" + " : " + mmu.Author);
                stringBuilder.AppendLine("Version" + " : " + mmu.Version);
                stringBuilder.AppendLine("Short Description" + " : " + mmu.ShortDescription);
                stringBuilder.AppendLine("Long Description" + " : " + mmu.LongDescription);
                if (mmu.Properties != null && mmu.Properties.Count > 0)
                {
                    stringBuilder.AppendLine("---------------------------------------------------------");

                    stringBuilder.AppendLine("Properties:");

                    foreach (var entry in mmu.Properties)
                    {
                        stringBuilder.AppendLine(entry.Key + " : " + entry.Value);
                    }
                }

                if (mmu.Parameters != null && mmu.Parameters.Count > 0)
                {
                    stringBuilder.AppendLine("---------------------------------------------------------");

                    stringBuilder.AppendLine("Parameters:");

                    foreach (var entry in mmu.Parameters)
                    {
                        stringBuilder.AppendLine(entry.Name + " , " + entry.Type + " , required:" + entry.Required + " , " + entry.Description);
                    }
                }

                //Print the events
                if (mmu.Events != null && mmu.Events.Count > 0)
                {
                    stringBuilder.AppendLine("---------------------------------------------------------");

                    stringBuilder.AppendLine("Events:");

                    foreach (var entry in mmu.Events)
                    {
                        stringBuilder.AppendLine(entry);
                    }
                }


                System.Windows.MessageBox.Show(stringBuilder.ToString(), mmu.Name);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion


        #region private methods 

        /// <summary>
        /// Displays the folder selection dialog and writes the result to the settings file
        /// </summary>
        private void ShowFolderSelectionDialog(string text = "")
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = text + "Please select the path of the data folder";

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    settings.DataPath = folderDialog.SelectedPath + "/";
                    SaveSettings();
                }
            }
        }

        /// <summary>
        /// Automatically arranges all started process windows
        /// </summary>
        private void AutoArrangeWindows()
        {
            int height = 220;
            int width = 200;

            int num = 0;
            foreach (ExecutableController process in RuntimeData.ExecutableControllers)
            {
                process.SetupWindow(num * height, 0, width, height);
                num++;
            }
        }


        /// <summary>
        /// Method checks if the required folders are defined. Otherwise they are automatically generated
        /// </summary>
        /// <param name="path"></param>
        private void CheckFolderStructure(string path)
        {
            try
            {                
                if (!Directory.Exists(path + "Adapters"))
                    Directory.CreateDirectory(path + "//Adapters");

                if (!Directory.Exists(path + "MMUs"))
                    Directory.CreateDirectory(path + "//MMUs");

                if (!Directory.Exists(path + "Services"))
                    Directory.CreateDirectory(path + "//Services");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Problem at automatically setting up folder structure" + e.Message, "Problem at setting up folder structure");
            }
        }



        /// <summary>
        /// Setups the environment and starts the modules
        /// </summary>
        /// <param name="adapterPath">The path of the modules</param>
        /// <param name="mmuPath">The path of the mmus</param>
        private void SetupEnvironment(List<string> adapterPaths, List<string> mmuPaths, List<string> servicePaths)
        {
            //To do 

            port = settings.MinPort - 1; //on the first use the port number is incremented so we don't want to skip the actual first port specified in the settings, thus -1
            //Set the running flag to true
            this.running = true;
        }


        /// <summary>
        /// Sets up the adapters
        /// </summary>
        /// <param name="adapterPath"></param>
        private void SetupAdapters(string adapterPath, string mmuPath, string servicePath)
        {
            //Fetch all modules and start them
            foreach (string folderPath in Directory.GetDirectories(adapterPath))
            {
                //Find the description file
                string descriptionFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains("description.json"));

                //Skip if no description file
                if (descriptionFile == null)
                    continue;

                //Get the ExecutableDescription of the adapter
                MExecutableDescription executableDescription = Serialization.FromJsonString<MExecutableDescription>(File.ReadAllText(descriptionFile));

                //Determine the filename of the executable file
                string executableFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains(executableDescription.ExecutableName));

                port = NetworkAdapters.getNextAvailablePort(port, this.settings.MaxPort);
                if (port == -1)
                {
                    System.Windows.MessageBox.Show("No ports are available to start service or adapters.", "Network error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Create a controller for the executable process
                ExecutableController exeController = new ExecutableController(executableDescription, new MIPAddress(RuntimeData.MMIRegisterAddress.Address, port), RuntimeData.MMIRegisterAddress, mmuPath, executableFile, settings.HideWindows);

                //Add the executable
                RuntimeData.ExecutableControllers.Add(exeController);
            }
        }


        /// <summary>
        /// Sets up the adapters
        /// </summary>
        /// <param name="adapterPath"></param>
        private void SetupServices(string adapterPath, string mmuPath, string servicePath)
        {
            //Setup the services
            foreach (string folderPath in Directory.GetDirectories(servicePath))
            {
                //Find the description file
                string descriptionFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains("description.json"));

                //Skip if no description file
                if (descriptionFile == null)
                    continue;


                //Get the ExecutableDescription of the service
                MExecutableDescription executableDescription = Serialization.FromJsonString<MExecutableDescription>(File.ReadAllText(descriptionFile));
                string executableFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains(executableDescription.ExecutableName));

                port = NetworkAdapters.getNextAvailablePort(port, this.settings.MaxPort);
                if (port == -1)
                {
                    System.Windows.MessageBox.Show("No ports are available to start service or adapters.", "Network error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ExecutableController exeController = new ExecutableController(executableDescription, new MIPAddress(RuntimeData.MMIRegisterAddress.Address, port), RuntimeData.MMIRegisterAddress, mmuPath, executableFile, settings.HideWindows);

                RuntimeData.ExecutableControllers.Add(exeController);

            }
        }

        /// <summary>
        /// Setups the environment and starts the modules
        /// </summary>
        /// <param name="adapterPath">The path of the modules</param>
        /// <param name="mmuPath">The path of the mmus</param>
        private void SetupEnvironment(string adapterPath, string mmuPath, string servicePath)
        {
            port = settings.MinPort - 1; //on the first use the port number is incremented so we don't want to skip the actual first port specified in the settings, thus -1
            //Set the running flag to true
            this.running = true;

            //Fetch all modules and start them
            foreach (string folderPath in Directory.GetDirectories(adapterPath))
            {
                //Find the description file
                string descriptionFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains("description.json"));

                //Skip if no description file
                if (descriptionFile == null)
                    continue;

                //Get the ExecutableDescription of the adapter
                MExecutableDescription executableDescription = Serialization.FromJsonString<MExecutableDescription>(File.ReadAllText(descriptionFile));

                //Determine the filename of the executable file
                string executableFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains(executableDescription.ExecutableName));

                port = NetworkAdapters.getNextAvailablePort(port, this.settings.MaxPort);
                if (port == -1)
                {
                    System.Windows.MessageBox.Show("No ports are available to start service or adapters.", "Network error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Create a controller for the executable process
                ExecutableController exeController = new ExecutableController(executableDescription, new MIPAddress(RuntimeData.MMIRegisterAddress.Address, port),RuntimeData.MMIRegisterAddress, mmuPath, executableFile, settings.HideWindows);

                RuntimeData.ExecutableControllers.Add(exeController);

                
            }

            //Setup the services
            foreach (string folderPath in Directory.GetDirectories(servicePath))
            {
                //Find the description file
                string descriptionFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains("description.json"));

                //Skip if no description file
                if (descriptionFile == null)
                    continue;


                //Get the ExecutableDescription of the service
                MExecutableDescription executableDescription = Serialization.FromJsonString<MExecutableDescription>(File.ReadAllText(descriptionFile));
                string executableFile = Directory.GetFiles(folderPath).ToList().Find(s => s.Contains(executableDescription.ExecutableName));

                port = NetworkAdapters.getNextAvailablePort(port, this.settings.MaxPort);
                if (port == -1)
                {
                    System.Windows.MessageBox.Show("No ports are available to start service or adapters.", "Network error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                ExecutableController exeController = new ExecutableController(executableDescription, new MIPAddress(RuntimeData.MMIRegisterAddress.Address, port), RuntimeData.MMIRegisterAddress, mmuPath, executableFile, settings.HideWindows);

                RuntimeData.ExecutableControllers.Add(exeController);

            }

            //Start the controllers
            foreach(ExecutableController executableController in RuntimeData.ExecutableControllers)
            {
                MBoolResponse response = executableController.Start();

                if (!response.Successful)
                {
                    System.Windows.MessageBox.Show("Cannot start application: " + executableController.Name + " " + (response.LogData.Count >0? response.LogData[0]: ""));
                }
            }

            //Create a new thread which checks the loadable MMUs
            ThreadPool.QueueUserWorkItem(delegate
            {
                while (this.running)
                {
                    Thread.Sleep(1000);

                    this.UpdateLoadableMMUs();
                }
            });
        }


        /// <summary>
        /// Method setus up the performance visualuzatuib
        /// </summary>
        private void SetupPerformanceBar()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            ThreadPool.QueueUserWorkItem(delegate
            {
                while (this.showPerformance)
                {
                    Thread.Sleep(200);

                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                    {
                        this.CpuProgessBar.Value = cpuCounter.NextValue();

                        float totalRam = new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024 / 1024;
                        float available = new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory / 1024 / 1024;

                        this.RamProgessBar.Value = 1 - (available / totalRam);
                        this.RamLabel.Content = "Ram: " + (totalRam - available) + " MB";
                    }));
                }
            });
        }


        /// <summary>
        /// Fetches the lodable MMUs from the adapters
        /// </summary>
        private void UpdateLoadableMMUs()
        {
            //Create a dictionary which holds the MMU Descriptions and the corresponding addresses
            Dictionary<MMUDescription, List<MIPAddress>> availableMMUs = new Dictionary<MMUDescription, List<MIPAddress>>();

            //Iterate over each adapter (do use for instead for-each due to possible changes of list)
            for (int i = RuntimeData.AdapterInstances.Count - 1; i >= 0; i--)
            {
                RemoteAdapter adapter = RuntimeData.AdapterInstances.Values.ElementAt(i); 

                List<MMUDescription> mmuDescriptions = new List<MMUDescription>();
                try
                {
                    mmuDescriptions = adapter.GetLoadableMMUs("default");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Problem receiving loadable MMUs: " + e.Message);
                }

                //Check if MMU is already available and add to dictionary
                foreach (MMUDescription description in mmuDescriptions)
                {
                    MMUDescription match = availableMMUs.Keys.ToList().Find(s => s.ID == description.ID && s.Name == description.Name);

                    if (match == null)
                    {
                        availableMMUs.Add(description, new List<MIPAddress>());
                        match = description;
                    }

                    availableMMUs[match].Add(new MIPAddress(adapter.Address, adapter.Port));
                }
            }

            //Update the MMU descriptions
            RuntimeData.MMUDescriptions.Clear();
            foreach (MMUDescription description in availableMMUs.Keys)
            {
                RuntimeData.MMUDescriptions.TryAdd(description.ID, description);
            }
            
            //Update the MMU collection in ui thread
            UIData.SetMMUDescriptions(availableMMUs.Keys.ToList());
        }

        #endregion

    }
}
