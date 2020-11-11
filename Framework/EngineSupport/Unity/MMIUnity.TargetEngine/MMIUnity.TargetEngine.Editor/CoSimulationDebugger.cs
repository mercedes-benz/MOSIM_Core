// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Felix Gaisbauer


//#if UNITY_EDITOR

using MMICoSimulation;
using MMIStandard;
using MMIUnity.TargetEngine;
using MMIUnity.TargetEngine.Scene;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MMIUnity.TargetEngine.Editor
{


    /// <summary>
    /// An editor window for the co-simulation debugger.
    /// This window provides further functionality and visualizes the hierachy.
    /// </summary>
    [CustomEditor(typeof(CoSimulationDebugger))]
    public class CoSimulationDebuggerEditorWindow : UnityEditor.Editor
    {

        /// <summary>
        /// Method responsible for the inspector visualization
        /// </summary>
        public override void OnInspectorGUI()
        {
            //Get the cosimulation debugger instance
            CoSimulationDebugger debugger = this.target as CoSimulationDebugger;

            if (debugger.SourceAvatar == null)
            {
                GUILayout.Label("Source avatar must be set in order to use the CoSimulationDebugger.");

                //Call the base inspector
                base.OnInspectorGUI();

                return;
            }


            //Check if the present frame is not null
            if(debugger.currentFrame != null)
            {

                GUILayout.Label("Hierachy:");
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                //Draw the initial
                this.DrawInitial(debugger.currentHierachyIndex == 0);

                int index = 0;
                for (int i=0; i< debugger.currentFrame.Results.Count;i++)
                {
                    MSimulationResult result = debugger.currentFrame.Results[i];
                    MInstruction instruction = debugger.GetInstructionByID(debugger.currentFrame.Instructions[i]);


                    DrawHierachyEntry(debugger, result, instruction, index +1);
                    index++;
                }

                GUI.color = Color.blue;

                for (int i = 0; i < debugger.currentFrame.CoSimulationSolverResults.Count; i++)
                {
                    MSimulationResult result = debugger.currentFrame.CoSimulationSolverResults[i];


                    DrawSolverEntry(debugger, result, index + 1);
                    index++;
                }

                GUI.color = Color.cyan;

                //Draw the merged resuls
                this.DrawMergedResult(debugger, debugger.currentFrame.MergedResult, debugger.currentHierachyIndex == -1);

                GUI.color = GUI.color = Color.white;
            }

            //Call the base inspector
            base.OnInspectorGUI();
        }

        /// <summary>
        /// Draws a respective hierachy level (corresponds to a MMU)
        /// </summary>
        /// <param name="debugger"></param>
        /// <param name="result"></param>
        /// <param name="instruction"></param>
        /// <param name="index"></param>
        private void DrawHierachyEntry(CoSimulationDebugger debugger, MSimulationResult result, MInstruction instruction, int index)
        {
            Rect rect = EditorGUILayout.BeginVertical();

            GUILayout.Label("Level: " + index);

            bool active = index == debugger.currentHierachyIndex;


            if (instruction != null)
            {
                GUILayout.Label("Name: " + instruction.Name);
                GUILayout.Label("MotionType: " + instruction.MotionType);

                GUILayout.Label("Events: " + (result.Events!=null?result.Events.Count:0));
                if (result.Events != null)
                    foreach (MSimulationEvent simEvent in result.Events)
                    {
                        GUILayout.Label("Name: " + simEvent.Name);
                        GUILayout.Label("Type: " + simEvent.Type);
                    }


                GUILayout.Label("Scene manipulations: " + (result.SceneManipulations != null ? result.SceneManipulations.Count : 0));
                if (result.SceneManipulations != null)
                    if (active)
                    {
                        this.ShowSceneManipulations(result.SceneManipulations);
                    }

                GUILayout.Label("Number of constraints: " + (result.Constraints != null ? result.Constraints.Count : 0));
                if (result.Constraints != null && result.Constraints.Count > 0)
                {
                    if (debugger.AutoDisplayConstraints)
                    {
                        if(active)
                        {
                            this.ShowConstraints(result.Constraints);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Show constraints"))
                        {
                            this.ShowConstraints(result.Constraints);
                        }
                    }
                }

                //Show the log data
               if(result.LogData != null)
               {
                    foreach(string log in result.LogData)
                    {
                        GUILayout.Label(log);
                    }
               }
            }


            GUILayout.EndVertical();

            if (active)
                EditorGUI.DrawRect(rect, new Color(0, 1.0f, 0, 0.2f));
            else
                EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.2f));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }


        /// <summary>
        /// Draws the merged result
        /// </summary>
        /// <param name="debugger"></param>
        /// <param name="result"></param>
        /// <param name="active"></param>
        private void DrawMergedResult (CoSimulationDebugger debugger, MSimulationResult result, bool active)
        {
            Rect rect = EditorGUILayout.BeginVertical();

            GUILayout.Label("Level: " + "merged");


            GUILayout.Label("Events: " + (result.Events != null ? result.Events.Count : 0));
            if (result.Events != null)
                foreach (MSimulationEvent simEvent in result.Events)
                {
                    GUILayout.Label("Name: " + simEvent.Name);
                    GUILayout.Label("Type: " + simEvent.Type);
                }

            GUILayout.Label("Scene manipulations: " + (result.SceneManipulations != null ? result.SceneManipulations.Count : 0));
            if (result.SceneManipulations != null)
                if (active)
                {
                    this.ShowSceneManipulations(result.SceneManipulations);
                }

            GUILayout.Label("Number of constraints: " + (result.Constraints != null ? result.Constraints.Count : 0));
            if (result.Constraints !=null && result.Constraints.Count > 0)
            {
                if (debugger.AutoDisplayConstraints)
                {
                    if (active)
                    {
                        this.ShowConstraints(result.Constraints);
                    }
                }
                else
                {
                    if (GUILayout.Button("Show constraints"))
                    {
                        this.ShowConstraints(result.Constraints);
                    }
                }
            }

            //Show the log data
            if (result.LogData != null)
            {
                foreach (string log in result.LogData)
                {
                    GUILayout.Label(log);
                }
            }


            GUILayout.EndVertical();

            if(active)
                EditorGUI.DrawRect(rect, new Color(0, 1.0f, 0, 0.2f));
            else
                EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.2f));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }


        /// <summary>
        /// Draws a respective hierachy level (corresponds to a MMU)
        /// </summary>
        /// <param name="debugger"></param>
        /// <param name="result"></param>
        /// <param name="instruction"></param>
        /// <param name="index"></param>
        private void DrawSolverEntry(CoSimulationDebugger debugger, MSimulationResult result, int index)
        {
            Rect rect = EditorGUILayout.BeginVertical();

            GUILayout.Label("Level: " + index);

            bool active = index == debugger.currentHierachyIndex;


                GUILayout.Label("Name: " + "Solver");

                GUILayout.Label("Events: " + (result.Events != null ? result.Events.Count : 0));
                if (result.Events != null)
                    foreach (MSimulationEvent simEvent in result.Events)
                    {
                        GUILayout.Label("Name: " + simEvent.Name);
                        GUILayout.Label("Type: " + simEvent.Type);
                    }

                GUILayout.Label("Scene manipulations: " + (result.SceneManipulations != null ? result.SceneManipulations.Count : 0));
                if (result.SceneManipulations != null)
                    if (active)
                    {
                        this.ShowSceneManipulations(result.SceneManipulations);
                    }

                GUILayout.Label("Number of constraints: " + (result.Constraints != null ? result.Constraints.Count : 0));
                if (result.Constraints != null && result.Constraints.Count > 0)
                {
                    if (debugger.AutoDisplayConstraints)
                    {
                        if (active)
                        {
                            this.ShowConstraints(result.Constraints);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("Show constraints"))
                        {
                            this.ShowConstraints(result.Constraints);
                        }
                    }


            }
        


            GUILayout.EndVertical();

            if (active)
                EditorGUI.DrawRect(rect, new Color(0, 1.0f, 0, 0.2f));
            else
                EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.2f));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        /// <summary>
        /// Draws the merged result
        /// </summary>
        /// <param name="debugger"></param>
        /// <param name="result"></param>
        /// <param name="active"></param>
        private void DrawInitial(bool active)
        {
            Rect rect = EditorGUILayout.BeginVertical();

            GUILayout.Label("Level: " + "initial");


            GUILayout.EndVertical();

            if (active)
                EditorGUI.DrawRect(rect, new Color(0, 1.0f, 0, 0.2f));
            else
                EditorGUI.DrawRect(rect, new Color(1.0f, 1.0f, 1.0f, 0.2f));

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        private List<GameObject> constraintObjects = new List<GameObject>();
        private List<GameObject> sceneManipulationObjects = new List<GameObject>();


        /// <summary>
        /// Visualizes the constraints 
        /// </summary>
        /// <param name="constraints"></param>
        private void ShowConstraints(List<MConstraint> constraints)
        {

            for(int i= constraintObjects.Count-1; i >= 0; i--)
            {
                constraintObjects[i].SetActive(false);
            }

            int index = 0;

            foreach(MConstraint constraint in constraints)
            {
                if(constraint.JointConstraint != null)
                {
                    MJointConstraint endeffectorConstraint = constraint.JointConstraint;

                    if (endeffectorConstraint.GeometryConstraint != null)
                    {
                        MTranslationConstraint posConstraint = endeffectorConstraint.GeometryConstraint.TranslationConstraint;

                        GameObject p = null;
                        if (index < constraintObjects.Count)
                        {
                            p = constraintObjects[index];
                            p.SetActive(true);
                        }
                        else
                        {
                            p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            p.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
                            p.GetComponent<Renderer>().material.color = Color.red;
                            p.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                            constraintObjects.Add(p);
                        }

                        index++;

                        p.transform.position = new Vector3((float)posConstraint.X(), (float)posConstraint.Y(), (float)posConstraint.Z());
                    }
                }
            }
        }


        /// <summary>
        /// Visualizes the scene manipulations
        /// </summary>
        /// <param name="sceneManipulations"></param>
        private void ShowSceneManipulations(List<MSceneManipulation> sceneManipulations)
        {

            for (int i = sceneManipulationObjects.Count - 1; i >= 0; i--)
            {
                sceneManipulationObjects[i].SetActive(false);
            }

            int index = 0;

            foreach (MSceneManipulation sceneManipulation in sceneManipulations)
            {
                if (sceneManipulation.Transforms !=null)
                {

                    foreach(MTransformManipulation tm in sceneManipulation.Transforms)
                    {
                        GameObject p = null;
                        if (index < sceneManipulationObjects.Count)
                        {
                            p = sceneManipulationObjects[index];
                            p.SetActive(true);

                            try
                            {
                                //Apply the scene manipulation
                                UnitySceneAccess.Instance.ApplyManipulations(new List<MSceneManipulation>() { new MSceneManipulation() { Transforms = new List<MTransformManipulation>() { tm } } });
                            }
                            catch (System.Exception)
                            {
                                //It might be the case that the ID is invalid for the current scene
                            }
                        }
                        else
                        {
                            p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                            p.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
                            p.GetComponent<Renderer>().material.color = Color.cyan;
                            p.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
                            sceneManipulationObjects.Add(p);
                        }
                    

                        p.transform.position = new Vector3((float)tm.Position.X, (float)tm.Position.Y, (float)tm.Position.Z);


                        index++;
                    }
                }
            }
        }

    }


    /// <summary>
    /// Class for debugging the CoSimulation
    /// </summary>
    public class CoSimulationDebugger : MonoBehaviour
    {
        /// <summary>
        /// The timelines comprising the instructions and its temporal window
        /// </summary>
        private List<InstructionWindow> instructionWindows = new List<InstructionWindow>();

        /// <summary>
        /// Class for representing an instruction window
        /// </summary>
        private class InstructionWindow
        {
            public int StartIndex;

            public int EndIndex;

            public MInstruction Instruction;

            public List<int> Events = new List<int>();

            public List<MSimulationEvent> RawEvents = new List<MSimulationEvent>();

        }


        public bool AutoDisplayConstraints = true;
        public MMIAvatar SourceAvatar;

        public static CoSimulationDebugger Instance;

        private CoSimulationRecord record;
        private float sliderValue = 0;
        private float sliderOffsetX = 10;
        private MInstruction currentInstruction;


        public int currentHierachyIndex = -1;
        public int frameIndex;
        public int HierachyElements = 0;

        public CoSimulationFrame currentFrame;

        private void Start()
        {
            if(this.SourceAvatar==null)
            {
                this.SourceAvatar = this.GetComponent<MMIAvatar>();
            }
        }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(sliderOffsetX, Screen.height - 45, 100, 40), "Start recording"))
            {
                this.SourceAvatar.CoSimulator.Recording = true;
            }

            if (record != null)
            {
                if (GUI.Button(new Rect(sliderOffsetX + 220, Screen.height - 45, 100, 40), "Save"))
                {
                    string outputPath = EditorUtility.SaveFilePanel("Select the output file", "", "record", "cosimRecord");

                    try
                    {
                        byte[] data = MMICSharp.Common.Communication.Serialization.SerializeBinary<CoSimulationRecord>(this.record);

                        System.IO.File.WriteAllBytes(outputPath, data);
                    }
                    catch (System.Exception)
                    {

                    }
                }
            }

            if (GUI.Button(new Rect(sliderOffsetX + 330, Screen.height - 45, 100, 40), "Load"))
            {
                string filePath = EditorUtility.OpenFilePanel("Select the file to be loaded", "", "cosimRecord");

                try
                {
                    byte[] data = System.IO.File.ReadAllBytes(filePath);


                    this.record = MMICSharp.Common.Communication.Serialization.DeserializeBinary<CoSimulationRecord>(data);
                    this.instructionWindows = new List<InstructionWindow>();

                    //Disable the simulation controller
                    GameObject.FindObjectOfType<SimulationController>().enabled = false;


                    //Add the instruction windows
                    foreach (MInstruction instruction in this.record.Instructions)
                    {
                        InstructionWindow w = this.GetInstructionWindow(instruction);

                        if (w.StartIndex == -1)
                            continue;

                        if (w.EndIndex == -1)
                            w.EndIndex = this.record.Frames.Count - 1;


                        this.instructionWindows.Add(w);
                    }
                }
                catch (System.Exception)
                {
                }
            }


            if (this.SourceAvatar.CoSimulator !=null && this.SourceAvatar.CoSimulator.Recording)
            {
                if (GUI.Button(new Rect(sliderOffsetX + 110, Screen.height - 45, 100, 40), "Stop"))
                {
                    this.record = this.SourceAvatar.CoSimulator.GetRecord();
                    this.SourceAvatar.CoSimulator.Recording = false;
                    this.SourceAvatar.GetComponentInParent<SimulationController>().enabled = false;



                    //Create the instrution windows



                    foreach (MInstruction instruction in this.record.Instructions)
                    {
                        InstructionWindow w = this.GetInstructionWindow(instruction);

                        if (w.StartIndex == -1)
                            continue;

                        if (w.EndIndex == -1)
                            w.EndIndex = this.record.Frames.Count - 1;


                        this.instructionWindows.Add(w);
                    }
                }
            }

            //Only display if a record has been loaded -> To do provide a way to load records from the file system
            if (this.record != null)
            {
                //Update the slider
                sliderValue = GUI.HorizontalSlider(new Rect(sliderOffsetX, Screen.height - 60, Screen.width - (sliderOffsetX * 2), 20), sliderValue, 0, (float)this.record.Frames.Count);

                //Estimate the current index
                frameIndex = (int)sliderValue;

                //Get the current frame
                currentFrame = this.record.Frames[frameIndex];

                //The amount of elements in the present frame
                this.HierachyElements = currentFrame.Results.Count + currentFrame.CoSimulationSolverResults.Count;

                //Handle if out of range
                if(currentHierachyIndex < -1 || currentHierachyIndex > HierachyElements)
                {
                    currentHierachyIndex = -1;
                }


                currentInstruction = null;


                if (currentHierachyIndex == -1)
                {
                    this.SourceAvatar.AssignPostureValues(currentFrame.MergedResult.Posture);
                }
                else if(currentHierachyIndex == 0)
                {
                    this.SourceAvatar.AssignPostureValues(currentFrame.Initial);
                }
                else
                {
                    if(currentHierachyIndex <= currentFrame.Results.Count)
                    {
                        this.SourceAvatar.AssignPostureValues(currentFrame.Results[currentHierachyIndex - 1].Posture);
                        currentInstruction = GetInstructionByID(currentFrame.Instructions[currentHierachyIndex - 1]);
                    }

                    else
                    {
                        this.SourceAvatar.AssignPostureValues(currentFrame.CoSimulationSolverResults[currentHierachyIndex - currentFrame.Instructions.Count-1].Posture);
                    }

                }



                GUI.color = Color.red;

                this.ShowInstructionWindows();
            }
        }

        private void ShowInstructionWindows()
        {
            GUIStyle guiStyle = new GUIStyle
            {
                padding = new RectOffset(0, 0, 0, 0)
            };


            int totalWidth = Screen.width - (int)(sliderOffsetX * 2);
            int hierachyLevel = 1;


            //To do Check how many are simulatanously active
            foreach(InstructionWindow w in instructionWindows)
            {

                //-----------------Start events--------------------------------------------------------------------

                GUI.color = Color.black;
                GUI.backgroundColor = Color.black;



                //Total height in pixels
                int height = 20;

                //Total width in pixel
                float width = 1f / (float)(this.record.Frames.Count) * (float)totalWidth;


                int index = 0;
                foreach (int eventIndex in w.Events)
                {
                    //Adjust color depending on event type
                    MSimulationEvent simEvent = w.RawEvents[index];


                    GUI.color = Color.black;
                    GUI.backgroundColor = Color.black;

                    if (simEvent.Type == mmiConstants.MSimulationEvent_Start)
                        GUI.color = Color.blue;

                    if (simEvent.Type == mmiConstants.MSimulationEvent_End)
                        GUI.color = Color.green;

                    //To do   
                    Texture2D texture = new Texture2D((int)width, height);
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                        {
                            texture.SetPixel(x, y, Color.black);
                        }


                    float relCoord = (float)eventIndex / (float)this.record.Frames.Count;

                    if (GUI.Button(new Rect(sliderOffsetX + relCoord * totalWidth - width /2f, Screen.height - 60 - hierachyLevel * 20, width, height), texture, guiStyle))
                    {
                        //Display the name of the event
                        this.sliderValue = eventIndex;
                    }

                    index++;
                }

                //-----------------End events--------------------------------------------------------------------


                GUI.color = Color.green;
                GUI.backgroundColor = Color.black;


                if (currentInstruction != null && currentInstruction.ID == w.Instruction.ID)
                {
                    GUI.backgroundColor = Color.green;

                }

                float start = (float)w.StartIndex / (float)this.record.Frames.Count;
                float end = (float)w.EndIndex / (float)this.record.Frames.Count;
                float relativeWidth = end - start;


                if(GUI.Button(new Rect(sliderOffsetX + start * totalWidth, Screen.height - 60 - hierachyLevel * 20, relativeWidth * totalWidth, 20 ), w.Instruction.Name))
                {
                    this.sliderValue = w.StartIndex;
                }






                hierachyLevel++;
            }
        }


        private InstructionWindow GetInstructionWindow (MInstruction instruction)
        {
            InstructionWindow window = new InstructionWindow()
            {
                StartIndex = -1,
                EndIndex = -1,
                Instruction = instruction
            };

            for(int i=0; i< this.record.Frames.Count; i++)
            {
                CoSimulationFrame frame = this.record.Frames[i];

                if (frame.Instructions.Contains(instruction.ID))
                {
                    if (window.StartIndex == -1)
                        window.StartIndex = i;


                    if (window.EndIndex == -1 || window.EndIndex < i)
                        window.EndIndex = i;


                    MSimulationResult result = frame.MergedResult;

                    if (result.Events != null && result.Events.Count > 0)
                    {
                        foreach (MSimulationEvent simEvent in result.Events)
                        {
                            if (simEvent.Reference == instruction.ID)
                            {
                                window.Events.Add(i);
                                window.RawEvents.Add(simEvent);
                                break;
                            }
                        }
                    }

                }
            }



            return window;
        }


        // Update is called once per frame
        void Update()
        {
            if (this.record != null)
            {

                //Move along the time line
                if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.RightArrow)) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    sliderValue++;

                    if (sliderValue >= this.record.Frames.Count)
                        sliderValue = 0;
                }

                //Move in the past
                if ((Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftArrow)) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    //Move to next
                    sliderValue--;

                    if (sliderValue < 0)
                        sliderValue = this.record.Frames.Count - 1;
                }


                //Do frame internal steering
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    switch (currentHierachyIndex)
                    {
                        case -1:
                            //Check if merged posture -> Use the last posture in hierarchy
                            currentHierachyIndex = currentFrame.Results.Count + currentFrame.CoSimulationSolverResults.Count;
                            break;

                        case 0:
                            //Do nothing
                            //Cannot move upwards in hierachy anymore
                            break;

                        default:
                            //Move to previous
                            currentHierachyIndex--;
                            break;
                    }
                }


                if (Input.GetKeyDown(KeyCode.DownArrow))
                {

                    //Move down the hierachy if not last element
                    if (currentHierachyIndex != -1)
                    {
                        //Move to next
                        currentHierachyIndex++;

                        //Directly jump to merged result
                        if (currentHierachyIndex == currentFrame.Results.Count+1 + currentFrame.CoSimulationSolverResults.Count)
                            currentHierachyIndex = -1;

                    }


                }
            }

        }


        public MInstruction GetInstructionByID(string id)
        {
            return this.record.Instructions.Find(s => s.ID == id);
        }
    }
}
//#endif
