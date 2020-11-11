package Adapter;

import MMIStandard.MMUDescription;

public interface IMMUInstantiation {
    /*
    Interface for a Java MMU instantiator
     */

    MotionModelUnitBase InstantiateMMU(MMUDescription mmuDescription);
}
