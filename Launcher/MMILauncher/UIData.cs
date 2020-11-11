// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Adam Klodowski

using MMILauncher.Core;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Threading;

namespace MMILauncher
{
    /// <summary>
    /// Class which contains UI related data
    /// </summary>
    public static class UIData
    {
        private static Dispatcher dispatcher;

        public static void Initialize(Dispatcher dispatcher)
        {
            UIData.dispatcher = dispatcher;
        }

        /// <summary>
        /// A collection which contains all available adapters
        /// </summary>
        public static ObservableCollection<RemoteAdapter> AdapterCollection
        {
            get;
            set;
        } = new ObservableCollection<RemoteAdapter>();

        /// <summary>
        /// A collection which contains all available services
        /// </summary>
        public static ObservableCollection<RemoteService> ServiceCollection
        {
            get;
            set;
        } = new ObservableCollection<RemoteService>();


        /// <summary>
        /// A collection of all available MMUDescriptions
        /// </summary>
        /*
        public static ObservableCollection<MMUDescription> MMUCollection
        {
            get;
            set;
        } = new ObservableCollection<MMUDescription>();
        */
        public static ObservableCollection<MMUOrderAndDescriptionData> MMUCollection
        {
            get;
            set;
        } = new ObservableCollection<MMUOrderAndDescriptionData>();

        #region methods


        public static void SynchronizeAdapters()
        {
            //Remove from collection on main trhead
            dispatcher.BeginInvoke((Action)(() =>
            {
                UIData.AdapterCollection.Clear();

                for(int i= RuntimeData.AdapterInstances.Count-1; i >= 0; i--)
                {
                    UIData.AdapterCollection.Add(RuntimeData.AdapterInstances.Values.ElementAt(i));
                }
            }));
        }


        public static void SynchronizeServices()
        {
            //Remove from collection on m ain trhead
            dispatcher.BeginInvoke((Action)(() =>
            {
                UIData.ServiceCollection.Clear();

                for (int i = RuntimeData.ServiceInstances.Count - 1; i >= 0; i--)
                {
                    UIData.ServiceCollection.Add(RuntimeData.ServiceInstances.Values.ElementAt(i));
                }
            }));
        }


        /// <summary>
        /// Clear all adapters (on UI thread)
        /// </summary>
        public static void ClearAdapters()
        {
            //Remove from collection on main thread
            dispatcher.BeginInvoke((Action)(() =>
            {
                UIData.AdapterCollection.Clear();
            }));
        }

        /// <summary>
        /// Clears all services (on UI thread)
        /// </summary>
        public static void ClearServices()
        {
            //Remove from collection on main thread
            dispatcher.BeginInvoke((Action)(() =>
            {
                UIData.ServiceCollection.Clear();
            }));
        }


        /// <summary>
        /// Sets the available MMU descriptions (on UI thread)
        /// </summary>
        /// <param name="mmuDescriptions"></param>
        public static void SetMMUDescriptions(List<MMUDescription> mmuDescriptions) //changed to avoid full refresh every second on the list as this makes it impossible to reorder MMUS on the list or edit in any way
        {
            //Remove from collection on main thread
            dispatcher.BeginInvoke((Action)(() =>
            {
                //UIData.MMUCollection.Clear();
                foreach (MMUDescription description in mmuDescriptions)
                {
                    bool found = false;
                    foreach (MMUOrderAndDescriptionData incollection in UIData.MMUCollection)
                        if (incollection.ID == description.ID)
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                    UIData.MMUCollection.Add(new MMUOrderAndDescriptionData(description,0));
                }

            for (int i=0; i< UIData.MMUCollection.Count; i++)
                {
                    bool found = false;
                    foreach (MMUDescription description in mmuDescriptions)
                        if (UIData.MMUCollection[i].ID == description.ID)
                        {
                            found = true;
                            break;
                        }
                    if (!found)
                    UIData.MMUCollection.RemoveAt(i);
                }
            }));
        }

        #endregion
    }



}
