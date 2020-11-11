/**
 * Autogenerated by Thrift Compiler (0.12.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package MMIStandard;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.12.0)", date = "2019-10-02")
public class MMeshColliderProperties implements org.apache.thrift.TBase<MMeshColliderProperties, MMeshColliderProperties._Fields>, java.io.Serializable, Cloneable, Comparable<MMeshColliderProperties> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MMeshColliderProperties");

  private static final org.apache.thrift.protocol.TField VERTICES_FIELD_DESC = new org.apache.thrift.protocol.TField("Vertices", org.apache.thrift.protocol.TType.LIST, (short)1);
  private static final org.apache.thrift.protocol.TField TRIANGLES_FIELD_DESC = new org.apache.thrift.protocol.TField("Triangles", org.apache.thrift.protocol.TType.LIST, (short)2);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MMeshColliderPropertiesStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MMeshColliderPropertiesTupleSchemeFactory();

  public @org.apache.thrift.annotation.Nullable java.util.List<MMIStandard.MVector3> Vertices; // required
  public @org.apache.thrift.annotation.Nullable java.util.List<java.lang.Integer> Triangles; // required

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    VERTICES((short)1, "Vertices"),
    TRIANGLES((short)2, "Triangles");

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
        case 1: // VERTICES
          return VERTICES;
        case 2: // TRIANGLES
          return TRIANGLES;
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
    tmpMap.put(_Fields.VERTICES, new org.apache.thrift.meta_data.FieldMetaData("Vertices", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MMIStandard.MVector3.class))));
    tmpMap.put(_Fields.TRIANGLES, new org.apache.thrift.meta_data.FieldMetaData("Triangles", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.I32))));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MMeshColliderProperties.class, metaDataMap);
  }

  public MMeshColliderProperties() {
  }

  public MMeshColliderProperties(
    java.util.List<MMIStandard.MVector3> Vertices,
    java.util.List<java.lang.Integer> Triangles)
  {
    this();
    this.Vertices = Vertices;
    this.Triangles = Triangles;
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MMeshColliderProperties(MMeshColliderProperties other) {
    if (other.isSetVertices()) {
      java.util.List<MMIStandard.MVector3> __this__Vertices = new java.util.ArrayList<MMIStandard.MVector3>(other.Vertices.size());
      for (MMIStandard.MVector3 other_element : other.Vertices) {
        __this__Vertices.add(new MMIStandard.MVector3(other_element));
      }
      this.Vertices = __this__Vertices;
    }
    if (other.isSetTriangles()) {
      java.util.List<java.lang.Integer> __this__Triangles = new java.util.ArrayList<java.lang.Integer>(other.Triangles);
      this.Triangles = __this__Triangles;
    }
  }

  public MMeshColliderProperties deepCopy() {
    return new MMeshColliderProperties(this);
  }

  @Override
  public void clear() {
    this.Vertices = null;
    this.Triangles = null;
  }

  public int getVerticesSize() {
    return (this.Vertices == null) ? 0 : this.Vertices.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<MMIStandard.MVector3> getVerticesIterator() {
    return (this.Vertices == null) ? null : this.Vertices.iterator();
  }

  public void addToVertices(MMIStandard.MVector3 elem) {
    if (this.Vertices == null) {
      this.Vertices = new java.util.ArrayList<MMIStandard.MVector3>();
    }
    this.Vertices.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<MMIStandard.MVector3> getVertices() {
    return this.Vertices;
  }

  public MMeshColliderProperties setVertices(@org.apache.thrift.annotation.Nullable java.util.List<MMIStandard.MVector3> Vertices) {
    this.Vertices = Vertices;
    return this;
  }

  public void unsetVertices() {
    this.Vertices = null;
  }

  /** Returns true if field Vertices is set (has been assigned a value) and false otherwise */
  public boolean isSetVertices() {
    return this.Vertices != null;
  }

  public void setVerticesIsSet(boolean value) {
    if (!value) {
      this.Vertices = null;
    }
  }

  public int getTrianglesSize() {
    return (this.Triangles == null) ? 0 : this.Triangles.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<java.lang.Integer> getTrianglesIterator() {
    return (this.Triangles == null) ? null : this.Triangles.iterator();
  }

  public void addToTriangles(int elem) {
    if (this.Triangles == null) {
      this.Triangles = new java.util.ArrayList<java.lang.Integer>();
    }
    this.Triangles.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<java.lang.Integer> getTriangles() {
    return this.Triangles;
  }

  public MMeshColliderProperties setTriangles(@org.apache.thrift.annotation.Nullable java.util.List<java.lang.Integer> Triangles) {
    this.Triangles = Triangles;
    return this;
  }

  public void unsetTriangles() {
    this.Triangles = null;
  }

  /** Returns true if field Triangles is set (has been assigned a value) and false otherwise */
  public boolean isSetTriangles() {
    return this.Triangles != null;
  }

  public void setTrianglesIsSet(boolean value) {
    if (!value) {
      this.Triangles = null;
    }
  }

  public void setFieldValue(_Fields field, @org.apache.thrift.annotation.Nullable java.lang.Object value) {
    switch (field) {
    case VERTICES:
      if (value == null) {
        unsetVertices();
      } else {
        setVertices((java.util.List<MMIStandard.MVector3>)value);
      }
      break;

    case TRIANGLES:
      if (value == null) {
        unsetTriangles();
      } else {
        setTriangles((java.util.List<java.lang.Integer>)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case VERTICES:
      return getVertices();

    case TRIANGLES:
      return getTriangles();

    }
    throw new java.lang.IllegalStateException();
  }

  /** Returns true if field corresponding to fieldID is set (has been assigned a value) and false otherwise */
  public boolean isSet(_Fields field) {
    if (field == null) {
      throw new java.lang.IllegalArgumentException();
    }

    switch (field) {
    case VERTICES:
      return isSetVertices();
    case TRIANGLES:
      return isSetTriangles();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MMeshColliderProperties)
      return this.equals((MMeshColliderProperties)that);
    return false;
  }

  public boolean equals(MMeshColliderProperties that) {
    if (that == null)
      return false;
    if (this == that)
      return true;

    boolean this_present_Vertices = true && this.isSetVertices();
    boolean that_present_Vertices = true && that.isSetVertices();
    if (this_present_Vertices || that_present_Vertices) {
      if (!(this_present_Vertices && that_present_Vertices))
        return false;
      if (!this.Vertices.equals(that.Vertices))
        return false;
    }

    boolean this_present_Triangles = true && this.isSetTriangles();
    boolean that_present_Triangles = true && that.isSetTriangles();
    if (this_present_Triangles || that_present_Triangles) {
      if (!(this_present_Triangles && that_present_Triangles))
        return false;
      if (!this.Triangles.equals(that.Triangles))
        return false;
    }

    return true;
  }

  @Override
  public int hashCode() {
    int hashCode = 1;

    hashCode = hashCode * 8191 + ((isSetVertices()) ? 131071 : 524287);
    if (isSetVertices())
      hashCode = hashCode * 8191 + Vertices.hashCode();

    hashCode = hashCode * 8191 + ((isSetTriangles()) ? 131071 : 524287);
    if (isSetTriangles())
      hashCode = hashCode * 8191 + Triangles.hashCode();

    return hashCode;
  }

  @Override
  public int compareTo(MMeshColliderProperties other) {
    if (!getClass().equals(other.getClass())) {
      return getClass().getName().compareTo(other.getClass().getName());
    }

    int lastComparison = 0;

    lastComparison = java.lang.Boolean.valueOf(isSetVertices()).compareTo(other.isSetVertices());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetVertices()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Vertices, other.Vertices);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetTriangles()).compareTo(other.isSetTriangles());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetTriangles()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Triangles, other.Triangles);
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
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MMeshColliderProperties(");
    boolean first = true;

    sb.append("Vertices:");
    if (this.Vertices == null) {
      sb.append("null");
    } else {
      sb.append(this.Vertices);
    }
    first = false;
    if (!first) sb.append(", ");
    sb.append("Triangles:");
    if (this.Triangles == null) {
      sb.append("null");
    } else {
      sb.append(this.Triangles);
    }
    first = false;
    sb.append(")");
    return sb.toString();
  }

  public void validate() throws org.apache.thrift.TException {
    // check for required fields
    if (Vertices == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'Vertices' was not present! Struct: " + toString());
    }
    if (Triangles == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'Triangles' was not present! Struct: " + toString());
    }
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

  private static class MMeshColliderPropertiesStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MMeshColliderPropertiesStandardScheme getScheme() {
      return new MMeshColliderPropertiesStandardScheme();
    }
  }

  private static class MMeshColliderPropertiesStandardScheme extends org.apache.thrift.scheme.StandardScheme<MMeshColliderProperties> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MMeshColliderProperties struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TField schemeField;
      iprot.readStructBegin();
      while (true)
      {
        schemeField = iprot.readFieldBegin();
        if (schemeField.type == org.apache.thrift.protocol.TType.STOP) { 
          break;
        }
        switch (schemeField.id) {
          case 1: // VERTICES
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list56 = iprot.readListBegin();
                struct.Vertices = new java.util.ArrayList<MMIStandard.MVector3>(_list56.size);
                @org.apache.thrift.annotation.Nullable MMIStandard.MVector3 _elem57;
                for (int _i58 = 0; _i58 < _list56.size; ++_i58)
                {
                  _elem57 = new MMIStandard.MVector3();
                  _elem57.read(iprot);
                  struct.Vertices.add(_elem57);
                }
                iprot.readListEnd();
              }
              struct.setVerticesIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 2: // TRIANGLES
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list59 = iprot.readListBegin();
                struct.Triangles = new java.util.ArrayList<java.lang.Integer>(_list59.size);
                int _elem60;
                for (int _i61 = 0; _i61 < _list59.size; ++_i61)
                {
                  _elem60 = iprot.readI32();
                  struct.Triangles.add(_elem60);
                }
                iprot.readListEnd();
              }
              struct.setTrianglesIsSet(true);
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

    public void write(org.apache.thrift.protocol.TProtocol oprot, MMeshColliderProperties struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      if (struct.Vertices != null) {
        oprot.writeFieldBegin(VERTICES_FIELD_DESC);
        {
          oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, struct.Vertices.size()));
          for (MMIStandard.MVector3 _iter62 : struct.Vertices)
          {
            _iter62.write(oprot);
          }
          oprot.writeListEnd();
        }
        oprot.writeFieldEnd();
      }
      if (struct.Triangles != null) {
        oprot.writeFieldBegin(TRIANGLES_FIELD_DESC);
        {
          oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.I32, struct.Triangles.size()));
          for (int _iter63 : struct.Triangles)
          {
            oprot.writeI32(_iter63);
          }
          oprot.writeListEnd();
        }
        oprot.writeFieldEnd();
      }
      oprot.writeFieldStop();
      oprot.writeStructEnd();
    }

  }

  private static class MMeshColliderPropertiesTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MMeshColliderPropertiesTupleScheme getScheme() {
      return new MMeshColliderPropertiesTupleScheme();
    }
  }

  private static class MMeshColliderPropertiesTupleScheme extends org.apache.thrift.scheme.TupleScheme<MMeshColliderProperties> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MMeshColliderProperties struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      {
        oprot.writeI32(struct.Vertices.size());
        for (MMIStandard.MVector3 _iter64 : struct.Vertices)
        {
          _iter64.write(oprot);
        }
      }
      {
        oprot.writeI32(struct.Triangles.size());
        for (int _iter65 : struct.Triangles)
        {
          oprot.writeI32(_iter65);
        }
      }
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MMeshColliderProperties struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      {
        org.apache.thrift.protocol.TList _list66 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, iprot.readI32());
        struct.Vertices = new java.util.ArrayList<MMIStandard.MVector3>(_list66.size);
        @org.apache.thrift.annotation.Nullable MMIStandard.MVector3 _elem67;
        for (int _i68 = 0; _i68 < _list66.size; ++_i68)
        {
          _elem67 = new MMIStandard.MVector3();
          _elem67.read(iprot);
          struct.Vertices.add(_elem67);
        }
      }
      struct.setVerticesIsSet(true);
      {
        org.apache.thrift.protocol.TList _list69 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.I32, iprot.readI32());
        struct.Triangles = new java.util.ArrayList<java.lang.Integer>(_list69.size);
        int _elem70;
        for (int _i71 = 0; _i71 < _list69.size; ++_i71)
        {
          _elem70 = iprot.readI32();
          struct.Triangles.add(_elem70);
        }
      }
      struct.setTrianglesIsSet(true);
    }
  }

  private static <S extends org.apache.thrift.scheme.IScheme> S scheme(org.apache.thrift.protocol.TProtocol proto) {
    return (org.apache.thrift.scheme.StandardScheme.class.equals(proto.getScheme()) ? STANDARD_SCHEME_FACTORY : TUPLE_SCHEME_FACTORY).getScheme();
  }
}

