/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.avatar;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2021-09-24")
public class MAvatarPostureValues implements org.apache.thrift.TBase<MAvatarPostureValues, MAvatarPostureValues._Fields>, java.io.Serializable, Cloneable, Comparable<MAvatarPostureValues> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MAvatarPostureValues");

  private static final org.apache.thrift.protocol.TField AVATAR_ID_FIELD_DESC = new org.apache.thrift.protocol.TField("AvatarID", org.apache.thrift.protocol.TType.STRING, (short)1);
  private static final org.apache.thrift.protocol.TField POSTURE_DATA_FIELD_DESC = new org.apache.thrift.protocol.TField("PostureData", org.apache.thrift.protocol.TType.LIST, (short)2);
  private static final org.apache.thrift.protocol.TField PARTIAL_JOINT_LIST_FIELD_DESC = new org.apache.thrift.protocol.TField("PartialJointList", org.apache.thrift.protocol.TType.LIST, (short)3);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MAvatarPostureValuesStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MAvatarPostureValuesTupleSchemeFactory();

  public @org.apache.thrift.annotation.Nullable java.lang.String AvatarID; // required
  public @org.apache.thrift.annotation.Nullable java.util.List<java.lang.Double> PostureData; // required
  public @org.apache.thrift.annotation.Nullable java.util.List<MJointType> PartialJointList; // optional

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    AVATAR_ID((short)1, "AvatarID"),
    POSTURE_DATA((short)2, "PostureData"),
    PARTIAL_JOINT_LIST((short)3, "PartialJointList");

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
        case 1: // AVATAR_ID
          return AVATAR_ID;
        case 2: // POSTURE_DATA
          return POSTURE_DATA;
        case 3: // PARTIAL_JOINT_LIST
          return PARTIAL_JOINT_LIST;
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
  private static final _Fields optionals[] = {_Fields.PARTIAL_JOINT_LIST};
  public static final java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> metaDataMap;
  static {
    java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> tmpMap = new java.util.EnumMap<_Fields, org.apache.thrift.meta_data.FieldMetaData>(_Fields.class);
    tmpMap.put(_Fields.AVATAR_ID, new org.apache.thrift.meta_data.FieldMetaData("AvatarID", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING)));
    tmpMap.put(_Fields.POSTURE_DATA, new org.apache.thrift.meta_data.FieldMetaData("PostureData", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.DOUBLE))));
    tmpMap.put(_Fields.PARTIAL_JOINT_LIST, new org.apache.thrift.meta_data.FieldMetaData("PartialJointList", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.EnumMetaData(org.apache.thrift.protocol.TType.ENUM, MJointType.class))));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MAvatarPostureValues.class, metaDataMap);
  }

  public MAvatarPostureValues() {
  }

  public MAvatarPostureValues(
    java.lang.String AvatarID,
    java.util.List<java.lang.Double> PostureData)
  {
    this();
    this.AvatarID = AvatarID;
    this.PostureData = PostureData;
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MAvatarPostureValues(MAvatarPostureValues other) {
    if (other.isSetAvatarID()) {
      this.AvatarID = other.AvatarID;
    }
    if (other.isSetPostureData()) {
      java.util.List<java.lang.Double> __this__PostureData = new java.util.ArrayList<java.lang.Double>(other.PostureData);
      this.PostureData = __this__PostureData;
    }
    if (other.isSetPartialJointList()) {
      java.util.List<MJointType> __this__PartialJointList = new java.util.ArrayList<MJointType>(other.PartialJointList.size());
      for (MJointType other_element : other.PartialJointList) {
        __this__PartialJointList.add(other_element);
      }
      this.PartialJointList = __this__PartialJointList;
    }
  }

  public MAvatarPostureValues deepCopy() {
    return new MAvatarPostureValues(this);
  }

  @Override
  public void clear() {
    this.AvatarID = null;
    this.PostureData = null;
    this.PartialJointList = null;
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.String getAvatarID() {
    return this.AvatarID;
  }

  public MAvatarPostureValues setAvatarID(@org.apache.thrift.annotation.Nullable java.lang.String AvatarID) {
    this.AvatarID = AvatarID;
    return this;
  }

  public void unsetAvatarID() {
    this.AvatarID = null;
  }

  /** Returns true if field AvatarID is set (has been assigned a value) and false otherwise */
  public boolean isSetAvatarID() {
    return this.AvatarID != null;
  }

  public void setAvatarIDIsSet(boolean value) {
    if (!value) {
      this.AvatarID = null;
    }
  }

  public int getPostureDataSize() {
    return (this.PostureData == null) ? 0 : this.PostureData.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<java.lang.Double> getPostureDataIterator() {
    return (this.PostureData == null) ? null : this.PostureData.iterator();
  }

  public void addToPostureData(double elem) {
    if (this.PostureData == null) {
      this.PostureData = new java.util.ArrayList<java.lang.Double>();
    }
    this.PostureData.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<java.lang.Double> getPostureData() {
    return this.PostureData;
  }

  public MAvatarPostureValues setPostureData(@org.apache.thrift.annotation.Nullable java.util.List<java.lang.Double> PostureData) {
    this.PostureData = PostureData;
    return this;
  }

  public void unsetPostureData() {
    this.PostureData = null;
  }

  /** Returns true if field PostureData is set (has been assigned a value) and false otherwise */
  public boolean isSetPostureData() {
    return this.PostureData != null;
  }

  public void setPostureDataIsSet(boolean value) {
    if (!value) {
      this.PostureData = null;
    }
  }

  public int getPartialJointListSize() {
    return (this.PartialJointList == null) ? 0 : this.PartialJointList.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<MJointType> getPartialJointListIterator() {
    return (this.PartialJointList == null) ? null : this.PartialJointList.iterator();
  }

  public void addToPartialJointList(MJointType elem) {
    if (this.PartialJointList == null) {
      this.PartialJointList = new java.util.ArrayList<MJointType>();
    }
    this.PartialJointList.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<MJointType> getPartialJointList() {
    return this.PartialJointList;
  }

  public MAvatarPostureValues setPartialJointList(@org.apache.thrift.annotation.Nullable java.util.List<MJointType> PartialJointList) {
    this.PartialJointList = PartialJointList;
    return this;
  }

  public void unsetPartialJointList() {
    this.PartialJointList = null;
  }

  /** Returns true if field PartialJointList is set (has been assigned a value) and false otherwise */
  public boolean isSetPartialJointList() {
    return this.PartialJointList != null;
  }

  public void setPartialJointListIsSet(boolean value) {
    if (!value) {
      this.PartialJointList = null;
    }
  }

  public void setFieldValue(_Fields field, @org.apache.thrift.annotation.Nullable java.lang.Object value) {
    switch (field) {
    case AVATAR_ID:
      if (value == null) {
        unsetAvatarID();
      } else {
        setAvatarID((java.lang.String)value);
      }
      break;

    case POSTURE_DATA:
      if (value == null) {
        unsetPostureData();
      } else {
        setPostureData((java.util.List<java.lang.Double>)value);
      }
      break;

    case PARTIAL_JOINT_LIST:
      if (value == null) {
        unsetPartialJointList();
      } else {
        setPartialJointList((java.util.List<MJointType>)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case AVATAR_ID:
      return getAvatarID();

    case POSTURE_DATA:
      return getPostureData();

    case PARTIAL_JOINT_LIST:
      return getPartialJointList();

    }
    throw new java.lang.IllegalStateException();
  }

  /** Returns true if field corresponding to fieldID is set (has been assigned a value) and false otherwise */
  public boolean isSet(_Fields field) {
    if (field == null) {
      throw new java.lang.IllegalArgumentException();
    }

    switch (field) {
    case AVATAR_ID:
      return isSetAvatarID();
    case POSTURE_DATA:
      return isSetPostureData();
    case PARTIAL_JOINT_LIST:
      return isSetPartialJointList();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MAvatarPostureValues)
      return this.equals((MAvatarPostureValues)that);
    return false;
  }

  public boolean equals(MAvatarPostureValues that) {
    if (that == null)
      return false;
    if (this == that)
      return true;

    boolean this_present_AvatarID = true && this.isSetAvatarID();
    boolean that_present_AvatarID = true && that.isSetAvatarID();
    if (this_present_AvatarID || that_present_AvatarID) {
      if (!(this_present_AvatarID && that_present_AvatarID))
        return false;
      if (!this.AvatarID.equals(that.AvatarID))
        return false;
    }

    boolean this_present_PostureData = true && this.isSetPostureData();
    boolean that_present_PostureData = true && that.isSetPostureData();
    if (this_present_PostureData || that_present_PostureData) {
      if (!(this_present_PostureData && that_present_PostureData))
        return false;
      if (!this.PostureData.equals(that.PostureData))
        return false;
    }

    boolean this_present_PartialJointList = true && this.isSetPartialJointList();
    boolean that_present_PartialJointList = true && that.isSetPartialJointList();
    if (this_present_PartialJointList || that_present_PartialJointList) {
      if (!(this_present_PartialJointList && that_present_PartialJointList))
        return false;
      if (!this.PartialJointList.equals(that.PartialJointList))
        return false;
    }

    return true;
  }

  @Override
  public int hashCode() {
    int hashCode = 1;

    hashCode = hashCode * 8191 + ((isSetAvatarID()) ? 131071 : 524287);
    if (isSetAvatarID())
      hashCode = hashCode * 8191 + AvatarID.hashCode();

    hashCode = hashCode * 8191 + ((isSetPostureData()) ? 131071 : 524287);
    if (isSetPostureData())
      hashCode = hashCode * 8191 + PostureData.hashCode();

    hashCode = hashCode * 8191 + ((isSetPartialJointList()) ? 131071 : 524287);
    if (isSetPartialJointList())
      hashCode = hashCode * 8191 + PartialJointList.hashCode();

    return hashCode;
  }

  @Override
  public int compareTo(MAvatarPostureValues other) {
    if (!getClass().equals(other.getClass())) {
      return getClass().getName().compareTo(other.getClass().getName());
    }

    int lastComparison = 0;

    lastComparison = java.lang.Boolean.valueOf(isSetAvatarID()).compareTo(other.isSetAvatarID());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetAvatarID()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.AvatarID, other.AvatarID);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetPostureData()).compareTo(other.isSetPostureData());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetPostureData()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.PostureData, other.PostureData);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetPartialJointList()).compareTo(other.isSetPartialJointList());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetPartialJointList()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.PartialJointList, other.PartialJointList);
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
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MAvatarPostureValues(");
    boolean first = true;

    sb.append("AvatarID:");
    if (this.AvatarID == null) {
      sb.append("null");
    } else {
      sb.append(this.AvatarID);
    }
    first = false;
    if (!first) sb.append(", ");
    sb.append("PostureData:");
    if (this.PostureData == null) {
      sb.append("null");
    } else {
      sb.append(this.PostureData);
    }
    first = false;
    if (isSetPartialJointList()) {
      if (!first) sb.append(", ");
      sb.append("PartialJointList:");
      if (this.PartialJointList == null) {
        sb.append("null");
      } else {
        sb.append(this.PartialJointList);
      }
      first = false;
    }
    sb.append(")");
    return sb.toString();
  }

  public void validate() throws org.apache.thrift.TException {
    // check for required fields
    if (AvatarID == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'AvatarID' was not present! Struct: " + toString());
    }
    if (PostureData == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'PostureData' was not present! Struct: " + toString());
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

  private static class MAvatarPostureValuesStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MAvatarPostureValuesStandardScheme getScheme() {
      return new MAvatarPostureValuesStandardScheme();
    }
  }

  private static class MAvatarPostureValuesStandardScheme extends org.apache.thrift.scheme.StandardScheme<MAvatarPostureValues> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MAvatarPostureValues struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TField schemeField;
      iprot.readStructBegin();
      while (true)
      {
        schemeField = iprot.readFieldBegin();
        if (schemeField.type == org.apache.thrift.protocol.TType.STOP) { 
          break;
        }
        switch (schemeField.id) {
          case 1: // AVATAR_ID
            if (schemeField.type == org.apache.thrift.protocol.TType.STRING) {
              struct.AvatarID = iprot.readString();
              struct.setAvatarIDIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 2: // POSTURE_DATA
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list0 = iprot.readListBegin();
                struct.PostureData = new java.util.ArrayList<java.lang.Double>(_list0.size);
                double _elem1;
                for (int _i2 = 0; _i2 < _list0.size; ++_i2)
                {
                  _elem1 = iprot.readDouble();
                  struct.PostureData.add(_elem1);
                }
                iprot.readListEnd();
              }
              struct.setPostureDataIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 3: // PARTIAL_JOINT_LIST
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list3 = iprot.readListBegin();
                struct.PartialJointList = new java.util.ArrayList<MJointType>(_list3.size);
                @org.apache.thrift.annotation.Nullable MJointType _elem4;
                for (int _i5 = 0; _i5 < _list3.size; ++_i5)
                {
                  _elem4 = de.mosim.mmi.avatar.MJointType.findByValue(iprot.readI32());
                  if (_elem4 != null)
                  {
                    struct.PartialJointList.add(_elem4);
                  }
                }
                iprot.readListEnd();
              }
              struct.setPartialJointListIsSet(true);
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

    public void write(org.apache.thrift.protocol.TProtocol oprot, MAvatarPostureValues struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      if (struct.AvatarID != null) {
        oprot.writeFieldBegin(AVATAR_ID_FIELD_DESC);
        oprot.writeString(struct.AvatarID);
        oprot.writeFieldEnd();
      }
      if (struct.PostureData != null) {
        oprot.writeFieldBegin(POSTURE_DATA_FIELD_DESC);
        {
          oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.DOUBLE, struct.PostureData.size()));
          for (double _iter6 : struct.PostureData)
          {
            oprot.writeDouble(_iter6);
          }
          oprot.writeListEnd();
        }
        oprot.writeFieldEnd();
      }
      if (struct.PartialJointList != null) {
        if (struct.isSetPartialJointList()) {
          oprot.writeFieldBegin(PARTIAL_JOINT_LIST_FIELD_DESC);
          {
            oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.I32, struct.PartialJointList.size()));
            for (MJointType _iter7 : struct.PartialJointList)
            {
              oprot.writeI32(_iter7.getValue());
            }
            oprot.writeListEnd();
          }
          oprot.writeFieldEnd();
        }
      }
      oprot.writeFieldStop();
      oprot.writeStructEnd();
    }

  }

  private static class MAvatarPostureValuesTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MAvatarPostureValuesTupleScheme getScheme() {
      return new MAvatarPostureValuesTupleScheme();
    }
  }

  private static class MAvatarPostureValuesTupleScheme extends org.apache.thrift.scheme.TupleScheme<MAvatarPostureValues> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MAvatarPostureValues struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      oprot.writeString(struct.AvatarID);
      {
        oprot.writeI32(struct.PostureData.size());
        for (double _iter8 : struct.PostureData)
        {
          oprot.writeDouble(_iter8);
        }
      }
      java.util.BitSet optionals = new java.util.BitSet();
      if (struct.isSetPartialJointList()) {
        optionals.set(0);
      }
      oprot.writeBitSet(optionals, 1);
      if (struct.isSetPartialJointList()) {
        {
          oprot.writeI32(struct.PartialJointList.size());
          for (MJointType _iter9 : struct.PartialJointList)
          {
            oprot.writeI32(_iter9.getValue());
          }
        }
      }
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MAvatarPostureValues struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      struct.AvatarID = iprot.readString();
      struct.setAvatarIDIsSet(true);
      {
        org.apache.thrift.protocol.TList _list10 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.DOUBLE, iprot.readI32());
        struct.PostureData = new java.util.ArrayList<java.lang.Double>(_list10.size);
        double _elem11;
        for (int _i12 = 0; _i12 < _list10.size; ++_i12)
        {
          _elem11 = iprot.readDouble();
          struct.PostureData.add(_elem11);
        }
      }
      struct.setPostureDataIsSet(true);
      java.util.BitSet incoming = iprot.readBitSet(1);
      if (incoming.get(0)) {
        {
          org.apache.thrift.protocol.TList _list13 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.I32, iprot.readI32());
          struct.PartialJointList = new java.util.ArrayList<MJointType>(_list13.size);
          @org.apache.thrift.annotation.Nullable MJointType _elem14;
          for (int _i15 = 0; _i15 < _list13.size; ++_i15)
          {
            _elem14 = de.mosim.mmi.avatar.MJointType.findByValue(iprot.readI32());
            if (_elem14 != null)
            {
              struct.PartialJointList.add(_elem14);
            }
          }
        }
        struct.setPartialJointListIsSet(true);
      }
    }
  }

  private static <S extends org.apache.thrift.scheme.IScheme> S scheme(org.apache.thrift.protocol.TProtocol proto) {
    return (org.apache.thrift.scheme.StandardScheme.class.equals(proto.getScheme()) ? STANDARD_SCHEME_FACTORY : TUPLE_SCHEME_FACTORY).getScheme();
  }
}

