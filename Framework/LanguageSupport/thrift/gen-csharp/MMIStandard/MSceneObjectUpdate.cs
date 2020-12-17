/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Thrift;
using Thrift.Collections;
using System.Runtime.Serialization;
using Thrift.Protocol;
using Thrift.Transport;

namespace MMIStandard
{

  #if !SILVERLIGHT
  [Serializable]
  #endif
  public partial class MSceneObjectUpdate : TBase
  {
    private string _Name;
    private MTransformUpdate _Transform;
    private MCollider _Collider;
    private MMesh _Mesh;
    private MPhysicsProperties _PhysicsProperties;
    private List<MMIStandard.MHandPose> _HandPoses;
    private List<MPropertyUpdate> _Properties;
    private List<MAttachment> _Attachments;
    private List<MMIStandard.MConstraint> _Constraints;

    public string ID { get; set; }

    public string Name
    {
      get
      {
        return _Name;
      }
      set
      {
        __isset.Name = true;
        this._Name = value;
      }
    }

    public MTransformUpdate Transform
    {
      get
      {
        return _Transform;
      }
      set
      {
        __isset.Transform = true;
        this._Transform = value;
      }
    }

    public MCollider Collider
    {
      get
      {
        return _Collider;
      }
      set
      {
        __isset.Collider = true;
        this._Collider = value;
      }
    }

    public MMesh Mesh
    {
      get
      {
        return _Mesh;
      }
      set
      {
        __isset.Mesh = true;
        this._Mesh = value;
      }
    }

    public MPhysicsProperties PhysicsProperties
    {
      get
      {
        return _PhysicsProperties;
      }
      set
      {
        __isset.PhysicsProperties = true;
        this._PhysicsProperties = value;
      }
    }

    public List<MMIStandard.MHandPose> HandPoses
    {
      get
      {
        return _HandPoses;
      }
      set
      {
        __isset.HandPoses = true;
        this._HandPoses = value;
      }
    }

    public List<MPropertyUpdate> Properties
    {
      get
      {
        return _Properties;
      }
      set
      {
        __isset.Properties = true;
        this._Properties = value;
      }
    }

    public List<MAttachment> Attachments
    {
      get
      {
        return _Attachments;
      }
      set
      {
        __isset.Attachments = true;
        this._Attachments = value;
      }
    }

    public List<MMIStandard.MConstraint> Constraints
    {
      get
      {
        return _Constraints;
      }
      set
      {
        __isset.Constraints = true;
        this._Constraints = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool Name;
      public bool Transform;
      public bool Collider;
      public bool Mesh;
      public bool PhysicsProperties;
      public bool HandPoses;
      public bool Properties;
      public bool Attachments;
      public bool Constraints;
    }

    public MSceneObjectUpdate() {
    }

    public MSceneObjectUpdate(string ID) : this() {
      this.ID = ID;
    }

    public void Read (TProtocol iprot)
    {
      iprot.IncrementRecursionDepth();
      try
      {
        bool isset_ID = false;
        TField field;
        iprot.ReadStructBegin();
        while (true)
        {
          field = iprot.ReadFieldBegin();
          if (field.Type == TType.Stop) { 
            break;
          }
          switch (field.ID)
          {
            case 1:
              if (field.Type == TType.String) {
                ID = iprot.ReadString();
                isset_ID = true;
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                Name = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 3:
              if (field.Type == TType.Struct) {
                Transform = new MTransformUpdate();
                Transform.Read(iprot);
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 4:
              if (field.Type == TType.Struct) {
                Collider = new MCollider();
                Collider.Read(iprot);
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 5:
              if (field.Type == TType.Struct) {
                Mesh = new MMesh();
                Mesh.Read(iprot);
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 6:
              if (field.Type == TType.Struct) {
                PhysicsProperties = new MPhysicsProperties();
                PhysicsProperties.Read(iprot);
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 7:
              if (field.Type == TType.List) {
                {
                  HandPoses = new List<MMIStandard.MHandPose>();
                  TList _list126 = iprot.ReadListBegin();
                  for( int _i127 = 0; _i127 < _list126.Count; ++_i127)
                  {
                    MMIStandard.MHandPose _elem128;
                    _elem128 = new MMIStandard.MHandPose();
                    _elem128.Read(iprot);
                    HandPoses.Add(_elem128);
                  }
                  iprot.ReadListEnd();
                }
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 8:
              if (field.Type == TType.List) {
                {
                  Properties = new List<MPropertyUpdate>();
                  TList _list129 = iprot.ReadListBegin();
                  for( int _i130 = 0; _i130 < _list129.Count; ++_i130)
                  {
                    MPropertyUpdate _elem131;
                    _elem131 = new MPropertyUpdate();
                    _elem131.Read(iprot);
                    Properties.Add(_elem131);
                  }
                  iprot.ReadListEnd();
                }
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 9:
              if (field.Type == TType.List) {
                {
                  Attachments = new List<MAttachment>();
                  TList _list132 = iprot.ReadListBegin();
                  for( int _i133 = 0; _i133 < _list132.Count; ++_i133)
                  {
                    MAttachment _elem134;
                    _elem134 = new MAttachment();
                    _elem134.Read(iprot);
                    Attachments.Add(_elem134);
                  }
                  iprot.ReadListEnd();
                }
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 10:
              if (field.Type == TType.List) {
                {
                  Constraints = new List<MMIStandard.MConstraint>();
                  TList _list135 = iprot.ReadListBegin();
                  for( int _i136 = 0; _i136 < _list135.Count; ++_i136)
                  {
                    MMIStandard.MConstraint _elem137;
                    _elem137 = new MMIStandard.MConstraint();
                    _elem137.Read(iprot);
                    Constraints.Add(_elem137);
                  }
                  iprot.ReadListEnd();
                }
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            default: 
              TProtocolUtil.Skip(iprot, field.Type);
              break;
          }
          iprot.ReadFieldEnd();
        }
        iprot.ReadStructEnd();
        if (!isset_ID)
          throw new TProtocolException(TProtocolException.INVALID_DATA, "required field ID not set");
      }
      finally
      {
        iprot.DecrementRecursionDepth();
      }
    }

    public void Write(TProtocol oprot) {
      oprot.IncrementRecursionDepth();
      try
      {
        TStruct struc = new TStruct("MSceneObjectUpdate");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (ID == null)
          throw new TProtocolException(TProtocolException.INVALID_DATA, "required field ID not set");
        field.Name = "ID";
        field.Type = TType.String;
        field.ID = 1;
        oprot.WriteFieldBegin(field);
        oprot.WriteString(ID);
        oprot.WriteFieldEnd();
        if (Name != null && __isset.Name) {
          field.Name = "Name";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Name);
          oprot.WriteFieldEnd();
        }
        if (Transform != null && __isset.Transform) {
          field.Name = "Transform";
          field.Type = TType.Struct;
          field.ID = 3;
          oprot.WriteFieldBegin(field);
          Transform.Write(oprot);
          oprot.WriteFieldEnd();
        }
        if (Collider != null && __isset.Collider) {
          field.Name = "Collider";
          field.Type = TType.Struct;
          field.ID = 4;
          oprot.WriteFieldBegin(field);
          Collider.Write(oprot);
          oprot.WriteFieldEnd();
        }
        if (Mesh != null && __isset.Mesh) {
          field.Name = "Mesh";
          field.Type = TType.Struct;
          field.ID = 5;
          oprot.WriteFieldBegin(field);
          Mesh.Write(oprot);
          oprot.WriteFieldEnd();
        }
        if (PhysicsProperties != null && __isset.PhysicsProperties) {
          field.Name = "PhysicsProperties";
          field.Type = TType.Struct;
          field.ID = 6;
          oprot.WriteFieldBegin(field);
          PhysicsProperties.Write(oprot);
          oprot.WriteFieldEnd();
        }
        if (HandPoses != null && __isset.HandPoses) {
          field.Name = "HandPoses";
          field.Type = TType.List;
          field.ID = 7;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.Struct, HandPoses.Count));
            foreach (MMIStandard.MHandPose _iter138 in HandPoses)
            {
              _iter138.Write(oprot);
            }
            oprot.WriteListEnd();
          }
          oprot.WriteFieldEnd();
        }
        if (Properties != null && __isset.Properties) {
          field.Name = "Properties";
          field.Type = TType.List;
          field.ID = 8;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.Struct, Properties.Count));
            foreach (MPropertyUpdate _iter139 in Properties)
            {
              _iter139.Write(oprot);
            }
            oprot.WriteListEnd();
          }
          oprot.WriteFieldEnd();
        }
        if (Attachments != null && __isset.Attachments) {
          field.Name = "Attachments";
          field.Type = TType.List;
          field.ID = 9;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.Struct, Attachments.Count));
            foreach (MAttachment _iter140 in Attachments)
            {
              _iter140.Write(oprot);
            }
            oprot.WriteListEnd();
          }
          oprot.WriteFieldEnd();
        }
        if (Constraints != null && __isset.Constraints) {
          field.Name = "Constraints";
          field.Type = TType.List;
          field.ID = 10;
          oprot.WriteFieldBegin(field);
          {
            oprot.WriteListBegin(new TList(TType.Struct, Constraints.Count));
            foreach (MMIStandard.MConstraint _iter141 in Constraints)
            {
              _iter141.Write(oprot);
            }
            oprot.WriteListEnd();
          }
          oprot.WriteFieldEnd();
        }
        oprot.WriteFieldStop();
        oprot.WriteStructEnd();
      }
      finally
      {
        oprot.DecrementRecursionDepth();
      }
    }

    public override string ToString() {
      StringBuilder __sb = new StringBuilder("MSceneObjectUpdate(");
      __sb.Append(", ID: ");
      __sb.Append(ID);
      if (Name != null && __isset.Name) {
        __sb.Append(", Name: ");
        __sb.Append(Name);
      }
      if (Transform != null && __isset.Transform) {
        __sb.Append(", Transform: ");
        __sb.Append(Transform== null ? "<null>" : Transform.ToString());
      }
      if (Collider != null && __isset.Collider) {
        __sb.Append(", Collider: ");
        __sb.Append(Collider== null ? "<null>" : Collider.ToString());
      }
      if (Mesh != null && __isset.Mesh) {
        __sb.Append(", Mesh: ");
        __sb.Append(Mesh== null ? "<null>" : Mesh.ToString());
      }
      if (PhysicsProperties != null && __isset.PhysicsProperties) {
        __sb.Append(", PhysicsProperties: ");
        __sb.Append(PhysicsProperties== null ? "<null>" : PhysicsProperties.ToString());
      }
      if (HandPoses != null && __isset.HandPoses) {
        __sb.Append(", HandPoses: ");
        __sb.Append(HandPoses);
      }
      if (Properties != null && __isset.Properties) {
        __sb.Append(", Properties: ");
        __sb.Append(Properties);
      }
      if (Attachments != null && __isset.Attachments) {
        __sb.Append(", Attachments: ");
        __sb.Append(Attachments);
      }
      if (Constraints != null && __isset.Constraints) {
        __sb.Append(", Constraints: ");
        __sb.Append(Constraints);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}