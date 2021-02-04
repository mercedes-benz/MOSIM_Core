/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
#ifndef constraints_TYPES_H
#define constraints_TYPES_H

#include <iosfwd>

#include <thrift/Thrift.h>
#include <thrift/TApplicationException.h>
#include <thrift/TBase.h>
#include <thrift/protocol/TProtocol.h>
#include <thrift/transport/TTransport.h>

#include <functional>
#include <memory>
#include "math_types.h"
#include "avatar_types.h"


namespace MMIStandard {

struct MTranslationConstraintType {
  enum type {
    BOX = 0,
    ELLIPSOID = 1
  };
};

extern const std::map<int, const char*> _MTranslationConstraintType_VALUES_TO_NAMES;

std::ostream& operator<<(std::ostream& out, const MTranslationConstraintType::type& val);

std::string to_string(const MTranslationConstraintType::type& val);

class MInterval;

class MInterval3;

class MTranslationConstraint;

class MRotationConstraint;

class MGeometryConstraint;

class MVelocityConstraint;

class MAccelerationConstraint;

class MPathConstraint;

class MJointConstraint;

class MJointPathConstraint;

class MPostureConstraint;

class MConstraint;


class MInterval : public virtual ::apache::thrift::TBase {
 public:

  MInterval(const MInterval&);
  MInterval& operator=(const MInterval&);
  MInterval() : Min(0), Max(0) {
  }

  virtual ~MInterval() noexcept;
  double Min;
  double Max;

  void __set_Min(const double val);

  void __set_Max(const double val);

  bool operator == (const MInterval & rhs) const
  {
    if (!(Min == rhs.Min))
      return false;
    if (!(Max == rhs.Max))
      return false;
    return true;
  }
  bool operator != (const MInterval &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MInterval & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MInterval &a, MInterval &b);

std::ostream& operator<<(std::ostream& out, const MInterval& obj);


class MInterval3 : public virtual ::apache::thrift::TBase {
 public:

  MInterval3(const MInterval3&);
  MInterval3& operator=(const MInterval3&);
  MInterval3() {
  }

  virtual ~MInterval3() noexcept;
  MInterval X;
  MInterval Y;
  MInterval Z;

  void __set_X(const MInterval& val);

  void __set_Y(const MInterval& val);

  void __set_Z(const MInterval& val);

  bool operator == (const MInterval3 & rhs) const
  {
    if (!(X == rhs.X))
      return false;
    if (!(Y == rhs.Y))
      return false;
    if (!(Z == rhs.Z))
      return false;
    return true;
  }
  bool operator != (const MInterval3 &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MInterval3 & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MInterval3 &a, MInterval3 &b);

std::ostream& operator<<(std::ostream& out, const MInterval3& obj);


class MTranslationConstraint : public virtual ::apache::thrift::TBase {
 public:

  MTranslationConstraint(const MTranslationConstraint&);
  MTranslationConstraint& operator=(const MTranslationConstraint&);
  MTranslationConstraint() : Type((MTranslationConstraintType::type)0) {
  }

  virtual ~MTranslationConstraint() noexcept;
  MTranslationConstraintType::type Type;
  MInterval3 Limits;

  void __set_Type(const MTranslationConstraintType::type val);

  void __set_Limits(const MInterval3& val);

  bool operator == (const MTranslationConstraint & rhs) const
  {
    if (!(Type == rhs.Type))
      return false;
    if (!(Limits == rhs.Limits))
      return false;
    return true;
  }
  bool operator != (const MTranslationConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MTranslationConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MTranslationConstraint &a, MTranslationConstraint &b);

std::ostream& operator<<(std::ostream& out, const MTranslationConstraint& obj);


class MRotationConstraint : public virtual ::apache::thrift::TBase {
 public:

  MRotationConstraint(const MRotationConstraint&);
  MRotationConstraint& operator=(const MRotationConstraint&);
  MRotationConstraint() {
  }

  virtual ~MRotationConstraint() noexcept;
  MInterval3 Limits;

  void __set_Limits(const MInterval3& val);

  bool operator == (const MRotationConstraint & rhs) const
  {
    if (!(Limits == rhs.Limits))
      return false;
    return true;
  }
  bool operator != (const MRotationConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MRotationConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MRotationConstraint &a, MRotationConstraint &b);

std::ostream& operator<<(std::ostream& out, const MRotationConstraint& obj);

typedef struct _MGeometryConstraint__isset {
  _MGeometryConstraint__isset() : ParentToConstraint(false), TranslationConstraint(false), RotationConstraint(false), WeightingFactor(false) {}
  bool ParentToConstraint :1;
  bool TranslationConstraint :1;
  bool RotationConstraint :1;
  bool WeightingFactor :1;
} _MGeometryConstraint__isset;

class MGeometryConstraint : public virtual ::apache::thrift::TBase {
 public:

  MGeometryConstraint(const MGeometryConstraint&);
  MGeometryConstraint& operator=(const MGeometryConstraint&);
  MGeometryConstraint() : ParentObjectID(), WeightingFactor(0) {
  }

  virtual ~MGeometryConstraint() noexcept;
  std::string ParentObjectID;
   ::MMIStandard::MTransform ParentToConstraint;
  MTranslationConstraint TranslationConstraint;
  MRotationConstraint RotationConstraint;
  double WeightingFactor;

  _MGeometryConstraint__isset __isset;

  void __set_ParentObjectID(const std::string& val);

  void __set_ParentToConstraint(const  ::MMIStandard::MTransform& val);

  void __set_TranslationConstraint(const MTranslationConstraint& val);

  void __set_RotationConstraint(const MRotationConstraint& val);

  void __set_WeightingFactor(const double val);

  bool operator == (const MGeometryConstraint & rhs) const
  {
    if (!(ParentObjectID == rhs.ParentObjectID))
      return false;
    if (__isset.ParentToConstraint != rhs.__isset.ParentToConstraint)
      return false;
    else if (__isset.ParentToConstraint && !(ParentToConstraint == rhs.ParentToConstraint))
      return false;
    if (__isset.TranslationConstraint != rhs.__isset.TranslationConstraint)
      return false;
    else if (__isset.TranslationConstraint && !(TranslationConstraint == rhs.TranslationConstraint))
      return false;
    if (__isset.RotationConstraint != rhs.__isset.RotationConstraint)
      return false;
    else if (__isset.RotationConstraint && !(RotationConstraint == rhs.RotationConstraint))
      return false;
    if (__isset.WeightingFactor != rhs.__isset.WeightingFactor)
      return false;
    else if (__isset.WeightingFactor && !(WeightingFactor == rhs.WeightingFactor))
      return false;
    return true;
  }
  bool operator != (const MGeometryConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MGeometryConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MGeometryConstraint &a, MGeometryConstraint &b);

std::ostream& operator<<(std::ostream& out, const MGeometryConstraint& obj);

typedef struct _MVelocityConstraint__isset {
  _MVelocityConstraint__isset() : ParentToConstraint(false), TranslationalVelocity(false), RotationalVelocity(false), WeightingFactor(false) {}
  bool ParentToConstraint :1;
  bool TranslationalVelocity :1;
  bool RotationalVelocity :1;
  bool WeightingFactor :1;
} _MVelocityConstraint__isset;

class MVelocityConstraint : public virtual ::apache::thrift::TBase {
 public:

  MVelocityConstraint(const MVelocityConstraint&);
  MVelocityConstraint& operator=(const MVelocityConstraint&);
  MVelocityConstraint() : ParentObjectID(), WeightingFactor(0) {
  }

  virtual ~MVelocityConstraint() noexcept;
  std::string ParentObjectID;
   ::MMIStandard::MTransform ParentToConstraint;
   ::MMIStandard::MVector3 TranslationalVelocity;
   ::MMIStandard::MVector3 RotationalVelocity;
  double WeightingFactor;

  _MVelocityConstraint__isset __isset;

  void __set_ParentObjectID(const std::string& val);

  void __set_ParentToConstraint(const  ::MMIStandard::MTransform& val);

  void __set_TranslationalVelocity(const  ::MMIStandard::MVector3& val);

  void __set_RotationalVelocity(const  ::MMIStandard::MVector3& val);

  void __set_WeightingFactor(const double val);

  bool operator == (const MVelocityConstraint & rhs) const
  {
    if (!(ParentObjectID == rhs.ParentObjectID))
      return false;
    if (__isset.ParentToConstraint != rhs.__isset.ParentToConstraint)
      return false;
    else if (__isset.ParentToConstraint && !(ParentToConstraint == rhs.ParentToConstraint))
      return false;
    if (__isset.TranslationalVelocity != rhs.__isset.TranslationalVelocity)
      return false;
    else if (__isset.TranslationalVelocity && !(TranslationalVelocity == rhs.TranslationalVelocity))
      return false;
    if (__isset.RotationalVelocity != rhs.__isset.RotationalVelocity)
      return false;
    else if (__isset.RotationalVelocity && !(RotationalVelocity == rhs.RotationalVelocity))
      return false;
    if (__isset.WeightingFactor != rhs.__isset.WeightingFactor)
      return false;
    else if (__isset.WeightingFactor && !(WeightingFactor == rhs.WeightingFactor))
      return false;
    return true;
  }
  bool operator != (const MVelocityConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MVelocityConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MVelocityConstraint &a, MVelocityConstraint &b);

std::ostream& operator<<(std::ostream& out, const MVelocityConstraint& obj);

typedef struct _MAccelerationConstraint__isset {
  _MAccelerationConstraint__isset() : ParentToConstraint(false), TranslationalAcceleration(false), RotationalAcceleration(false), WeightingFactor(false) {}
  bool ParentToConstraint :1;
  bool TranslationalAcceleration :1;
  bool RotationalAcceleration :1;
  bool WeightingFactor :1;
} _MAccelerationConstraint__isset;

class MAccelerationConstraint : public virtual ::apache::thrift::TBase {
 public:

  MAccelerationConstraint(const MAccelerationConstraint&);
  MAccelerationConstraint& operator=(const MAccelerationConstraint&);
  MAccelerationConstraint() : ParentObjectID(), WeightingFactor(0) {
  }

  virtual ~MAccelerationConstraint() noexcept;
  std::string ParentObjectID;
   ::MMIStandard::MTransform ParentToConstraint;
   ::MMIStandard::MVector3 TranslationalAcceleration;
   ::MMIStandard::MVector3 RotationalAcceleration;
  double WeightingFactor;

  _MAccelerationConstraint__isset __isset;

  void __set_ParentObjectID(const std::string& val);

  void __set_ParentToConstraint(const  ::MMIStandard::MTransform& val);

  void __set_TranslationalAcceleration(const  ::MMIStandard::MVector3& val);

  void __set_RotationalAcceleration(const  ::MMIStandard::MVector3& val);

  void __set_WeightingFactor(const double val);

  bool operator == (const MAccelerationConstraint & rhs) const
  {
    if (!(ParentObjectID == rhs.ParentObjectID))
      return false;
    if (__isset.ParentToConstraint != rhs.__isset.ParentToConstraint)
      return false;
    else if (__isset.ParentToConstraint && !(ParentToConstraint == rhs.ParentToConstraint))
      return false;
    if (__isset.TranslationalAcceleration != rhs.__isset.TranslationalAcceleration)
      return false;
    else if (__isset.TranslationalAcceleration && !(TranslationalAcceleration == rhs.TranslationalAcceleration))
      return false;
    if (__isset.RotationalAcceleration != rhs.__isset.RotationalAcceleration)
      return false;
    else if (__isset.RotationalAcceleration && !(RotationalAcceleration == rhs.RotationalAcceleration))
      return false;
    if (__isset.WeightingFactor != rhs.__isset.WeightingFactor)
      return false;
    else if (__isset.WeightingFactor && !(WeightingFactor == rhs.WeightingFactor))
      return false;
    return true;
  }
  bool operator != (const MAccelerationConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MAccelerationConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MAccelerationConstraint &a, MAccelerationConstraint &b);

std::ostream& operator<<(std::ostream& out, const MAccelerationConstraint& obj);

typedef struct _MPathConstraint__isset {
  _MPathConstraint__isset() : WeightingFactor(false) {}
  bool WeightingFactor :1;
} _MPathConstraint__isset;

class MPathConstraint : public virtual ::apache::thrift::TBase {
 public:

  MPathConstraint(const MPathConstraint&);
  MPathConstraint& operator=(const MPathConstraint&);
  MPathConstraint() : WeightingFactor(0) {
  }

  virtual ~MPathConstraint() noexcept;
  std::vector<MGeometryConstraint>  PolygonPoints;
  double WeightingFactor;

  _MPathConstraint__isset __isset;

  void __set_PolygonPoints(const std::vector<MGeometryConstraint> & val);

  void __set_WeightingFactor(const double val);

  bool operator == (const MPathConstraint & rhs) const
  {
    if (!(PolygonPoints == rhs.PolygonPoints))
      return false;
    if (__isset.WeightingFactor != rhs.__isset.WeightingFactor)
      return false;
    else if (__isset.WeightingFactor && !(WeightingFactor == rhs.WeightingFactor))
      return false;
    return true;
  }
  bool operator != (const MPathConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MPathConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MPathConstraint &a, MPathConstraint &b);

std::ostream& operator<<(std::ostream& out, const MPathConstraint& obj);

typedef struct _MJointConstraint__isset {
  _MJointConstraint__isset() : GeometryConstraint(false), VelocityConstraint(false), AccelerationConstraint(false) {}
  bool GeometryConstraint :1;
  bool VelocityConstraint :1;
  bool AccelerationConstraint :1;
} _MJointConstraint__isset;

class MJointConstraint : public virtual ::apache::thrift::TBase {
 public:

  MJointConstraint(const MJointConstraint&);
  MJointConstraint& operator=(const MJointConstraint&);
  MJointConstraint() : JointType(( ::MMIStandard::MJointType::type)0) {
  }

  virtual ~MJointConstraint() noexcept;
   ::MMIStandard::MJointType::type JointType;
  MGeometryConstraint GeometryConstraint;
  MVelocityConstraint VelocityConstraint;
  MAccelerationConstraint AccelerationConstraint;

  _MJointConstraint__isset __isset;

  void __set_JointType(const  ::MMIStandard::MJointType::type val);

  void __set_GeometryConstraint(const MGeometryConstraint& val);

  void __set_VelocityConstraint(const MVelocityConstraint& val);

  void __set_AccelerationConstraint(const MAccelerationConstraint& val);

  bool operator == (const MJointConstraint & rhs) const
  {
    if (!(JointType == rhs.JointType))
      return false;
    if (__isset.GeometryConstraint != rhs.__isset.GeometryConstraint)
      return false;
    else if (__isset.GeometryConstraint && !(GeometryConstraint == rhs.GeometryConstraint))
      return false;
    if (__isset.VelocityConstraint != rhs.__isset.VelocityConstraint)
      return false;
    else if (__isset.VelocityConstraint && !(VelocityConstraint == rhs.VelocityConstraint))
      return false;
    if (__isset.AccelerationConstraint != rhs.__isset.AccelerationConstraint)
      return false;
    else if (__isset.AccelerationConstraint && !(AccelerationConstraint == rhs.AccelerationConstraint))
      return false;
    return true;
  }
  bool operator != (const MJointConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MJointConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MJointConstraint &a, MJointConstraint &b);

std::ostream& operator<<(std::ostream& out, const MJointConstraint& obj);


class MJointPathConstraint : public virtual ::apache::thrift::TBase {
 public:

  MJointPathConstraint(const MJointPathConstraint&);
  MJointPathConstraint& operator=(const MJointPathConstraint&);
  MJointPathConstraint() : JointType(( ::MMIStandard::MJointType::type)0) {
  }

  virtual ~MJointPathConstraint() noexcept;
   ::MMIStandard::MJointType::type JointType;
  MPathConstraint PathConstraint;

  void __set_JointType(const  ::MMIStandard::MJointType::type val);

  void __set_PathConstraint(const MPathConstraint& val);

  bool operator == (const MJointPathConstraint & rhs) const
  {
    if (!(JointType == rhs.JointType))
      return false;
    if (!(PathConstraint == rhs.PathConstraint))
      return false;
    return true;
  }
  bool operator != (const MJointPathConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MJointPathConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MJointPathConstraint &a, MJointPathConstraint &b);

std::ostream& operator<<(std::ostream& out, const MJointPathConstraint& obj);

typedef struct _MPostureConstraint__isset {
  _MPostureConstraint__isset() : JointConstraints(false) {}
  bool JointConstraints :1;
} _MPostureConstraint__isset;

class MPostureConstraint : public virtual ::apache::thrift::TBase {
 public:

  MPostureConstraint(const MPostureConstraint&);
  MPostureConstraint& operator=(const MPostureConstraint&);
  MPostureConstraint() {
  }

  virtual ~MPostureConstraint() noexcept;
   ::MMIStandard::MAvatarPostureValues posture;
  std::vector<MJointConstraint>  JointConstraints;

  _MPostureConstraint__isset __isset;

  void __set_posture(const  ::MMIStandard::MAvatarPostureValues& val);

  void __set_JointConstraints(const std::vector<MJointConstraint> & val);

  bool operator == (const MPostureConstraint & rhs) const
  {
    if (!(posture == rhs.posture))
      return false;
    if (__isset.JointConstraints != rhs.__isset.JointConstraints)
      return false;
    else if (__isset.JointConstraints && !(JointConstraints == rhs.JointConstraints))
      return false;
    return true;
  }
  bool operator != (const MPostureConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MPostureConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MPostureConstraint &a, MPostureConstraint &b);

std::ostream& operator<<(std::ostream& out, const MPostureConstraint& obj);

typedef struct _MConstraint__isset {
  _MConstraint__isset() : GeometryConstraint(false), VelocityConstraint(false), AccelerationConstraint(false), PathConstraint(false), JointPathConstraint(false), PostureConstraint(false), JointConstraint(false), Properties(false) {}
  bool GeometryConstraint :1;
  bool VelocityConstraint :1;
  bool AccelerationConstraint :1;
  bool PathConstraint :1;
  bool JointPathConstraint :1;
  bool PostureConstraint :1;
  bool JointConstraint :1;
  bool Properties :1;
} _MConstraint__isset;

class MConstraint : public virtual ::apache::thrift::TBase {
 public:

  MConstraint(const MConstraint&);
  MConstraint& operator=(const MConstraint&);
  MConstraint() : ID() {
  }

  virtual ~MConstraint() noexcept;
  std::string ID;
  MGeometryConstraint GeometryConstraint;
  MVelocityConstraint VelocityConstraint;
  MAccelerationConstraint AccelerationConstraint;
  MPathConstraint PathConstraint;
  MJointPathConstraint JointPathConstraint;
  MPostureConstraint PostureConstraint;
  MJointConstraint JointConstraint;
  std::map<std::string, std::string>  Properties;

  _MConstraint__isset __isset;

  void __set_ID(const std::string& val);

  void __set_GeometryConstraint(const MGeometryConstraint& val);

  void __set_VelocityConstraint(const MVelocityConstraint& val);

  void __set_AccelerationConstraint(const MAccelerationConstraint& val);

  void __set_PathConstraint(const MPathConstraint& val);

  void __set_JointPathConstraint(const MJointPathConstraint& val);

  void __set_PostureConstraint(const MPostureConstraint& val);

  void __set_JointConstraint(const MJointConstraint& val);

  void __set_Properties(const std::map<std::string, std::string> & val);

  bool operator == (const MConstraint & rhs) const
  {
    if (!(ID == rhs.ID))
      return false;
    if (__isset.GeometryConstraint != rhs.__isset.GeometryConstraint)
      return false;
    else if (__isset.GeometryConstraint && !(GeometryConstraint == rhs.GeometryConstraint))
      return false;
    if (__isset.VelocityConstraint != rhs.__isset.VelocityConstraint)
      return false;
    else if (__isset.VelocityConstraint && !(VelocityConstraint == rhs.VelocityConstraint))
      return false;
    if (__isset.AccelerationConstraint != rhs.__isset.AccelerationConstraint)
      return false;
    else if (__isset.AccelerationConstraint && !(AccelerationConstraint == rhs.AccelerationConstraint))
      return false;
    if (__isset.PathConstraint != rhs.__isset.PathConstraint)
      return false;
    else if (__isset.PathConstraint && !(PathConstraint == rhs.PathConstraint))
      return false;
    if (__isset.JointPathConstraint != rhs.__isset.JointPathConstraint)
      return false;
    else if (__isset.JointPathConstraint && !(JointPathConstraint == rhs.JointPathConstraint))
      return false;
    if (__isset.PostureConstraint != rhs.__isset.PostureConstraint)
      return false;
    else if (__isset.PostureConstraint && !(PostureConstraint == rhs.PostureConstraint))
      return false;
    if (__isset.JointConstraint != rhs.__isset.JointConstraint)
      return false;
    else if (__isset.JointConstraint && !(JointConstraint == rhs.JointConstraint))
      return false;
    if (__isset.Properties != rhs.__isset.Properties)
      return false;
    else if (__isset.Properties && !(Properties == rhs.Properties))
      return false;
    return true;
  }
  bool operator != (const MConstraint &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MConstraint & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

  virtual void printTo(std::ostream& out) const;
};

void swap(MConstraint &a, MConstraint &b);

std::ostream& operator<<(std::ostream& out, const MConstraint& obj);

} // namespace

#endif
