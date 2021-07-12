// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Kłodowski

using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.IO;
using MMIUnity.TargetEngine.Scene;
using System.Linq;

namespace MMIUnity.TargetEngine.Scene
{
    [Serializable]
    public class glTF_Asset
    {
        public glTF_Asset()
        {
            this.version = "2.0";
            this.generator = "MOSIM Unity glTF generator";
            this.copyright = "MIT license";
        }

        public glTF_Asset(string customGenerator, string customCopyright)
        {
            this.version = "2.0";
            this.generator = customGenerator == "" ? "MOSIM Unity glTF generator" : customGenerator;
            this.copyright = customCopyright == "" ? "MIT license" : customCopyright;
        }

        public string version;
        public string generator;
        public string copyright;
    }

    [Serializable]
    public class glTF_Scene
    {
        public glTF_Scene()
        {
            this.name = "";
            this.nodes = new List<uint>();
        }

        public glTF_Scene(string sceneName)
        {
            this.name = sceneName;
            this.nodes = new List<uint>();
        }

        public string name;
        public List<uint> nodes;
    }

    [Serializable]
    public class gltf_MosimExtra
    {
        public gltf_MosimExtra()
        {
            this.TaskEditorID = 0;
            this.TaskEditorLocalID = 0;
            this.Type = MMISceneObject.TypesToString(MMISceneObject.Types.MSceneObject);
            this.Tool = "";
            this.StationRef = 0;
            this.GroupRef = 0;
        }

        public gltf_MosimExtra(MMISceneObject mmiSceneObject)
        {
            this.TaskEditorID = mmiSceneObject.TaskEditorID;
            this.TaskEditorLocalID = mmiSceneObject.TaskEditorLocalID;
            this.Type = MMISceneObject.TypesToString(mmiSceneObject.Type);
            this.Tool = mmiSceneObject.Tool;
            this.StationRef = mmiSceneObject.StationRef;
            this.GroupRef = mmiSceneObject.GroupRef;
        }

        public ulong TaskEditorLocalID;
        public ulong TaskEditorID;
        public string Type;
        public string Tool;
        public ulong StationRef;
        public ulong GroupRef;
        //add constraints
    }

    [Serializable]
    public class glTF_Node
    {
        public glTF_Node()
        {
            this.name = "";
            this.children = new List<uint>();
            this.mesh = -1;
        }

        public glTF_Node(string nodeName)
        {
            this.name = nodeName;
            this.children = new List<uint>();
            this.mesh = -1;
            /*this.rotation = new float[4];
            this.translation = new float[3];
            this.scale = new float[3];*/
        }

        public bool ShouldSerializemesh()
        {
            return mesh > -1;
        }

        public bool ShouldSerializechildren()
        {
            return children.Count > 0;
        }

        public string name;
        public int mesh;
        public List<uint> children;
        public float[] rotation;
        public float[] scale;
        public float[] translation;
        public gltf_MosimExtra extras;

        public void SetRotation(Quaternion rot)
        {
            if (rot == new Quaternion() && this.rotation == null)
                return; //do not set if the value is default
            this.rotation = new float[4];
            this.rotation[0] = rot.x; //x and z are swaped for coordinate transform
            this.rotation[1] = -rot.y;
            this.rotation[2] = -rot.z;
            this.rotation[3] = rot.w; //minus for coordinate transfrom taking into account that positiv rotation is counterclockwise (oposite to what unity uses)
        }

        public void SetTranslation(Vector3 pos)
        {
            if (pos == new Vector3(0, 0, 0) && this.translation == null)
                return; //do not set if the value is default
            this.translation = new float[3];
            this.translation[0] = -pos.x; //x and z are swaped for coordinate transform 
            this.translation[1] = pos.y;
            this.translation[2] = pos.z;
        }

        public void SetScale(Vector3 scalefactors)
        {
            if (scalefactors == new Vector3(1, 1, 1) && this.scale == null)
                return; //do not set if the value is default
            this.scale = new float[3];
            this.scale[0] = scalefactors.x; //x and z are swaped for coordinate transform 
            this.scale[1] = scalefactors.y;
            this.scale[2] = scalefactors.z;
        }
    }

    public class gltf_Material_MettalicRoughness
    {
        public gltf_Material_MettalicRoughness()
        {
            this.baseColorFactor = new float[4] { 0.8f, 0, 0, 1.0f };
            this.metallicFactor = 0.0f;
        }

        public gltf_Material_MettalicRoughness(float[] baseColor, float metalicValue, float roughnessValue = 0)
        {
            this.baseColorFactor = baseColor;
            this.metallicFactor = metalicValue;
            this.roughnessFactor = roughnessValue;
        }

        public float[] baseColorFactor;
        public float metallicFactor;
        public float roughnessFactor;
    }

    public class gltf_Material
    {
        public gltf_Material(string Name)
        {
            this.pbrMetallicRoughness = new gltf_Material_MettalicRoughness();
            this.name = Name;
        }

        public gltf_Material(string Name, float[] baseColor, float metalicValue, float roughnessValue, string alphamode)
        {
            this.pbrMetallicRoughness = new gltf_Material_MettalicRoughness(baseColor, metalicValue, roughnessValue);
            this.name = Name;
            this.alphaMode = alphamode;
        }

        public const string alphaModeOpaque = "OPAQUE";
        public const string alphaModeTransparent = "BLEND";
        public gltf_Material_MettalicRoughness pbrMetallicRoughness;
        public string name;
        public string alphaMode;

        public bool ShouldSerializealphaMode()
        {
            return alphaMode != alphaModeOpaque;
        }
    }

    public class gltf_Attributes
    {
        public gltf_Attributes(UInt32 normalsAcessorIndex, UInt32 positionAccessorIndex)
        {
            this.NORMAL = normalsAcessorIndex;
            this.POSITION = positionAccessorIndex;
        }

        public UInt32 NORMAL;
        public UInt32 POSITION;
    }

    public class gltf_Primitives
    {
        public gltf_Primitives(UInt32 normalsAcessorIndex, UInt32 positionAccessorIndex, UInt32 trianglesAccessorIndex, UInt32 materialIndex = 0)
        {
            this.attributes = new gltf_Attributes(normalsAcessorIndex, positionAccessorIndex);
            this.indices = trianglesAccessorIndex;
            this.mode = 4; //traingle set
            this.material = materialIndex;
        }

        public gltf_Attributes attributes;
        public UInt32 indices;
        public UInt32 mode;
        public UInt32 material;
    }

    public class gltf_Mesh
    {
        public gltf_Mesh(UInt32 normalsAcessorIndex, UInt32 positionAccessorIndex, UInt32 trianglesAccessorIndex, UInt32 materialIndex = 0)
        {
            this.primitives = new List<gltf_Primitives>();
            this.primitives.Add(new gltf_Primitives(normalsAcessorIndex, positionAccessorIndex, trianglesAccessorIndex, materialIndex));
        }

        public List<gltf_Primitives> primitives;
        public string name;
    }

    public static class gltf_AccessorTypes
    {
        public const string Scalar = "SCALAR";
        public const string Vector3 = "VEC3";
        public const string Vector2 = "VEC2";
        public const string Vector4 = "VEC4";
        public const string Matrix2 = "MAT2";
        public const string Matrix3 = "MAT3";
        public const string Matrix4 = "MAT4";

        public static UInt32 size(string atype)
        {
            if (atype == "SCALAR") return 1;
            if (atype == "VEC3") return 3;
            if (atype == "VEC2") return 2;
            if (atype == "VEC4") return 4;
            if (atype == "MAT2") return 4;
            if (atype == "MAT3") return 9;
            if (atype == "MAT4") return 16;

            return 0;
        }
    }


    public static class gltf_ComponentTypes
    {
        public const UInt32 Byte = 5120;
        public const UInt32 UnsignedByte = 5121;
        public const UInt32 Short = 5122;
        public const UInt32 UnsignedShort = 5123;
        public const UInt32 UnsignedInt = 5125;
        public const UInt32 Float = 5126;
    }

    public static class gltf_ComponentSizes
    {
        public const UInt32 Byte = 1;
        public const UInt32 UnsignedByte = 1;
        public const UInt32 Short = 2;
        public const UInt32 UnsignedShort = 2;
        public const UInt32 UnsignedInt = 4;
        public const UInt32 Float = 4;

        public static UInt32 size(UInt32 componentType)
        {
            switch (componentType)
            {
                case gltf_ComponentTypes.Byte:
                case gltf_ComponentTypes.UnsignedByte: return 1;
                case gltf_ComponentTypes.Short:
                case gltf_ComponentTypes.UnsignedShort: return 2;
            }
            return 4;
        }
    }

    public class FloatInt32JsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            FloatUInt32 obj = (FloatUInt32)value;
            if (obj.UseInt)
            {
                for (int i = 0; i < obj.Uint32.Length; i++)
                    writer.WriteValue(obj.Uint32[i]);
            }
            else
            {
                for (int i = 0; i < obj.Float.Length; i++)
                    writer.WriteValue(obj.Float[i]);
            }
            writer.WriteEndArray();
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    [JsonConverter(typeof(FloatInt32JsonConverter))]
    public class FloatUInt32
    {
        public FloatUInt32(int length, bool isInt, bool setToMax = false, bool setToMin = false)
        {
            this.Float = isInt ? null : new float[length];
            this.Uint32 = isInt ? new UInt32[length] : null;
            this.UseInt = isInt;
            if (isInt)
                for (int i = 0; i < length; i++)
                    this.Uint32[i] = setToMax ? UInt32.MaxValue : (setToMin ? UInt32.MinValue : 0);
            else
                for (int i = 0; i < length; i++)
                    this.Float[i] = setToMax ? float.MaxValue : (setToMin ? float.MinValue : 0);
        }

        public FloatUInt32(float[] value)
        {
            this.Float = value;
            this.Uint32 = null;
            this.UseInt = false;
        }

        public FloatUInt32(UInt32[] value)
        {
            this.Float = null;
            this.Uint32 = value;
            this.UseInt = true;
        }

        public UInt32[] Uint32;
        public float[] Float;
        public bool UseInt;
    }

    [Serializable]
    public class gltf_Accessors
    {
        public gltf_Accessors(UInt32 bufferViewIndex, UInt32 ComponentType, string AccessorType, gltf_BufferView bufferView, gltf_Buffer buffer)
        {
            UInt32 varsize = gltf_AccessorTypes.size(AccessorType);
            this.bufferView = bufferViewIndex;
            this.componentType = ComponentType;
            this.byteOffset = 0;
            this.count = Convert.ToUInt32(buffer.byteLength / (gltf_ComponentSizes.size(ComponentType) * varsize));
            this.type = AccessorType;
            if (ComponentType == gltf_ComponentTypes.Float)
            {
                if (varsize == 3 || varsize == 1)
                {
                    this.min = new FloatUInt32(buffer.fmin);
                    this.max = new FloatUInt32(buffer.fmax);
                }
                else
                {
                    this.min = new FloatUInt32(Convert.ToInt32(varsize), false, false, true);
                    this.min = new FloatUInt32(Convert.ToInt32(varsize), false, true, false);
                }
            }
            else
            {
                if (varsize == 1)
                {
                    this.min = new FloatUInt32(buffer.uimin);
                    this.max = new FloatUInt32(buffer.uimax);
                }
                else
                {
                    this.min = new FloatUInt32(Convert.ToInt32(varsize), true, false, true);
                    this.min = new FloatUInt32(Convert.ToInt32(varsize), true, true, false);
                }
            }
        }

        public bool IsSameAs(gltf_Accessors otherAccessor)
        {
            return this.bufferView == otherAccessor.bufferView && this.byteOffset == otherAccessor.byteOffset && this.componentType == otherAccessor.componentType &&
                   this.count == otherAccessor.count && this.type == otherAccessor.type; //min and max do not have to be compared as they must be euql for same bufferView if type is same
        }

        public UInt32 bufferView;
        public UInt32 byteOffset;
        public UInt32 componentType;
        public UInt32 count;
        public FloatUInt32 min;
        public FloatUInt32 max;
        public string type;
    }

    public enum gltf_Targets
    {
        ARRAY_BUFFER = 34962, //for Vec3 type
        ELEMENT_ARRAY_BUFFER = 34963 //for scalar
    }

    [Serializable]
    public class gltf_BufferView
    {
        public gltf_BufferView(UInt32 bufferIndex, UInt32 byteOffsetValue, UInt32 byteLengthvValue, UInt32 targetValue)
        {
            this.buffer = bufferIndex;
            this.byteOffset = byteOffsetValue;
            this.byteLength = byteLengthvValue;
            this.byteStride = 0;
            this.target = targetValue;
        }

        public gltf_BufferView(UInt32 bufferIndex, UInt32 byteOffsetValue, UInt32 byteLengthvValue, UInt32 targetValue, UInt32 byteStrideValue)
        {
            this.buffer = bufferIndex;
            this.byteOffset = byteOffsetValue;
            this.byteLength = byteLengthvValue;
            this.byteStride = byteStrideValue;
            this.target = targetValue;
        }

        public gltf_BufferView(UInt32 bufferIndex, UInt32 byteOffsetValue, UInt32 byteLengthvValue, gltf_Targets targetValue)
        {
            this.buffer = bufferIndex;
            this.byteOffset = byteOffsetValue;
            this.byteLength = byteLengthvValue;
            this.byteStride = 0;
            this.target = (UInt32)targetValue;
        }

        public gltf_BufferView(UInt32 bufferIndex, UInt32 byteOffsetValue, UInt32 byteLengthvValue, gltf_Targets targetValue, UInt32 byteStrideValue)
        {
            this.buffer = bufferIndex;
            this.byteOffset = byteOffsetValue;
            this.byteLength = byteLengthvValue;
            this.byteStride = byteStrideValue;
            this.target = (UInt32)targetValue;
        }

        public bool ShouldSerializebyteStride()
        {
            return byteStride >= 4;
        }

        public bool IsSameAs(gltf_BufferView otherBuffer)
        {
            return this.buffer == otherBuffer.buffer && this.byteLength == otherBuffer.byteLength && this.byteOffset == otherBuffer.byteOffset &&
                   this.byteStride == otherBuffer.byteStride && this.target == otherBuffer.target;
        }

        public UInt32 buffer;
        public UInt32 byteOffset;
        public UInt32 byteLength;
        public UInt32 byteStride;
        public UInt32 target;
    }

    [Serializable]
    public class gltf_Buffer
    {
        public gltf_Buffer()
        {
            this.byteLength = 0;
            this.uri = "";
        }

        public gltf_Buffer(Vector3[] vertices, bool flipVectors = false)
        {
            this.byteLength = Convert.ToUInt32(sizeof(float) * 3 * vertices.Length);
            this.data = new byte[this.byteLength];
            this.uri = "data:application/octet-stream;base64,";
            this.fmin = new float[3];
            this.fmax = new float[3];
            for (int i = 0; i < 3; i++)
            {
                this.fmin[i] = float.MaxValue;
                this.fmax[i] = float.MinValue;
            }


            for (int i = 0; i < vertices.Length; i++)
            {
                if (flipVectors)
                {
                    vertices[i].x = -vertices[i].x;
                    vertices[i].y = -vertices[i].y;
                    vertices[i].z = -vertices[i].z;
                }

                this.fmin[0] = Mathf.Min(this.fmin[0], -vertices[i].x);
                this.fmin[1] = Mathf.Min(this.fmin[1], vertices[i].y);
                this.fmin[2] = Mathf.Min(this.fmin[2], vertices[i].z);
                this.fmax[0] = Mathf.Max(this.fmax[0], -vertices[i].x);
                this.fmax[1] = Mathf.Max(this.fmax[1], vertices[i].y);
                this.fmax[2] = Mathf.Max(this.fmax[2], vertices[i].z);
                var x = BitConverter.GetBytes(-vertices[i].x);
                for (int j = 0; j < x.Length; j++)
                    data[i * sizeof(float) * 3 + j] = x[j];
                x = BitConverter.GetBytes(vertices[i].y);
                for (int j = 0; j < x.Length; j++)
                    data[i * sizeof(float) * 3 + j + sizeof(float)] = x[j];
                x = BitConverter.GetBytes(vertices[i].z);
                for (int j = 0; j < x.Length; j++)
                    data[i * sizeof(float) * 3 + j + sizeof(float) * 2] = x[j];
            }
            this.uri += Convert.ToBase64String(data);
            computeHash();
        }

        public gltf_Buffer(int[] indeces)
        {
            this.byteLength = Convert.ToUInt32(sizeof(UInt32) * indeces.Length);
            this.data = new byte[this.byteLength];
            this.uri = "data:application/octet-stream;base64,";
            this.uimin = new UInt32[1] { UInt32.MaxValue };
            this.uimax = new UInt32[1] { UInt32.MinValue };
            for (int i = 0; i < indeces.Length / 3; i++)
            {
                int swap = indeces[i * 3];
                indeces[i * 3] = indeces[i * 3 + 2];
                indeces[i * 3 + 2] = swap;
            }

            for (int i = 0; i < indeces.Length; i++)
            {
                this.uimin[0] = this.uimin[0] < Convert.ToUInt32(indeces[i]) ? this.uimin[0] : Convert.ToUInt32(indeces[i]);
                this.uimax[0] = this.uimax[0] > Convert.ToUInt32(indeces[i]) ? this.uimax[0] : Convert.ToUInt32(indeces[i]);
                var x = BitConverter.GetBytes(Convert.ToUInt32(indeces[i]));
                for (int j = 0; j < x.Length; j++)
                    data[i * sizeof(UInt32) + j] = x[j];
            }
            this.uri += Convert.ToBase64String(data);
            computeHash();
        }

        private void computeHash()
        {
            var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            this.datahash = string.Concat(sha1.ComputeHash(data).Select(x => x.ToString("X2")));
            sha1.Dispose();
        }

        [JsonIgnore]
        public float[] fmin;
        [JsonIgnore]
        public float[] fmax;
        [JsonIgnore]
        public UInt32[] uimin;
        [JsonIgnore]
        public UInt32[] uimax;
        [JsonIgnore]
        public string datahash;

        [JsonIgnore]
        public byte[] data;

        public ulong byteLength;
        public string uri;
    }

    public enum gltf_ExportFormat
    {
        json,
        glb
    }

    [Serializable]
    public class glTF_JSON
    {
        public glTF_JSON()
        {
            this.asset = new glTF_Asset();
            this.scene = -1;
            this.scenes = new List<glTF_Scene>();
            this.nodes = new List<glTF_Node>();
            this.buffers = new List<gltf_Buffer>();
            this.bufferViews = new List<gltf_BufferView>();
            this.accessors = new List<gltf_Accessors>();
            this.meshes = new List<gltf_Mesh>();
            this.materials = new List<gltf_Material>();
            this.materials.Add(new gltf_Material("Default"));
            this.exportFormat = gltf_ExportFormat.json;
        }

        private gltf_ExportFormat exportFormat;

        public glTF_Asset asset;
        public int scene;
        public List<glTF_Scene> scenes;
        public List<glTF_Node> nodes;
        public List<gltf_BufferView> bufferViews;
        public List<gltf_Buffer> buffers;
        public List<gltf_Accessors> accessors;
        public List<gltf_Mesh> meshes;
        public List<gltf_Material> materials;

        public void Clear()
        {
            this.scenes.Clear();
            this.nodes.Clear();
            this.buffers.Clear();
            this.bufferViews.Clear();
            this.accessors.Clear();
            this.meshes.Clear();
            this.materials.Clear();
            this.materials.Add(new gltf_Material("Default"));
        }
        #region smartAddingOfElements
        public int AddAccessor(gltf_Accessors newAccessor)
        {
            for (int i = 0; i < accessors.Count; i++)
                if (accessors[i].IsSameAs(newAccessor))
                    return i;
            accessors.Add(newAccessor);
            return accessors.Count - 1;
        }

        public int AddBufferView(gltf_BufferView newBufferView)
        {
            for (int i = 0; i < bufferViews.Count; i++)
                if (bufferViews[i].IsSameAs(newBufferView))
                    return i;
            bufferViews.Add(newBufferView);
            return bufferViews.Count - 1;
        }

        public int AddBuffer(gltf_Buffer newBuffer)
        {
            for (int i = 0; i < buffers.Count; i++)
                if (buffers[i].datahash == newBuffer.datahash)
                    return i;
            buffers.Add(newBuffer);
            return buffers.Count - 1;
        }

        public UInt32 AddMaterial(string Name, float[] baseColor, float metallicValue, float roughnessValue = 0, string alphaMode = gltf_Material.alphaModeOpaque)
        {
            for (int i = 0; i < materials.Count; i++)
                if (materials[i].pbrMetallicRoughness.baseColorFactor[0] == baseColor[0] && materials[i].pbrMetallicRoughness.baseColorFactor[1] == baseColor[1] &&
                    materials[i].pbrMetallicRoughness.baseColorFactor[2] == baseColor[2] && materials[i].pbrMetallicRoughness.baseColorFactor[3] == baseColor[3] &&
                    materials[i].pbrMetallicRoughness.metallicFactor == metallicValue && materials[i].pbrMetallicRoughness.roughnessFactor == roughnessValue &&
                    materials[i].alphaMode == alphaMode)
                    return Convert.ToUInt32(i);
            materials.Add(new gltf_Material(Name, baseColor, metallicValue, roughnessValue, alphaMode));
            return Convert.ToUInt32(materials.Count - 1);
        }
        #endregion
    }

    [ExecuteInEditMode]
    public class GLTFExport : MonoBehaviour
    {
        public MMISceneObject ObjectToExport;
        public String ExportPath;
        public String ExportFileName;
        [HideInInspector]
        public glTF_JSON gltf = new glTF_JSON();

        public String ReplaceIllegal(String inputString)
        {
            const String illegal = "\\/?><\"':;}]{[=+*&^%$#@!`~";
            for (int i = 0; i < illegal.Length; i++)
                inputString = inputString.Replace(illegal[i].ToString(), String.Empty);
            return inputString;
        }

        public void ExportglTF(String fname = "")
        {
            if (ObjectToExport != null)
            {
                gltf.Clear();
                gltf.scenes.Add(new glTF_Scene("My scene"));
                gltf.scene = 0;
                gltf.scenes[0].nodes.Add(0);

                var nodes = ObjectToExport.gameObject.GetComponentsInChildren<Transform>();
                for (uint i = 0; i < nodes.Length; i++)
                {
                    glTF_Node newNode = new glTF_Node(nodes[i].name);
                    newNode.SetRotation(nodes[i].localRotation);
                    newNode.SetTranslation(nodes[i].localPosition);
                    newNode.SetScale(nodes[i].localScale);
                    var mmiSceneObject = nodes[i].gameObject.GetComponent<MMISceneObject>(); //case when more than one MMISceneObject is added to a node is not covered
                    if (mmiSceneObject != null)
                        newNode.extras = new gltf_MosimExtra(mmiSceneObject);

                    for (uint j = 0; j < i; j++)
                        if (nodes[i].parent == nodes[j])
                            gltf.nodes[Convert.ToInt32(j)].children.Add(i);

                    var meshes = nodes[i].gameObject.GetComponent<MeshFilter>(); //only one mesh filter per object is supported by Unity
                    if (meshes != null)
                    {
                        int buffIndex = gltf.AddBuffer(new gltf_Buffer(meshes.sharedMesh.vertices));
                        int buffViewIndes = gltf.AddBufferView(new gltf_BufferView(Convert.ToUInt32(buffIndex), 0, Convert.ToUInt32(gltf.buffers[buffIndex].byteLength), gltf_Targets.ARRAY_BUFFER));
                        int verticesAccessorIndex = gltf.AddAccessor(new gltf_Accessors(Convert.ToUInt32(buffViewIndes), gltf_ComponentTypes.Float, gltf_AccessorTypes.Vector3, gltf.bufferViews[buffViewIndes], gltf.buffers[buffIndex]));
                        buffIndex = gltf.AddBuffer(new gltf_Buffer(meshes.sharedMesh.normals, false));
                        buffViewIndes = gltf.AddBufferView(new gltf_BufferView(Convert.ToUInt32(buffIndex), 0, Convert.ToUInt32(gltf.buffers[buffIndex].byteLength), gltf_Targets.ARRAY_BUFFER));
                        int normalsAccessorIndex = gltf.AddAccessor(new gltf_Accessors(Convert.ToUInt32(buffViewIndes), gltf_ComponentTypes.Float, gltf_AccessorTypes.Vector3, gltf.bufferViews[buffViewIndes], gltf.buffers[buffIndex]));
                        buffIndex = gltf.AddBuffer(new gltf_Buffer(meshes.sharedMesh.triangles));
                        buffViewIndes = gltf.AddBufferView(new gltf_BufferView(Convert.ToUInt32(buffIndex), 0, Convert.ToUInt32(gltf.buffers[buffIndex].byteLength), gltf_Targets.ELEMENT_ARRAY_BUFFER));
                        int trianglesAccessorIndex = gltf.AddAccessor(new gltf_Accessors(Convert.ToUInt32(buffViewIndes), gltf_ComponentTypes.UnsignedInt, gltf_AccessorTypes.Scalar, gltf.bufferViews[buffViewIndes], gltf.buffers[buffIndex]));
                        //UInt32 trianglesAccessorIndex = Convert.ToUInt32(gltf.accessors.Count - 1);
                        var mats = nodes[i].gameObject.GetComponent<MeshRenderer>(); //there can be only one material per object in Unity
                        if (mats != null)
                        {
                            string blendMode = gltf_Material.alphaModeOpaque;
                            float metalic = 0;
                            float roughness = 1;
                            for (int q = 0; q < mats.sharedMaterial.shader.GetPropertyCount(); q++)
                            {
                                if (mats.sharedMaterial.shader.GetPropertyDescription(q) == "Metallic")
                                    if (mats.sharedMaterial.HasProperty(mats.sharedMaterial.shader.GetPropertyNameId(q)))
                                        metalic = mats.sharedMaterial.GetFloat("_Metallic"); //mats.sharedMaterial.shader.GetPropertyNameId(q) <- this should work but returns soemthing wrong and it throws errors
                                if (mats.sharedMaterial.shader.GetPropertyDescription(q) == "Smoothness")
                                    if (mats.sharedMaterial.HasProperty(mats.sharedMaterial.shader.GetPropertyNameId(q)))
                                        roughness = 1 - mats.sharedMaterial.GetFloat(mats.sharedMaterial.shader.GetPropertyNameId(q));
                            }

                            if (mats.sharedMaterial.GetInt("_SrcBlend") == (int)UnityEngine.Rendering.BlendMode.One &&
                                mats.sharedMaterial.GetInt("_DstBlend") == (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha) //checking for blending mode settig
                                blendMode = gltf_Material.alphaModeTransparent;

                            var matindex = gltf.AddMaterial(mats.sharedMaterial.name, new float[4] { mats.sharedMaterial.color.r, mats.sharedMaterial.color.g, mats.sharedMaterial.color.b, mats.sharedMaterial.color.a },
                                                            metalic, roughness, blendMode);
                            gltf.meshes.Add(new gltf_Mesh(Convert.ToUInt32(normalsAccessorIndex), Convert.ToUInt32(verticesAccessorIndex), Convert.ToUInt32(trianglesAccessorIndex), matindex));
                        }
                        else
                            gltf.meshes.Add(new gltf_Mesh(Convert.ToUInt32(normalsAccessorIndex), Convert.ToUInt32(verticesAccessorIndex), Convert.ToUInt32(trianglesAccessorIndex))); //add mesh with default material
                        newNode.mesh = gltf.meshes.Count - 1;
                    }

                    gltf.nodes.Add(newNode);
                }
            }

            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Formatting = Formatting.Indented;
            if (ExportPath == "")
                ExportPath = Application.dataPath + "/gltf_exports";
            if (!Directory.Exists(ExportPath))
                Directory.CreateDirectory(ExportPath);
            if (fname != "")
                ExportFileName = fname;
            using (StreamWriter sw = new StreamWriter(ExportPath + "/" + ExportFileName + ".gltf"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, gltf);
            }
        }

        public void GenerateFileName()
        {
            if (ObjectToExport != null)
            ExportFileName = ReplaceIllegal(ObjectToExport.name);
        }

        private void OnEnable()
        {
            if (ExportPath == "")
                ExportPath = Application.dataPath + "/gltf_exports";
            if (ExportFileName == "" && ObjectToExport != null)
                GenerateFileName();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}