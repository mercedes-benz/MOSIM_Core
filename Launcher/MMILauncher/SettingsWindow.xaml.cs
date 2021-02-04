// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Klodowski

using MahApps.Metro.Controls;
using MMICSharp.Common.Communication;
using MMILauncher.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MMIULibrary;

namespace MMILauncher
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : MetroWindow
    {
        /// <summary>
        /// Instance of the mainwindow class
        /// </summary>
        private readonly MainWindow mainWindow;

        public SettingsWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.portInput.Text = RuntimeData.MMIRegisterAddress.Port.ToString();
            this.taskEditorCombo.Items.Clear();
            for (int i = 0; i < mainWindow.mmus.Remotes.Count; i++)
            {
                this.taskEditorCombo.Items.Add(mainWindow.mmus.Remotes[i]);
                if ((mainWindow.mmus.Remotes[i].URL == mainWindow.settings.TaskEditorApiUrl) &&
                    (mainWindow.mmus.Remotes[i].Token == mainWindow.settings.TaskEditorToken))
                    this.taskEditorCombo.SelectedIndex = i;
            }
            this.taskEditorCombo.DisplayMemberPath = "Name";
            this.taskEditorCombo.Items.Add(new RemoteLibrary("","","<New connection>"));
            this.taskEditorUrlInput.Text = mainWindow.settings.TaskEditorApiUrl;
            this.taskEditorTokenInput.Text = mainWindow.settings.TaskEditorToken;
            this.portMinInput.Text = mainWindow.settings.MinPort.ToString();
            this.portMaxInput.Text = mainWindow.settings.MaxPort.ToString();
            this.PathInput.Text = mainWindow.settings.DataPath;
            this.HideWindows.IsChecked = mainWindow.settings.HideWindows;
            this.AutoStart.IsChecked = mainWindow.settings.AutoStart;
            this.RegisterSaved.Visibility = Visibility.Hidden;
            this.ServiceSaved.Visibility = Visibility.Hidden;
            this.TaskEditorSaved.Visibility = Visibility.Hidden;
            this.ProxySaved.Visibility = Visibility.Hidden;
            this.MMULibSaved.Visibility = Visibility.Hidden;
            MMULibraryNameInput.Text = mainWindow.MMULibraryName;
            ProxyEnable.IsChecked=mainWindow.settings.ProxyEnable;
            ProxyHTTPSEnable.IsChecked=mainWindow.settings.ProxyUseHTTPS;
            proxyHostInput.Text = mainWindow.settings.ProxyHost;
            proxyPortInput.Text = mainWindow.settings.ProxyPort;
            ProxyAuthentication.IsChecked = mainWindow.settings.ProxyAuthenticate;
            proxyUserInput.Text = mainWindow.settings.ProxyUser;
            proxyPassInput.Password = mainWindow.settings.ProxyPass;
            mainWindow.NetworkAdapters.GetNetworkAdapters();
            ProxyEnable_Change(ProxyEnable, null);
            updateAdapterComboBox();
        }

        private void updateAdapterComboBox()
        {
            interfaceComboBox.Items.Clear();
            int selected = -1;
            for (int i = 0; i < mainWindow.NetworkAdapters.AvailableIp.Count; i++)
            {
                interfaceComboBox.Items.Add(mainWindow.NetworkAdapters.AvailableIp[i].Name + " / " + mainWindow.NetworkAdapters.AvailableIp[i].IP);
                if ((mainWindow.NetworkAdapters.AvailableIp[i].Name == mainWindow.settings.RegisterInterface) || (mainWindow.NetworkAdapters.AvailableIp[i].IP == mainWindow.settings.RegisterAddress))
                    selected = i;
            }
            interfaceComboBox.SelectedIndex = selected;
        }
        
        private void Interface_Change(object sender, RoutedEventArgs e)
        {
            if (RegisterSaved!=null)
            RegisterSaved.Visibility = Visibility.Hidden;
            PortValue_Change(sender, e);
        }

        private void Project_Change(object sender, RoutedEventArgs e)
        {
            if (taskEditorCombo == null)
                return;
            if (taskEditorCombo.SelectedIndex == -1)
                return;
            taskEditorUrlInput.Text = (taskEditorCombo.SelectedItem as RemoteLibrary).URL;
            taskEditorTokenInput.Text = (taskEditorCombo.SelectedItem as RemoteLibrary).Token;
        }

        private void Url_Change(object sender, RoutedEventArgs e)
        {
            if (taskEditorUrlOK == null)
                return;
            taskEditorUrlOK.Content = "-";
            taskEditorTokenOK.Content = "-";
            TaskEditorSaved.Visibility = Visibility.Hidden;
        }

        private void Token_Change(object sender, RoutedEventArgs e)
        {
            if (taskEditorTokenInput == null)
                return;
            taskEditorTokenOK.Content = "-";
            TaskEditorSaved.Visibility = Visibility.Hidden;
        }

        private void Proxy_Change(object sender, RoutedEventArgs e)
        {
        }

        private void ProxyEnable_Change(object sender, RoutedEventArgs e)
        {
            ProxyHTTPSEnable.IsEnabled = ProxyEnable.IsChecked.HasValue && ProxyEnable.IsChecked.Value;
            proxyHostInput.IsEnabled = ProxyEnable.IsChecked.HasValue && ProxyEnable.IsChecked.Value;
            proxyPortInput.IsEnabled = ProxyEnable.IsChecked.HasValue && ProxyEnable.IsChecked.Value;
            proxyHostLabel.IsEnabled = proxyHostInput.IsEnabled;
            proxyPortLabel.IsEnabled = proxyPortInput.IsEnabled;
            ProxyAuthentication.IsEnabled = ProxyEnable.IsChecked.HasValue && ProxyEnable.IsChecked.Value;
            ProxyAuthentication_Change(sender, e);
        }

        private void ProxyAuthentication_Change(object sender, RoutedEventArgs e)
        {
            proxyUserInput.IsEnabled = ProxyAuthentication.IsEnabled && ProxyAuthentication.IsChecked.Value && ProxyAuthentication.IsChecked.HasValue;
            proxyPassInput.IsEnabled = ProxyAuthentication.IsEnabled && ProxyAuthentication.IsChecked.Value && ProxyAuthentication.IsChecked.HasValue;
            proxyUserLabel.IsEnabled = proxyUserInput.IsEnabled;
            proxyPassLabel.IsEnabled = proxyPassInput.IsEnabled;
            //if (RegisterSaved != null)
            //    RegisterSaved.Visibility = Visibility.Hidden;
            //PortValue_Change(sender, e);
        }

        private void SaveProxyButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.settings.ProxyEnable = ProxyEnable.IsChecked.Value && ProxyEnable.IsChecked.HasValue;
            mainWindow.settings.ProxyUseHTTPS = ProxyHTTPSEnable.IsChecked.Value && ProxyHTTPSEnable.IsChecked.HasValue;
            mainWindow.settings.ProxyHost = proxyHostInput.Text;
            mainWindow.settings.ProxyPort = proxyPortInput.Text;
            mainWindow.settings.ProxyAuthenticate = ProxyAuthentication.IsChecked.Value && ProxyAuthentication.IsChecked.HasValue;
            mainWindow.settings.ProxyUser = proxyUserInput.Text;
            mainWindow.settings.ProxyPass = proxyPassInput.Password;
            mainWindow.SaveRegistrySettings();
            ProxySaved.Visibility = Visibility.Visible;
        }

        private void SaveRegisterButton_Click(object sender, RoutedEventArgs e)
        {
            int port = Convert.ToInt32(portInput.Text);
            bool changes = (mainWindow.settings.RegisterAddress != mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].IP) || (mainWindow.settings.RegisterPort != port)
                            || (mainWindow.settings.RegisterInterface != mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].Name);
   
            if (interfaceComboBox.SelectedIndex > -1)
            {
                mainWindow.settings.RegisterAddress = mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].IP;
                mainWindow.settings.RegisterInterface = mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].Name;
            }
            RuntimeData.MMIRegisterAddress.Address = mainWindow.settings.RegisterAddress;
            RuntimeData.MMIRegisterAddress.Port = port;
            mainWindow.settings.RegisterPort = port;
            mainWindow.SaveSettings();
            this.RegisterSaved.Visibility = Visibility.Visible;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            int port;
            if (int.TryParse(this.portInput.Text, out port))
            {
                //Restart the sever with the new port and address
                bool changes = (mainWindow.settings.RegisterAddress != mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].IP) || (mainWindow.settings.RegisterPort != port)
                            || (mainWindow.settings.RegisterInterface != mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].Name);
                
                if (interfaceComboBox.SelectedIndex > -1)
                {
                    mainWindow.settings.RegisterAddress = mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].IP;
                    mainWindow.settings.RegisterInterface = mainWindow.NetworkAdapters.AvailableIp[interfaceComboBox.SelectedIndex].Name;
                }
                RuntimeData.MMIRegisterAddress.Address = mainWindow.settings.RegisterAddress;
                RuntimeData.MMIRegisterAddress.Port = port;
                mainWindow.settings.RegisterPort = port;
                //if settings were changed, save the settings to file
                if (changes)
                {
                    mainWindow.SaveSettings();
                    this.RegisterSaved.Visibility = Visibility.Visible;
                }

                //restart server and components
                mainWindow.Restart();
                
                //Finally close the window
                //this.Close();
            }
        }

        private bool CheckFolderStructure(string path)
        {
            if (!((path[path.Length - 1] == '\\') || (path[path.Length - 1] == '/')))
            path += "/";
                return (Directory.Exists(path + "Adapters")) &&
                   (Directory.Exists(path + "MMUs")) &&
                   (Directory.Exists(path + "Services"));
        }

        private bool CreateDirectoryStructure(string path)
        {
            try
            {
                if (!((path[path.Length - 1] == '\\') || (path[path.Length - 1] == '/')))
                    path += "/";

                if (!Directory.Exists(path + "Adapters"))
                    Directory.CreateDirectory(path + "Adapters");

                if (!Directory.Exists(path + "MMUs"))
                    Directory.CreateDirectory(path + "MMUs");

                if (!Directory.Exists(path + "Services"))
                    Directory.CreateDirectory(path + "Services");
            }
            catch {
                
            }
            return CheckFolderStructure(path);
        }

        private void PathBrowse(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select data folder path";
                if ((PathInput.Text[PathInput.Text.Length - 1] == '\\') || (PathInput.Text[PathInput.Text.Length - 1]=='/'))
                folderDialog.SelectedPath = PathInput.Text.Substring(0, PathInput.Text.Length - 1);
                else
                    folderDialog.SelectedPath = PathInput.Text;
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    if (CheckFolderStructure(folderDialog.SelectedPath))
                        this.PathInput.Text = folderDialog.SelectedPath + "/";
                    else
                    {
                        StringBuilder msg = new StringBuilder();
                        msg.AppendLine("Selected folder does not contain valid directory structure, the following directory structure is expected:");
                        msg.AppendLine("|-Adapters");
                        msg.AppendLine("|-MMus");
                        msg.AppendLine("|-Services");
                        msg.AppendLine("Do you want to Launcher to create this folder structure inside selected directory?");
                        if (System.Windows.MessageBox.Show(msg.ToString(), "Invalid folder", MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.Yes)
                            if (CreateDirectoryStructure(folderDialog.SelectedPath))
                                this.PathInput.Text = folderDialog.SelectedPath + "/";
                            else
                                System.Windows.MessageBox.Show("Cannot create folder structure, are you sure you have write permissions in the selected directory?", "Folder structure creation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    
            }
        }

        private void Service_Change(object sender, RoutedEventArgs e)
        {
            if (ServiceSaved != null)
                ServiceSaved.Visibility = Visibility.Hidden;
            PortValue_Change(sender, e);
        }

        private void PortToolTip(System.Windows.Controls.TextBox portBox)
        {
            System.Windows.Controls.ToolTip toolTip = new System.Windows.Controls.ToolTip();
            toolTip.Content = "Port value must be within 1024 to 65535 range.";
            portBox.ToolTip = toolTip;
        }

        private int PortValueOK(System.Windows.Controls.TextBox portBox)
        {
            if (portBox == null)
                return 0;
            int minport = 0;
            try
            {
                minport = Convert.ToInt32(portBox.Text);
            }
            catch
            {
                minport = 0;
            }
            if ((minport < 1024) || (minport > 65535))
                return 0;
            return minport;
        }

        private void PortValue_Change(object sender, RoutedEventArgs e)
        {
            if ((sender as System.Windows.Controls.TextBox) == null)
                return;
            if (PortValueOK((sender as System.Windows.Controls.TextBox)) == 0)
            {
                (sender as System.Windows.Controls.TextBox).Background = Brushes.BurlyWood;
                PortToolTip((sender as System.Windows.Controls.TextBox));
            }
            else
            {
                (sender as System.Windows.Controls.TextBox).Background = Brushes.White;
                (sender as System.Windows.Controls.TextBox).ToolTip = null;
            }
        }

        private void ApplyServiceButton_Click(object sender, RoutedEventArgs e)
        {
            SaveServiceButton_Click(sender, e);
            if (ServiceSaved.Visibility == Visibility.Visible)
            mainWindow.Restart();

        }

        private void SaveServiceButton_Click(object sender, RoutedEventArgs e)
        {
            var portmin = PortValueOK(portMinInput);
            var portmax = PortValueOK(portMaxInput);
            bool changes = false;
            if ((portmin > 0) && (portmax > 0))
            {
                changes = (mainWindow.settings.MinPort != portmin) || (mainWindow.settings.MaxPort != portmax) ||
                    (mainWindow.settings.HideWindows == HideWindows.IsChecked.Value) || (mainWindow.settings.AutoStart == this.AutoStart.IsChecked.Value);
                if (portmin > portmax)
                {
                    portMinInput.Text = portmax.ToString();
                    portMaxInput.Text = portmin.ToString();
                    mainWindow.settings.MinPort = portmax;
                    mainWindow.settings.MaxPort = portmin;
                }
                else
                {
                    mainWindow.settings.MinPort = portmin;
                    mainWindow.settings.MaxPort = portmax;
                }
                mainWindow.settings.HideWindows = this.HideWindows.IsChecked.Value;
                mainWindow.settings.AutoStart = this.AutoStart.IsChecked.Value;
                mainWindow.settings.DataPath = this.PathInput.Text;
                if (changes)
                {
                    ServiceSaved.Visibility = Visibility.Visible;
                    mainWindow.SaveSettings();
                }
                else
                    ServiceSaved.Visibility = Visibility.Hidden;
            }
        }

        [Serializable] //this is also used in main window so it should be moved to a separate task editor function library
        public class TaskEditorTestResponse {
            public int projectid;
            public string projectName;
            public TaskEditorTestResponse()
            {
                this.projectid = 0;
                this.projectName = "";
            }
        }

        private void TestAndSaveButton_Click(object sender, RoutedEventArgs e)
        {
            string url = this.taskEditorUrlInput.Text.Trim();
            if (url == "") //Disable task editor connection by using empty url
            {
                this.mainWindow.settings.TaskEditorApiUrl = "";
                this.mainWindow.settings.TaskEditorToken = "";
                this.mainWindow.SaveSettings();
                this.TaskEditorSaved.Visibility = Visibility.Visible;
                this.mainWindow.LoadMMULibraries();
                return;
            }
            if (url.IndexOf("api.php") == -1)
                if (url[url.Length-1]!='/')
                url += "/api.php";
                else
                url += "api.php";
            this.taskEditorUrlInput.Text = url;
            this.taskEditorTokenInput.Text = this.taskEditorTokenInput.Text.Trim();
            Dictionary<string, string> PostData = new Dictionary<string, string>();
            PostData.Add("token", this.taskEditorTokenInput.Text);
            PostData.Add("action", "testConnection");
            
            System.Net.HttpWebResponse Response = WebAccess.PutData(this.taskEditorUrlInput.Text, "", PostData);
            if (Response == null)
            {
                this.taskEditorUrlOK.Content = "ERR";
            }
            else
            {
                this.taskEditorUrlOK.Content = "OK";
                string s = Response.ReadResponseStream().Trim();
                TaskEditorTestResponse reply = new TaskEditorTestResponse();
                try
                {
                    reply = Serialization.FromJsonString<TaskEditorTestResponse>(s);
                }
                catch //(Utf8Json.JsonParsingException ex)
                {
                    this.taskEditorUrlOK.Content = "ERR";
                }

                if (reply.projectid == 0)
                    this.taskEditorTokenOK.Content = "ERR";
                else
                {
                    this.mainWindow.settings.TaskEditorApiUrl = this.taskEditorUrlInput.Text;
                    this.mainWindow.settings.TaskEditorToken = this.taskEditorTokenInput.Text;
                    this.mainWindow.SaveSettings();
                    LibraryLink LL = new LibraryLink();
                    LL.url = this.taskEditorUrlInput.Text;
                    LL.token = this.taskEditorTokenInput.Text;
                    LL.name=reply.projectName==""?"<no name>":reply.projectName;
                    
                    int found = -1;
                    for (int i = 0; i < taskEditorCombo.Items.Count; i++)
                        if ((LL.url == (taskEditorCombo.Items[i] as RemoteLibrary).URL) && (LL.token == (taskEditorCombo.Items[i] as RemoteLibrary).Token))
                            found = i;
                    if (found == -1)
                    {
                        this.mainWindow.AddMMULibrary(LL);
                        this.taskEditorCombo.Items.Insert(taskEditorCombo.Items.Count - 1, new RemoteLibrary(LL));
                        taskEditorCombo.SelectedIndex = taskEditorCombo.Items.Count - 1;
                    }
                    else
                        if (LL.name != (taskEditorCombo.Items[found] as RemoteLibrary).Name)
                        {
                            (this.taskEditorCombo.Items[found] as RemoteLibrary).Name = LL.name;
                            //if (this.taskEditorCombo.SelectedIndex != found)
                            this.taskEditorCombo.Items.Refresh();
                            this.taskEditorCombo.UpdateLayout();
                            this.taskEditorCombo.Text = LL.name;
                            this.taskEditorCombo.SelectedIndex=- 1;
                            this.taskEditorCombo.SelectedIndex = found;
                            this.mainWindow.UpdateMMULibrary(LL);
                        }
                    this.taskEditorUrlOK.Content = "OK";
                    this.taskEditorTokenOK.Content = "OK";
                    this.TaskEditorSaved.Visibility = Visibility.Visible;
                }
            }
        }

        private void SaveMMULibButton_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.MMULibraryName = MMULibraryNameInput.Text;
            if (mainWindow.RegisterAppInstance(true))
                MMULibSaved.Visibility = Visibility.Visible;
            else
                System.Windows.MessageBox.Show("Name " + MMULibraryNameInput.Text + " is already taken, use another name.", "MMU library name change", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void MMULibraryNameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            MMULibSaved.Visibility = Visibility.Hidden;
        }
    }
}
