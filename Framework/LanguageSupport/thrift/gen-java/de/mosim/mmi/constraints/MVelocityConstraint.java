/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.constraints;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2020-10-20")
public class MVelocityConstraint implements org.apache.thrift.TBase<MVelocityConstraint, MVelocityConstraint._Fields>, java.io.Serializable, Cloneable, Comparable<MVelocityConstraint> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MVelocityConstraint");

  private static final org.apache.thrift.protocol.TField PARENT_OBJECT_ID_FIELD_DESC = new org.apache.thrift.protocol.TField("ParentObjectID", org.apache.thrift.protocol.TType.STRING, (short)1);
  private static final org.apache.thrift.protocol.TField PARENT_TO_CONSTRAINT_FIELD_DESC = new org.apache.thrift.protocol.TField("ParentToConstraint", org.apache.thrift.protocol.TType.STRUCT, (short)2);
  private static final org.apache.thrift.protocol.TField TRANSLATIONAL_VELOCITY_FIELD_DESC = new org.apache.thrift.protocol.TField("TranslationalVelocity", org.apache.thrift.protocol.TType.STRUCT, (short)3);
  private static final org.apache.thrift.protocol.TField ROTATIONAL_VELOCITY_FIELD_DESC = new org.apache.thrift.protocol.TField("RotationalVelocity", org.apache.thrift.protocol.TType.STRUCT, (short)4);
  private static final org.apache.thrift.protocol.TField WEIGHTING_FACTOR_FIELD_DESC = new org.apache.thrift.protocol.TField("WeightingFactor", org.apache.thrift.protocol.TType.DOUBLE, (short)5);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MVelocityConstraintStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MVelocityConstraintTupleSchemeFactory();

  public @org.apache.thrift.annotation.Nullable java.lang.String ParentObjectID; // required
  public @org.apache.thrift.annotation.Nullable de.mosim.mmi.math.MTransform ParentToConstraint; // optional
  public @org.apache.thrift.annotation.Nullable de.mosim.mmi.math.MVector3 TranslationalVelocity; // optional
  public @org.apache.thrift.annotation.Nullable de.mosim.mmi.math.MVector3 RotationalVelocity; // optional
  public double WeightingFactor; // optional

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    PARENT_OBJECT_ID((short)1, "ParentObjectID"),
    PARENT_TO_CONSTRAINT((short)2, "ParentToConstraint"),
    TRANSLATIONAL_VELOCITY((short)3, "TranslationalVelocity"),
    ROTATIONAL_VELOCITY((short)4, "RotationalVelocity"),
    WEIGHTING_FACTOR((short)5, "WeightingFactor");

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
        case 1: // PARENT_OBJECT_ID
          return PARENT_OBJECT_ID;
        case 2: // PARENT_TO_CONSTRAINT
          return PARENT_TO_CONSTRAINT;
        case 3: // TRANSLATIONAL_VELOCITY
          return TRANSLATIONAL_VELOCITY;
        case 4: // ROTATIONAL_VELOCITY
          return ROTATIONAL_VELOCITY;
        case 5: // WEIGHTING_FACTOR
          return WEIGHTING_FACTOR;
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
  private static final int __WEIGHTINGFACTOR_ISSET_ID = 0;
  private byte __isset_bitfield = 0;
  private static final _Fields optionals[] = {_Fields.PARENT_TO_CONSTRAINT,_Fields.TRANSLATIONAL_VELOCITY,_Fields.ROTATIONAL_VELOCITY,_Fields.WEIGHTING_FACTOR};
  public static final java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> metaDataMap;
  static {
    java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> tmpMap = new java.util.EnumMap<_Fields, org.apache.thrift.meta_data.FieldMetaData>(_Fields.class);
    tmpMap.put(_Fields.PARENT_OBJECT_ID, new org.apache.thrift.meta_data.FieldMetaData("ParentObjectID", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING)));
    tmpMap.put(_Fields.PARENT_TO_CONSTRAINT, new org.apache.thrift.meta_data.FieldMetaData("ParentToConstraint", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, de.mosim.mmi.math.MTransform.class)));
    tmpMap.put(_Fields.TRANSLATIONAL_VELOCITY, new org.apache.thrift.meta_data.FieldMetaData("TranslationalVelocity", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, de.mosim.mmi.math.MVector3.class)));
    tmpMap.put(_Fields.ROTATIONAL_VELOCITY, new org.apache.thrift.meta_data.FieldMetaData("RotationalVelocity", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, de.mosim.mmi.math.MVector3.class)));
    tmpMap.put(_Fields.WEIGHTING_FACTOR, new org.apache.thrift.meta_data.FieldMetaData("WeightingFactor", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.DOUBLE)));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MVelocityConstraint.class, metaDataMap);
  }

  public MVelocityConstraint() {
  }

  public MVelocityConstraint(
    java.lang.String ParentObjectID)
  {
    this();
    this.ParentObjectID = ParentObjectID;
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MVelocityConstraint(MVelocityConstraint other) {
    __isset_bitfield = other.__isset_bitfield;
    if (other.isSetParentObjectID()) {
      this.ParentObjectID = other.ParentObjectID;
    }
    if (other.isSetParentToConstraint()) {
      this.ParentToConstraint = new de.mosim.mmi.math.MTransform(other.ParentToConstraint);
    }
    if (other.isSetTranslationalVelocity()) {
      this.TranslationalVelocity = new de.mosim.mmi.math.MVector3(other.TranslationalVelocity);
    }
    if (other.isSetRotationalVelocity()) {
      this.RotationalVelocity = new de.mosim.mmi.math.MVector3(other.RotationalVelocity);
    }
    this.WeightingFactor = other.WeightingFactor;
  }

  public MVelocityConstraint deepCopy() {
    return new MVelocityConstraint(this);
  }

  @Override
  public void clear() {
    this.ParentObjectID = null;
    this.ParentToConstraint = null;
    this.TranslationalVelocity = null;
    this.RotationalVelocity = null;
    setWeightingFactorIsSet(false);
    this.WeightingFactor = 0.0;
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.String getParentObjectID() {
    return this.ParentObjectID;
  }

  public MVelocityConstraint setParentObjectID(@org.apache.thrift.annotation.Nullable java.lang.String ParentObjectID) {
    this.ParentObjectID = ParentObjectID;
    return this;
  }

  public void unsetParentObjectID() {
    this.ParentObjectID = null;
  }

  /** Returns true if field ParentObjectID is set (has been assigned a value) and false otherwise */
  public boolean isSetParentObjectID() {
    return this.ParentObjectID != null;
  }

  public void setParentObjectIDIsSet(boolean value) {
    if (!value) {
      this.ParentObjectID = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public de.mosim.mmi.math.MTransform getParentToConstraint() {
    return this.ParentToConstraint;
  }

  public MVelocityConstraint setParentToConstraint(@org.apache.thrift.annotation.Nullable de.mosim.mmi.math.MTransform ParentToConstraint) {
    this.ParentToConstraint = ParentToConstraint;
    return this;
  }

  public void unsetParentToConstraint() {
    this.ParentToConstraint = null;
  }

  /** Returns true if field ParentToConstraint is set (has been assigned a value) and false otherwise */
  public boolean isSetParentToConstraint() {
    return this.ParentToConstraint != null;
  }

  public void setParentToConstraintIsSet(boolean value) {
    if (!value) {
      this.ParentToConstraint = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public de.mosim.mmi.math.MVector3 getTranslationalVelocity() {
    return this.TranslationalVelocity;
  }

  public MVelocityConstraint setTranslationalVelocity(@org.apache.thrift.annotation.Nullable de.mosim.mmi.math.MVector3 TranslationalVelocity) {
    this.TranslationalVelocity = TranslationalVelocity;
    return this;
  }

  public void unsetTranslationalVelocity() {
    this.TranslationalVelocity = null;
  }

  /** Returns true if field TranslationalVelocity is set (has been assigned a value) and false otherwise */
  public boolean isSetTranslationalVelocity() {
    return this.TranslationalVelocity != null;
  }

  public void setTranslationalVelocityIsSet(boolean value) {
    if (!value) {
      this.TranslationalVelocity = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public de.mosim.mmi.math.MVector3 getRotationalVelocity() {
    return this.RotationalVelocity;
  }

  public MVelocityConstraint setRotationalVelocity(@org.apache.thrift.annotation.Nullable de.mosim.mmi.math.MVector3 RotationalVelocity) {
    this.RotationalVelocity = RotationalVelocity;
    return this;
  }

  public void unsetRotationalVelocity() {
    this.RotationalVelocity = null;
  }

  /** Returns true if field RotationalVelocity is set (has been assigned a value) and false otherwise */
  public boolean isSetRotationalVelocity() {
    return this.RotationalVelocity != null;
  }

  public void setRotationalVelocityIsSet(boolean value) {
    if (!value) {
      this.RotationalVelocity = null;
    }
  }

  public double getWeightingFactor() {
    return this.WeightingFactor;
  }

  public MVelocityConstraint setWeightingFactor(double WeightingFactor) {
    this.WeightingFactor = WeightingFactor;
    setWeightingFactorIsSet(true);
    return this;
  }

  public void unsetWeightingFactor() {
    __isset_bitfield = org.apache.thrift.EncodingUtils.clearBit(__isset_bitfield, __WEIGHTINGFACTOR_ISSET_ID);
  }

  /** Returns true if field WeightingFactor is set (has been assigned a value) and false otherwise */
  public boolean isSetWeightingFactor() {
    return org.apache.thrift.EncodingUtils.testBit(__isset_bitfield, __WEIGHTINGFACTOR_ISSET_ID);
  }

  public void setWeightingFactorIsSet(boolean value) {
    __isset_bitfield = org.apache.thrift.EncodingUtils.setBit(__isset_bitfield, __WEIGHTINGFACTOR_ISSET_ID, value);
  }

  public void setFieldValue(_Fields field, @org.apache.thrift.annotation.Nullable java.lang.Object value) {
    switch (field) {
    case PARENT_OBJECT_ID:
      if (value == null) {
        unsetParentObjectID();
      } else {
        setParentObjectID((java.lang.String)value);
      }
      break;

    case PARENT_TO_CONSTRAINT:
      if (value == null) {
        unsetParentToConstraint();
      } else {
        setParentToConstraint((de.mosim.mmi.math.MTransform)value);
      }
      break;

    case TRANSLATIONAL_VELOCITY:
      if (value == null) {
        unsetTranslationalVelocity();
      } else {
        setTranslationalVelocity((de.mosim.mmi.math.MVector3)value);
      }
      break;

    case ROTATIONAL_VELOCITY:
      if (value == null) {
        unsetRotationalVelocity();
      } else {
        setRotationalVelocity((de.mosim.mmi.math.MVector3)value);
      }
      break;

    case WEIGHTING_FACTOR:
      if (value == null) {
        unsetWeightingFactor();
      } else {
        setWeightingFactor((java.lang.Double)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case PARENT_OBJECT_ID:
      return getParentObjectID();

    case PARENT_TO_CONSTRAINT:
      return getParentToConstraint();

    case TRANSLATIONAL_VELOCITY:
      return getTranslationalVelocity();

    case ROTATIONAL_VELOCITY:
      return getRotationalVelocity();

    case WEIGHTING_FACTOR:
      return getWeightingFactor();

    }
    throw new java.lang.IllegalStateException();
  }

  /** Returns true if field corresponding to fieldID is set (has been assigned a value) and false otherwise */
  public boolean isSet(_Fields field) {
    if (field == null) {
      throw new java.lang.IllegalArgumentException();
    }

    switch (field) {
    case PARENT_OBJECT_ID:
      return isSetParentObjectID();
    case PARENT_TO_CONSTRAINT:
      return isSetParentToConstraint();
    case TRANSLATIONAL_VELOCITY:
      return isSetTranslationalVelocity();
    case ROTATIONAL_VELOCITY:
      return isSetRotationalVelocity();
    case WEIGHTING_FACTOR:
      return isSetWeightingFactor();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MVelocityConstraint)
      return this.equals((MVelocityConstraint)that);
    return false;
  }

  public boolean equals(MVelocityConstraint that) {
    if (that == null)
      return false;
    if (this == that)
      return true;

    boolean this_present_ParentObjectID = true && this.isSetParentObjectID();
    boolean that_present_ParentObjectID = true && that.isSetParentObjectID();
    if (this_present_ParentObjectID || that_present_ParentObjectID) {
      if (!(this_present_ParentObjectID && that_present_ParentObjectID))
        return false;
      if (!this.ParentObjectID.equals(that.ParentObjectID))
        return false;
    }

    boolean this_present_ParentToConstraint = true && this.isSetParentToConstraint();
    boolean that_present_ParentToConstraint = true && that.isSetParentToConstraint();
    if (this_present_ParentToConstraint || that_present_ParentToConstraint) {
      if (!(this_present_ParentToConstraint && that_present_ParentToConstraint))
        return false;
      if (!this.ParentToConstraint.equals(that.ParentToConstraint))
        return false;
    }

    boolean this_present_TranslationalVelocity = true && this.isSetTranslationalVelocity();
    boolean that_present_TranslationalVelocity = true && that.isSetTranslationalVelocity();
    if (this_present_TranslationalVelocity || that_present_TranslationalVelocity) {
      if (!(this_present_TranslationalVelocity && that_present_TranslationalVelocity))
        return false;
      if (!this.TranslationalVelocity.equals(that.TranslationalVelocity))
        return false;
    }

    boolean this_present_RotationalVelocity = true && this.isSetRotationalVelocity();
    boolean that_present_RotationalVelocity = true && that.isSetRotationalVelocity();
    if (this_present_RotationalVelocity || that_present_RotationalVelocity) {
      if (!(this_present_RotationalVelocity && that_present_RotationalVelocity))
        return false;
      if (!this.RotationalVelocity.equals(that.RotationalVelocity))
        return false;
    }

    boolean this_present_WeightingFactor = true && this.isSetWeightingFactor();
    boolean that_present_WeightingFactor = true && that.isSetWeightingFactor();
    if (this_present_WeightingFactor || that_present_WeightingFactor) {
      if (!(this_present_WeightingFactor && that_present_WeightingFactor))
        return false;
      if (this.WeightingFactor != that.WeightingFactor)
        return false;
    }

    return true;
  }

  @Override
  public int hashCode() {
    int hashCode = 1;

    hashCode = hashCode * 8191 + ((isSetParentObjectID()) ? 131071 : 524287);
    if (isSetParentObjectID())
      hashCode = hashCode * 8191 + ParentObjectID.hashCode();

    hashCode = hashCode * 8191 + ((isSetParentToConstraint()) ? 131071 : 524287);
    if (isSetParentToConstraint())
      hashCode = hashCode * 8191 + ParentToConstraint.hashCode();

    hashCode = hashCode * 8191 + ((isSetTranslationalVelocity()) ? 131071 : 524287);
    if (isSetTranslationalVelocity())
      hashCode = hashCode * 8191 + TranslationalVelocity.hashCode();

    hashCode = hashCode * 8191 + ((isSetRotationalVelocity()) ? 131071 : 524287);
    if (isSetRotationalVelocity())
      hashCode = hashCode * 8191 + RotationalVelocity.hashCode();

    hashCode = hashCode * 8191 + ((isSetWeightingFactor()) ? 131071 : 524287);
    if (isSetWeightingFactor())
      hashCode = hashCode * 8191 + org.apache.thrift.TBaseHelper.hashCode(WeightingFactor);

    return hashCode;
  }

  @Override
  public int compareTo(MVelocityConstraint other) {
    if (!getClass().equals(other.getClass())) {
      return getClass().getName().compareTo(other.getClass().getName());
    }

    int lastComparison = 0;

    lastComparison = java.lang.Boolean.valueOf(isSetParentObjectID()).compareTo(other.isSetParentObjectID());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetParentObjectID()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.ParentObjectID, other.ParentObjectID);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetParentToConstraint()).compareTo(other.isSetParentToConstraint());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetParentToConstraint()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.ParentToConstraint, other.ParentToConstraint);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetTranslationalVelocity()).compareTo(other.isSetTranslationalVelocity());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetTranslationalVelocity()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.TranslationalVelocity, other.TranslationalVelocity);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetRotationalVelocity()).compareTo(other.isSetRotationalVelocity());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetRotationalVelocity()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.RotationalVelocity, other.RotationalVelocity);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetWeightingFactor()).compareTo(other.isSetWeightingFactor());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetWeightingFactor()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.WeightingFactor, other.WeightingFactor);
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
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MVelocityConstraint(");
    boolean first = true;

    sb.append("ParentObjectID:");
    if (this.ParentObjectID == null) {
      sb.append("null");
    } else {
      sb.append(this.ParentObjectID);
    }
    first = false;
    if (isSetParentToConstraint()) {
      if (!first) sb.append(", ");
      sb.append("ParentToConstraint:");
      if (this.ParentToConstraint == null) {
        sb.append("null");
      } else {
        sb.append(this.ParentToConstraint);
      }
      first = false;
    }
    if (isSetTranslationalVelocity()) {
      if (!first) sb.append(", ");
      sb.append("TranslationalVelocity:");
      if (this.TranslationalVelocity == null) {
        sb.append("null");
      } else {
        sb.append(this.TranslationalVelocity);
      }
      first = false;
    }
    if (isSetRotationalVelocity()) {
      if (!first) sb.append(", ");
      sb.append("RotationalVelocity:");
      if (this.RotationalVelocity == null) {
        sb.append("null");
      } else {
        sb.append(this.RotationalVelocity);
      }
      first = false;
    }
    if (isSetWeightingFactor()) {
      if (!first) sb.append(", ");
      sb.append("WeightingFactor:");
      sb.append(this.WeightingFactor);
      first = false;
    }
    sb.append(")");
    return sb.toString();
  }

  public void validate() throws org.apache.thrift.TException {
    // check for required fields
    if (ParentObjectID == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'ParentObjectID' was not present! Struct: " + toString());
    }
    // check for sub-struct validity
    if (ParentToConstraint != null) {
      ParentToConstraint.validate();
    }
    if (TranslationalVelocity != null) {
      TranslationalVelocity.validate();
    }
    if (RotationalVelocity != null) {
      RotationalVelocity.validate();
    }
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

  private static class MVelocityConstraintStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MVelocityConstraintStandardScheme getScheme() {
      return new MVelocityConstraintStandardScheme();
    }
  }

  private static class MVelocityConstraintStandardScheme extends org.apache.thrift.scheme.StandardScheme<MVelocityConstraint> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MVelocityConstraint struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TField schemeField;
      iprot.readStructBegin();
      while (true)
      {
        schemeField = iprot.readFieldBegin();
        if (schemeField.type == org.apache.thrift.protocol.TType.STOP) { 
          break;
        }
        switch (schemeField.id) {
          case 1: // PARENT_OBJECT_ID
            if (schemeField.type == org.apache.thrift.protocol.TType.STRING) {
              struct.ParentObjectID = iprot.readString();
              struct.setParentObjectIDIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 2: // PARENT_TO_CONSTRAINT
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.ParentToConstraint = new de.mosim.mmi.math.MTransform();
              struct.ParentToConstraint.read(iprot);
              struct.setParentToConstraintIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 3: // TRANSLATIONAL_VELOCITY
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.TranslationalVelocity = new de.mosim.mmi.math.MVector3();
              struct.TranslationalVelocity.read(iprot);
              struct.setTranslationalVelocityIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 4: // ROTATIONAL_VELOCITY
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.RotationalVelocity = new de.mosim.mmi.math.MVector3();
              struct.RotationalVelocity.read(iprot);
              struct.setRotationalVelocityIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 5: // WEIGHTING_FACTOR
            if (schemeField.type == org.apache.thrift.protocol.TType.DOUBLE) {
              struct.WeightingFactor = iprot.readDouble();
              struct.setWeightingFactorIsSet(true);
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

    public void write(org.apache.thrift.protocol.TProtocol oprot, MVelocityConstraint struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      if (struct.ParentObjectID != null) {
        oprot.writeFieldBegin(PARENT_OBJECT_ID_FIELD_DESC);
        oprot.writeString(struct.ParentObjectID);
        oprot.writeFieldEnd();
      }
      if (struct.ParentToConstraint != null) {
        if (struct.isSetParentToConstraint()) {
          oprot.writeFieldBegin(PARENT_TO_CONSTRAINT_FIELD_DESC);
          struct.ParentToConstraint.write(oprot);
          oprot.writeFieldEnd();
        }
      }
      if (struct.TranslationalVelocity != null) {
        if (struct.isSetTranslationalVelocity()) {
          oprot.writeFieldBegin(TRANSLATIONAL_VELOCITY_FIELD_DESC);
          struct.TranslationalVelocity.write(oprot);
          oprot.writeFieldEnd();
        }
      }
      if (struct.RotationalVelocity != null) {
        if (struct.isSetRotationalVelocity()) {
          oprot.writeFieldBegin(ROTATIONAL_VELOCITY_FIELD_DESC);
          struct.RotationalVelocity.write(oprot);
          oprot.writeFieldEnd();
        }
      }
      if (struct.isSetWeightingFactor()) {
        oprot.writeFieldBegin(WEIGHTING_FACTOR_FIELD_DESC);
        oprot.writeDouble(struct.WeightingFactor);
        oprot.writeFieldEnd();
      }
      oprot.writeFieldStop();
      oprot.writeStructEnd();
    }

  }

  private static class MVelocityConstraintTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MVelocityConstraintTupleScheme getScheme() {
      return new MVelocityConstraintTupleScheme();
    }
  }

  private static class MVelocityConstraintTupleScheme extends org.apache.thrift.scheme.TupleScheme<MVelocityConstraint> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MVelocityConstraint struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      oprot.writeString(struct.ParentObjectID);
      java.util.BitSet optionals = new java.util.BitSet();
      if (struct.isSetParentToConstraint()) {
        optionals.set(0);
      }
      if (struct.isSetTranslationalVelocity()) {
        optionals.set(1);
      }
      if (struct.isSetRotationalVelocity()) {
        optionals.set(2);
      }
      if (struct.isSetWeightingFactor()) {
        optionals.set(3);
      }
      oprot.writeBitSet(optionals, 4);
      if (struct.isSetParentToConstraint()) {
        struct.ParentToConstraint.write(oprot);
      }
      if (struct.isSetTranslationalVelocity()) {
        struct.TranslationalVelocity.write(oprot);
      }
      if (struct.isSetRotationalVelocity()) {
        struct.RotationalVelocity.write(oprot);
      }
      if (struct.isSetWeightingFactor()) {
        oprot.writeDouble(struct.WeightingFactor);
      }
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MVelocityConstraint struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      struct.ParentObjectID = iprot.readString();
      struct.setParentObjectIDIsSet(true);
      java.util.BitSet incoming = iprot.readBitSet(4);
      if (incoming.get(0)) {
        struct.ParentToConstraint = new de.mosim.mmi.math.MTransform();
        struct.ParentToConstraint.read(iprot);
        struct.setParentToConstraintIsSet(true);
      }
      if (incoming.get(1)) {
        struct.TranslationalVelocity = new de.mosim.mmi.math.MVector3();
        struct.TranslationalVelocity.read(iprot);
        struct.setTranslationalVelocityIsSet(true);
      }
      if (incoming.get(2)) {
        struct.RotationalVelocity = new de.mosim.mmi.math.MVector3();
        struct.RotationalVelocity.read(iprot);
        struct.setRotationalVelocityIsSet(true);
      }
      if (incoming.get(3)) {
        struct.WeightingFactor = iprot.readDouble();
        struct.setWeightingFactorIsSet(true);
      }
    }
  }

  private static <S extends org.apache.thrift.scheme.IScheme> S scheme(org.apache.thrift.protocol.TProtocol proto) {
    return (org.apache.thrift.scheme.StandardScheme.class.equals(proto.getScheme()) ? STANDARD_SCHEME_FACTORY : TUPLE_SCHEME_FACTORY).getScheme();
  }
}

