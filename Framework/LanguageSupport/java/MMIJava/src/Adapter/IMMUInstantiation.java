// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Adapter;


import de.mosim.mmi.mmu.MMUDescription;

public interface IMMUInstantiation {
    /*
    Interface for a Java MMU instantiator
     */

    MotionModelUnitBase InstantiateMMU(MMUDescription mmuDescription);
}
