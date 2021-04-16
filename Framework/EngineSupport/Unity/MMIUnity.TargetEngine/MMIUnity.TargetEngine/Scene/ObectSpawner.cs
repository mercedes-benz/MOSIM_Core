// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer

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
    public MMISceneObject MaterialZone;
    public MMISceneObject ObjectToSpawn;
    public Transform Initial;
    public Transform Final;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(300, 300, 300, 50), "Spawn (Translation constraint box)"))
        {
            //Create manually a dummy constraint
            MGeometryConstraint constr = new MGeometryConstraint("");
            constr.ParentToConstraint = new MTransform("", MaterialZone.MSceneObject.Transform.Position, MaterialZone.MSceneObject.Transform.Rotation);
            constr.TranslationConstraint = new MTranslationConstraint(MTranslationConstraintType.BOX, new MInterval3(new MInterval(-1, 1), new MInterval(0, 0), new MInterval(-0.5f, 0.5f)));

            //Rotation should be fixed
            constr.RotationConstraint = new MRotationConstraint(new MInterval3(new MInterval(0, 0), new MInterval(0, 0), new MInterval(0, 0)));

            this.SpawnObject(ObjectToSpawn, new List<MGeometryConstraint>() { constr }, Initial, Final, true);
        }

        if (GUI.Button(new Rect(600, 300, 300, 50), "Spawn (Translation constraint ELLIPSOID)"))
        {
            //Create manually a dummy constraint
            MGeometryConstraint constr = new MGeometryConstraint("");
            constr.ParentToConstraint = new MTransform("", MaterialZone.MSceneObject.Transform.Position, MaterialZone.MSceneObject.Transform.Rotation);
            constr.TranslationConstraint = new MTranslationConstraint(MTranslationConstraintType.ELLIPSOID, new MInterval3(new MInterval(-0.5f, 0.5f), new MInterval(0, 0), new MInterval(-0.5f, 0.5f)));

            //Rotation should be fixed
            constr.RotationConstraint = new MRotationConstraint(new MInterval3(new MInterval(0, 0), new MInterval(0, 0), new MInterval(0, 0)));

            this.SpawnObject(ObjectToSpawn, new List<MGeometryConstraint>() { constr }, Initial, Final, true);
        }
    }



    /// <summary>
    /// Spawns a specific scene object within the scene considering the defiend spawning zones defined as MGeometryConstraints.
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

                //Create an object for the start and end configuration
                GameObject finalLocationObj = new GameObject(sceneObject.name + "_final");
                finalLocationObj.transform.position = sceneObject.transform.position;
                finalLocationObj.transform.rotation = sceneObject.transform.rotation;
                finalLocationObj.AddComponent<MMISceneObject>();
                finalLocationObj.GetComponent<MMISceneObject>().Type = MMISceneObject.Types.FinalLocation;

                if (finalLocationsRoot != null)
                    finalLocationObj.transform.parent = finalLocationsRoot.transform;


                //Update the position of the scene object (Perform acutal spawning)
                sceneObject.transform.position = sample.Position.ToVector3() + new Vector3(0, bounds.extents.y, 0);

                //Optionally use the estimated rotation
                if (useRotation)
                    sceneObject.transform.rotation = sample.Rotation.ToQuaternion();

                //Create an object for the start and end configuration
                GameObject initialLocationObj = new GameObject(sceneObject.name + "_initial");
                initialLocationObj.transform.position = sceneObject.transform.position;
                initialLocationObj.transform.rotation = sceneObject.transform.rotation;
                initialLocationObj.AddComponent<MMISceneObject>();
                initialLocationObj.GetComponent<MMISceneObject>().Type = MMISceneObject.Types.InitialLocation;

                if (initialLocationsRoot != null)
                    initialLocationObj.transform.parent = initialLocationsRoot.transform;


                if (sceneObject.MSceneObject.Properties == null)
                    sceneObject.MSceneObject.Properties = new Dictionary<string, string>();

                //Add the properties
                sceneObject.MSceneObject.Properties.Add("initialLocation", initialLocationObj.GetComponent<MMISceneObject>().MSceneObject.ID);
                sceneObject.MSceneObject.Properties.Add("finalLocation", finalLocationObj.GetComponent<MMISceneObject>().MSceneObject.ID);

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

        Vector3 sampledPosition = comPosition + comRotation * new Vector3(Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.X.Min, (float)geomConstraint.TranslationConstraint.Limits.X.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.Y.Min, (float)geomConstraint.TranslationConstraint.Limits.Y.Max, UnityEngine.Random.value),
            Mathf.Lerp((float)geomConstraint.TranslationConstraint.Limits.Z.Min, (float)geomConstraint.TranslationConstraint.Limits.Z.Max, UnityEngine.Random.value));

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
