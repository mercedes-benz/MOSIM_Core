/**
 * Autogenerated by Thrift Compiler (0.12.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package MMIStandard;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.12.0)", date = "2019-10-02")
public class MSceneObjectUpdate implements org.apache.thrift.TBase<MSceneObjectUpdate, MSceneObjectUpdate._Fields>, java.io.Serializable, Cloneable, Comparable<MSceneObjectUpdate> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MSceneObjectUpdate");

  private static final org.apache.thrift.protocol.TField ID_FIELD_DESC = new org.apache.thrift.protocol.TField("ID", org.apache.thrift.protocol.TType.STRING, (short)1);
  private static final org.apache.thrift.protocol.TField TRANSFORM_FIELD_DESC = new org.apache.thrift.protocol.TField("Transform", org.apache.thrift.protocol.TType.STRUCT, (short)2);
  private static final org.apache.thrift.protocol.TField COLLIDER_FIELD_DESC = new org.apache.thrift.protocol.TField("Collider", org.apache.thrift.protocol.TType.STRUCT, (short)3);
  private static final org.apache.thrift.protocol.TField MESH_FIELD_DESC = new org.apache.thrift.protocol.TField("Mesh", org.apache.thrift.protocol.TType.STRUCT, (short)4);
  private static final org.apache.thrift.protocol.TField PHYSICS_PROPERTIES_FIELD_DESC = new org.apache.thrift.protocol.TField("PhysicsProperties", org.apache.thrift.protocol.TType.STRUCT, (short)5);
  private static final org.apache.thrift.protocol.TField PROPERTIES_FIELD_DESC = new org.apache.thrift.protocol.TField("Properties", org.apache.thrift.protocol.TType.LIST, (short)6);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MSceneObjectUpdateStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MSceneObjectUpdateTupleSchemeFactory();

  public @org.apache.thrift.annotation.Nullable java.lang.String ID; // required
  public @org.apache.thrift.annotation.Nullable MTransformUpdate Transform; // optional
  public @org.apache.thrift.annotation.Nullable MCollider Collider; // optional
  public @org.apache.thrift.annotation.Nullable MMesh Mesh; // optional
  public @org.apache.thrift.annotation.Nullable MPhysicsProperties PhysicsProperties; // optional
  public @org.apache.thrift.annotation.Nullable java.util.List<MPropertyUpdate> Properties; // optional

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    ID((short)1, "ID"),
    TRANSFORM((short)2, "Transform"),
    COLLIDER((short)3, "Collider"),
    MESH((short)4, "Mesh"),
    PHYSICS_PROPERTIES((short)5, "PhysicsProperties"),
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
        case 2: // TRANSFORM
          return TRANSFORM;
        case 3: // COLLIDER
          return COLLIDER;
        case 4: // MESH
          return MESH;
        case 5: // PHYSICS_PROPERTIES
          return PHYSICS_PROPERTIES;
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
  private static final _Fields optionals[] = {_Fields.TRANSFORM,_Fields.COLLIDER,_Fields.MESH,_Fields.PHYSICS_PROPERTIES,_Fields.PROPERTIES};
  public static final java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> metaDataMap;
  static {
    java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> tmpMap = new java.util.EnumMap<_Fields, org.apache.thrift.meta_data.FieldMetaData>(_Fields.class);
    tmpMap.put(_Fields.ID, new org.apache.thrift.meta_data.FieldMetaData("ID", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRING)));
    tmpMap.put(_Fields.TRANSFORM, new org.apache.thrift.meta_data.FieldMetaData("Transform", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MTransformUpdate.class)));
    tmpMap.put(_Fields.COLLIDER, new org.apache.thrift.meta_data.FieldMetaData("Collider", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MCollider.class)));
    tmpMap.put(_Fields.MESH, new org.apache.thrift.meta_data.FieldMetaData("Mesh", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MMesh.class)));
    tmpMap.put(_Fields.PHYSICS_PROPERTIES, new org.apache.thrift.meta_data.FieldMetaData("PhysicsProperties", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MPhysicsProperties.class)));
    tmpMap.put(_Fields.PROPERTIES, new org.apache.thrift.meta_data.FieldMetaData("Properties", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, MPropertyUpdate.class))));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MSceneObjectUpdate.class, metaDataMap);
  }

  public MSceneObjectUpdate() {
  }

  public MSceneObjectUpdate(
    java.lang.String ID)
  {
    this();
    this.ID = ID;
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MSceneObjectUpdate(MSceneObjectUpdate other) {
    if (other.isSetID()) {
      this.ID = other.ID;
    }
    if (other.isSetTransform()) {
      this.Transform = new MTransformUpdate(other.Transform);
    }
    if (other.isSetCollider()) {
      this.Collider = new MCollider(other.Collider);
    }
    if (other.isSetMesh()) {
      this.Mesh = new MMesh(other.Mesh);
    }
    if (other.isSetPhysicsProperties()) {
      this.PhysicsProperties = new MPhysicsProperties(other.PhysicsProperties);
    }
    if (other.isSetProperties()) {
      java.util.List<MPropertyUpdate> __this__Properties = new java.util.ArrayList<MPropertyUpdate>(other.Properties.size());
      for (MPropertyUpdate other_element : other.Properties) {
        __this__Properties.add(new MPropertyUpdate(other_element));
      }
      this.Properties = __this__Properties;
    }
  }

  public MSceneObjectUpdate deepCopy() {
    return new MSceneObjectUpdate(this);
  }

  @Override
  public void clear() {
    this.ID = null;
    this.Transform = null;
    this.Collider = null;
    this.Mesh = null;
    this.PhysicsProperties = null;
    this.Properties = null;
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.String getID() {
    return this.ID;
  }

  public MSceneObjectUpdate setID(@org.apache.thrift.annotation.Nullable java.lang.String ID) {
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
  public MTransformUpdate getTransform() {
    return this.Transform;
  }

  public MSceneObjectUpdate setTransform(@org.apache.thrift.annotation.Nullable MTransformUpdate Transform) {
    this.Transform = Transform;
    return this;
  }

  public void unsetTransform() {
    this.Transform = null;
  }

  /** Returns true if field Transform is set (has been assigned a value) and false otherwise */
  public boolean isSetTransform() {
    return this.Transform != null;
  }

  public void setTransformIsSet(boolean value) {
    if (!value) {
      this.Transform = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public MCollider getCollider() {
    return this.Collider;
  }

  public MSceneObjectUpdate setCollider(@org.apache.thrift.annotation.Nullable MCollider Collider) {
    this.Collider = Collider;
    return this;
  }

  public void unsetCollider() {
    this.Collider = null;
  }

  /** Returns true if field Collider is set (has been assigned a value) and false otherwise */
  public boolean isSetCollider() {
    return this.Collider != null;
  }

  public void setColliderIsSet(boolean value) {
    if (!value) {
      this.Collider = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public MMesh getMesh() {
    return this.Mesh;
  }

  public MSceneObjectUpdate setMesh(@org.apache.thrift.annotation.Nullable MMesh Mesh) {
    this.Mesh = Mesh;
    return this;
  }

  public void unsetMesh() {
    this.Mesh = null;
  }

  /** Returns true if field Mesh is set (has been assigned a value) and false otherwise */
  public boolean isSetMesh() {
    return this.Mesh != null;
  }

  public void setMeshIsSet(boolean value) {
    if (!value) {
      this.Mesh = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public MPhysicsProperties getPhysicsProperties() {
    return this.PhysicsProperties;
  }

  public MSceneObjectUpdate setPhysicsProperties(@org.apache.thrift.annotation.Nullable MPhysicsProperties PhysicsProperties) {
    this.PhysicsProperties = PhysicsProperties;
    return this;
  }

  public void unsetPhysicsProperties() {
    this.PhysicsProperties = null;
  }

  /** Returns true if field PhysicsProperties is set (has been assigned a value) and false otherwise */
  public boolean isSetPhysicsProperties() {
    return this.PhysicsProperties != null;
  }

  public void setPhysicsPropertiesIsSet(boolean value) {
    if (!value) {
      this.PhysicsProperties = null;
    }
  }

  public int getPropertiesSize() {
    return (this.Properties == null) ? 0 : this.Properties.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<MPropertyUpdate> getPropertiesIterator() {
    return (this.Properties == null) ? null : this.Properties.iterator();
  }

  public void addToProperties(MPropertyUpdate elem) {
    if (this.Properties == null) {
      this.Properties = new java.util.ArrayList<MPropertyUpdate>();
    }
    this.Properties.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<MPropertyUpdate> getProperties() {
    return this.Properties;
  }

  public MSceneObjectUpdate setProperties(@org.apache.thrift.annotation.Nullable java.util.List<MPropertyUpdate> Properties) {
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

    case TRANSFORM:
      if (value == null) {
        unsetTransform();
      } else {
        setTransform((MTransformUpdate)value);
      }
      break;

    case COLLIDER:
      if (value == null) {
        unsetCollider();
      } else {
        setCollider((MCollider)value);
      }
      break;

    case MESH:
      if (value == null) {
        unsetMesh();
      } else {
        setMesh((MMesh)value);
      }
      break;

    case PHYSICS_PROPERTIES:
      if (value == null) {
        unsetPhysicsProperties();
      } else {
        setPhysicsProperties((MPhysicsProperties)value);
      }
      break;

    case PROPERTIES:
      if (value == null) {
        unsetProperties();
      } else {
        setProperties((java.util.List<MPropertyUpdate>)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case ID:
      return getID();

    case TRANSFORM:
      return getTransform();

    case COLLIDER:
      return getCollider();

    case MESH:
      return getMesh();

    case PHYSICS_PROPERTIES:
      return getPhysicsProperties();

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
    case TRANSFORM:
      return isSetTransform();
    case COLLIDER:
      return isSetCollider();
    case MESH:
      return isSetMesh();
    case PHYSICS_PROPERTIES:
      return isSetPhysicsProperties();
    case PROPERTIES:
      return isSetProperties();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MSceneObjectUpdate)
      return this.equals((MSceneObjectUpdate)that);
    return false;
  }

  public boolean equals(MSceneObjectUpdate that) {
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

    boolean this_present_Transform = true && this.isSetTransform();
    boolean that_present_Transform = true && that.isSetTransform();
    if (this_present_Transform || that_present_Transform) {
      if (!(this_present_Transform && that_present_Transform))
        return false;
      if (!this.Transform.equals(that.Transform))
        return false;
    }

    boolean this_present_Collider = true && this.isSetCollider();
    boolean that_present_Collider = true && that.isSetCollider();
    if (this_present_Collider || that_present_Collider) {
      if (!(this_present_Collider && that_present_Collider))
        return false;
      if (!this.Collider.equals(that.Collider))
        return false;
    }

    boolean this_present_Mesh = true && this.isSetMesh();
    boolean that_present_Mesh = true && that.isSetMesh();
    if (this_present_Mesh || that_present_Mesh) {
      if (!(this_present_Mesh && that_present_Mesh))
        return false;
      if (!this.Mesh.equals(that.Mesh))
        return false;
    }

    boolean this_present_PhysicsProperties = true && this.isSetPhysicsProperties();
    boolean that_present_PhysicsProperties = true && that.isSetPhysicsProperties();
    if (this_present_PhysicsProperties || that_present_PhysicsProperties) {
      if (!(this_present_PhysicsProperties && that_present_PhysicsProperties))
        return false;
      if (!this.PhysicsProperties.equals(that.PhysicsProperties))
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

    hashCode = hashCode * 8191 + ((isSetTransform()) ? 131071 : 524287);
    if (isSetTransform())
      hashCode = hashCode * 8191 + Transform.hashCode();

    hashCode = hashCode * 8191 + ((isSetCollider()) ? 131071 : 524287);
    if (isSetCollider())
      hashCode = hashCode * 8191 + Collider.hashCode();

    hashCode = hashCode * 8191 + ((isSetMesh()) ? 131071 : 524287);
    if (isSetMesh())
      hashCode = hashCode * 8191 + Mesh.hashCode();

    hashCode = hashCode * 8191 + ((isSetPhysicsProperties()) ? 131071 : 524287);
    if (isSetPhysicsProperties())
      hashCode = hashCode * 8191 + PhysicsProperties.hashCode();

    hashCode = hashCode * 8191 + ((isSetProperties()) ? 131071 : 524287);
    if (isSetProperties())
      hashCode = hashCode * 8191 + Properties.hashCode();

    return hashCode;
  }

  @Override
  public int compareTo(MSceneObjectUpdate other) {
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
    lastComparison = java.lang.Boolean.valueOf(isSetTransform()).compareTo(other.isSetTransform());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetTransform()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Transform, other.Transform);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetCollider()).compareTo(other.isSetCollider());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetCollider()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Collider, other.Collider);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetMesh()).compareTo(other.isSetMesh());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetMesh()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Mesh, other.Mesh);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetPhysicsProperties()).compareTo(other.isSetPhysicsProperties());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetPhysicsProperties()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.PhysicsProperties, other.PhysicsProperties);
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
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MSceneObjectUpdate(");
    boolean first = true;

    sb.append("ID:");
    if (this.ID == null) {
      sb.append("null");
    } else {
      sb.append(this.ID);
    }
    first = false;
    if (isSetTransform()) {
      if (!first) sb.append(", ");
      sb.append("Transform:");
      if (this.Transform == null) {
        sb.append("null");
      } else {
        sb.append(this.Transform);
      }
      first = false;
    }
    if (isSetCollider()) {
      if (!first) sb.append(", ");
      sb.append("Collider:");
      if (this.Collider == null) {
        sb.append("null");
      } else {
        sb.append(this.Collider);
      }
      first = false;
    }
    if (isSetMesh()) {
      if (!first) sb.append(", ");
      sb.append("Mesh:");
      if (this.Mesh == null) {
        sb.append("null");
      } else {
        sb.append(this.Mesh);
      }
      first = false;
    }
    if (isSetPhysicsProperties()) {
      if (!first) sb.append(", ");
      sb.append("PhysicsProperties:");
      if (this.PhysicsProperties == null) {
        sb.append("null");
      } else {
        sb.append(this.PhysicsProperties);
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
    // check for sub-struct validity
    if (Transform != null) {
      Transform.validate();
    }
    if (Collider != null) {
      Collider.validate();
    }
    if (Mesh != null) {
      Mesh.validate();
    }
    if (PhysicsProperties != null) {
      PhysicsProperties.validate();
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

  private static class MSceneObjectUpdateStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MSceneObjectUpdateStandardScheme getScheme() {
      return new MSceneObjectUpdateStandardScheme();
    }
  }

  private static class MSceneObjectUpdateStandardScheme extends org.apache.thrift.scheme.StandardScheme<MSceneObjectUpdate> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MSceneObjectUpdate struct) throws org.apache.thrift.TException {
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
          case 2: // TRANSFORM
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.Transform = new MTransformUpdate();
              struct.Transform.read(iprot);
              struct.setTransformIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 3: // COLLIDER
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.Collider = new MCollider();
              struct.Collider.read(iprot);
              struct.setColliderIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 4: // MESH
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.Mesh = new MMesh();
              struct.Mesh.read(iprot);
              struct.setMeshIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 5: // PHYSICS_PROPERTIES
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.PhysicsProperties = new MPhysicsProperties();
              struct.PhysicsProperties.read(iprot);
              struct.setPhysicsPropertiesIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 6: // PROPERTIES
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list272 = iprot.readListBegin();
                struct.Properties = new java.util.ArrayList<MPropertyUpdate>(_list272.size);
                @org.apache.thrift.annotation.Nullable MPropertyUpdate _elem273;
                for (int _i274 = 0; _i274 < _list272.size; ++_i274)
                {
                  _elem273 = new MPropertyUpdate();
                  _elem273.read(iprot);
                  struct.Properties.add(_elem273);
                }
                iprot.readListEnd();
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

    public void write(org.apache.thrift.protocol.TProtocol oprot, MSceneObjectUpdate struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      if (struct.ID != null) {
        oprot.writeFieldBegin(ID_FIELD_DESC);
        oprot.writeString(struct.ID);
        oprot.writeFieldEnd();
      }
      if (struct.Transform != null) {
        if (struct.isSetTransform()) {
          oprot.writeFieldBegin(TRANSFORM_FIELD_DESC);
          struct.Transform.write(oprot);
          oprot.writeFieldEnd();
        }
      }
      if (struct.Collider != null) {
        if (struct.isSetCollider()) {
          oprot.writeFieldBegin(COLLIDER_FIELD_DESC);
          struct.Collider.write(oprot);
          oprot.writeFieldEnd();
        }
      }
      if (struct.Mesh != null) {
        if (struct.isSetMesh()) {
          oprot.writeFieldBegin(MESH_FIELD_DESC);
          struct.Mesh.write(oprot);
          oprot.writeFieldEnd();
        }
      }
      if (struct.PhysicsProperties != null) {
        if (struct.isSetPhysicsProperties()) {
          oprot.writeFieldBegin(PHYSICS_PROPERTIES_FIELD_DESC);
          struct.PhysicsProperties.write(oprot);
          oprot.writeFieldEnd();
        }
      }
      if (struct.Properties != null) {
        if (struct.isSetProperties()) {
          oprot.writeFieldBegin(PROPERTIES_FIELD_DESC);
          {
            oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, struct.Properties.size()));
            for (MPropertyUpdate _iter275 : struct.Properties)
            {
              _iter275.write(oprot);
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

  private static class MSceneObjectUpdateTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MSceneObjectUpdateTupleScheme getScheme() {
      return new MSceneObjectUpdateTupleScheme();
    }
  }

  private static class MSceneObjectUpdateTupleScheme extends org.apache.thrift.scheme.TupleScheme<MSceneObjectUpdate> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MSceneObjectUpdate struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      oprot.writeString(struct.ID);
      java.util.BitSet optionals = new java.util.BitSet();
      if (struct.isSetTransform()) {
        optionals.set(0);
      }
      if (struct.isSetCollider()) {
        optionals.set(1);
      }
      if (struct.isSetMesh()) {
        optionals.set(2);
      }
      if (struct.isSetPhysicsProperties()) {
        optionals.set(3);
      }
      if (struct.isSetProperties()) {
        optionals.set(4);
      }
      oprot.writeBitSet(optionals, 5);
      if (struct.isSetTransform()) {
        struct.Transform.write(oprot);
      }
      if (struct.isSetCollider()) {
        struct.Collider.write(oprot);
      }
      if (struct.isSetMesh()) {
        struct.Mesh.write(oprot);
      }
      if (struct.isSetPhysicsProperties()) {
        struct.PhysicsProperties.write(oprot);
      }
      if (struct.isSetProperties()) {
        {
          oprot.writeI32(struct.Properties.size());
          for (MPropertyUpdate _iter276 : struct.Properties)
          {
            _iter276.write(oprot);
          }
        }
      }
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MSceneObjectUpdate struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      struct.ID = iprot.readString();
      struct.setIDIsSet(true);
      java.util.BitSet incoming = iprot.readBitSet(5);
      if (incoming.get(0)) {
        struct.Transform = new MTransformUpdate();
        struct.Transform.read(iprot);
        struct.setTransformIsSet(true);
      }
      if (incoming.get(1)) {
        struct.Collider = new MCollider();
        struct.Collider.read(iprot);
        struct.setColliderIsSet(true);
      }
      if (incoming.get(2)) {
        struct.Mesh = new MMesh();
        struct.Mesh.read(iprot);
        struct.setMeshIsSet(true);
      }
      if (incoming.get(3)) {
        struct.PhysicsProperties = new MPhysicsProperties();
        struct.PhysicsProperties.read(iprot);
        struct.setPhysicsPropertiesIsSet(true);
      }
      if (incoming.get(4)) {
        {
          org.apache.thrift.protocol.TList _list277 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, iprot.readI32());
          struct.Properties = new java.util.ArrayList<MPropertyUpdate>(_list277.size);
          @org.apache.thrift.annotation.Nullable MPropertyUpdate _elem278;
          for (int _i279 = 0; _i279 < _list277.size; ++_i279)
          {
            _elem278 = new MPropertyUpdate();
            _elem278.read(iprot);
            struct.Properties.add(_elem278);
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

