// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Klodowski

using MMILauncher.Core;
using MMIULibrary;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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
using System.Windows.Threading;

namespace MMILauncher
{
    public static class MMUUIData
    {
        private static Dispatcher dispatcher;

        static MMUUIData()
        {
            UploadTargetLibraries.Filter += new FilterEventHandler(ShowOnlyUploadTargets);
            SyncToTargetLibraries.Filter += new FilterEventHandler(ShowOnlyOtherTargets);
            AllLibraries.View.CurrentChanged += new EventHandler(UpdateViews);
        }

        public static void Initialize(Dispatcher dispatcher)
        {
            MMUUIData.dispatcher = dispatcher;
        }
    
        public static ObservableCollection<RemoteMMUList> MMUCollection
        { get; set; } = new ObservableCollection<RemoteMMUList>();

        public static ObservableCollection<details> MMUDetailsCollection
        { get; set; } = new ObservableCollection<details>();

        public static ObservableCollection<LibraryDetails> LibraryCollection
        { get; set; } = new ObservableCollection<LibraryDetails>();

        public static void AddMMUs(RemoteMMUList[] mmuData)
        {
            dispatcher.BeginInvoke((Action)(() =>
                {
                    MMUUIData.MMUCollection.Clear();

                    for (int i =0; i<mmuData.Length; i++)
                    {
                        MMUUIData.MMUCollection.Add(mmuData[i]);
                    }
                }));
        }

        public static void AddMMUs(List<MMUOrderAndDescriptionData> mmuData)
        {
            dispatcher.BeginInvoke((Action)(() =>
            {
                MMUUIData.MMUCollection.Clear();

                for (int i = 0; i < mmuData.Count; i++)
                {
                    var mmu = new RemoteMMUList()
                    {
                        enabled = true,
                        excludeFromSync = false,
                        inLocal = false, //to keep the sync icon off
                        name = mmuData[i].Name,
                        author = mmuData[i].Author,
                        sortorder = mmuData[i].Priority,
                        vendorID = mmuData[i].ID,
                        version = mmuData[i].Version,
                        url = mmuData[i].FolderPath
                    };
                    MMUUIData.MMUCollection.Add(mmu);
                }
            }));
        }

        private static void UpdateViews(object sender, EventArgs e)
        {
            SyncToTargetLibraries.View.Refresh();
            UploadTargetLibraries.View.Refresh();
        }

        public static CollectionViewSource AllLibraries
        { get; set; } = new CollectionViewSource()
        {
            Source = LibraryCollection
        };

        public static CollectionViewSource UploadTargetLibraries
        { get; set; } = new CollectionViewSource()
            {
             Source = LibraryCollection
            };

        public static CollectionViewSource SyncToTargetLibraries
        { get; set; } = new CollectionViewSource()
        {
            Source = LibraryCollection
        };

        private static void ShowOnlyUploadTargets(object sender, FilterEventArgs e)
        {
            LibraryDetails lib = e.Item as LibraryDetails;

            if (AllLibraries.View.CurrentItem == null)
                e.Accepted = lib.canAdd;
            else
                e.Accepted = lib.index != (AllLibraries.View.CurrentItem as LibraryDetails).index;
        }
        
        private static void ShowOnlyOtherTargets(object sender, FilterEventArgs e)
        {
            //SyncToTargetLibraries.View.CurrentItem
            LibraryDetails lib = e.Item as LibraryDetails;
            if (AllLibraries.View.CurrentItem == null)
                e.Accepted = true;
            else
                e.Accepted = lib.index!=(AllLibraries.View.CurrentItem as LibraryDetails).index;
        }
    }
    
    public class details
    {
        public details(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public string Value { get; set; }
    }

        /// <summary>
        /// Interaction logic for MMULibraryWindow.xaml
        /// </summary>
    public partial class MMULibraryWindow : Window
    {
        private readonly MainWindow mainWindow;

        public MMULibraryWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            ResetProgressBars();
            this.mainWindow = mainWindow;
            MMUUIData.Initialize(this.Dispatcher);
            mmuListView.ItemsSource = MMUUIData.MMUCollection;
            mmuDetailsListView.ItemsSource = MMUUIData.MMUDetailsCollection;
            if (mainWindow.mmus.Remotes.Count>0)
            MMUUIData.AddMMUs(mainWindow.mmus.Remotes[0].MMUs);

            MMUUIData.MMUDetailsCollection.Clear();
            MMUUIData.MMUDetailsCollection.Add(new details("Name", ""));
            MMUUIData.MMUDetailsCollection.Add(new details("Version", ""));
            MMUUIData.MMUDetailsCollection.Add(new details("Motion type", ""));
            MMUUIData.MMUDetailsCollection.Add(new details("Author", ""));
            MMUUIData.MMUDetailsCollection.Add(new details("ID", ""));
            MMUUIData.MMUDetailsCollection.Add(new details("URL", ""));
            MMUUIData.MMUDetailsCollection.Add(new details("Synchronize", ""));

            LibraryCombo.ItemsSource = MMUUIData.AllLibraries.View;
            SyncWithCombo.ItemsSource = MMUUIData.SyncToTargetLibraries.View;
            UploadToCombo.ItemsSource = MMUUIData.UploadTargetLibraries.View;

            MMUUIData.LibraryCollection.Clear();
            MMUUIData.LibraryCollection.Add(new LibraryDetails(-1, "Local"));
            MMUUIData.LibraryCollection[0].canRemove = !mainWindow.isRunning;
            for (int i = 0; i < mainWindow.mmus.Remotes.Count; i++)
                MMUUIData.LibraryCollection.Add(new LibraryDetails(i, mainWindow.mmus.Remotes[i].Name,
                                                                      mainWindow.mmus.Remotes[i].canRemove, 
                                                                      mainWindow.mmus.Remotes[i].canUpload, 
                                                                      mainWindow.mmus.Remotes[i].canDownload));

            if (MMUUIData.LibraryCollection.Count>0)
            LibraryCombo.SelectedIndex = 0;
            //MMUUIData.LibraryCollection.Add(new LibraryDetails(0, "TaskEditor.mosim.eu"));
            //MMUUIData.LibraryCollection.Add(new LibraryDetails(1, "TaskEditor.nasq.local",false,true,true));
        }

        private async void UpdateLibraries()
        {
            mainWindow.mmus.ScanLibrary();
            for (int i = 0; i < mainWindow.mmus.Remotes.Count; i++)
            {
                await mainWindow.mmus.Remotes[i].GetMMUListFromServer();
                mainWindow.mmus.CompareRemoteAndLocal(i);
            }
            if (LibraryCombo.SelectedIndex == 0)
                MMUUIData.AddMMUs(mainWindow.mmus.MMUs);
            else
                if (LibraryCombo.SelectedIndex > 0)
                MMUUIData.AddMMUs(mainWindow.mmus.Remotes[(LibraryCombo.SelectedItem as LibraryDetails).index].MMUs);
            mmuListView.Items.Refresh();
        }

        private void MmuListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (mmuListView.SelectedIndex > -1)
            {
                MMUUIData.MMUDetailsCollection[0].Value = (mmuListView.SelectedItem as RemoteMMUList).name;
                MMUUIData.MMUDetailsCollection[1].Value = (mmuListView.SelectedItem as RemoteMMUList).version;
                MMUUIData.MMUDetailsCollection[2].Value = (mmuListView.SelectedItem as RemoteMMUList).author;
                MMUUIData.MMUDetailsCollection[3].Value = (mmuListView.SelectedItem as RemoteMMUList).vendorID;
                MMUUIData.MMUDetailsCollection[4].Value = (mmuListView.SelectedItem as RemoteMMUList).url;
                if ((mmuListView.SelectedItem as RemoteMMUList).inLocal)
                    MMUUIData.MMUDetailsCollection[5].Value = "in sync";
                else
                    MMUUIData.MMUDetailsCollection[5].Value = ((mmuListView.SelectedItem as RemoteMMUList).excludeFromSync ? "excluded" : "download");
                mmuDetailsListView.ItemsSource = null; //there should be INotifyPropertyChanged used to update changes
                mmuDetailsListView.ItemsSource = MMUUIData.MMUDetailsCollection;
            }
        }

        void UploadProgress(long max, long progress)
        {
            UploadProgressBar.Maximum = max;
            UploadProgressBar.Value = progress;
            if (progress == max)
                UpdateLibraries();
        }

        void DownloadProgress(long max, long progress)
        {
            DownloadProgressBar.Maximum = max;
            DownloadProgressBar.Value = progress;
            if (progress == max)
                UpdateLibraries();
        }

        void ResetProgressBars()
        {
            UploadProgressBar.Maximum = 1;
            UploadProgressBar.Value = 0;
            DownloadProgressBar.Maximum = 1;
            DownloadProgressBar.Value = 0;
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if ((mmuListView.SelectedItems.Count == 0) || (LibraryCombo.SelectedItem==null) || ((LibraryCombo.SelectedItem as LibraryDetails).index<0))
                return; //if no library is selected or none mmu is selected or selected library is local simply do nothing (download is only possible from remote library)

            mainWindow.mmus.ClearDownloadList();

            for (int i = mmuListView.SelectedItems.Count - 1; i >= 0; i--)
            {
                if (!(mmuListView.SelectedItems[i] as RemoteMMUList).inLocal)
                mainWindow.mmus.AddToDownload((mmuListView.SelectedItems[i] as RemoteMMUList).url, (LibraryCombo.SelectedItem as LibraryDetails).index);
            }
            ResetProgressBars();
            mainWindow.mmus.GetFromServer(DownloadProgress);
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog();
            openDialog.Filter = "MMU zip archives (*.zip)|*.zip|All files|*.*";
            openDialog.FilterIndex = 0;
            openDialog.Multiselect = true;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ResetProgressBars();
                mainWindow.mmus.ConfigureConnection(mainWindow.client, mainWindow.settings.DataPath + "\\MMUs\\", mainWindow.settings.DataPath + "\\MMU-zip\\", mainWindow.settings.TaskEditorApiUrl, mainWindow.settings.TaskEditorToken);
                int importedCount = mainWindow.mmus.ImportMMUs(openDialog.FileNames,(LibraryCombo.SelectedItem as LibraryDetails).index,UploadProgress);
                //                if (importedCount>0)
                //              {
                //        mmuListView.ItemsSource = null;
                //      mmuListView.ItemsSource = MMUUIData.MMUCollection;
                //            }
                if (importedCount>0)
                UpdateLibraries();
            }
        }

        private async void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (mmuListView.SelectedItems.Count==0)
                return;

            if (System.Windows.MessageBox.Show("Are you sure you want to remove selected "+ mmuListView.SelectedItems.Count.ToString() + " MMUs from the library?","Remove MMUs",MessageBoxButton.YesNoCancel,MessageBoxImage.Question)==MessageBoxResult.Yes)
            {
                if (MMUUIData.LibraryCollection[LibraryCombo.SelectedIndex].index < 0) //local libraries
                {
                    for (int i = mmuListView.SelectedItems.Count-1; i>=0; i--)
                            System.IO.Directory.Delete((mmuListView.SelectedItems[i] as RemoteMMUList).url, true);
                    
                }
                else //remote libraries
                {
                    for (int i = mmuListView.SelectedItems.Count - 1; i >= 0; i--)
                    {
                       var mmu = (mmuListView.SelectedItems[i] as RemoteMMUList).vendorID;
                       await mainWindow.mmus.Remotes[MMUUIData.LibraryCollection[LibraryCombo.SelectedIndex].index].RemoveMMU(mmu);
                    }
                }
                UpdateLibraries();
            }
        }

        private void LibraryCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MMUUIData.MMUCollection.Clear();
            if (LibraryCombo.SelectedIndex == -1)
                return;

            LibraryRemoveButton.IsEnabled = (LibraryCombo.SelectedIndex > 0) && (mainWindow.mmus.Remotes[MMUUIData.LibraryCollection[LibraryCombo.SelectedIndex].index].LocalFileName!=null) &&
                (mainWindow.mmus.Remotes[MMUUIData.LibraryCollection[LibraryCombo.SelectedIndex].index].LocalFileName!="");
            if (MMUUIData.LibraryCollection[LibraryCombo.SelectedIndex].index < 0)
                MMUUIData.AddMMUs(mainWindow.mmus.MMUs);
            else
                if (MMUUIData.LibraryCollection[LibraryCombo.SelectedIndex].index < mainWindow.mmus.Remotes.Count)
                    MMUUIData.AddMMUs(mainWindow.mmus.Remotes[MMUUIData.LibraryCollection[LibraryCombo.SelectedIndex].index].MMUs);
        }

        private void LibraryAddButton_Click(object sender, RoutedEventArgs e)
        {
            ResetProgressBars();

        }

        private void LibraryRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ResetProgressBars();
            if (LibraryCombo.SelectedIndex < 1)
                return;

            if (System.Windows.MessageBox.Show("Are you sure you want to remove " + (LibraryCombo.SelectedItem as LibraryDetails).name + " remote library?","Remove remote MMU library",MessageBoxButton.YesNoCancel,MessageBoxImage.Question)==MessageBoxResult.Yes)
            {
                mainWindow.mmus.RemoveRemoteLibrary((LibraryCombo.SelectedItem as LibraryDetails).index);
                for (int i = 0; i < MMUUIData.LibraryCollection.Count; i++) //updating indexes so they would match after removal of an element
                    if (MMUUIData.LibraryCollection[i].index > (LibraryCombo.SelectedItem as LibraryDetails).index)
                        MMUUIData.LibraryCollection[i].index--;
                MMUUIData.LibraryCollection.RemoveAt(LibraryCombo.SelectedIndex);
                LibraryCombo.Items.Refresh();
                LibraryCombo.SelectedIndex = 0; //select local library
            }
        }

        private async void LibrarySyncButton_Click(object sender, RoutedEventArgs e)
        {
            if ((LibraryCombo.SelectedItem==null) || (SyncWithCombo.SelectedItem==null))
               return;

            if (((int)LibraryCombo.SelectedValue!= -1) && ((int)SyncWithCombo.SelectedValue!= -1))   //allow only syncing between remote library and local (later remote to remote sync will also be enabled)
                return;

            int remoteIndex = Math.Max((int)LibraryCombo.SelectedValue, (int)SyncWithCombo.SelectedValue);
            mainWindow.mmus.CompareAndPackRemoteAndLocal(remoteIndex);
            if (mainWindow.mmus.SyncDownCount() > 0)
                await mainWindow.mmus.GetFromServer(UploadProgress);
            if (mainWindow.mmus.SyncUpCount() > 0)
                await mainWindow.mmus.SendToServer(UploadProgress);
        }

        private void UploadToButton_Click(object sender, RoutedEventArgs e)
        {
            if (UploadToCombo.SelectedItem == null)
                return;


        }
    }
}
