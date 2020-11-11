// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;
using System.Linq;
using MMIStandard;

namespace MMICSharp.Adapter.MMUProvider
{
    /// <summary>
    /// Provides MMU loading properties based on available descriptions. 
    /// Can be utilized to load MMU during runtime without accessing the filesystem.
    /// </summary>
    public class DescriptionBasedMMUProvider : IMMUProvider
    {
        public event EventHandler<EventArgs> MMUsChanged;

        private readonly List<MMUDescription> availableMMUs = new List<MMUDescription>(); 
            

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="availableMMUs"></param>
        public DescriptionBasedMMUProvider(List<MMUDescription> availableMMUs)
        {
            this.availableMMUs = availableMMUs;
        }

        /// <summary>
        /// Simplified constructor
        /// </summary>
        /// <param name="availableMMUs"></param>
        public DescriptionBasedMMUProvider(params MMUDescription[] availableMMUs) : this(availableMMUs.ToList())
        {

        }

        /// <summary>
        /// Adds a new mmu to the already existing ones
        /// </summary>
        /// <param name="description"></param>
        public void AddMMU(MMUDescription description)
        {
            this.availableMMUs.Add(description);
            this.MMUsChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Removes a MMU
        /// </summary>
        /// <param name="description"></param>
        public void RemoveMMU(MMUDescription description)
        {
            this.availableMMUs.Remove(description);
            this.MMUsChanged?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Returns all available MMUs
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, MMULoadingProperty> GetAvailableMMUs()
        {
            Dictionary<string, MMULoadingProperty> loadingProperties = new Dictionary<string, MMULoadingProperty>();
            foreach(MMUDescription description in this.availableMMUs)
            {
                loadingProperties.Add(description.ID, new MMULoadingProperty()
                {
                    Description = description
                });
            }
            return loadingProperties;
        }
    }
}
