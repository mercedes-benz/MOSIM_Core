// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer, Adam Kłodowski

using MMIStandard;
using MMIUnity;
using MMIUnity.TargetEngine.Scene;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class spwans objects to a desired material zone
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    public bool showButton = true;
    [Header("Do not set below parameters - debug only")]
    public MMISceneObject MaterialZone; 
    public List<MMISceneObject> ObjectToSpawn = new List<MMISceneObject>();
    public Transform Initial;
    public Transform Final;
    public float xmin = -1;
    public float xmax = 1;
    public float ymin = 0;
    public float ymax = 0;
    public float zmin = -0.5f;
    public float zmax = 0.5f;
    public float rotationx = 0;
    public float rotationy = 0;
    public float rotationz = 0;

    private class MZone : System.IEquatable<MZone> , System.IComparable<MZone>
    {
        public MMISceneObject LinkedMaterialZone;
        public ulong ZoneID { get; set; } //taskeditorlocalid
        public int ZoneIndex { get; set; } //index of the constraint that is describing material zone within the object
        public double FreeSpace { get; set; } //total free space volume counter - updated whenever some space is used
        public List<MInterval3> SubZones; //subdivision of the material zones
        public MTransform ObjCoordinates; //new object placement coordinates (used to get return value from UseSubZone.

        public MZone(ulong id, int index, double freespace, MInterval3 zonesize, MMISceneObject materialzone)
        {
            this.LinkedMaterialZone = materialzone;
            this.ZoneID=id;
            this.ZoneIndex=index;
            this.FreeSpace=freespace;
            this.SubZones = new List<MInterval3>();
            this.SubZones.Add(new MInterval3(new MInterval(zonesize.X.Min,zonesize.X.Max),new MInterval(zonesize.Y.Min, zonesize.Y.Max),new MInterval(zonesize.Z.Min, zonesize.Z.Max)));
            this.ObjCoordinates=new MTransform();
        }

        public bool UseSubZone(MInterval3 space)
        {
            for (int i=0; i<SubZones.Count; i++)
                if ((SubZones[i].X.Max-SubZones[i].X.Min>=space.X.Max-space.X.Min) &&
                    (SubZones[i].Y.Max-SubZones[i].Y.Min>=space.Y.Max-space.Y.Min) &&
                    (SubZones[i].Z.Max-SubZones[i].Z.Min>=space.Z.Max-space.Z.Min))
                {
                    double dx=space.X.Max-space.X.Min; //object size in x
                    double dy=space.Y.Max-space.Y.Min; //object size in z
                    double dz=space.Z.Max-space.Z.Min; //object size in z
                    FreeSpace=FreeSpace-dx*(SubZones[i].Y.Max-SubZones[i].Y.Min)*dz; //consider all y axis space taken (no stacking objects on top of eachother

                    double Ax=SubZones[i].X.Max-SubZones[i].X.Min-dx;
                    double Az=SubZones[i].Z.Max-SubZones[i].Z.Min;

                    double Bx=SubZones[i].X.Max-SubZones[i].X.Min;
                    double Bz=SubZones[i].Z.Max-SubZones[i].Z.Min-dz;
                    //|--------------|          |--------------|
                    //| New  |       |          |       B      |
                    //|------|   A   |          |--------------|
                    //|X     |       |          |X  |   New    |  
                    //|--------------|          |--------------|

                    ObjCoordinates.Position = (LinkedMaterialZone.Constraints[ZoneIndex].GeometryConstraint.ParentToConstraint.Position.ToVector3() +
                         (LinkedMaterialZone.Constraints[ZoneIndex].GeometryConstraint.ParentToConstraint.Rotation.ToQuaternion() * new MVector3(SubZones[i].X.Min-space.X.Min, SubZones[i].Y.Min- space.Y.Min, SubZones[i].Z.Min-space.Z.Min).ToVector3())).ToMVector3();
                    ObjCoordinates.Rotation = LinkedMaterialZone.Constraints[ZoneIndex].GeometryConstraint.ParentToConstraint.Rotation;

                    if (Ax*Az>Bx*Bz)
                    { 
                        //addine New zone
                        SubZones.Add(new MInterval3(new MInterval(SubZones[i].X.Min,SubZones[i].X.Min+dx),new MInterval(SubZones[i].Y.Min,SubZones[i].Y.Max),new MInterval(SubZones[i].Z.Min+dz,SubZones[i].Z.Max)));
                        SubZones[i].X.Min=SubZones[i].X.Min+dx; //modifying current to be A
                    }
                    else
                    { //adding New zone
                        SubZones.Add(new MInterval3(new MInterval(SubZones[i].X.Min+dx,SubZones[i].X.Max),new MInterval(SubZones[i].Y.Min,SubZones[i].Y.Max),new MInterval(SubZones[i].Z.Min,SubZones[i].Z.Min+dz)));
                        SubZones[i].Z.Min=SubZones[i].Z.Min+dz; //Modifying current to be B
                    }

                    return true;
                }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            MZone objAsPart = obj as MZone;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public static int SortBySpaceDescending(double space1, double space2)
        {
            return space2.CompareTo(space1);
        }

        public static int SortByZoneIDAsscending(ulong zone1, ulong zone2)
        {
            return zone1.CompareTo(zone2);
        }

        // Default comparer for Part type.
        public int CompareTo(MZone comparePart)
        {
          // A null value means that this object is greater.
            if (comparePart == null)
            return 1;
            else
            return this.FreeSpace.CompareTo(comparePart.FreeSpace);
        }

        public override int GetHashCode()
        {
            return System.Tuple.Create(ZoneID, ZoneIndex).GetHashCode();
        }

        public bool Equals(MZone other)
        {
            if (other == null) return false;
            return (this.ZoneIndex.Equals(other.ZoneIndex));
        }
    // Should also override == and != operators.
    }

    private List<MZone> MaterialZones = new List<MZone>();

    public void SpawnAllObjects()
    {
       ObjectToSpawn = GameObject.FindObjectsOfType<MMISceneObject>().ToList();
       UpdateMaterialZonesFreeSpace(); 
        for (int i=0; i<ObjectToSpawn.Count; i++) //spawn only parts, groups, and tools
         if (ObjectToSpawn[i].Type==MMISceneObject.Types.Part || ObjectToSpawn[i].Type==MMISceneObject.Types.Group || ObjectToSpawn[i].Type==MMISceneObject.Types.Tool)
         this.SpawnObject(ObjectToSpawn[i], Initial, Final, false);
    }

    private void OnGUI()
    {
       if (showButton)
        if (GUI.Button(new Rect(300, 300, 300, 50), "Spawn"))
                SpawnAllObjects();
    }

    //adds parentToConstraint transform to the object's limits
    public MInterval3 TransformToMaterialZoneLimits(MGeometryConstraint mZone, MInterval3 limits) 
    {
       var spaceMin = mZone.ParentToConstraint.Position.ToVector3() + 
                    mZone.ParentToConstraint.Rotation.ToQuaternion() * new Vector3((float)limits.X.Min,(float)limits.Y.Min,(float)limits.Z.Min);
              
       var spaceMax = mZone.ParentToConstraint.Position.ToVector3() + 
                    mZone.ParentToConstraint.Rotation.ToQuaternion() * new Vector3((float)limits.X.Max,(float)limits.Y.Max,(float)limits.Z.Max);

       return new MInterval3(new MInterval(spaceMin.x,spaceMax.x),new MInterval(spaceMin.y,spaceMax.y),new MInterval(spaceMin.z,spaceMax.z));
    }

    //adds parentObject transform to the given limits
    public MInterval3 TransformToGlobalLimits(Transform GlobalTransform, MInterval3 limits) 
    {
       var spaceMin = GlobalTransform.position + 
                    GlobalTransform.rotation * new Vector3((float)limits.X.Min,(float)limits.Y.Min,(float)limits.Z.Min);
              
       var spaceMax = GlobalTransform.position + 
                    GlobalTransform.rotation * new Vector3((float)limits.X.Max,(float)limits.Y.Max,(float)limits.Z.Max);

       return new MInterval3(new MInterval(spaceMin.x,spaceMax.x),new MInterval(spaceMin.y,spaceMax.y),new MInterval(spaceMin.z,spaceMax.z));
    }

    public Bounds TransformToMaterialZoneBounds()
    {
        return new Bounds();
    }

    public void UpdateMaterialZonesFreeSpace()
    {
        MaterialZones.Clear();
        for (int k=0; k<ObjectToSpawn.Count; k++)
         if (ObjectToSpawn[k].HasSpawningZones())
         {
          if (MaterialZone==null)
             MaterialZone=ObjectToSpawn[k];
          for (int i=0; i<ObjectToSpawn[k].Constraints.Count; i++)
            if (ObjectToSpawn[k].isConstraintASpawningZone(i))
            {
             // if (ObjectToSpawn[k].Constraints[i].Properties==null)
               //     ObjectToSpawn[k].Constraints[i].Properties = new Dictionary<string, string>();

              var limits = ObjectToSpawn[k].Constraints[i].GeometryConstraint.TranslationConstraint.Limits;
              double freeSpace=(limits.X.Max-limits.X.Min)*(limits.Y.Max-limits.Y.Min)*(limits.Z.Max-limits.Z.Min);
              //var constSpace =  TransformToGlobalLimits(ObjectToSpawn[k].transform,TransformToMaterialZoneLimits(ObjectToSpawn[k].Constraints[i].GeometryConstraint,limits));
                /*
              constSpaceMin = MaterialZone.Constraints[i].GeometryConstraint.ParentToConstraint.Position + 
                    MaterialZone.Constraints[i].GeometryConstraint.ParentToConstraint.Rotation.ToQuaternion() * new Vector3((float)limits.X.Min,(float)limits.Y.Min,(float)limits.Z.Min);
              
              constSpaceMax = MaterialZone.Constraints[i].GeometryConstraint.ParentToConstraint.Position + 
                    MaterialZone.Constraints[i].GeometryConstraint.ParentToConstraint.Rotation.ToQuaternion() * new Vector3((float)limits.X.Max,(float)limits.Y.Max,(float)limits.Z.Max);
                */
                //currently presence of objects in the spawning zones reduces the volume but does not reserve specific areas of the spawning zones, this has to be improved.
              /*
               var SpawnedParts = ObjectToSpawn[k].GetComponentsInChildren<MMISceneObject>();
                for (int j=0; j<SpawnedParts.Length; j++)
                    if (SpawnedParts[j].GetParentMMIScenObject()==ObjectToSpawn[k]) //only consider direct children of the spawning zone
                    {
                      Bounds componentBounds = SpawnedParts[j].GetCompleteBounds();
                      if (componentBounds.center.x>=constSpace.X.Min && componentBounds.center.x<=constSpace.X.Max && //if most of th object is within spawn zone
                            componentBounds.center.y>=constSpace.Y.Min && componentBounds.center.y<=constSpace.Y.Max &&
                            componentBounds.center.z>=constSpace.Z.Min && componentBounds.center.z<=constSpace.Z.Max)
                      freeSpace=freeSpace - componentBounds.size.x*componentBounds.size.y*componentBounds.size.z; //decrementing free space in the zone
                    }
              */
              MaterialZones.Add(new MZone(ObjectToSpawn[k].TaskEditorLocalID,i,freeSpace,limits,ObjectToSpawn[k]));
            }
          }
        MaterialZones.Sort();
        Debug.Log("---Sorting list---");
        for (int i=0; i<MaterialZones.Count; i++)
        Debug.Log("Spawner sorted: "+MaterialZones[i].ZoneIndex.ToString()+". "+MaterialZones[i].FreeSpace.ToString());;
    }

    public void CreateFinalLocationMarker(MMISceneObject sceneObject, Transform finalLocationsRoot)
    {
       GameObject finalLocationObj = new GameObject(sceneObject.name + "_final");
       finalLocationObj.transform.position = sceneObject.transform.position;
       finalLocationObj.transform.rotation = sceneObject.transform.rotation;
       finalLocationObj.transform.parent = sceneObject.transform.parent;
       var finalMMIScenobject = finalLocationObj.AddComponent<MMISceneObject>();
       finalMMIScenobject.Type = MMISceneObject.Types.FinalLocation;

       if (finalLocationsRoot != null)
         finalLocationObj.transform.parent = finalLocationsRoot.transform;

       sceneObject.FinalLocation=finalMMIScenobject;

       if (sceneObject.MSceneObject.Properties == null)
         sceneObject.MSceneObject.Properties = new Dictionary<string, string>();
       sceneObject.MSceneObject.Properties.Add("finalLocation", finalMMIScenobject.MSceneObject.ID);
    }

    public void SpanwObjectToInitialLocation(MMISceneObject sceneObject)
    {
        Debug.Log("Spawner: "+sceneObject.name + " moved to initial location "+sceneObject.InitialLocation.name);
        sceneObject.transform.position=sceneObject.InitialLocation.transform.position;
        sceneObject.transform.rotation=sceneObject.InitialLocation.transform.rotation;
        sceneObject.transform.parent=sceneObject.InitialLocation.transform.parent;
    }

    public bool SpanwObjectToInitialLocationMarker(MMISceneObject sceneObject)
    {
        MTransform Location = sceneObject.InitialLocation.GetIniitalLocationConstriaint(sceneObject.InitialLocationConstraint);
        if (sceneObject.MarkerFound(Location))
        { 
         Debug.Log("Spawner: "+sceneObject.name + " moved to initial location "+sceneObject.InitialLocation.name+"/"+sceneObject.InitialLocationConstraint);
         sceneObject.transform.position=sceneObject.InitialLocation.transform.position + sceneObject.InitialLocation.transform.rotation*Location.Position.ToVector3();
         sceneObject.transform.rotation=Location.Rotation.ToQuaternion();
         sceneObject.transform.parent=sceneObject.InitialLocation.transform;
         return true;
        }
        else
            Debug.LogWarning("Spawner: "+sceneObject.name + " initial location constraint "+sceneObject.InitialLocation.name+"/"+sceneObject.InitialLocationConstraint+" not found");
        return false;
    }

    //this oveloaded function uses spawning zones defined as constraints inside the material zone object
    public bool SpawnObject(MMISceneObject sceneObject, Transform initialLocationsRoot, Transform finalLocationsRoot, bool useRotation)
    {
        //Determine the size of the object based on the bounding box (use the first renderer in hiearchy -> can be optimized in future)
        //TODO: this should use all renderers to make sure all object extremes are considered
        Bounds bounds = sceneObject.GetCompleteBounds(); 

        //Aproximation of the size of the object
        float size = bounds.size.magnitude;
        bool hasMatch = false;

        //spawning objects to predefined locations
        if (sceneObject.InitialLocation!=null)
        {
            if (sceneObject.FinalLocation==null)
            CreateFinalLocationMarker(sceneObject, finalLocationsRoot);

            if (sceneObject.InitialLocationConstraint=="")
            SpanwObjectToInitialLocation(sceneObject);
            else
             if (!SpanwObjectToInitialLocationMarker(sceneObject)) //first try spawning at virtual marker location (constraint), only then at the marker itself
             SpanwObjectToInitialLocation(sceneObject);
            /*Debug.Log("Spawner: "+sceneObject.name + " moved to initial location "+sceneObject.InitialLocation.name);
            sceneObject.transform.position=sceneObject.InitialLocation.transform.position;
            sceneObject.transform.rotation=sceneObject.InitialLocation.transform.rotation;
            sceneObject.transform.parent=sceneObject.InitialLocation.transform.parent;*/
            
            if (Application.isPlaying)
              sceneObject.Synchronize();
            return true;
        }

        
        //Determine the available space in each zone to spawn objects that do not have predefined initial locaiton
        for (int n=0; n<MaterialZones.Count; n++)
        {
         int i= MaterialZones[n].ZoneIndex;
        if (MaterialZones[n].LinkedMaterialZone.isConstraintASpawningZone(i))  //checking if the constraint is proper spawningzone constraint
        {
            MGeometryConstraint constr = MaterialZones[n].LinkedMaterialZone.Constraints[i].GeometryConstraint;
            MVector3 space = new MVector3(constr.TranslationConstraint.Limits.X.Max - constr.TranslationConstraint.Limits.X.Min, constr.TranslationConstraint.Limits.Y.Max - constr.TranslationConstraint.Limits.Y.Min, constr.TranslationConstraint.Limits.Z.Max - constr.TranslationConstraint.Limits.Z.Min);

            //Check is zone has sufficient space (assmed that object is not going to be rotated)
            if (MaterialZones[n].UseSubZone(new MInterval3(new MInterval(bounds.min.x, bounds.max.x), new MInterval(bounds.min.y, bounds.max.y), new MInterval(bounds.min.z, bounds.max.z))))
            {
                //Zone found
                hasMatch = true;
                //MTransform sample = null;
                
                var sample = MaterialZones[n].ObjCoordinates;//SampleTransformFromBox(constr,bounds);
                

                //Create an object for the start and end configuration if it is not already defined
                if (sceneObject.FinalLocation==null)
                 CreateFinalLocationMarker(sceneObject, finalLocationsRoot);

                 //spawning object to spawning zone
                    Debug.Log("Spawner: "+sceneObject.name + " moved to spawning zone ");
                    //Update the position of the scene object (Perform acutal spawning) // + constr.ParentToConstraint.Rotation.ToQuaternion()*(new Vector3(0, bounds.center.y-bounds.min.y, 0)
                    sceneObject.transform.position = MaterialZones[n].LinkedMaterialZone.transform.position + MaterialZones[n].LinkedMaterialZone.transform.rotation*sample.Position.ToVector3();
                    //var originalScale = sceneObject.transform.localScale;
                    sceneObject.transform.rotation = constr.ParentToConstraint.Rotation.ToQuaternion()*MaterialZones[n].LinkedMaterialZone.transform.rotation;
                    sceneObject.transform.parent = MaterialZones[n].LinkedMaterialZone.transform;
                    //sceneObject.transform.localScale = originalScale;

                //Optionally use the estimated rotation
                if (useRotation)
                    sceneObject.transform.rotation = sample.Rotation.ToQuaternion();
                if (Application.isPlaying)
                sceneObject.Synchronize();
                Debug.Log("Spawning: "+sceneObject.name+(useRotation?"(use rotation)":"(dont use rotation)")
                                                       +sceneObject.transform.rotation.eulerAngles.x.ToString()+", "
                                                       +sceneObject.transform.rotation.eulerAngles.y.ToString()+", "
                                                       +sceneObject.transform.rotation.eulerAngles.z.ToString());              
                break;
            }
            }
        }


        if (!hasMatch)
            Debug.LogError("No spawning zone available for " + sceneObject.name);

        return hasMatch;
    }



    /// <summary>
    /// Spawns a specific scene object within the scene considering the defiend spawning zones defined as MGeometryConstraints which needs to be given as a paramerer
    /// </summary>
    /// <param name="sceneObject">The scene object that should be spawned</param>
    /// <param name="spawningZones">The available spawning zones defined as MGeometryConstraints</param>
    /// <param name="initialLocationsRoot">Transform which holds all initial locations (parent object)</param>
    /// <param name="finalLocationsRoot">Transform which holds all final locations (parent object)</param>
    /// <param name="useRotation">Defines whether the rotation of the spawned object is adapted</param>
    public bool SpawnObject(MMISceneObject sceneObject, List<MGeometryConstraint> spawningZones, Transform initialLocationsRoot, Transform finalLocationsRoot, bool useRotation)
    {
        //Determine the size of the object based on the bounding box (use the first renderer in hiearchy -> can be optimized in future)
        Bounds bounds = sceneObject.GetComponentInChildren<Renderer>().bounds;

        //Aproximation of the size of the object
        float size = bounds.size.magnitude;

        bool hasMatch = false;

        //Determine the available space in each zone
        foreach (MGeometryConstraint constr in spawningZones)
        {
            MVector3 space = new MVector3(constr.TranslationConstraint.Limits.X.Max - constr.TranslationConstraint.Limits.X.Min, constr.TranslationConstraint.Limits.Y.Max - constr.TranslationConstraint.Limits.Y.Min, constr.TranslationConstraint.Limits.Z.Max - constr.TranslationConstraint.Limits.Z.Min);

            //Check is zone has sufficient space
            if (space.Magnitude() > size)
            {
                //To do-> Incorporate futher metrics
                //Zone found
                hasMatch = true;
                MTransform sample = null;

                switch (constr.TranslationConstraint.Type)
                {
                    case MTranslationConstraintType.BOX:
                        //Sample position and rotation within bounds (defined by the constraint limits)
                        sample = SampleTransformFromBox(constr);
                        break;

                    case MTranslationConstraintType.ELLIPSOID:
                        sample = SampleFromEllipsoid(constr);
                        break;
                }


                //Debug.DrawLine(sample.Position.ToVector3(), sample.Position.ToVector3() + Vector3.up * 0.2f, Color.red, 30f);

                //Create an object for the start and end configuration if it is not already defined
                if (sceneObject.FinalLocation==null)
                {
                    GameObject finalLocationObj = new GameObject(sceneObject.name + "_final");
                    finalLocationObj.transform.position = sceneObject.transform.position;
                    finalLocationObj.transform.rotation = sceneObject.transform.rotation;
                    finalLocationObj.transform.parent = sceneObject.transform.parent;
                    var finalMMIScenobject = finalLocationObj.AddComponent<MMISceneObject>();
                    finalMMIScenobject.Type = MMISceneObject.Types.FinalLocation;

                    if (finalLocationsRoot != null)
                    finalLocationObj.transform.parent = finalLocationsRoot.transform;

                    sceneObject.FinalLocation=finalMMIScenobject;
                    if (sceneObject.MSceneObject.Properties == null)
                    sceneObject.MSceneObject.Properties = new Dictionary<string, string>();
                    sceneObject.MSceneObject.Properties.Add("finalLocation", finalMMIScenobject.MSceneObject.ID);
                }

                if (sceneObject.InitialLocation==null)
                {
                //Update the position of the scene object (Perform acutal spawning)
                Debug.Log("Sample position y: "+sample.Position.Y.ToString());
                sceneObject.transform.position = sample.Position.ToVector3() - MaterialZone.transform.rotation*(new Vector3(0, bounds.center.y-bounds.min.y, 0));
                //var originalScale = sceneObject.transform.localScale;
                sceneObject.transform.rotation = Quaternion.Euler(new Vector3(rotationx,rotationy,rotationz))*MaterialZone.transform.rotation;
                sceneObject.transform.parent = MaterialZone.transform;
                //sceneObject.transform.localScale = originalScale;
                //Optionally use the estimated rotation
                if (useRotation)
                    sceneObject.transform.rotation = sample.Rotation.ToQuaternion();
                }
                 else //spawning object to a predefined location
                {
                    Debug.Log("Spawner: "+sceneObject.name + " moved to initial location "+sceneObject.InitialLocation.name);
                    sceneObject.transform.position=sceneObject.InitialLocation.transform.position;
                    sceneObject.transform.rotation=sceneObject.InitialLocation.transform.rotation;
                    sceneObject.transform.parent=sceneObject.InitialLocation.transform.parent;
                }


                if (Application.isPlaying)
                sceneObject.Synchronize();
                Debug.Log("Spawning: "+sceneObject.name+(useRotation?"(use rotation)":"(dont use rotation)")
                                                       +sceneObject.transform.rotation.eulerAngles.x.ToString()+", "
                                                       +sceneObject.transform.rotation.eulerAngles.y.ToString()+", "
                                                       +sceneObject.transform.rotation.eulerAngles.z.ToString());

                //Create an object for the start and end configuration
                //Not neceessary currently, as the initial location should be used by the spawning script to place object at designated space instead of spawning it automatically
                /*
                GameObject initialLocationObj = new GameObject(sceneObject.name + "_initial");
                initialLocationObj.transform.position = sceneObject.transform.position;
                initialLocationObj.transform.rotation = sceneObject.transform.rotation;
                initialLocationObj.AddComponent<MMISceneObject>();
                initialLocationObj.GetComponent<MMISceneObject>().Type = MMISceneObject.Types.InitialLocation;

                if (initialLocationsRoot != null)
                    initialLocationObj.transform.parent = initialLocationsRoot.transform;
                if (sceneObject.MSceneObject.Properties == null)
                    sceneObject.MSceneObject.Properties = new Dictionary<string, string>();
                sceneObject.MSceneObject.Properties.Add("initialLocation", initialLocationObj.GetComponent<MMISceneObject>().MSceneObject.ID);
                */
                

                //Add the properties
                
                
                
                break;
            }
        }


        if (!hasMatch)
        {
            Debug.LogError("No spawning zone available for " + sceneObject.name);
        }

        return hasMatch;
    }

    private MTransform SampleFromEllipsoid(MGeometryConstraint geomConstraint)
    {
        Vector3 comPosition = geomConstraint.ParentToConstraint.Position.ToVector3();
        Quaternion comRotation = geomConstraint.ParentToConstraint.Rotation.ToQuaternion();

        //Determine the radius for each dimensions
        Vector3 radius = new Vector3(((float)geomConstraint.TranslationConstraint.Limits.X.Max - (float)geomConstraint.TranslationConstraint.Limits.X.Min) / 2f,
            ((float)geomConstraint.TranslationConstraint.Limits.Y.Max - (float)geomConstraint.TranslationConstraint.Limits.Y.Min) / 2f,
            ((float)geomConstraint.TranslationConstraint.Limits.Z.Max - (float)geomConstraint.TranslationConstraint.Limits.Z.Min) / 2f);

        //Sample from 3D ball
        float[] ballSample = SampleFromNBall(3);

        //Determine the samples position
        Vector3 sampledPosition = comPosition + comRotation * (new Vector3(radius.x * ballSample[0], radius.y * ballSample[1], radius.z * ballSample[2]));


        Quaternion sampledRotation = comRotation * Quaternion.Euler(new Vector3((float)geomConstraint.RotationConstraint.Limits.X.Min,
                                                                                (float)geomConstraint.RotationConstraint.Limits.Y.Min,
                                                                                (float)geomConstraint.RotationConstraint.Limits.Z.Min));

        /*
        Quaternion sampledRotation = comRotation * Quaternion.Euler(new Vector3(Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.X.Min, (float)geomConstraint.RotationConstraint.Limits.X.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.Y.Min, (float)geomConstraint.RotationConstraint.Limits.Y.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.Z.Min, (float)geomConstraint.RotationConstraint.Limits.Z.Max, UnityEngine.Random.value)));
        */

        return new MTransform(System.Guid.NewGuid().ToString(), sampledPosition.ToMVector3(), sampledRotation.ToMQuaternion());
    }

    /// <summary>
    /// Samples a position from the box defined by the geom constriant taking into account also object bounds
    /// </summary>
    /// <param name="geomConstraint"></param>
    /// <returns></returns>
    private static MTransform SampleTransformFromBox(MGeometryConstraint geomConstraint, Bounds bounds)
    {

        Vector3 comPosition = geomConstraint.ParentToConstraint.Position.ToVector3();
        Quaternion comRotation = geomConstraint.ParentToConstraint.Rotation.ToQuaternion();

        var boundsVector = geomConstraint.ParentToConstraint.Rotation.ToQuaternion()*(new Vector3(bounds.center.x-bounds.min.x,bounds.center.y-bounds.min.y,bounds.center.z-bounds.min.z));
        var boundsVectorMax = geomConstraint.ParentToConstraint.Rotation.ToQuaternion()*(new Vector3(bounds.center.x-bounds.max.x,bounds.center.y-bounds.max.y,bounds.center.z-bounds.max.z));

        Vector3 sampledPosition = comPosition + comRotation * new Vector3(
            Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.X.Min+ boundsVector.x, (float)geomConstraint.TranslationConstraint.Limits.X.Max+ boundsVectorMax.x, UnityEngine.Random.value),
            System.Convert.ToSingle(geomConstraint.TranslationConstraint.Limits.Y.Min)+ boundsVector.y,
            Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.Z.Min+ boundsVector.z, (float)geomConstraint.TranslationConstraint.Limits.Z.Max+ boundsVectorMax.z, UnityEngine.Random.value));
        
        Quaternion sampledRotation = comRotation * Quaternion.Euler(new Vector3(Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.X.Min, (float)geomConstraint.RotationConstraint.Limits.X.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.Y.Min, (float)geomConstraint.RotationConstraint.Limits.Y.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.Z.Min, (float)geomConstraint.RotationConstraint.Limits.Z.Max, UnityEngine.Random.value)));


        return new MTransform(System.Guid.NewGuid().ToString(), sampledPosition.ToMVector3(), sampledRotation.ToMQuaternion());
    }

    /// <summary>
    /// Samples a position from the box defined by the geom constriant
    /// </summary>
    /// <param name="geomConstraint"></param>
    /// <returns></returns>
    private static MTransform SampleTransformFromBox(MGeometryConstraint geomConstraint)
    {

        Vector3 comPosition = geomConstraint.ParentToConstraint.Position.ToVector3();
        Quaternion comRotation = geomConstraint.ParentToConstraint.Rotation.ToQuaternion();

        Vector3 sampledPosition = comPosition + comRotation * new Vector3(
            //Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.X.Min, (float)geomConstraint.TranslationConstraint.Limits.X.Max, UnityEngine.Random.value),
            System.Convert.ToSingle(geomConstraint.TranslationConstraint.Limits.X.Min),
            //(float)geomConstraint.TranslationConstraint.Limits.X.Min,
            System.Convert.ToSingle(geomConstraint.TranslationConstraint.Limits.Y.Min),
            System.Convert.ToSingle(geomConstraint.TranslationConstraint.Limits.Z.Min));
            //Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.Z.Min, (float)geomConstraint.TranslationConstraint.Limits.Z.Max, UnityEngine.Random.value));
        /*
        Vector3 sampledPosition = comPosition + comRotation * new Vector3(Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.X.Min, (float)geomConstraint.TranslationConstraint.Limits.X.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.Y.Min, (float)geomConstraint.TranslationConstraint.Limits.Y.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.Z.Min, (float)geomConstraint.TranslationConstraint.Limits.Z.Max, UnityEngine.Random.value));
        */
        Quaternion sampledRotation = comRotation * Quaternion.Euler(new Vector3(Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.X.Min, (float)geomConstraint.RotationConstraint.Limits.X.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.Y.Min, (float)geomConstraint.RotationConstraint.Limits.Y.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.RotationConstraint.Limits.Z.Min, (float)geomConstraint.RotationConstraint.Limits.Z.Max, UnityEngine.Random.value)));


        return new MTransform(System.Guid.NewGuid().ToString(), sampledPosition.ToMVector3(), sampledRotation.ToMQuaternion());
    }


    /// <summary>
    /// Samples uniformly from a n ball
    /// </summary>
    /// <param name="dimensions">The dimensions of the ball</param>
    /// <returns></returns>
    private static float[] SampleFromNBall(int dimensions)
    {
        float[] xValues = new float[dimensions];
        float[] yValues = new float[dimensions];

        //Generate random value for radius
        float r = Random.Range(0, 1f);
        //float r = RandomValueGenerator.GetRandomUniform(0, 1);


        //d Sqrt of r 
        r = Mathf.Pow(r, 1.0f / dimensions);

        float denominator = 0;

        for (int i = 0; i < xValues.Length; i++)
        {
            //Generate random gaussian
            xValues[i] = GetRandomGaussian();

            denominator += Mathf.Pow(xValues[i], 2);
        }

        denominator = Mathf.Sqrt(denominator);


        for (int i = 0; i < yValues.Length; i++)
        {
            //Generate random gaussian
            yValues[i] = r * xValues[i] / denominator;
        }
        return yValues;
    }

    /// <summary>
    /// Returns a gaussian distributed random value
    /// </summary>
    /// <param name="mu"></param>
    /// <param name="sigma"></param>
    /// <returns></returns>
    private static float GetRandomGaussian(float mu = 0, float sigma = 1)
    {
        var u1 = Random.value;
        var u2 = Random.value;

        var rand_std_normal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) *
                            Mathf.Sin(2.0f * Mathf.PI * u2);

        var rand_normal = mu + sigma * rand_std_normal;

        return rand_normal;
    }
}
