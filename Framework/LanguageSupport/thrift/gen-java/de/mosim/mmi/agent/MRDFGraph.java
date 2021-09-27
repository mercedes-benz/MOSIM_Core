/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.agent;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2021-09-24")
public class MRDFGraph implements org.apache.thrift.TBase<MRDFGraph, MRDFGraph._Fields>, java.io.Serializable, Cloneable, Comparable<MRDFGraph> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MRDFGraph");

  private static final org.apache.thrift.protocol.TField CONTENT_TYPE_FIELD_DESC = new org.apache.thrift.protocol.TField("ContentType", org.apache.thrift.protocol.TType.STRING, (short)1);
  private static final org.apache.thrift.protocol.TField GRAPH_FIELD_DESC = new org.apache.thrift.protocol.TField("Graph", org.apache.thrift.protocol.TType.STRING, (short)2);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MRDFGraphStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MRDFGraphTupleSchemeFactory();

  public @org.apache.thrift.annotation.Nullable java.lang.String ContentType; // required
  public @org.apache.thrift.annotation.Nullable java.lang.String Graph; // required

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    CONTENT_TYPE((short)1, "ContentType"),
    GRAPH((short)2, "Graph");

    private static final java.util.Map<java.lang.String, _Fields> byName = new java.util.HashMap<java.lang.String, _Fields>();

    static {
      for (_Fields field : java.util.EnumSet.allOf(_Fields.class)) {
        byName.put(field.getFieldName(), field);
      }
    }

    /**
     * Find the _Fields constant that matches fieldId, or null if its not found.
     */
    @org.apache.thrift.annotation.Nullable
    public static _Fields findByThriftId(int fieldId) {
      switch(fieldId) {
        case 1: // CONTENT_TYPE
          return CONTENT_TYPE;
        case 2: // GRAPH
          return GRAPH;
        default:
          return null;
      }
    }

    /**
     * Find the _Fields constant that matches fieldId, throwing an exception
     * if it is not found.
     */
    public static _Fields findByThriftIdOrThrow(int fieldId) {
      _Fields fields = findByThriftId(fieldId);
      if (fields == null) throw new java.lang.IllegalArgumentException("Field " + fieldId + " doesn't exist!");
      return fields;
    }

    /**
     * Find the _Fields constant that matches name, or null if its not found.
     */
    @org.apache.thrift.annotation.Nullable
    public static _Fields findByName(java.lang.String name) {
      return byName.get(name);
    }

    private final short _thriftId;
    private final java.lang.String _fieldName;

    _Fields(short thriftId, java.lang.String fieldName) {
      _thriftId = thriftId;
      _fieldName = fieldName;
    }

    public short getThriftFieldId() {
      return _thriftId;
    }

    public java.lang.String getFieldName() {
      return _fieldName;
    }
  }

  // isset id assignments
  public static final java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> metaDataMap;
  static {
    java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> tmpMap = new java.util.EnumMap<_Fields, org.apache.thrift.meta_data.FieldMetaData>(_Fields.class);
    tmpMap.put(_Fields.CONTENT_TYPE, new org.apache.thrift.meta_data.FieldMetaData("ContentType", org.apache.thrift.TFieldRequirementType.DEFAULT, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING)));
    tmpMap.put(_Fields.GRAPH, new org.apache.thrift.meta_data.FieldMetaData("Graph", org.apache.thrift.TFieldRequirementType.DEFAULT, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING)));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MRDFGraph.class, metaDataMap);
  }

  public MRDFGraph() {
  }

  public MRDFGraph(
    java.lang.String ContentType,
    java.lang.String Graph)
  {
    this();
    this.ContentType = ContentType;
    this.Graph = Graph;
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MRDFGraph(MRDFGraph other) {
    if (other.isSetContentType()) {
      this.ContentType = other.ContentType;
    }
    if (other.isSetGraph()) {
      this.Graph = other.Graph;
    }
  }

  public MRDFGraph deepCopy() {
    return new MRDFGraph(this);
  }

  @Override
  public void clear() {
    this.ContentType = null;
    this.Graph = null;
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.String getContentType() {
    return this.ContentType;
  }

  public MRDFGraph setContentType(@org.apache.thrift.annotation.Nullable java.lang.String ContentType) {
    this.ContentType = ContentType;
    return this;
  }

  public void unsetContentType() {
    this.ContentType = null;
  }

  /** Returns true if field ContentType is set (has been assigned a value) and false otherwise */
  public boolean isSetContentType() {
    return this.ContentType != null;
  }

  public void setContentTypeIsSet(boolean value) {
    if (!value) {
      this.ContentType = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.String getGraph() {
    return this.Graph;
  }

  public MRDFGraph setGraph(@org.apache.thrift.annotation.Nullable java.lang.String Graph) {
    this.Graph = Graph;
    return this;
  }

  public void unsetGraph() {
    this.Graph = null;
  }

  /** Returns true if field Graph is set (has been assigned a value) and false otherwise */
  public boolean isSetGraph() {
    return this.Graph != null;
  }

  public void setGraphIsSet(boolean value) {
    if (!value) {
      this.Graph = null;
    }
  }

  public void setFieldValue(_Fields field, @org.apache.thrift.annotation.Nullable java.lang.Object value) {
    switch (field) {
    case CONTENT_TYPE:
      if (value == null) {
        unsetContentType();
      } else {
        setContentType((java.lang.String)value);
      }
      break;

    case GRAPH:
      if (value == null) {
        unsetGraph();
      } else {
        setGraph((java.lang.String)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case CONTENT_TYPE:
      return getContentType();

    case GRAPH:
      return getGraph();

    }
    throw new java.lang.IllegalStateException();
  }

  /** Returns true if field corresponding to fieldID is set (has been assigned a value) and false otherwise */
  public boolean isSet(_Fields field) {
    if (field == null) {
      throw new java.lang.IllegalArgumentException();
    }

    switch (field) {
    case CONTENT_TYPE:
      return isSetContentType();
    case GRAPH:
      return isSetGraph();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MRDFGraph)
      return this.equals((MRDFGraph)that);
    return false;
  }

  public boolean equals(MRDFGraph that) {
    if (that == null)
      return false;
    if (this == that)
      return true;

    boolean this_present_ContentType = true && this.isSetContentType();
    boolean that_present_ContentType = true && that.isSetContentType();
    if (this_present_ContentType || that_present_ContentType) {
      if (!(this_present_ContentType && that_present_ContentType))
        return false;
      if (!this.ContentType.equals(that.ContentType))
        return false;
    }

    boolean this_present_Graph = true && this.isSetGraph();
    boolean that_present_Graph = true && that.isSetGraph();
    if (this_present_Graph || that_present_Graph) {
      if (!(this_present_Graph && that_present_Graph))
        return false;
      if (!this.Graph.equals(that.Graph))
        return false;
    }

    return true;
  }

  @Override
  public int hashCode() {
    int hashCode = 1;

    hashCode = hashCode * 8191 + ((isSetContentType()) ? 131071 : 524287);
    if (isSetContentType())
      hashCode = hashCode * 8191 + ContentType.hashCode();

    hashCode = hashCode * 8191 + ((isSetGraph()) ? 131071 : 524287);
    if (isSetGraph())
      hashCode = hashCode * 8191 + Graph.hashCode();

    return hashCode;
  }

  @Override
  public int compareTo(MRDFGraph other) {
    if (!getClass().equals(other.getClass())) {
      return getClass().getName().compareTo(other.getClass().getName());
    }

    int lastComparison = 0;

    lastComparison = java.lang.Boolean.valueOf(isSetContentType()).compareTo(other.isSetContentType());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetContentType()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.ContentType, other.ContentType);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetGraph()).compareTo(other.isSetGraph());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetGraph()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Graph, other.Graph);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    return 0;
  }

  @org.apache.thrift.annotation.Nullable
  public _Fields fieldForId(int fieldId) {
    return _Fields.findByThriftId(fieldId);
  }

  public void read(org.apache.thrift.protocol.TProtocol iprot) throws org.apache.thrift.TException {
    scheme(iprot).read(iprot, this);
  }

  public void write(org.apache.thrift.protocol.TProtocol oprot) throws org.apache.thrift.TException {
    scheme(oprot).write(oprot, this);
  }

  @Override
  public java.lang.String toString() {
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MRDFGraph(");
    boolean first = true;

    sb.append("ContentType:");
    if (this.ContentType == null) {
      sb.append("null");
    } else {
      sb.append(this.ContentType);
    }
    first = false;
    if (!first) sb.append(", ");
    sb.append("Graph:");
    if (this.Graph == null) {
      sb.append("null");
    } else {
      sb.append(this.Graph);
    }
    first = false;
    sb.append(")");
    return sb.toString();
  }

  public void validate() throws org.apache.thrift.TException {
    // check for required fields
    // check for sub-struct validity
  }

  private void writeObject(java.io.ObjectOutputStream out) throws java.io.IOException {
    try {
      write(new org.apache.thrift.protocol.TCompactProtocol(new org.apache.thrift.transport.TIOStreamTransport(out)));
    } catch (org.apache.thrift.TException te) {
      throw new java.io.IOException(te);
    }
  }

  private void readObject(java.io.ObjectInputStream in) throws java.io.IOException, java.lang.ClassNotFoundException {
    try {
      read(new org.apache.thrift.protocol.TCompactProtocol(new org.apache.thrift.transport.TIOStreamTransport(in)));
    } catch (org.apache.thrift.TException te) {
      throw new java.io.IOException(te);
    }
  }

  private static class MRDFGraphStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MRDFGraphStandardScheme getScheme() {
      return new MRDFGraphStandardScheme();
    }
  }

  private static class MRDFGraphStandardScheme extends org.apache.thrift.scheme.StandardScheme<MRDFGraph> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MRDFGraph struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TField schemeField;
      iprot.readStructBegin();
      while (true)
      {
        schemeField = iprot.readFieldBegin();
        if (schemeField.type == org.apache.thrift.protocol.TType.STOP) { 
          break;
        }
        switch (schemeField.id) {
          case 1: // CONTENT_TYPE
            if (schemeField.type == org.apache.thrift.protocol.TType.STRING) {
              struct.ContentType = iprot.readString();
              struct.setContentTypeIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 2: // GRAPH
            if (schemeField.type == org.apache.thrift.protocol.TType.STRING) {
              struct.Graph = iprot.readString();
              struct.setGraphIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          default:
            org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
        }
        iprot.readFieldEnd();
      }
      iprot.readStructEnd();

      // check for required fields of primitive type, which can't be checked in the validate method
      struct.validate();
    }

    public void write(org.apache.thrift.protocol.TProtocol oprot, MRDFGraph struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      if (struct.ContentType != null) {
        oprot.writeFieldBegin(CONTENT_TYPE_FIELD_DESC);
        oprot.writeString(struct.ContentType);
        oprot.writeFieldEnd();
      }
      if (struct.Graph != null) {
        oprot.writeFieldBegin(GRAPH_FIELD_DESC);
        oprot.writeString(struct.Graph);
        oprot.writeFieldEnd();
      }
      oprot.writeFieldStop();
      oprot.writeStructEnd();
    }

  }

  private static class MRDFGraphTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MRDFGraphTupleScheme getScheme() {
      return new MRDFGraphTupleScheme();
    }
  }

  private static class MRDFGraphTupleScheme extends org.apache.thrift.scheme.TupleScheme<MRDFGraph> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MRDFGraph struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      java.util.BitSet optionals = new java.util.BitSet();
      if (struct.isSetContentType()) {
        optionals.set(0);
      }
      if (struct.isSetGraph()) {
        optionals.set(1);
      }
      oprot.writeBitSet(optionals, 2);
      if (struct.isSetContentType()) {
        oprot.writeString(struct.ContentType);
      }
      if (struct.isSetGraph()) {
        oprot.writeString(struct.Graph);
      }
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MRDFGraph struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      java.util.BitSet incoming = iprot.readBitSet(2);
      if (incoming.get(0)) {
        struct.ContentType = iprot.readString();
        struct.setContentTypeIsSet(true);
      }
      if (incoming.get(1)) {
        struct.Graph = iprot.readString();
        struct.setGraphIsSet(true);
      }
    }
  }

  private static <S extends org.apache.thrift.scheme.IScheme> S scheme(org.apache.thrift.protocol.TProtocol proto) {
    return (org.apache.thrift.scheme.StandardScheme.class.equals(proto.getScheme()) ? STANDARD_SCHEME_FACTORY : TUPLE_SCHEME_FACTORY).getScheme();
  }
}

