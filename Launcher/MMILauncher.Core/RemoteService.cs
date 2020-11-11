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
    /// Class encapsulates the access to a remote service.
    /// </summary>
    public class RemoteService : IDisposable, INotifyPropertyChanged
    {
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


        public MServiceDescription Description
        {
            get;
            private set;
        }

        #endregion

        public TimeSpan UpdateTime = TimeSpan.FromMilliseconds(1000);


        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<RemoteService> OnInactive;


        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="description"></param>
        /// <param name="hideWindow"></param>
        public RemoteService(MServiceDescription description)
        {
            this.Description = description;
            this.Port = description.Addresses[0].Port;
            this.Address = description.Addresses[0].Address;
            this.Name = description.Name;
        }

        /// <summary>
        /// Start method
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
                        this.Status = this.GetStatus().GetReadableString();

                        this.Initialized = true;
                        this.InactiveTime = TimeSpan.Zero;
                        this.Active = true;
                    }
                    catch //(Exception e)
                    {
                        this.InactiveTime += this.UpdateTime;
                        this.Active = false;
                        this.Status = "Inactive: " + this.InactiveTime.Duration().ToString();

                        if (this.InactiveTime > RuntimeData.InactiveRemoveTime)
                        {
                            RemoteService removed = null;
                            RuntimeData.ServiceInstances.TryRemove(this.Description.ID, out removed);

                            //Fire event if it gets inactiv
                            this.OnInactive?.Invoke(this, this);
                        }

                    }
                }
            });

        }


        public Dictionary<string, string> GetStatus()
        {
            using (MMIServiceBaseClient client = new MMIServiceBaseClient(this.Address, this.Port))
            {
                client.Start();
                return client.Access.GetStatus();
            }
        }



        /// <summary>
        /// Disposes the serverice
        /// </summary>
        public void Dispose()
        {
            this.Aborted = true;
        }

    }
}
