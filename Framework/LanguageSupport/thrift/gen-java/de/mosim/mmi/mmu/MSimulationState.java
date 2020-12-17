/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.mmu;

@SuppressWarnings({"cast", "rawtypes", "serial", "unchecked", "unused"})
@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2020-12-08")
public class MSimulationState implements org.apache.thrift.TBase<MSimulationState, MSimulationState._Fields>, java.io.Serializable, Cloneable, Comparable<MSimulationState> {
  private static final org.apache.thrift.protocol.TStruct STRUCT_DESC = new org.apache.thrift.protocol.TStruct("MSimulationState");

  private static final org.apache.thrift.protocol.TField INITIAL_FIELD_DESC = new org.apache.thrift.protocol.TField("Initial", org.apache.thrift.protocol.TType.STRUCT, (short)1);
  private static final org.apache.thrift.protocol.TField CURRENT_FIELD_DESC = new org.apache.thrift.protocol.TField("Current", org.apache.thrift.protocol.TType.STRUCT, (short)2);
  private static final org.apache.thrift.protocol.TField CONSTRAINTS_FIELD_DESC = new org.apache.thrift.protocol.TField("Constraints", org.apache.thrift.protocol.TType.LIST, (short)3);
  private static final org.apache.thrift.protocol.TField SCENE_MANIPULATIONS_FIELD_DESC = new org.apache.thrift.protocol.TField("SceneManipulations", org.apache.thrift.protocol.TType.LIST, (short)4);
  private static final org.apache.thrift.protocol.TField EVENTS_FIELD_DESC = new org.apache.thrift.protocol.TField("Events", org.apache.thrift.protocol.TType.LIST, (short)5);

  private static final org.apache.thrift.scheme.SchemeFactory STANDARD_SCHEME_FACTORY = new MSimulationStateStandardSchemeFactory();
  private static final org.apache.thrift.scheme.SchemeFactory TUPLE_SCHEME_FACTORY = new MSimulationStateTupleSchemeFactory();

  public @org.apache.thrift.annotation.Nullable de.mosim.mmi.avatar.MAvatarPostureValues Initial; // required
  public @org.apache.thrift.annotation.Nullable de.mosim.mmi.avatar.MAvatarPostureValues Current; // required
  public @org.apache.thrift.annotation.Nullable java.util.List<de.mosim.mmi.constraints.MConstraint> Constraints; // optional
  public @org.apache.thrift.annotation.Nullable java.util.List<de.mosim.mmi.scene.MSceneManipulation> SceneManipulations; // optional
  public @org.apache.thrift.annotation.Nullable java.util.List<MSimulationEvent> Events; // optional

  /** The set of fields this struct contains, along with convenience methods for finding and manipulating them. */
  public enum _Fields implements org.apache.thrift.TFieldIdEnum {
    INITIAL((short)1, "Initial"),
    CURRENT((short)2, "Current"),
    CONSTRAINTS((short)3, "Constraints"),
    SCENE_MANIPULATIONS((short)4, "SceneManipulations"),
    EVENTS((short)5, "Events");

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
        case 1: // INITIAL
          return INITIAL;
        case 2: // CURRENT
          return CURRENT;
        case 3: // CONSTRAINTS
          return CONSTRAINTS;
        case 4: // SCENE_MANIPULATIONS
          return SCENE_MANIPULATIONS;
        case 5: // EVENTS
          return EVENTS;
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
  private static final _Fields optionals[] = {_Fields.CONSTRAINTS,_Fields.SCENE_MANIPULATIONS,_Fields.EVENTS};
  public static final java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> metaDataMap;
  static {
    java.util.Map<_Fields, org.apache.thrift.meta_data.FieldMetaData> tmpMap = new java.util.EnumMap<_Fields, org.apache.thrift.meta_data.FieldMetaData>(_Fields.class);
    tmpMap.put(_Fields.INITIAL, new org.apache.thrift.meta_data.FieldMetaData("Initial", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, de.mosim.mmi.avatar.MAvatarPostureValues.class)));
    tmpMap.put(_Fields.CURRENT, new org.apache.thrift.meta_data.FieldMetaData("Current", org.apache.thrift.TFieldRequirementType.REQUIRED, 
        new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, de.mosim.mmi.avatar.MAvatarPostureValues.class)));
    tmpMap.put(_Fields.CONSTRAINTS, new org.apache.thrift.meta_data.FieldMetaData("Constraints", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, de.mosim.mmi.constraints.MConstraint.class))));
    tmpMap.put(_Fields.SCENE_MANIPULATIONS, new org.apache.thrift.meta_data.FieldMetaData("SceneManipulations", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.StructMetaData(org.apache.thrift.protocol.TType.STRUCT, de.mosim.mmi.scene.MSceneManipulation.class))));
    tmpMap.put(_Fields.EVENTS, new org.apache.thrift.meta_data.FieldMetaData("Events", org.apache.thrift.TFieldRequirementType.OPTIONAL, 
        new org.apache.thrift.meta_data.ListMetaData(org.apache.thrift.protocol.TType.LIST, 
            new org.apache.thrift.meta_data.FieldValueMetaData(org.apache.thrift.protocol.TType.STRUCT            , "MSimulationEvent"))));
    metaDataMap = java.util.Collections.unmodifiableMap(tmpMap);
    org.apache.thrift.meta_data.FieldMetaData.addStructMetaDataMap(MSimulationState.class, metaDataMap);
  }

  public MSimulationState() {
  }

  public MSimulationState(
    de.mosim.mmi.avatar.MAvatarPostureValues Initial,
    de.mosim.mmi.avatar.MAvatarPostureValues Current)
  {
    this();
    this.Initial = Initial;
    this.Current = Current;
  }

  /**
   * Performs a deep copy on <i>other</i>.
   */
  public MSimulationState(MSimulationState other) {
    if (other.isSetInitial()) {
      this.Initial = new de.mosim.mmi.avatar.MAvatarPostureValues(other.Initial);
    }
    if (other.isSetCurrent()) {
      this.Current = new de.mosim.mmi.avatar.MAvatarPostureValues(other.Current);
    }
    if (other.isSetConstraints()) {
      java.util.List<de.mosim.mmi.constraints.MConstraint> __this__Constraints = new java.util.ArrayList<de.mosim.mmi.constraints.MConstraint>(other.Constraints.size());
      for (de.mosim.mmi.constraints.MConstraint other_element : other.Constraints) {
        __this__Constraints.add(new de.mosim.mmi.constraints.MConstraint(other_element));
      }
      this.Constraints = __this__Constraints;
    }
    if (other.isSetSceneManipulations()) {
      java.util.List<de.mosim.mmi.scene.MSceneManipulation> __this__SceneManipulations = new java.util.ArrayList<de.mosim.mmi.scene.MSceneManipulation>(other.SceneManipulations.size());
      for (de.mosim.mmi.scene.MSceneManipulation other_element : other.SceneManipulations) {
        __this__SceneManipulations.add(new de.mosim.mmi.scene.MSceneManipulation(other_element));
      }
      this.SceneManipulations = __this__SceneManipulations;
    }
    if (other.isSetEvents()) {
      java.util.List<MSimulationEvent> __this__Events = new java.util.ArrayList<MSimulationEvent>(other.Events.size());
      for (MSimulationEvent other_element : other.Events) {
        __this__Events.add(new MSimulationEvent(other_element));
      }
      this.Events = __this__Events;
    }
  }

  public MSimulationState deepCopy() {
    return new MSimulationState(this);
  }

  @Override
  public void clear() {
    this.Initial = null;
    this.Current = null;
    this.Constraints = null;
    this.SceneManipulations = null;
    this.Events = null;
  }

  @org.apache.thrift.annotation.Nullable
  public de.mosim.mmi.avatar.MAvatarPostureValues getInitial() {
    return this.Initial;
  }

  public MSimulationState setInitial(@org.apache.thrift.annotation.Nullable de.mosim.mmi.avatar.MAvatarPostureValues Initial) {
    this.Initial = Initial;
    return this;
  }

  public void unsetInitial() {
    this.Initial = null;
  }

  /** Returns true if field Initial is set (has been assigned a value) and false otherwise */
  public boolean isSetInitial() {
    return this.Initial != null;
  }

  public void setInitialIsSet(boolean value) {
    if (!value) {
      this.Initial = null;
    }
  }

  @org.apache.thrift.annotation.Nullable
  public de.mosim.mmi.avatar.MAvatarPostureValues getCurrent() {
    return this.Current;
  }

  public MSimulationState setCurrent(@org.apache.thrift.annotation.Nullable de.mosim.mmi.avatar.MAvatarPostureValues Current) {
    this.Current = Current;
    return this;
  }

  public void unsetCurrent() {
    this.Current = null;
  }

  /** Returns true if field Current is set (has been assigned a value) and false otherwise */
  public boolean isSetCurrent() {
    return this.Current != null;
  }

  public void setCurrentIsSet(boolean value) {
    if (!value) {
      this.Current = null;
    }
  }

  public int getConstraintsSize() {
    return (this.Constraints == null) ? 0 : this.Constraints.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<de.mosim.mmi.constraints.MConstraint> getConstraintsIterator() {
    return (this.Constraints == null) ? null : this.Constraints.iterator();
  }

  public void addToConstraints(de.mosim.mmi.constraints.MConstraint elem) {
    if (this.Constraints == null) {
      this.Constraints = new java.util.ArrayList<de.mosim.mmi.constraints.MConstraint>();
    }
    this.Constraints.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<de.mosim.mmi.constraints.MConstraint> getConstraints() {
    return this.Constraints;
  }

  public MSimulationState setConstraints(@org.apache.thrift.annotation.Nullable java.util.List<de.mosim.mmi.constraints.MConstraint> Constraints) {
    this.Constraints = Constraints;
    return this;
  }

  public void unsetConstraints() {
    this.Constraints = null;
  }

  /** Returns true if field Constraints is set (has been assigned a value) and false otherwise */
  public boolean isSetConstraints() {
    return this.Constraints != null;
  }

  public void setConstraintsIsSet(boolean value) {
    if (!value) {
      this.Constraints = null;
    }
  }

  public int getSceneManipulationsSize() {
    return (this.SceneManipulations == null) ? 0 : this.SceneManipulations.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<de.mosim.mmi.scene.MSceneManipulation> getSceneManipulationsIterator() {
    return (this.SceneManipulations == null) ? null : this.SceneManipulations.iterator();
  }

  public void addToSceneManipulations(de.mosim.mmi.scene.MSceneManipulation elem) {
    if (this.SceneManipulations == null) {
      this.SceneManipulations = new java.util.ArrayList<de.mosim.mmi.scene.MSceneManipulation>();
    }
    this.SceneManipulations.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<de.mosim.mmi.scene.MSceneManipulation> getSceneManipulations() {
    return this.SceneManipulations;
  }

  public MSimulationState setSceneManipulations(@org.apache.thrift.annotation.Nullable java.util.List<de.mosim.mmi.scene.MSceneManipulation> SceneManipulations) {
    this.SceneManipulations = SceneManipulations;
    return this;
  }

  public void unsetSceneManipulations() {
    this.SceneManipulations = null;
  }

  /** Returns true if field SceneManipulations is set (has been assigned a value) and false otherwise */
  public boolean isSetSceneManipulations() {
    return this.SceneManipulations != null;
  }

  public void setSceneManipulationsIsSet(boolean value) {
    if (!value) {
      this.SceneManipulations = null;
    }
  }

  public int getEventsSize() {
    return (this.Events == null) ? 0 : this.Events.size();
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.Iterator<MSimulationEvent> getEventsIterator() {
    return (this.Events == null) ? null : this.Events.iterator();
  }

  public void addToEvents(MSimulationEvent elem) {
    if (this.Events == null) {
      this.Events = new java.util.ArrayList<MSimulationEvent>();
    }
    this.Events.add(elem);
  }

  @org.apache.thrift.annotation.Nullable
  public java.util.List<MSimulationEvent> getEvents() {
    return this.Events;
  }

  public MSimulationState setEvents(@org.apache.thrift.annotation.Nullable java.util.List<MSimulationEvent> Events) {
    this.Events = Events;
    return this;
  }

  public void unsetEvents() {
    this.Events = null;
  }

  /** Returns true if field Events is set (has been assigned a value) and false otherwise */
  public boolean isSetEvents() {
    return this.Events != null;
  }

  public void setEventsIsSet(boolean value) {
    if (!value) {
      this.Events = null;
    }
  }

  public void setFieldValue(_Fields field, @org.apache.thrift.annotation.Nullable java.lang.Object value) {
    switch (field) {
    case INITIAL:
      if (value == null) {
        unsetInitial();
      } else {
        setInitial((de.mosim.mmi.avatar.MAvatarPostureValues)value);
      }
      break;

    case CURRENT:
      if (value == null) {
        unsetCurrent();
      } else {
        setCurrent((de.mosim.mmi.avatar.MAvatarPostureValues)value);
      }
      break;

    case CONSTRAINTS:
      if (value == null) {
        unsetConstraints();
      } else {
        setConstraints((java.util.List<de.mosim.mmi.constraints.MConstraint>)value);
      }
      break;

    case SCENE_MANIPULATIONS:
      if (value == null) {
        unsetSceneManipulations();
      } else {
        setSceneManipulations((java.util.List<de.mosim.mmi.scene.MSceneManipulation>)value);
      }
      break;

    case EVENTS:
      if (value == null) {
        unsetEvents();
      } else {
        setEvents((java.util.List<MSimulationEvent>)value);
      }
      break;

    }
  }

  @org.apache.thrift.annotation.Nullable
  public java.lang.Object getFieldValue(_Fields field) {
    switch (field) {
    case INITIAL:
      return getInitial();

    case CURRENT:
      return getCurrent();

    case CONSTRAINTS:
      return getConstraints();

    case SCENE_MANIPULATIONS:
      return getSceneManipulations();

    case EVENTS:
      return getEvents();

    }
    throw new java.lang.IllegalStateException();
  }

  /** Returns true if field corresponding to fieldID is set (has been assigned a value) and false otherwise */
  public boolean isSet(_Fields field) {
    if (field == null) {
      throw new java.lang.IllegalArgumentException();
    }

    switch (field) {
    case INITIAL:
      return isSetInitial();
    case CURRENT:
      return isSetCurrent();
    case CONSTRAINTS:
      return isSetConstraints();
    case SCENE_MANIPULATIONS:
      return isSetSceneManipulations();
    case EVENTS:
      return isSetEvents();
    }
    throw new java.lang.IllegalStateException();
  }

  @Override
  public boolean equals(java.lang.Object that) {
    if (that == null)
      return false;
    if (that instanceof MSimulationState)
      return this.equals((MSimulationState)that);
    return false;
  }

  public boolean equals(MSimulationState that) {
    if (that == null)
      return false;
    if (this == that)
      return true;

    boolean this_present_Initial = true && this.isSetInitial();
    boolean that_present_Initial = true && that.isSetInitial();
    if (this_present_Initial || that_present_Initial) {
      if (!(this_present_Initial && that_present_Initial))
        return false;
      if (!this.Initial.equals(that.Initial))
        return false;
    }

    boolean this_present_Current = true && this.isSetCurrent();
    boolean that_present_Current = true && that.isSetCurrent();
    if (this_present_Current || that_present_Current) {
      if (!(this_present_Current && that_present_Current))
        return false;
      if (!this.Current.equals(that.Current))
        return false;
    }

    boolean this_present_Constraints = true && this.isSetConstraints();
    boolean that_present_Constraints = true && that.isSetConstraints();
    if (this_present_Constraints || that_present_Constraints) {
      if (!(this_present_Constraints && that_present_Constraints))
        return false;
      if (!this.Constraints.equals(that.Constraints))
        return false;
    }

    boolean this_present_SceneManipulations = true && this.isSetSceneManipulations();
    boolean that_present_SceneManipulations = true && that.isSetSceneManipulations();
    if (this_present_SceneManipulations || that_present_SceneManipulations) {
      if (!(this_present_SceneManipulations && that_present_SceneManipulations))
        return false;
      if (!this.SceneManipulations.equals(that.SceneManipulations))
        return false;
    }

    boolean this_present_Events = true && this.isSetEvents();
    boolean that_present_Events = true && that.isSetEvents();
    if (this_present_Events || that_present_Events) {
      if (!(this_present_Events && that_present_Events))
        return false;
      if (!this.Events.equals(that.Events))
        return false;
    }

    return true;
  }

  @Override
  public int hashCode() {
    int hashCode = 1;

    hashCode = hashCode * 8191 + ((isSetInitial()) ? 131071 : 524287);
    if (isSetInitial())
      hashCode = hashCode * 8191 + Initial.hashCode();

    hashCode = hashCode * 8191 + ((isSetCurrent()) ? 131071 : 524287);
    if (isSetCurrent())
      hashCode = hashCode * 8191 + Current.hashCode();

    hashCode = hashCode * 8191 + ((isSetConstraints()) ? 131071 : 524287);
    if (isSetConstraints())
      hashCode = hashCode * 8191 + Constraints.hashCode();

    hashCode = hashCode * 8191 + ((isSetSceneManipulations()) ? 131071 : 524287);
    if (isSetSceneManipulations())
      hashCode = hashCode * 8191 + SceneManipulations.hashCode();

    hashCode = hashCode * 8191 + ((isSetEvents()) ? 131071 : 524287);
    if (isSetEvents())
      hashCode = hashCode * 8191 + Events.hashCode();

    return hashCode;
  }

  @Override
  public int compareTo(MSimulationState other) {
    if (!getClass().equals(other.getClass())) {
      return getClass().getName().compareTo(other.getClass().getName());
    }

    int lastComparison = 0;

    lastComparison = java.lang.Boolean.valueOf(isSetInitial()).compareTo(other.isSetInitial());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetInitial()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Initial, other.Initial);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetCurrent()).compareTo(other.isSetCurrent());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetCurrent()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Current, other.Current);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetConstraints()).compareTo(other.isSetConstraints());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetConstraints()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Constraints, other.Constraints);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetSceneManipulations()).compareTo(other.isSetSceneManipulations());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetSceneManipulations()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.SceneManipulations, other.SceneManipulations);
      if (lastComparison != 0) {
        return lastComparison;
      }
    }
    lastComparison = java.lang.Boolean.valueOf(isSetEvents()).compareTo(other.isSetEvents());
    if (lastComparison != 0) {
      return lastComparison;
    }
    if (isSetEvents()) {
      lastComparison = org.apache.thrift.TBaseHelper.compareTo(this.Events, other.Events);
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
    java.lang.StringBuilder sb = new java.lang.StringBuilder("MSimulationState(");
    boolean first = true;

    sb.append("Initial:");
    if (this.Initial == null) {
      sb.append("null");
    } else {
      sb.append(this.Initial);
    }
    first = false;
    if (!first) sb.append(", ");
    sb.append("Current:");
    if (this.Current == null) {
      sb.append("null");
    } else {
      sb.append(this.Current);
    }
    first = false;
    if (isSetConstraints()) {
      if (!first) sb.append(", ");
      sb.append("Constraints:");
      if (this.Constraints == null) {
        sb.append("null");
      } else {
        sb.append(this.Constraints);
      }
      first = false;
    }
    if (isSetSceneManipulations()) {
      if (!first) sb.append(", ");
      sb.append("SceneManipulations:");
      if (this.SceneManipulations == null) {
        sb.append("null");
      } else {
        sb.append(this.SceneManipulations);
      }
      first = false;
    }
    if (isSetEvents()) {
      if (!first) sb.append(", ");
      sb.append("Events:");
      if (this.Events == null) {
        sb.append("null");
      } else {
        sb.append(this.Events);
      }
      first = false;
    }
    sb.append(")");
    return sb.toString();
  }

  public void validate() throws org.apache.thrift.TException {
    // check for required fields
    if (Initial == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'Initial' was not present! Struct: " + toString());
    }
    if (Current == null) {
      throw new org.apache.thrift.protocol.TProtocolException("Required field 'Current' was not present! Struct: " + toString());
    }
    // check for sub-struct validity
    if (Initial != null) {
      Initial.validate();
    }
    if (Current != null) {
      Current.validate();
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

  private static class MSimulationStateStandardSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MSimulationStateStandardScheme getScheme() {
      return new MSimulationStateStandardScheme();
    }
  }

  private static class MSimulationStateStandardScheme extends org.apache.thrift.scheme.StandardScheme<MSimulationState> {

    public void read(org.apache.thrift.protocol.TProtocol iprot, MSimulationState struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TField schemeField;
      iprot.readStructBegin();
      while (true)
      {
        schemeField = iprot.readFieldBegin();
        if (schemeField.type == org.apache.thrift.protocol.TType.STOP) { 
          break;
        }
        switch (schemeField.id) {
          case 1: // INITIAL
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.Initial = new de.mosim.mmi.avatar.MAvatarPostureValues();
              struct.Initial.read(iprot);
              struct.setInitialIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 2: // CURRENT
            if (schemeField.type == org.apache.thrift.protocol.TType.STRUCT) {
              struct.Current = new de.mosim.mmi.avatar.MAvatarPostureValues();
              struct.Current.read(iprot);
              struct.setCurrentIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 3: // CONSTRAINTS
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list0 = iprot.readListBegin();
                struct.Constraints = new java.util.ArrayList<de.mosim.mmi.constraints.MConstraint>(_list0.size);
                @org.apache.thrift.annotation.Nullable de.mosim.mmi.constraints.MConstraint _elem1;
                for (int _i2 = 0; _i2 < _list0.size; ++_i2)
                {
                  _elem1 = new de.mosim.mmi.constraints.MConstraint();
                  _elem1.read(iprot);
                  struct.Constraints.add(_elem1);
                }
                iprot.readListEnd();
              }
              struct.setConstraintsIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 4: // SCENE_MANIPULATIONS
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list3 = iprot.readListBegin();
                struct.SceneManipulations = new java.util.ArrayList<de.mosim.mmi.scene.MSceneManipulation>(_list3.size);
                @org.apache.thrift.annotation.Nullable de.mosim.mmi.scene.MSceneManipulation _elem4;
                for (int _i5 = 0; _i5 < _list3.size; ++_i5)
                {
                  _elem4 = new de.mosim.mmi.scene.MSceneManipulation();
                  _elem4.read(iprot);
                  struct.SceneManipulations.add(_elem4);
                }
                iprot.readListEnd();
              }
              struct.setSceneManipulationsIsSet(true);
            } else { 
              org.apache.thrift.protocol.TProtocolUtil.skip(iprot, schemeField.type);
            }
            break;
          case 5: // EVENTS
            if (schemeField.type == org.apache.thrift.protocol.TType.LIST) {
              {
                org.apache.thrift.protocol.TList _list6 = iprot.readListBegin();
                struct.Events = new java.util.ArrayList<MSimulationEvent>(_list6.size);
                @org.apache.thrift.annotation.Nullable MSimulationEvent _elem7;
                for (int _i8 = 0; _i8 < _list6.size; ++_i8)
                {
                  _elem7 = new MSimulationEvent();
                  _elem7.read(iprot);
                  struct.Events.add(_elem7);
                }
                iprot.readListEnd();
              }
              struct.setEventsIsSet(true);
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

    public void write(org.apache.thrift.protocol.TProtocol oprot, MSimulationState struct) throws org.apache.thrift.TException {
      struct.validate();

      oprot.writeStructBegin(STRUCT_DESC);
      if (struct.Initial != null) {
        oprot.writeFieldBegin(INITIAL_FIELD_DESC);
        struct.Initial.write(oprot);
        oprot.writeFieldEnd();
      }
      if (struct.Current != null) {
        oprot.writeFieldBegin(CURRENT_FIELD_DESC);
        struct.Current.write(oprot);
        oprot.writeFieldEnd();
      }
      if (struct.Constraints != null) {
        if (struct.isSetConstraints()) {
          oprot.writeFieldBegin(CONSTRAINTS_FIELD_DESC);
          {
            oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, struct.Constraints.size()));
            for (de.mosim.mmi.constraints.MConstraint _iter9 : struct.Constraints)
            {
              _iter9.write(oprot);
            }
            oprot.writeListEnd();
          }
          oprot.writeFieldEnd();
        }
      }
      if (struct.SceneManipulations != null) {
        if (struct.isSetSceneManipulations()) {
          oprot.writeFieldBegin(SCENE_MANIPULATIONS_FIELD_DESC);
          {
            oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, struct.SceneManipulations.size()));
            for (de.mosim.mmi.scene.MSceneManipulation _iter10 : struct.SceneManipulations)
            {
              _iter10.write(oprot);
            }
            oprot.writeListEnd();
          }
          oprot.writeFieldEnd();
        }
      }
      if (struct.Events != null) {
        if (struct.isSetEvents()) {
          oprot.writeFieldBegin(EVENTS_FIELD_DESC);
          {
            oprot.writeListBegin(new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, struct.Events.size()));
            for (MSimulationEvent _iter11 : struct.Events)
            {
              _iter11.write(oprot);
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

  private static class MSimulationStateTupleSchemeFactory implements org.apache.thrift.scheme.SchemeFactory {
    public MSimulationStateTupleScheme getScheme() {
      return new MSimulationStateTupleScheme();
    }
  }

  private static class MSimulationStateTupleScheme extends org.apache.thrift.scheme.TupleScheme<MSimulationState> {

    @Override
    public void write(org.apache.thrift.protocol.TProtocol prot, MSimulationState struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol oprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      struct.Initial.write(oprot);
      struct.Current.write(oprot);
      java.util.BitSet optionals = new java.util.BitSet();
      if (struct.isSetConstraints()) {
        optionals.set(0);
      }
      if (struct.isSetSceneManipulations()) {
        optionals.set(1);
      }
      if (struct.isSetEvents()) {
        optionals.set(2);
      }
      oprot.writeBitSet(optionals, 3);
      if (struct.isSetConstraints()) {
        {
          oprot.writeI32(struct.Constraints.size());
          for (de.mosim.mmi.constraints.MConstraint _iter12 : struct.Constraints)
          {
            _iter12.write(oprot);
          }
        }
      }
      if (struct.isSetSceneManipulations()) {
        {
          oprot.writeI32(struct.SceneManipulations.size());
          for (de.mosim.mmi.scene.MSceneManipulation _iter13 : struct.SceneManipulations)
          {
            _iter13.write(oprot);
          }
        }
      }
      if (struct.isSetEvents()) {
        {
          oprot.writeI32(struct.Events.size());
          for (MSimulationEvent _iter14 : struct.Events)
          {
            _iter14.write(oprot);
          }
        }
      }
    }

    @Override
    public void read(org.apache.thrift.protocol.TProtocol prot, MSimulationState struct) throws org.apache.thrift.TException {
      org.apache.thrift.protocol.TTupleProtocol iprot = (org.apache.thrift.protocol.TTupleProtocol) prot;
      struct.Initial = new de.mosim.mmi.avatar.MAvatarPostureValues();
      struct.Initial.read(iprot);
      struct.setInitialIsSet(true);
      struct.Current = new de.mosim.mmi.avatar.MAvatarPostureValues();
      struct.Current.read(iprot);
      struct.setCurrentIsSet(true);
      java.util.BitSet incoming = iprot.readBitSet(3);
      if (incoming.get(0)) {
        {
          org.apache.thrift.protocol.TList _list15 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, iprot.readI32());
          struct.Constraints = new java.util.ArrayList<de.mosim.mmi.constraints.MConstraint>(_list15.size);
          @org.apache.thrift.annotation.Nullable de.mosim.mmi.constraints.MConstraint _elem16;
          for (int _i17 = 0; _i17 < _list15.size; ++_i17)
          {
            _elem16 = new de.mosim.mmi.constraints.MConstraint();
            _elem16.read(iprot);
            struct.Constraints.add(_elem16);
          }
        }
        struct.setConstraintsIsSet(true);
      }
      if (incoming.get(1)) {
        {
          org.apache.thrift.protocol.TList _list18 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, iprot.readI32());
          struct.SceneManipulations = new java.util.ArrayList<de.mosim.mmi.scene.MSceneManipulation>(_list18.size);
          @org.apache.thrift.annotation.Nullable de.mosim.mmi.scene.MSceneManipulation _elem19;
          for (int _i20 = 0; _i20 < _list18.size; ++_i20)
          {
            _elem19 = new de.mosim.mmi.scene.MSceneManipulation();
            _elem19.read(iprot);
            struct.SceneManipulations.add(_elem19);
          }
        }
        struct.setSceneManipulationsIsSet(true);
      }
      if (incoming.get(2)) {
        {
          org.apache.thrift.protocol.TList _list21 = new org.apache.thrift.protocol.TList(org.apache.thrift.protocol.TType.STRUCT, iprot.readI32());
          struct.Events = new java.util.ArrayList<MSimulationEvent>(_list21.size);
          @org.apache.thrift.annotation.Nullable MSimulationEvent _elem22;
          for (int _i23 = 0; _i23 < _list21.size; ++_i23)
          {
            _elem22 = new MSimulationEvent();
            _elem22.read(iprot);
            struct.Events.add(_elem22);
          }
        }
        struct.setEventsIsSet(true);
      }
    }
  }

  private static <S extends org.apache.thrift.scheme.IScheme> S scheme(org.apache.thrift.protocol.TProtocol proto) {
    return (org.apache.thrift.scheme.StandardScheme.class.equals(proto.getScheme()) ? STANDARD_SCHEME_FACTORY : TUPLE_SCHEME_FACTORY).getScheme();
  }
}

