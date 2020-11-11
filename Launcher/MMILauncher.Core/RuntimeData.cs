// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Adam Klodowski

using MMIStandard;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Data;

namespace MMILauncher.Core
{
    
    public class MMUOrderAndDescriptionData : MMUDescription
    {
        public UInt32 Priority { get; set; }
        public string FolderPath { get; set; }

        public MMUOrderAndDescriptionData() {
            this.Priority=0;
            this.FolderPath = "";
        }

        public MMUOrderAndDescriptionData(MMUDescription description, UInt32 priority)
        {
            //derived properties
            this.AssemblyName = description.AssemblyName;
            this.Author = description.Author;
            this.Dependencies = description.Dependencies;
            this.Events = description.Events;
            this.ID = description.ID;
            this.Language = description.Language;
            this.LongDescription = description.LongDescription;
            this.MotionType = description.MotionType;
            this.Name = description.Name;
            this.Parameters = description.Parameters;
            this.Prerequisites = description.Prerequisites;
            this.Properties = description.Properties;
            this.SceneParameters = description.SceneParameters;
            this.ShortDescription = description.ShortDescription;
            this.Version = description.Version;
            this.__isset = description.__isset;
            //new properties
            this.Priority = priority;
            this.FolderPath = "";
        }

        public MMUOrderAndDescriptionData(MMUDescription description, UInt32 priority, string folder)
        {
            //derived properties
            this.AssemblyName = description.AssemblyName;
            this.Author = description.Author;
            this.Dependencies = description.Dependencies;
            this.Events = description.Events;
            this.ID = description.ID;
            this.Language = description.Language;
            this.LongDescription = description.LongDescription;
            this.MotionType = description.MotionType;
            this.Name = description.Name;
            this.Parameters = description.Parameters;
            this.Prerequisites = description.Prerequisites;
            this.Properties = description.Properties;
            this.SceneParameters = description.SceneParameters;
            this.ShortDescription = description.ShortDescription;
            this.Version = description.Version;
            this.__isset = description.__isset;
            //new properties
            this.Priority = priority;
            this.FolderPath = folder;
        }
    }

    public class SubTypeConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((values[1]==null) || ((values[1] as string)==""))
            return String.Format("{0}", values[0], values[1]);
            else
            return String.Format("{0} ({1})", values[0], values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Static class which contains all the data
    /// </summary>
    public static class RuntimeData
    {
        public static MIPAddress MMIRegisterAddress = new MIPAddress("127.0.0.1", 9009);
        public static int CurrentPort = 8900;
        public static List<string> SessionIds = new List<string>();

        /// <summary>
        /// The list contains all managed executable controller
        /// </summary>
        public static List<IExecutableController> ExecutableControllers = new List<IExecutableController>();


        /// <summary>
        /// The list contains all instances of the remote adapters
        /// </summary>
        public static ConcurrentDictionary<string, RemoteAdapter> AdapterInstances = new ConcurrentDictionary<string, RemoteAdapter>();

        /// <summary>
        /// The list contains all instances of the remote services
        /// </summary>
        public static ConcurrentDictionary<string, RemoteService> ServiceInstances = new ConcurrentDictionary<string, RemoteService>();

        /// <summary>
        /// The list contains all available MMUDescriptions
        /// </summary>
        public static ConcurrentDictionary<string, MMUDescription> MMUDescriptions = new ConcurrentDictionary<string, MMUDescription>();
        //public static ConcurrentDictionary<string, MMUOrderAndDescriptionData> MMUDescriptions = new ConcurrentDictionary<string, MMUOrderAndDescriptionData>();
        

        /// <summary>
        /// The time after which adapters/services are automatically removed from the register
        /// </summary>
        public static TimeSpan InactiveRemoveTime = TimeSpan.FromSeconds(5);

    }
}
