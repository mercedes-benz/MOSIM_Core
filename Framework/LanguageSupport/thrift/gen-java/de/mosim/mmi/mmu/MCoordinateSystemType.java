/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.mmu;


@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2020-12-08")
public enum MCoordinateSystemType implements org.apache.thrift.TEnum {
  Global(0),
  Local(1);

  private final int value;

  private MCoordinateSystemType(int value) {
    this.value = value;
  }

  /**
   * Get the integer value of this enum value, as defined in the Thrift IDL.
   */
  public int getValue() {
    return value;
  }

  /**
   * Find a the enum type by its integer value, as defined in the Thrift IDL.
   * @return null if the value is not found.
   */
  @org.apache.thrift.annotation.Nullable
  public static MCoordinateSystemType findByValue(int value) { 
    switch (value) {
      case 0:
        return Global;
      case 1:
        return Local;
      default:
        return null;
    }
  }
}
