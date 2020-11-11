/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.avatar;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2020-10-20")
public class MAvatar implements org.apache.thrift.TBase<MAvatar, MAvatar._Fields>, java.io.Serializable, Cloneable, Comparable<MAvatar> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MAvatar");

  private static final org.apache.thrift.protocol.TField ID_FIELD_DESC = new org.apache.thrift.protocol.TField("ID", org.apache.thrift.protocol.TType.STRING, (short)1);
  private static final org.apache.thrift.protocol.TField NAME_FIELD_DESC = new org.apache.thrift.protocol.TField("Name", org.apache.thrift.protocol.TType.STRING, (short)2);
  private static final org.apache.thrift.protocol.TField DESCRIPTION_FIELD_DESC = new org.apache.thrift.protocol.TField("Description", org.apache.thrift.protocol.TType.STRUCT, (short)3);
  private static final org.apache.thrift.protocol.TField POSTURE_VALUES_FIELD_DESC = new org.apache.thrift.protocol.TField("PostureValues", org.apache.thrift.protocol.TType.STRUCT, (short)4);
  private static final org.apache.thrift.protocol.TField SCENE_OBJECTS_FIELD_DESC = new org.apache.thrift.protocol.TField("SceneObjects", org.apache.thrift.protocol.TType.LIST, (short)5);
  private static final org.apache.thrift.protocol.TField PROPERTIES_FIELD_DESC = new org.apache.thrift.protocol.TField("Properties", org.apache.thrift.protocol.TType.MAP, (short)6);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MAvatarStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MAvatarTupleSchemeFactory();

  public @org.apache.thrift.annotation.Nullable java.lang.String ID; // required
  public @org.apache.thrift.annotation.Nullable java.lang.String Name; // required
  public @org.apache.thrift.annotation.Nullable MAvatarDescription Description; // required
  public @org.apache.thrift.annotation.Nullable MAvatarPostureValues PostureValues; // required
  public @org.apache.thrift.annotation.Nullable java.util.List<java.lang.String> SceneObjects; // optional
  public @org.apache.thrift.annotation.Nullable java.util.Map<java.lang.String,java.lang.String> Properties; // optional

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    ID((short)1, "ID"),
    NAME((short)2, "Name"),
    DESCRIPTION((short)3, "Description"),
    POSTURE_VALUES((short)4, "PostureValues"),
    SCENE_OBJECTS((short)5, "SceneObjects"),
    PROPERTIES((short)6, "Properties");

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
        case 1: // ID
          return ID;
        case 2: // NAME
          return NAME;
        case 3: // DESCRIPTION
          return DESCRIPTION;
        case 4: // POSTURE_VALUES
          return POSTURE_VALUES;
        case 5: // SCENE_OBJECTS
          return SCENE_OBJECTS;
        case 6: // PROPERTIES
          return PROPERTIES;
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
  private static final _Fields optionals[] = {_Fields.SCENE_OBJECTS,_Fields.PROPERTIES};
  public static final java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> metaDataMap;
  static {
    java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> tmpMap = new java.util.EnumMap<_Fields, org.apache.thrift.meta_data.FieldMetaData>(_Fields.class);
    tmpMap.put(_Fields.ID, new org.apache.thrift.meta_data.FieldMetaData("ID", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING)));
    tmpMap.put(_Fields.NAME, new org.apache.thrift.meta_data.FieldMetaData("Name", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING)));
    tmpMap.put(_Fields.DESCRIPTION, new org.apache.thrift.meta_data.FieldMetaData("Description", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MAvatarDescription.class)));
    tmpMap.put(_Fields.POSTURE_VALUES, new org.apache.thrift.meta_data.FieldMetaData("PostureValues", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MAvatarPostureValues.class)));
    tmpMap.put(_Fields.SCENE_OBJECTS, new org.apache.thrift.meta_data.FieldMetaData("SceneObjects", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING))));
    tmpMap.put(_Fields.PROPERTIES, new org.apache.thrift.meta_data.FieldMetaData("Properties", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.MapMetaData(org.apache.thrift.protocol.TType.MAP, 
            new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING), 
            new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING))));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MAvatar.class, metaDataMap);
  }

  public MAvatar() {
  }

  public MAvatar(
    java.lang.String ID,
    java.lang.String Name,
    MAvatarDescription Description,
    MAvatarPostureValues PostureValues)
  {
    this();
    this.ID = ID;
    this.Name = Name;
    this.Description = Description;
    this.PostureValues = PostureValues;
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MAvatar(MAvatar other) {
    if (other.isSetID()) {
      this.ID = other.ID;
    }
    if (other.isSetName()) {
      this.Name = other.Name;
    }
    if (other.isSetDescription()) {
      this.Description = new MAvatarDescription(other.Description);
    }
    if (other.isSetPostureValues()) {
      this.PostureValues = new MAvatarPostureValues(other.PostureValues);
    }
    if (other.isSetSceneObjects()) {
      java.util.List<java.lang.String> __this__SceneObjects = new java.util.ArrayList<java.lang.String>(other.SceneObjects);
      this.SceneObjects = __this__SceneObjects;
    }
    if (other.isSetProperties()) {
      java.util.Map<java.lang.String,java.lang.String> __this__Properties = new java.util.HashMap<java.lang.String,java.lang.String>(other.Properties);
      this.Properties = __this__Properties;
    }
  }

  public MAvatar deepCopy() {
    return new MAvatar(this);
  }

  @Override
  public void clear() {
    this.ID = null;
    this.Name = null;
    this.Description = null;
    this.PostureValues = null;
    this.SceneObjects = null;
    this.Properties = null;
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.String getID() {
    return this.ID;
  }

  public MAvatar setID(@org.apache.thrift.annotation.Nullable java.lang.String ID) {
    this.ID = ID;
    return this;
  }

  public void unsetID() {
    this.ID = null;
  }

  /** Returns true if field ID is set (has been assigned a value) and false otherwise */
  public boolean isSetID() {
    return this.ID != null;
  }

  public void setIDIsSet(boolean value) {
    if (!value) {
      this.ID = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.String getName() {
    return this.Name;
  }

  public MAvatar setName(@org.apache.thrift.annotation.Nullable java.lang.String Name) {
    this.Name = Name;
    return this;
  }

  public void unsetName() {
    this.Name = null;
  }

  /** Returns true if field Name is set (has been assigned a value) and false otherwise */
  public boolean isSetName() {
    return this.Name != null;
  }

  public void setNameIsSet(boolean value) {
    if (!value) {
      this.Name = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public MAvatarDescription getDescription() {
    return this.Description;
  }

  public MAvatar setDescription(@org.apache.thrift.annotation.Nullable MAvatarDescription Description) {
    this.Description = Description;
    return this;
  }

  public void unsetDescription() {
    this.Description = null;
  }

  /** Returns true if field Description is set (has been assigned a value) and false otherwise */
  public boolean isSetDescription() {
    return this.Description != null;
  }

  public void setDescriptionIsSet(boolean value) {
    if (!value) {
      this.Description = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public MAvatarPostureValues getPostureValues() {
    return this.PostureValues;
  }

  public MAvatar setPostureValues(@org.apache.thrift.annotation.Nullable MAvatarPostureValues PostureValues) {
    this.PostureValues = PostureValues;
    return this;
  }

  public void unsetPostureValues() {
    this.PostureValues = null;
  }

  /** Returns true if field PostureValues is set (has been assigned a value) and false otherwise */
  public boolean isSetPostureValues() {
    return this.PostureValues != null;
  }

  public void setPostureValuesIsSet(boolean value) {
    if (!value) {
      this.PostureValues = null;
    }
  }

  public int getSceneObjectsSize() {
    return (this.SceneObjects == null) ? 0 : this.SceneObjects.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<java.lang.String> getSceneObjectsIterator() {
    return (this.SceneObjects == null) ? null : this.SceneObjects.iterator();
  }

  public void addToSceneObjects(java.lang.String elem) {
    if (this.SceneObjects == null) {
      this.SceneObjects = new java.util.ArrayList<java.lang.String>();
    }
    this.SceneObjects.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<java.lang.String> getSceneObjects() {
    return this.SceneObjects;
  }

  public MAvatar setSceneObjects(@org.apache.thrift.annotation.Nullable java.util.List<java.lang.String> SceneObjects) {
    this.SceneObjects = SceneObjects;
    return this;
  }

  public void unsetSceneObjects() {
    this.SceneObjects = null;
  }

  /** Returns true if field SceneObjects is set (has been assigned a value) and false otherwise */
  public boolean isSetSceneObjects() {
    return this.SceneObjects != null;
  }

  public void setSceneObjectsIsSet(boolean value) {
    if (!value) {
      this.SceneObjects = null;
    }
  }

  public int getPropertiesSize() {
    return (this.Properties == null) ? 0 : this.Properties.size();
  }

  public void putToProperties(java.lang.String key, java.lang.String val) {
    if (this.Properties == null) {
      this.Properties = new java.util.HashMap<java.lang.String,java.lang.String>();
    }
    this.Properties.put(key, val);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Map<java.lang.String,java.lang.String> getProperties() {
    return this.Properties;
  }

  public MAvatar setProperties(@org.apache.thrift.annotation.Nullable java.util.Map<java.lang.String,java.lang.String> Properties) {
    this.Properties = Properties;
    return this;
  }

  public void unsetProperties() {
    this.Properties = null;
  }

  /** Returns true if field Properties is set (has been assigned a value) and false otherwise */
  public boolean isSetProperties() {
    return this.Properties != null;
  }

  public void setPropertiesIsSet(boolean value) {
    if (!value) {
      this.Properties = null;
    }
  }

  public void setFieldValue(_Fields field, @org.apache.thrift.annotation.Nullable java.lang.Object value) {
    switch (field) {
    case ID:
      if (value == null) {
        unsetID();
      } else {
        setID((java.lang.String)value);
      }
      break;

    case NAME:
      if (value == null) {
        unsetName();
      } else {
        setName((java.lang.String)value);
      }
      break;

    case DESCRIPTION:
      if (value == null) {
        unsetDescription();
      } else {
        setDescription((MAvatarDescription)value);
      }
      break;

    case POSTURE_VALUES:
      if (value == null) {
        unsetPostureValues();
      } else {
        setPostureValues((MAvatarPostureValues)value);
      }
      break;

    case SCENE_OBJECTS:
      if (value == null) {
        unsetSceneObjects();
      } else {
        setSceneObjects((java.util.List<java.lang.String>)value);
      }
      break;

    case PROPERTIES:
      if (value == null) {
        unsetProperties();
      } else {
        setProperties((java.util.Map<java.lang.String,java.lang.String>)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case ID:
      return getID();

    case NAME:
      return getName();

    case DESCRIPTION:
      return getDescription();

    case POSTURE_VALUES:
      return getPostureValues();

    case SCENE_OBJECTS:
      return getSceneObjects();

    case PROPERTIES:
      return getProperties();

    }
    throw new java.lang.IllegalStateException();
  }

  /** Returns true if field corresponding to fieldID is set (has been assigned a value) and false otherwise */
  public boolean isSet(_Fields field) {
    if (field == null) {
      throw new java.lang.IllegalArgumentException();
    }

    switch (field) {
    case ID:
      return isSetID();
    case NAME:
      return isSetName();
    case DESCRIPTION:
      return isSetDescription();
    case POSTURE_VALUES:
      return isSetPostureValues();
    case SCENE_OBJECTS:
      return isSetSceneObjects();
    case PROPERTIES:
      return isSetProperties();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MAvatar)
      return this.equals((MAvatar)that);
    return false;
  }

  public boolean equals(MAvatar that) {
    if (that == null)
      return false;
    if (this == that)
      return true;

    boolean this_present_ID = true && this.isSetID();
    boolean that_present_ID = true && that.isSetID();
    if (this_present_ID || that_present_ID) {
      if (!(this_present_ID && that_present_ID))
        return false;
      if (!this.ID.equals(that.ID))
        return false;
    }

    boolean this_present_Name = true && this.isSetName();
    boolean that_present_Name = true && that.isSetName();
    if (this_present_Name || that_present_Name) {
      if (!(this_present_Name && that_present_Name))
        return false;
      if (!this.Name.equals(that.Name))
        return false;
    }

    boolean this_present_Description = true && this.isSetDescription();
    boolean that_present_Description = true && that.isSetDescription();
    if (this_present_Description || that_present_Description) {
      if (!(this_present_Description && that_present_Description))
        return false;
      if (!this.Description.equals(that.Description))
        return false;
    }

    boolean this_present_PostureValues = true && this.isSetPostureValues();
    boolean that_present_PostureValues = true && that.isSetPostureValues();
    if (this_present_PostureValues || that_present_PostureValues) {
      if (!(this_present_PostureValues && that_present_PostureValues))
        return false;
      if (!this.PostureValues.equals(that.PostureValues))
        return false;
    }

    boolean this_present_SceneObjects = true && this.isSetSceneObjects();
    boolean that_present_SceneObjects = true && that.isSetSceneObjects();
    if (this_present_SceneObjects || that_present_SceneObjects) {
      if (!(this_present_SceneObjects && that_present_SceneObjects))
        return false;
      if (!this.SceneObjects.equals(that.SceneObjects))
        return false;
    }

    boolean this_present_Properties = true && this.isSetProperties();
    boolean that_present_Properties = true && that.isSetProperties();
    if (this_present_Properties || that_present_Properties) {
      if (!(this_present_Properties && that_present_Properties))
        return false;
      if (!this.Properties.equals(that.Properties))
        return false;
    }

    return true;
  }

  @Override
  public int hashCode() {
    int hashCode = 1;

    hashCode = hashCode * 8191 + ((isSetID()) ? 131071 : 524287);
    if (isSetID())
      hashCode = hashCode * 8191 + ID.hashCode();

    hashCode = hashCode * 8191 + ((isSetName()) ? 131071 : 524287);
    if (isSetName())
      hashCode = hashCode * 8191 + Name.hashCode();

    hashCode = hashCode * 8191 + ((isSetDescription()) ? 131071 : 524287);
    if (isSetDescription())
      hashCode = hashCode * 8191 + Description.hashCode();

    hashCode = hashCode * 8191 + ((isSetPostureValues()) ? 131071 : 524287);
    if (isSetPostureValues())
      hashCode = hashCode * 8191 + PostureValues.hashCode();

    hashCode = hashCode * 8191 + ((isSetSceneObjects()) ? 131071 : 524287);
    if (isSetSceneObjects())
      hashCode = hashCode * 8191 + SceneObjects.hashCode();

    hashCode = hashCode * 8191 + ((isSetProperties()) ? 131071 : 524287);
    if (isSetProperties())
      hashCode = hashCode * 8191 + Properties.hashCode();

    return hashCode;
  }

  @Override
  public int compareTo(MAvatar other) {
    if (!getClass().equals(other.getClass())) {
      return getClass().getName().compareTo(other.getClass().getName());
    }

    int lastComparison = 0;

    lastComparison = java.lang.Boolean.valueOf(isSetID()).compareTo(other.isSetID());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetID()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.ID, other.ID);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetName()).compareTo(other.isSetName());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetName()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Name, other.Name);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetDescription()).compareTo(other.isSetDescription());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetDescription()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Description, other.Description);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetPostureValues()).compareTo(other.isSetPostureValues());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetPostureValues()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.PostureValues, other.PostureValues);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetSceneObjects()).compareTo(other.isSetSceneObjects());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetSceneObjects()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.SceneObjects, other.SceneObjects);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetProperties()).compareTo(other.isSetProperties());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetProperties()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Properties, other.Properties);
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
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MAvatar(");
    boolean first = true;

    sb.append("ID:");
    if (this.ID == null) {
      sb.append("null");
    } else {
      sb.append(this.ID);
    }
    first = false;
    if (!first) sb.append(", ");
    sb.append("Name:");
    if (this.Name == null) {
      sb.append("null");
    } else {
      sb.append(this.Name);
    }
    first = false;
    if (!first) sb.append(", ");
    sb.append("Description:");
    if (this.Description == null) {
      sb.append("null");
    } else {
      sb.append(this.Description);
    }
    first = false;
    if (!first) sb.append(", ");
    sb.append("PostureValues:");
    if (this.PostureValues == null) {
      sb.append("null");
    } else {
      sb.append(this.PostureValues);
    }
    first = false;
    if (isSetSceneObjects()) {
      if (!first) sb.append(", ");
      sb.append("SceneObjects:");
      if (this.SceneObjects == null) {
        sb.append("null");
      } else {
        sb.append(this.SceneObjects);
      }
      first = false;
    }
    if (isSetProperties()) {
      if (!first) sb.append(", ");
      sb.append("Properties:");
      if (this.Properties == null) {
        sb.append("null");
      } else {
        sb.append(this.Properties);
      }
      first = false;
    }
    sb.append(")");
    return sb.toString();
  }

  public void validate() throws org.apache.thrift.TException {
    // check for required fields
    if (ID == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'ID' was not present! Struct: " + toString());
    }
    if (Name == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'Name' was not present! Struct: " + toString());
    }
    if (Description == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'Description' was not present! Struct: " + toString());
    }
    if (PostureValues == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'PostureValues' was not present! Struct: " + toString());
    }
    // check for sub-struct validity
    if (Description != null) {
      Description.validate();
    }
    if (PostureValues != null) {
      PostureValues.validate();
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
      read(new org.apache.thrift.protocol.TCompactProtocol(new org.apache.thrift.transport.TIOStreamTransport(in)));
    } catch (org.apache.thrift.TException te) {
      throw new java.io.IOException(te);
    }
  }

  private static class MAvatarStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MAvatarStandardScheme getScheme() {
      return new MAvatarStandardScheme();
    }
  }

  private static class MAvatarStandardScheme extends org.apache.thrift.scheme.StandardScheme<MAvatar> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MAvatar struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TField schemeField;
      iprot.readStructBegin();
      while (true)
      {
        schemeField = iprot.readFieldBegin();
        if (schemeField.type == org.apache.thrift.protocol.TType.STOP) { 
          break;
        }
        switch (schemeField.id) {
          case 1: // ID
            if (schemeField.type == org.apache.thrift.protocol.TType.STRING) {
              struct.ID = iprot.readString();
              struct.setIDIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 2: // NAME
            if (schemeField.type == org.apache.thrift.protocol.TType.STRING) {
              struct.Name = iprot.readString();
              struct.setNameIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 3: // DESCRIPTION
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.Description = new MAvatarDescription();
              struct.Description.read(iprot);
              struct.setDescriptionIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 4: // POSTURE_VALUES
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.PostureValues = new MAvatarPostureValues();
              struct.PostureValues.read(iprot);
              struct.setPostureValuesIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 5: // SCENE_OBJECTS
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list60 = iprot.readListBegin();
                struct.SceneObjects = new java.util.ArrayList<java.lang.String>(_list60.size);
                @org.apache.thrift.annotation.Nullable java.lang.String _elem61;
                for (int _i62 = 0; _i62 < _list60.size; ++_i62)
                {
                  _elem61 = iprot.readString();
                  struct.SceneObjects.add(_elem61);
                }
                iprot.readListEnd();
              }
              struct.setSceneObjectsIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 6: // PROPERTIES
            if (schemeField.type == org.apache.thrift.protocol.TType.MAP) {
              {
                org.apache.thrift.protocol.TMap _map63 = iprot.readMapBegin();
                struct.Properties = new java.util.HashMap<java.lang.String,java.lang.String>(2*_map63.size);
                @org.apache.thrift.annotation.Nullable java.lang.String _key64;
                @org.apache.thrift.annotation.Nullable java.lang.String _val65;
                for (int _i66 = 0; _i66 < _map63.size; ++_i66)
                {
                  _key64 = iprot.readString();
                  _val65 = iprot.readString();
                  struct.Properties.put(_key64, _val65);
                }
                iprot.readMapEnd();
              }
              struct.setPropertiesIsSet(true);
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

    public void write(org.apache.thrift.protocol.TProtocol oprot, MAvatar struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      if (struct.ID != null) {
        oprot.writeFieldBegin(ID_FIELD_DESC);
        oprot.writeString(struct.ID);
        oprot.writeFieldEnd();
      }
      if (struct.Name != null) {
        oprot.writeFieldBegin(NAME_FIELD_DESC);
        oprot.writeString(struct.Name);
        oprot.writeFieldEnd();
      }
      if (struct.Description != null) {
        oprot.writeFieldBegin(DESCRIPTION_FIELD_DESC);
        struct.Description.write(oprot);
        oprot.writeFieldEnd();
      }
      if (struct.PostureValues != null) {
        oprot.writeFieldBegin(POSTURE_VALUES_FIELD_DESC);
        struct.PostureValues.write(oprot);
        oprot.writeFieldEnd();
      }
      if (struct.SceneObjects != null) {
        if (struct.isSetSceneObjects()) {
          oprot.writeFieldBegin(SCENE_OBJECTS_FIELD_DESC);
          {
            oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRING, struct.SceneObjects.size()));
            for (java.lang.String _iter67 : struct.SceneObjects)
            {
              oprot.writeString(_iter67);
            }
            oprot.writeListEnd();
          }
          oprot.writeFieldEnd();
        }
      }
      if (struct.Properties != null) {
        if (struct.isSetProperties()) {
          oprot.writeFieldBegin(PROPERTIES_FIELD_DESC);
          {
            oprot.writeMapBegin(new org.apache.thrift.protocol.TMap(org.apache.thrift.protocol.TType.STRING, org.apache.thrift.protocol.TType.STRING, struct.Properties.size()));
            for (java.util.Map.Entry<java.lang.String, java.lang.String> _iter68 : struct.Properties.entrySet())
            {
              oprot.writeString(_iter68.getKey());
              oprot.writeString(_iter68.getValue());
            }
            oprot.writeMapEnd();
          }
          oprot.writeFieldEnd();
        }
      }
      oprot.writeFieldStop();
      oprot.writeStructEnd();
    }

  }

  private static class MAvatarTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MAvatarTupleScheme getScheme() {
      return new MAvatarTupleScheme();
    }
  }

  private static class MAvatarTupleScheme extends org.apache.thrift.scheme.TupleScheme<MAvatar> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MAvatar struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      oprot.writeString(struct.ID);
      oprot.writeString(struct.Name);
      struct.Description.write(oprot);
      struct.PostureValues.write(oprot);
      java.util.BitSet optionals = new java.util.BitSet();
      if (struct.isSetSceneObjects()) {
        optionals.set(0);
      }
      if (struct.isSetProperties()) {
        optionals.set(1);
      }
      oprot.writeBitSet(optionals, 2);
      if (struct.isSetSceneObjects()) {
        {
          oprot.writeI32(struct.SceneObjects.size());
          for (java.lang.String _iter69 : struct.SceneObjects)
          {
            oprot.writeString(_iter69);
          }
        }
      }
      if (struct.isSetProperties()) {
        {
          oprot.writeI32(struct.Properties.size());
          for (java.util.Map.Entry<java.lang.String, java.lang.String> _iter70 : struct.Properties.entrySet())
          {
            oprot.writeString(_iter70.getKey());
            oprot.writeString(_iter70.getValue());
          }
        }
      }
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MAvatar struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      struct.ID = iprot.readString();
      struct.setIDIsSet(true);
      struct.Name = iprot.readString();
      struct.setNameIsSet(true);
      struct.Description = new MAvatarDescription();
      struct.Description.read(iprot);
      struct.setDescriptionIsSet(true);
      struct.PostureValues = new MAvatarPostureValues();
      struct.PostureValues.read(iprot);
      struct.setPostureValuesIsSet(true);
      java.util.BitSet incoming = iprot.readBitSet(2);
      if (incoming.get(0)) {
        {
          org.apache.thrift.protocol.TList _list71 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRING, iprot.readI32());
          struct.SceneObjects = new java.util.ArrayList<java.lang.String>(_list71.size);
          @org.apache.thrift.annotation.Nullable java.lang.String _elem72;
          for (int _i73 = 0; _i73 < _list71.size; ++_i73)
          {
            _elem72 = iprot.readString();
            struct.SceneObjects.add(_elem72);
          }
        }
        struct.setSceneObjectsIsSet(true);
      }
      if (incoming.get(1)) {
        {
          org.apache.thrift.protocol.TMap _map74 = new org.apache.thrift.protocol.TMap(org.apache.thrift.protocol.TType.STRING, org.apache.thrift.protocol.TType.STRING, iprot.readI32());
          struct.Properties = new java.util.HashMap<java.lang.String,java.lang.String>(2*_map74.size);
          @org.apache.thrift.annotation.Nullable java.lang.String _key75;
          @org.apache.thrift.annotation.Nullable java.lang.String _val76;
          for (int _i77 = 0; _i77 < _map74.size; ++_i77)
          {
            _key75 = iprot.readString();
            _val76 = iprot.readString();
            struct.Properties.put(_key75, _val76);
          }
        }
        struct.setPropertiesIsSet(true);
      }
    }
  }

  private static <S extends org.apache.thrift.scheme.IScheme> S scheme(org.apache.thrift.protocol.TProtocol proto) {
    return (org.apache.thrift.scheme.StandardScheme.class.equals(proto.getScheme()) ? STANDARD_SCHEME_FACTORY : TUPLE_SCHEME_FACTORY).getScheme();
  }
}

