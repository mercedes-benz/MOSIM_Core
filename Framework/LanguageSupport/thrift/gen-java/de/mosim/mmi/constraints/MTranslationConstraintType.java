/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
package de.mosim.mmi.constraints;


@javax.annotation.Generated(value = "Autogenerated by Thrift Compiler (0.13.0)", date = "2020-10-20")
public enum MTranslationConstraintType implements org.apache.thrift.TEnum {
  BOX(0),
  ELLIPSOID(1);

  private final int value;

  private MTranslationConstraintType(int value) {
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
  public static MTranslationConstraintType findByValue(int value) { 
    switch (value) {
      case 0:
        return BOX;
      case 1:
        return ELLIPSOID;
      default:
        return null;
    }
  }
}
