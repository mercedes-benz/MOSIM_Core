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
using System.Windows.Media.Animation;

namespace MMILauncher
{

    public class TWatcherError
    {
        private class PathErrPair
        {
            public PathErrPair(string filePath)
            {
                this.path = filePath;
                this.ErrCount = 1;
            }
            public string path;
            public int ErrCount;
        }

        private List<PathErrPair> data;
        private System.Timers.Timer ErrTimer;

        public TWatcherError(int ErrorTreshold, OnFileCheck CheckFile)
        {
            data = new List<PathErrPair>();
            ErrTreshold = ErrorTreshold;
            OnCheckFile = CheckFile;

            ErrTimer = new System.Timers.Timer();
            ErrTimer.Interval = 2000;
            // Hook up the Elapsed event for the timer. 
            ErrTimer.Elapsed += OnTimer;
            // Have the timer fire repeated events (true is the default)
            ErrTimer.AutoReset = true;
        }
        
        private void OnTimer(Object source, System.Timers.ElapsedEventArgs e)
        {
            for (int i = data.Count-1; i>=0 ; i--)
                if (OnCheckFile(data[i].path))
                {
                    data.RemoveAt(i);
                    if (data.Count == 0)
                        ErrTimer.Enabled = false;
                }
        }

        public bool AddError(string filePath)
        {
            bool found = false;
            for (int i = 0; i < data.Count; i++)
                if (data[i].path == filePath)
                {
                    found = true;
                    data[i].ErrCount++;
                    if (data[i].ErrCount > ErrTreshold)
                    {
                        OnTresholdEvent(data[i].path);
                        data.RemoveAt(i);
                        if (data.Count == 0)
                            ErrTimer.Enabled = false;
                        return true;
                    }
                }
            if (!found)
            {
                data.Add(new PathErrPair(filePath));
                ErrTimer.Enabled = true;
            }
            return false;
        }

        public bool RemoveError(string filePath)
        {
            for (int i = 0; i < data.Count; i++)
                if (data[i].path == filePath)
                {
                    data.RemoveAt(i);
                     if (data.Count == 0)
                     ErrTimer.Enabled = false;
                     return true;
                }
            return false;
        }

        public int ErrTreshold;
        public OnTreshold OnTresholdEvent;
        public OnFileCheck OnCheckFile;
        public delegate void OnTreshold(string path);
        public delegate bool OnFileCheck(string path);
    }

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

        private const string version = "5.1.4";

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

        public bool isRunning { get { return running;  } }

        private HttpClientHandler httpClientHandler;
        private WebProxy proxy;
        public HttpClient client;
        public MMULibrary mmus = new MMULibrary();
        public string MMULibraryName = "Default";
        FileSystemWatcher watcherMMULib; //file system watcher for MMU library settings files

        private TWatcherError WatcherErrors;

        #endregion

        public NetworkAdapters NetworkAdapters;


        /// <summary>
        /// Main constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            //Register for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            this.StopButton.IsEnabled = false;
            //Important assign the dispatcher at the beginning
            UIData.Initialize(this.Dispatcher);
            SetupLibraries();
            RegisterAppInstance(); //register app instance in system's registry to allow passing MMUs and other resources to specific instances.

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

            if (!File.Exists(AppPath() + "settings.json"))
                initiateSettingsFile();
            else //Parse the settings from file
            try
            {  //Load the settings file from json
                settings = Serialization.FromJsonString<ServerSettings>(File.ReadAllText(AppPath() + "settings.json"));
                    if (settings.DataPath.EndsWith("/"))
                        settings.DataPath = settings.DataPath.Substring(0, settings.DataPath.Length - 1) + "\\";
                    else
                    if (!settings.DataPath.EndsWith("\\"))
                        settings.DataPath += "\\";
                //Check if directory exists -> if not user must provide input
                if (!FolderStructureOK(settings.DataPath))
                ShowFolderSelectionDialog("Invalid path: ");
            }
            catch (Exception)
            {
               initiateSettingsFile();
            }

            //Read settings from system registry
            encryptionService = new Encrypt();
            LoadRegistrySettings();

            //Assign the port (common for loaded settings from file or settings from default settings file.
            RuntimeData.MMIRegisterAddress.Port = settings.RegisterPort;
            RuntimeData.MMIRegisterAddress.Address = settings.RegisterAddress;

            //Loads MMU libraries
            LoadMMULibraries();

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
                this.StartButton_Click(this, new RoutedEventArgs());
        }

        public void initiateSettingsFile()
        {
            settings = new ServerSettings();
            string path = AppPath();
            path = path.Substring(0, path.Length - 1);
            int i = path.LastIndexOf("\\");
                if (i > -1)
                {
                    if (FolderStructureOK(path.Substring(0, i+1)))
                    settings.DataPath = path.Substring(0, i+1);
                    else
                    ShowFolderSelectionDialog("No settings file found: "); //Show the folder selection dialog
                }
                else
                    if (FolderStructureOK(path))
                    settings.DataPath = path;
                    else
                    ShowFolderSelectionDialog("No settings file found: "); //Show the folder selection dialog
            SaveSettings(); //if settings did not exist before save the default settings
        }

        public string AppPath()
        {
            var path = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            if (path[path.Length - 1] != '\\')
                path += "\\";
            return path;
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
            this.SetupEnvironment(datapath + "Adapters\\", datapath + "MMUs\\", datapath + "Services\\");
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

        public void SetupLibraries()
        {
            WatcherErrors = new TWatcherError(5, AddNewLibraryFile);
            string path = AppPath() + "settings\\libraries\\mmu";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            watcherMMULib = new FileSystemWatcher();
            watcherMMULib.Path = path;
            watcherMMULib.Filter = "*.json";
            watcherMMULib.Created += WatcherMMULib_Created;
            watcherMMULib.Changed += WatcherMMULib_Created;
            watcherMMULib.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.FileName | NotifyFilters.LastWrite;
            watcherMMULib.EnableRaisingEvents = true;
        }

        private void WatcherMMULib_Created(object sender, FileSystemEventArgs e)
        {
            AddNewLibraryFile(e.FullPath);            
        }

        private bool AddNewLibraryFile(string path)
        {
            try
            {
                LibraryLink newMMULib = Serialization.FromJsonString<LibraryLink>(File.ReadAllText(path)); //when file is incomplete this will throw exception
                bool found = false;
                for (int i = 0; (i < mmus.Remotes.Count) && (!found); i++)
                    if ((mmus.Remotes[i].URL == newMMULib.url) && (mmus.Remotes[i].Token == newMMULib.token))
                        found = true;
                WatcherErrors.RemoveError(path); //if until now there is no exception remove the file from error list to avoid multiple dialog boxes showing up.
                if (!found)
                {
                    mmus.AddRemote(new RemoteLibrary(newMMULib,path),client);
                    string msg = UpdateDefaultTaskEditor()==true ? "\r\nTask editor settings have been updated to match the new library data." : "";
                    System.Windows.Forms.MessageBox.Show("New MMU library has been added: " + newMMULib.name + msg, "MMU library discovery", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch
            {
                if (WatcherErrors.AddError(path)) //if adding the same path for nth time and n is larger than treshold, show error message.
                    System.Windows.Forms.MessageBox.Show("New MMU library file has been added, but it cannot be read or file content is corrupted.\r\n" + Path.GetFileName(path), "MMU library discovery", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return true;
            }
            return false;
        }

        public void AddMMULibrary(LibraryLink LibLink)
        {
            var path = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            if (path[path.Length - 1] != '\\')
                path += "\\";
            var mmupath = path + "settings\\libraries\\mmu";
            if (!Directory.Exists(mmupath))
                Directory.CreateDirectory(mmupath);
            int n = 1;
            while (System.IO.File.Exists(mmupath + "lib" + n.ToString() + ".json"))
                n++;
            System.IO.File.WriteAllText(mmupath + "lib" + n.ToString() + ".json", Serialization.ToJsonString(LibLink));
            LoadMMULibraries();
        }

        public void UpdateMMULibrary(LibraryLink LibLink)
        {
            int index = -1;
            for (int i = 0; i < mmus.Remotes.Count; i++)
                if ((mmus.Remotes[i].URL == LibLink.url) && (mmus.Remotes[i].Token == LibLink.token))
                    index = i;
            if (index > -1)
            {
                if (mmus.Remotes[index].LocalFileName != "")
                {
                    mmus.Remotes[index].Name = LibLink.name;
                    if (System.IO.File.Exists(mmus.Remotes[index].LocalFileName))
                        System.IO.File.Delete(mmus.Remotes[index].LocalFileName);
                    System.IO.File.WriteAllText(mmus.Remotes[index].LocalFileName, Serialization.ToJsonString(LibLink));
                    //LoadMMULibraries();
                }
                else
                    AddMMULibrary(LibLink);
            }
        }

        public bool UpdateDefaultTaskEditor()
        {
            string path = AppPath() + "settings\\libraries\\mmu";
            string newDefault = "-1";
            if (File.Exists(path + "\\defaultlib.txt"))
            {
                newDefault = path + "\\lib" + File.ReadAllText(path + "\\defaultlib.txt") + ".json";
                for (int i=0; i<mmus.Remotes.Count; i++)
                    if (mmus.Remotes[i].LocalFileName==newDefault)
                    {
                        settings.TaskEditorApiUrl = mmus.Remotes[i].URL;
                        settings.TaskEditorToken = mmus.Remotes[i].Token;
                        try
                        {
                            SaveSettings(false);
                        }
                        catch
                        {
                            return false;
                        }
                        try
                        {
                            System.IO.File.Delete(path + "\\defaultlib.txt");
                            return true;
                        }
                        catch { }
                    }
            }
            return false;
        }

        public void LoadMMULibraries()
        {
            mmus.Remotes.Clear();
            string path = AppPath() + "settings\\libraries\\mmu";

            var mmuLibs = Directory.GetFiles(path);
            for (int i = 0; i < mmuLibs.Length; i++)
                if (mmuLibs[i].Substring(mmuLibs[i].Length-5) ==".json")
                {
                    try
                    {
                        LibraryLink newMMULib = Serialization.FromJsonString<LibraryLink>(File.ReadAllText(mmuLibs[i]));
                        if (!mmus.FindLibrary(newMMULib))
                            mmus.AddRemote(new RemoteLibrary(newMMULib, mmuLibs[i]),client);
                    }
                    catch (Exception)
                    { }
                }

            if ((settings.TaskEditorApiUrl != "") && !mmus.FindLibrary(settings.TaskEditorApiUrl, settings.TaskEditorToken))
                mmus.InsertRemote(0,new RemoteLibrary(settings.TaskEditorApiUrl, settings.TaskEditorToken, "Project library"), client);

            mmus.ConfigureConnection(client, settings.DataPath + "MMUs\\", settings.DataPath + "MMU-zip\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            UpdateDefaultTaskEditor();
        }

        public void LoadRegistrySettings()
        {
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\MOSIM\\Launcher"); //key with proxy settings that are same for task editor and launcher
            if (key != null)
            {
                int instanceCount = 0;
                var values = key.GetSubKeyNames();
                for (int i = 0; i < key.SubKeyCount; i++)
                    if (values[i].IndexOf("Instance") == 0)
                    {
                        int curinstance = 0;
                        if (Int32.TryParse(values[i].Substring(8), out curinstance))
                        {
                            instanceCount = Math.Max(curinstance, instanceCount);
                            var subkey = key.OpenSubKey(values[i], false);
                            if (subkey != null)
                            {
                                if (subkey.GetValue("Path").ToString() == System.Windows.Forms.Application.ExecutablePath)
                                {
                                    var val = subkey.GetValue("Name");
                                    if (val != null)
                                        MMULibraryName = val.ToString();
                                }
                            }
                            subkey.Close();
                        }
                    }   
            }
            LoadProxySettings();
        }

        public void LoadProxySettings()
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

        public bool RegisterAppInstance(bool saveName = false)
        {
            string instanceKey = "";
            List<string> names = new List<string>();
            Microsoft.Win32.RegistryKey key;
            key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("SOFTWARE\\MOSIM\\Launcher");
            if (key!=null)
            {
                int instanceCount = 0;
                bool found = false;
                var values = key.GetSubKeyNames();
                for (int i = 0; i < key.SubKeyCount; i++)
                    if (values[i].IndexOf("Instance") == 0)
                    {
                        int curinstance = 0;
                        if (Int32.TryParse(values[i].Substring(8), out curinstance))
                        {
                            instanceCount = Math.Max(curinstance, instanceCount);
                            var subkey = key.OpenSubKey(values[i], false);
                            if (subkey != null)
                            {
                                if (subkey.GetValue("Path").ToString() == System.Windows.Forms.Application.ExecutablePath)
                                {
                                    found = true;
                                    instanceKey = values[i];
                                }
                                else
                                    names.Add(subkey.GetValue("Name").ToString());
                            }
                            subkey.Close();
                        }
                    }
                if ((!found) || (saveName))
                {
                    instanceCount++;
                    var subkey = key.CreateSubKey(found?instanceKey:("Instance" + instanceCount.ToString()));
                    if (subkey != null)
                    {
                        subkey.SetValue("Path", System.Windows.Forms.Application.ExecutablePath);
                        subkey.SetValue("Version", version);
                        if (saveName)
                        {
                            if (names.Exists(x=> x==MMULibraryName))
                                saveName = false;
                            else
                                subkey.SetValue("Name", MMULibraryName);
                        }
                        else
                            subkey.SetValue("Name", (instanceCount == 1 ? "Default" : ("Instance " + instanceCount.ToString())));
                        subkey.Close();
                    }
                }
                key.Close();
            }
            return saveName;
        }

        public void SaveSettings(bool updateTitle = true)
        {
            ServerSettings saveset = new ServerSettings(); //making sure the settings file does not contain proxy user name and password.
            saveset = settings;
            saveset.ProxyPass = "";
            saveset.ProxyUser = "";
            File.WriteAllText(AppPath() + "settings.json", Serialization.ToJsonString(saveset));
            if (updateTitle)
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
                    executableController.Dispose();

                //Dispose every adapter
                foreach (RemoteAdapter remoteAdapter in RuntimeData.AdapterInstances.Values)
                    remoteAdapter.Dispose();
            }
            catch (Exception) {}

            try
            {
                //Dispose every service connection
                foreach (RemoteService service in RuntimeData.ServiceInstances.Values)
                    service.Dispose();
            }
            catch (Exception) {}

            try
            {
                this.registerServer.Dispose();
            }
            catch (Exception) { }

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

        private void TaskEditorMsgNoSetup()
        {
            System.Windows.MessageBox.Show("Task editor connection details are missing, go to settings to configure task list editor connectivity.", "Task Editor connection", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [Serializable] //this is also used in settings window - create separate task editor library
        public class TaskEditorTestResponse
        {
            public int projectid;
            public string projectName;
            public TaskEditorTestResponse()
            {
                this.projectid = 0;
                this.projectName = "";
            }
        }

        //Testing task editor connectivity - this should also be used on the settings window instead of separate implementation
        private async void TaskEditorButton_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> PostData = new Dictionary<string, string>();

            PostData.Add("token", settings.TaskEditorToken);
            PostData.Add("action", "testConnection");

            if (settings.TaskEditorApiUrl == "")
                TaskEditorMsgNoSetup();
            else
            {
                string html = "Connection succesfull to: ";
                var PostForm = new FormUrlEncodedContent(PostData);
                try
                {
                    var content = await client.PostAsync(settings.TaskEditorApiUrl, PostForm);
                    var testResult = Serialization.FromJsonString<TaskEditorTestResponse>(content.Content.ReadAsStringAsync().Result);
                    html += testResult.projectName;
                }
                catch (Exception err)
                {
                    if (err.InnerException != null)
                        html = "Connection error: " + err.InnerException.Message;
                    else
                        html = "Connection error: " + err.Message;
                }
                System.Windows.MessageBox.Show(html, "Task Editor connection test", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void MMULibraryBrowse_Click(object sender, RoutedEventArgs e)
        {
            mmus.ConfigureConnection(client, settings.DataPath + "MMUs\\", settings.DataPath + "MMU-zip\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            mmus.ScanLibrary();
            for (int i=0; i<mmus.Remotes.Count; i++)
            {
                await mmus.Remotes[i].GetSettings();
                await mmus.Remotes[i].GetMMUListFromServer();
                mmus.CompareRemoteAndLocal(i);
            }

            MMULibraryWindow dialog = new MMULibraryWindow(this);
            dialog.ShowDialog();
        }

        private void MMULibraryAdd_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "MMU zip archives (*.zip)|*.zip|All files|*.*";
            openDialog.FilterIndex = 0;
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mmus.ConfigureConnection(client, settings.DataPath + "MMUs\\", settings.DataPath + "MMU-zip\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
                int importedCount = mmus.ImportMMUs(openDialog.FileNames);
                System.Windows.MessageBox.Show("Successfully imported " + importedCount.ToString() + " out of " + openDialog.FileNames.Length.ToString() + ".", "MMU import",MessageBoxButton.OK,MessageBoxImage.Information);
            }
        }

        private async void MMULibrarySyncDown_Click(object sender, RoutedEventArgs e)
        {
            if (settings.TaskEditorApiUrl=="")
            {
                TaskEditorMsgNoSetup();
                return;
            }
            Storyboard s = (Storyboard)TryFindResource("clearText");
            s.Stop();
            ActionProgressBar.Maximum = 100;
            ActionProgressBar.Value = 0;
            ActionProgressBar.Visibility = Visibility.Visible;
            ActionLabel.Content = "Comparing MMU lists...";
            ActionLabel.Opacity = 1;
            mmus.ConfigureConnection(client, settings.DataPath + "MMUs\\", settings.DataPath + "MMU-zip\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            await mmus.GetMMUListFromServer();
            mmus.ScanLibrary();
            if (!Directory.Exists(settings.DataPath + "MMU-zip"))
                Directory.CreateDirectory(settings.DataPath + "MMU-zip");
            mmus.CompareAndPackRemoteAndLocal(0);
            if (mmus.SyncDownCount() == 0)
                System.Windows.MessageBox.Show("All remote MMUs are already available locally", "MMU library synchronization");
            else
                System.Windows.MessageBox.Show("There are " + mmus.SyncDownCount().ToString() + " MMUs to load from server", "MMU library synchronization");
            if (mmus.SyncDownCount() > 0)
            {
                ActionLabel.Content = "Downloading MMUs...";
                await mmus.GetFromServer();
            }
            ActionProgressBar.Visibility = Visibility.Hidden;
            ActionLabel.Content = "MMU download complete";
            s.Begin();
        }

        private async void MMULibrarySyncUp_Click(object sender, RoutedEventArgs e)
        {
            if (settings.TaskEditorApiUrl == "")
            {
                TaskEditorMsgNoSetup();
                return;
            }
            Storyboard s = (Storyboard)TryFindResource("clearText");
            s.Stop();
            ActionProgressBar.Maximum = 100;
            ActionProgressBar.Value = 0;
            ActionProgressBar.Visibility = Visibility.Visible;
            ActionLabel.Content = "Comparing MMU lists...";
            ActionLabel.Opacity = 1;
            mmus.ConfigureConnection(client, settings.DataPath + "MMUs\\", settings.DataPath + "MMU-zip\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            await mmus.GetMMUListFromServer();
            mmus.ScanLibrary();
            if (!Directory.Exists(settings.DataPath + "MMU-zip"))
                Directory.CreateDirectory(settings.DataPath + "MMU-zip");
            mmus.CompareAndPackRemoteAndLocal(0);
            if (mmus.SyncUpCount() + mmus.SyncDownCount() == 0)
                System.Windows.MessageBox.Show("ALL local MMUs are already available on the server", "MMU library synchronization");
            else
                System.Windows.MessageBox.Show("There are " + mmus.SyncUpCount().ToString() + " MMUs to send to server.", "MMU library synchronization");
            if (mmus.SyncUpCount() > 0)
            {
                ActionLabel.Content = "Uploading MMUs...";
                await mmus.SendToServer(ActionProgress);
            }
            ActionProgressBar.Visibility = Visibility.Hidden;
            ActionLabel.Content = "MMU sync up complete";
            s.Begin();
        }

        private void appsecurity()
        {
            var md5 = System.Security.Cryptography.MD5.Create();
            var b = File.OpenRead(System.Windows.Forms.Application.ExecutablePath);
            var hashcode = md5.ComputeHash(b);
            var s=BitConverter.ToString(hashcode).Replace("-", "");
            b.Close();
            md5.Clear();
            //System.Text.Encoding.Default.GetString(hashcode)
            System.Windows.Forms.MessageBox.Show(s,"Hash code"); //verifies the code has not be modified by comparing the code with the code from license server.
            var aes = System.Security.Cryptography.Aes.Create();
            aes.Clear();
        }

        private void ActionProgress(long maxvalue, long value)
        {
            ActionProgressBar.Maximum=maxvalue;
            ActionProgressBar.Value = value;
        }

        private async void MMULibrarySync_Click(object sender, RoutedEventArgs e)
        {
            if (settings.TaskEditorApiUrl == "")
            {
                TaskEditorMsgNoSetup();
                return;
            }
            Storyboard s = (Storyboard)TryFindResource("clearText");
            s.Stop();
            ActionProgressBar.Maximum = 100;
            ActionProgressBar.Value = 0;
            ActionProgressBar.Visibility = Visibility.Visible;
            ActionLabel.Content = "Comparing MMU lists...";
            ActionLabel.Opacity = 1;
            mmus.ConfigureConnection(client,settings.DataPath+"MMUs\\", settings.DataPath + "MMU-zip\\", settings.TaskEditorApiUrl, settings.TaskEditorToken);
            await mmus.GetMMUListFromServer();
            mmus.ScanLibrary();
            if (!Directory.Exists(settings.DataPath + "MMU-zip"))
                Directory.CreateDirectory(settings.DataPath + "MMU-zip");
            mmus.CompareAndPackRemoteAndLocal(0);
            if (mmus.SyncUpCount()+ mmus.SyncDownCount() == 0)
                System.Windows.MessageBox.Show("MMU libraries are in sync", "MMU library synchronization");
            else
                System.Windows.MessageBox.Show("There are " + mmus.SyncUpCount().ToString() + " MMUs to send to server.\r\nThere are " + mmus.SyncDownCount().ToString() + " MMUs to load from server", "MMU library synchronization");
            if (mmus.SyncUpCount() > 0)
            {
                ActionLabel.Content = "Uploading MMUs...";
                await mmus.SendToServer(ActionProgress);
            }
            if (mmus.SyncDownCount() > 0)
            {
                ActionLabel.Content = "Downloading MMUs...";
                await mmus.GetFromServer(ActionProgress);
            }

            ActionProgressBar.Visibility = Visibility.Hidden;
            ActionLabel.Content = "MMU sync complete";
            s.Begin();
            
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

        [Serializable]
        class mycl
        {
            public string[] data;
            public string name;
        };

        public void RegisterURLHandler(string protocolName, string applicationPath) //this should be removed or linked to MMU Linker Library
        {
            mycl var1 = new mycl();
            var1.name="my name";
            var1.data = new string[2];
            var1.data[0]="string 1";
            var1.data[1]="string 2";
            
            File.WriteAllText(@"C:\Users\a0308730\Downloads\MOSIM\Idle\test.json", Serialization.ToJsonString<mycl>(var1));
            appsecurity();
            /*
            var mainkey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
            var key = mainkey.CreateSubKey(protocolName);
            key.SetValue("URL Protocol", protocolName);
            key.CreateSubKey(@"shell\open\command").SetValue("", "\"" + applicationPath + "\"");
            key.Close();
            mainkey.Close();*/
        }

        private void RegisterURLHandler_Click(object sender, RoutedEventArgs e)
        {

            RegisterURLHandler("mmulib",System.Windows.Forms.Application.ExecutablePath);
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
                        stringBuilder.AppendLine(entry.Name + " , " + entry.Type + " , required:" + entry.Required);
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
                        stringBuilder.AppendLine(entry.Name + " , " + entry.Type + " , required:" + entry.Required);
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
                        stringBuilder.AppendLine(entry.Key + " : " + entry.Value);
                }

                if (mmu.Parameters != null && mmu.Parameters.Count > 0)
                {
                    stringBuilder.AppendLine("---------------------------------------------------------");
                    stringBuilder.AppendLine("Parameters:");

                    foreach (var entry in mmu.Parameters)
                        stringBuilder.AppendLine(entry.Name + " , " + entry.Type + " , required:" + entry.Required + " , " + entry.Description);
                }

                //Print the events
                if (mmu.Events != null && mmu.Events.Count > 0)
                {
                    stringBuilder.AppendLine("---------------------------------------------------------");
                    stringBuilder.AppendLine("Events:");

                    foreach (var entry in mmu.Events)
                        stringBuilder.AppendLine(entry);
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
                    if (folderDialog.SelectedPath.EndsWith("\\"))
                        settings.DataPath = folderDialog.SelectedPath;
                    else
                        settings.DataPath = folderDialog.SelectedPath + "\\";
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

        private bool FolderStructureOK(string path)
        {
            if (!Directory.Exists(path + "Adapters"))
                return false;
            if (!Directory.Exists(path + "MMUs"))
                return false;
            if (!Directory.Exists(path + "Services"))
                return false;
            return true;
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
                    Directory.CreateDirectory(path + "Adapters");

                if (!Directory.Exists(path + "MMUs"))
                    Directory.CreateDirectory(path + "MMUs");

                if (!Directory.Exists(path + "Services"))
                    Directory.CreateDirectory(path + "Services");
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
            mmuPath=mmuPath.Replace("\\", "/"); //passing as argument escaped string leads to error in CSharpAdapter interpreting such windows path, hence changing of backward to forward slashes solves the problem
            adapterPath = adapterPath.Replace("\\", "/"); 
            servicePath = servicePath.Replace("\\", "/");
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
                    System.Windows.MessageBox.Show("Cannot start application: " + executableController.Name + " " + (response.LogData.Count >0? response.LogData[0]: ""));
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
