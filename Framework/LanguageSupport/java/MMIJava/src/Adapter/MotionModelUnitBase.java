package Adapter;


import MMIStandard.MSceneAccess;
import MMIStandard.MSkeletonAccess;
import MMIStandard.MotionModelUnit;

public abstract class MotionModelUnitBase implements MotionModelUnit.Iface {
    /*
		Basic MMU Interface for representing MMUs in Java
    */

    //	The access to the scene
    public MSceneAccess.Iface SceneAccess;

    //	The access to the services
    public Access.Abstraction.ServiceAccess ServiceAccess;

    //	The access to the skeleton
    public MSkeletonAccess.Iface SkeletonAccess;

    //	The name of the MMU
    public String name;
    //	The id of the MMU
    public String id;


    public String getName() {
        return name;
    }

    public void setName(String name) {
        this.name = name;
    }
}
