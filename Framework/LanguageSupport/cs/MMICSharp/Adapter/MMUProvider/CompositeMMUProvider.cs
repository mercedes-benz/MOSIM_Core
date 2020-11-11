// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

using System;
using System.Collections.Generic;


namespace MMICSharp.Adapter.MMUProvider
{
    /// <summary>
    /// A composite MMU provider which can utilize multiple IMMUProviders
    /// </summary>
    public class CompositeMMUProvider : IMMUProvider
    {
        public event EventHandler<EventArgs> MMUsChanged;


        private readonly IMMUProvider[] mmuProviders;

        public CompositeMMUProvider(params IMMUProvider[] mmuProviders)
        {
            this.mmuProviders = mmuProviders;
            foreach (IMMUProvider mmuProvider in this.mmuProviders)
            {
                mmuProvider.MMUsChanged += MmuProvider_MMUsChanged;
            }
        }



        public Dictionary<string, MMULoadingProperty> GetAvailableMMUs()
        {
            Dictionary<string, MMULoadingProperty> result = new Dictionary<string, MMULoadingProperty>();

            foreach (IMMUProvider mmuProvider in this.mmuProviders)
            {
                var availableMMUs = mmuProvider.GetAvailableMMUs();

                if (availableMMUs != null)
                {
                    foreach (var entry in availableMMUs)
                    {
                        if (!result.ContainsKey(entry.Key))
                            result.Add(entry.Key, entry.Value);
                    }
                }
            }

            return result;
        }


        private void MmuProvider_MMUsChanged(object sender, EventArgs e)
        {
            this.MMUsChanged?.Invoke(this, new EventArgs());
        }


    }
}
