/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.scene;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2020-12-08")
public class MSphereColliderProperties implements org.apache.thrift.TBase<MSphereColliderProperties, MSphereColliderProperties._Fields>, java.io.Serializable, Cloneable, Comparable<MSphereColliderProperties> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MSphereColliderProperties");

  private static final org.apache.thrift.protocol.TField RADIUS_FIELD_DESC = new org.apache.thrift.protocol.TField("Radius", org.apache.thrift.protocol.TType.DOUBLE, (short)1);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MSphereColliderPropertiesStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MSphereColliderPropertiesTupleSchemeFactory();

  public double Radius; // required

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    RADIUS((short)1, "Radius");

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
        case 1: // RADIUS
          return RADIUS;
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
  private static final int __RADIUS_ISSET_ID = 0;
  private byte __isset_bitfield = 0;
  public static final java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> metaDataMap;
  static {
    java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> tmpMap = new java.util.EnumMap<_Fields, org.apache.thrift.meta_data.FieldMetaData>(_Fields.class);
    tmpMap.put(_Fields.RADIUS, new org.apache.thrift.meta_data.FieldMetaData("Radius", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.DOUBLE)));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MSphereColliderProperties.class, metaDataMap);
  }

  public MSphereColliderProperties() {
  }

  public MSphereColliderProperties(
    double Radius)
  {
    this();
    this.Radius = Radius;
    setRadiusIsSet(true);
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MSphereColliderProperties(MSphereColliderProperties other) {
    __isset_bitfield = other.__isset_bitfield;
    this.Radius = other.Radius;
  }

  public MSphereColliderProperties deepCopy() {
    return new MSphereColliderProperties(this);
  }

  @Override
  public void clear() {
    setRadiusIsSet(false);
    this.Radius = 0.0;
  }

  public double getRadius() {
    return this.Radius;
  }

  public MSphereColliderProperties setRadius(double Radius) {
    this.Radius = Radius;
    setRadiusIsSet(true);
    return this;
  }

  public void unsetRadius() {
    __isset_bitfield = org.apache.thrift.EncodingUtils.clearBit(__isset_bitfield, __RADIUS_ISSET_ID);
  }

  /** Returns true if field Radius is set (has been assigned a value) and false otherwise */
  public boolean isSetRadius() {
    return org.apache.thrift.EncodingUtils.testBit(__isset_bitfield, __RADIUS_ISSET_ID);
  }

  public void setRadiusIsSet(boolean value) {
    __isset_bitfield = org.apache.thrift.EncodingUtils.setBit(__isset_bitfield, __RADIUS_ISSET_ID, value);
  }

  public void setFieldValue(_Fields field, @org.apache.thrift.annotation.Nullable java.lang.Object value) {
    switch (field) {
    case RADIUS:
      if (value == null) {
        unsetRadius();
      } else {
        setRadius((java.lang.Double)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case RADIUS:
      return getRadius();

    }
    throw new java.lang.IllegalStateException();
  }

  /** Returns true if field corresponding to fieldID is set (has been assigned a value) and false otherwise */
  public boolean isSet(_Fields field) {
    if (field == null) {
      throw new java.lang.IllegalArgumentException();
    }

    switch (field) {
    case RADIUS:
      return isSetRadius();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MSphereColliderProperties)
      return this.equals((MSphereColliderProperties)that);
    return false;
  }

  public boolean equals(MSphereColliderProperties that) {
    if (that == null)
      return false;
    if (this == that)
      return true;

    boolean this_present_Radius = true;
    boolean that_present_Radius = true;
    if (this_present_Radius || that_present_Radius) {
      if (!(this_present_Radius && that_present_Radius))
        return false;
      if (this.Radius != that.Radius)
        return false;
    }

    return true;
  }

  @Override
  public int hashCode() {
    int hashCode = 1;

    hashCode = hashCode * 8191 + org.apache.thrift.TBaseHelper.hashCode(Radius);

    return hashCode;
  }

  @Override
  public int compareTo(MSphereColliderProperties other) {
    if (!getClass().equals(other.getClass())) {
      return getClass().getName().compareTo(other.getClass().getName());
    }

    int lastComparison = 0;

    lastComparison = java.lang.Boolean.valueOf(isSetRadius()).compareTo(other.isSetRadius());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetRadius()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Radius, other.Radius);
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
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MSphereColliderProperties(");
    boolean first = true;

    sb.append("Radius:");
    sb.append(this.Radius);
    first = false;
    sb.append(")");
    return sb.toString();
  }

  public void validate() throws org.apache.thrift.TException {
    // check for required fields
    // alas, we cannot check 'Radius' because it's a primitive and you chose the non-beans generator.
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
      // it doesn't seem like you should have to do this, but java serialization is wacky, and doesn't call the default constructor.
      __isset_bitfield = 0;
      read(new org.apache.thrift.protocol.TCompactProtocol(new org.apache.thrift.transport.TIOStreamTransport(in)));
    } catch (org.apache.thrift.TException te) {
      throw new java.io.IOException(te);
    }
  }

  private static class MSphereColliderPropertiesStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MSphereColliderPropertiesStandardScheme getScheme() {
      return new MSphereColliderPropertiesStandardScheme();
    }
  }

  private static class MSphereColliderPropertiesStandardScheme extends org.apache.thrift.scheme.StandardScheme<MSphereColliderProperties> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MSphereColliderProperties struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TField schemeField;
      iprot.readStructBegin();
      while (true)
      {
        schemeField = iprot.readFieldBegin();
        if (schemeField.type == org.apache.thrift.protocol.TType.STOP) { 
          break;
        }
        switch (schemeField.id) {
          case 1: // RADIUS
            if (schemeField.type == org.apache.thrift.protocol.TType.DOUBLE) {
              struct.Radius = iprot.readDouble();
              struct.setRadiusIsSet(true);
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
      if (!struct.isSetRadius()) {
        throw new org.apache.thrift.protocol.TProtocolException("Required field 'Radius' was not found in serialized data! Struct: " + toString());
      }
      struct.validate();
    }

    public void write(org.apache.thrift.protocol.TProtocol oprot, MSphereColliderProperties struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      oprot.writeFieldBegin(RADIUS_FIELD_DESC);
      oprot.writeDouble(struct.Radius);
      oprot.writeFieldEnd();
      oprot.writeFieldStop();
      oprot.writeStructEnd();
    }

  }

  private static class MSphereColliderPropertiesTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MSphereColliderPropertiesTupleScheme getScheme() {
      return new MSphereColliderPropertiesTupleScheme();
    }
  }

  private static class MSphereColliderPropertiesTupleScheme extends org.apache.thrift.scheme.TupleScheme<MSphereColliderProperties> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MSphereColliderProperties struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      oprot.writeDouble(struct.Radius);
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MSphereColliderProperties struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      struct.Radius = iprot.readDouble();
      struct.setRadiusIsSet(true);
    }
  }

  private static <S extends org.apache.thrift.scheme.IScheme> S scheme(org.apache.thrift.protocol.TProtocol proto) {
    return (org.apache.thrift.scheme.StandardScheme.class.equals(proto.getScheme()) ? STANDARD_SCHEME_FACTORY : TUPLE_SCHEME_FACTORY).getScheme();
  }
}

