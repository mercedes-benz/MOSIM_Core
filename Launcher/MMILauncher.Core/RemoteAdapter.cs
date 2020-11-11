// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using MMICSharp.Clients;
using MMIStandard;
using System;
using System.Collections.Generic;
using System.ComponentModel;


namespace MMILauncher.Core
{
    /// <summary>
    /// Class is repsonsible for communicating with a program lanuage specific MMU adapter via inter process comunication
    /// </summary>
    public class RemoteAdapter : IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<RemoteAdapter> OnInactive;


        #region properties

        private bool active = false;
        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                this.active = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Active"));
            }
        }


        /// <summary>
        /// The time since the connection is inactive
        /// </summary>
        public TimeSpan InactiveTime
        {
            get;
            set;
        } = TimeSpan.Zero;

        public MAdapterDescription Description
        {
            get;
            set;
        }

        public string Address
        {
            get;
            set;
        }

        /// <summary>
        /// The assigned port
        /// </summary>
        public int Port
        {
            get;
            set;
        }


        public string RegisterAddress
        {
            get;
            set;
        }

        public int RegisterPort
        {
            get;
            set;
        }

        public bool UseThrift = true;



        private string name;
        private string status = "Loading";

        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            }
        }

        /// <summary>
        /// The language of the module
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.name = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Language"));
            }
        }


        /// <summary>
        /// The filepath of the assembly
        /// </summary>
        public string Filepath
        {
            get;
            set;
        }

        /// <summary>
        /// Flag which indicates whether the communciaton has been successfully initialized
        /// </summary>
        public bool Initialized
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Flag which indicates whether the initialization has been aborted
        /// </summary>
        public bool Aborted
        {
            get;
            set;
        } = false;

        /// <summary>
        /// Specifies whether the process window is hidden
        /// </summary>
        public bool HideWindow
        {
            get;
            set;
        } = false;

        public bool SceneSynchronized
        {
            get;
            set;
        } = false;

        #endregion


        public TimeSpan UpdateTime = TimeSpan.FromMilliseconds(1000);



        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="webserverAddress"></param>
        /// <param name="filepath"></param>
        public RemoteAdapter(MAdapterDescription description)
        {
            this.Name = description.Name;
            this.Description = description;
            this.Address = description.Addresses[0].Address;
            this.Port = description.Addresses[0].Port;
        }



        /// <summary>
        /// Method starts the process
        /// </summary>
        public void Start()
        {
            //Cyclicyll update the status
            System.Threading.ThreadPool.QueueUserWorkItem(delegate
            {
                while (!this.Aborted)
                {
                    System.Threading.Thread.Sleep(this.UpdateTime);

                    try
                    {
                        //Get the status 
                        this.Status = this.GetStatus().GetReadableString();

                        this.Initialized = true;

                        this.InactiveTime = TimeSpan.Zero;
                        this.Active = true;
                    }
                    catch (Exception)
                    {

                        this.InactiveTime += UpdateTime;
                        this.Active = false;
                        this.Status = "Inactive: " + this.InactiveTime.Duration().ToString();

                        if (this.InactiveTime > RuntimeData.InactiveRemoveTime)
                        {
                            RemoteAdapter removed = null;
                            RuntimeData.AdapterInstances.TryRemove(this.Description.ID, out removed);

                            //Fire event if it gets inactiv
                            this.OnInactive?.Invoke(this, this);

                            //UIData.SynchronizeAdapters();
                        }
                    }

                }
            });
        }




        /// <summary>
        /// Requests the status of the adapter
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetStatus()
        {
            using (AdapterClient client = new AdapterClient(this.Address, this.Port))
            {
                return client.Access.GetStatus();
            }
        }

        public List<MMUDescription> GetLoadableMMUs(string sessionId)
        {
            using (AdapterClient client = new AdapterClient(this.Address, this.Port))
            {
                return client.Access.GetLoadableMMUs();
            }
        }

        /// <summary>   
        /// Dispose method which closes the used application
        /// </summary>
        public void Dispose()
        {
            this.Aborted = true;
            //Nothing to do

        }
    }

}
