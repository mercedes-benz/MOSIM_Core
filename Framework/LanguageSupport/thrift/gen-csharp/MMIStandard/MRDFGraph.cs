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
  public partial class MRDFGraph : TBase
  {
    private string _ContentType;
    private string _Graph;

    public string ContentType
    {
      get
      {
        return _ContentType;
      }
      set
      {
        __isset.ContentType = true;
        this._ContentType = value;
      }
    }

    public string Graph
    {
      get
      {
        return _Graph;
      }
      set
      {
        __isset.Graph = true;
        this._Graph = value;
      }
    }


    public Isset __isset;
    #if !SILVERLIGHT
    [Serializable]
    #endif
    public struct Isset {
      public bool ContentType;
      public bool Graph;
    }

    public MRDFGraph() {
    }

    public void Read (TProtocol iprot)
    {
      iprot.IncrementRecursionDepth();
      try
      {
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
                ContentType = iprot.ReadString();
              } else { 
                TProtocolUtil.Skip(iprot, field.Type);
              }
              break;
            case 2:
              if (field.Type == TType.String) {
                Graph = iprot.ReadString();
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
        TStruct struc = new TStruct("MRDFGraph");
        oprot.WriteStructBegin(struc);
        TField field = new TField();
        if (ContentType != null && __isset.ContentType) {
          field.Name = "ContentType";
          field.Type = TType.String;
          field.ID = 1;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(ContentType);
          oprot.WriteFieldEnd();
        }
        if (Graph != null && __isset.Graph) {
          field.Name = "Graph";
          field.Type = TType.String;
          field.ID = 2;
          oprot.WriteFieldBegin(field);
          oprot.WriteString(Graph);
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
      StringBuilder __sb = new StringBuilder("MRDFGraph(");
      bool __first = true;
      if (ContentType != null && __isset.ContentType) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("ContentType: ");
        __sb.Append(ContentType);
      }
      if (Graph != null && __isset.Graph) {
        if(!__first) { __sb.Append(", "); }
        __first = false;
        __sb.Append("Graph: ");
        __sb.Append(Graph);
      }
      __sb.Append(")");
      return __sb.ToString();
    }

  }

}