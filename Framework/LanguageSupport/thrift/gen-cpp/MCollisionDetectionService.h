/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
#ifndef MCollisionDetectionService_H
#define MCollisionDetectionService_H

#include <thrift/TDispatchProcessor.h>
#include <thrift/async/TConcurrentClientSyncInfo.h>
#include <memory>
#include "services_types.h"
#include "MMIServiceBase.h"

namespace MMIStandard {

#ifdef _MSC_VER
  #pragma warning( push )
  #pragma warning (disable : 4250 ) //inheriting methods via dominance 
#endif

class MCollisionDetectionServiceIf : virtual public MMIServiceBaseIf {
 public:
  virtual ~MCollisionDetectionServiceIf() {}
  virtual void ComputePenetration( ::MMIStandard::MVector3& _return, const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB) = 0;
  virtual bool CausesCollision(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB) = 0;
};

class MCollisionDetectionServiceIfFactory : virtual public MMIServiceBaseIfFactory {
 public:
  typedef MCollisionDetectionServiceIf Handler;

  virtual ~MCollisionDetectionServiceIfFactory() {}

  virtual MCollisionDetectionServiceIf* getHandler(const ::apache::thrift::TConnectionInfo& connInfo) = 0;
  virtual void releaseHandler(MMIServiceBaseIf* /* handler */) = 0;
};

class MCollisionDetectionServiceIfSingletonFactory : virtual public MCollisionDetectionServiceIfFactory {
 public:
  MCollisionDetectionServiceIfSingletonFactory(const ::std::shared_ptr<MCollisionDetectionServiceIf>& iface) : iface_(iface) {}
  virtual ~MCollisionDetectionServiceIfSingletonFactory() {}

  virtual MCollisionDetectionServiceIf* getHandler(const ::apache::thrift::TConnectionInfo&) {
    return iface_.get();
  }
  virtual void releaseHandler(MMIServiceBaseIf* /* handler */) {}

 protected:
  ::std::shared_ptr<MCollisionDetectionServiceIf> iface_;
};

class MCollisionDetectionServiceNull : virtual public MCollisionDetectionServiceIf , virtual public MMIServiceBaseNull {
 public:
  virtual ~MCollisionDetectionServiceNull() {}
  void ComputePenetration( ::MMIStandard::MVector3& /* _return */, const  ::MMIStandard::MCollider& /* colliderA */, const  ::MMIStandard::MTransform& /* transformA */, const  ::MMIStandard::MCollider& /* colliderB */, const  ::MMIStandard::MTransform& /* transformB */) {
    return;
  }
  bool CausesCollision(const  ::MMIStandard::MCollider& /* colliderA */, const  ::MMIStandard::MTransform& /* transformA */, const  ::MMIStandard::MCollider& /* colliderB */, const  ::MMIStandard::MTransform& /* transformB */) {
    bool _return = false;
    return _return;
  }
};

typedef struct _MCollisionDetectionService_ComputePenetration_args__isset {
  _MCollisionDetectionService_ComputePenetration_args__isset() : colliderA(false), transformA(false), colliderB(false), transformB(false) {}
  bool colliderA :1;
  bool transformA :1;
  bool colliderB :1;
  bool transformB :1;
} _MCollisionDetectionService_ComputePenetration_args__isset;

class MCollisionDetectionService_ComputePenetration_args {
 public:

  MCollisionDetectionService_ComputePenetration_args(const MCollisionDetectionService_ComputePenetration_args&);
  MCollisionDetectionService_ComputePenetration_args& operator=(const MCollisionDetectionService_ComputePenetration_args&);
  MCollisionDetectionService_ComputePenetration_args() {
  }

  virtual ~MCollisionDetectionService_ComputePenetration_args() noexcept;
   ::MMIStandard::MCollider colliderA;
   ::MMIStandard::MTransform transformA;
   ::MMIStandard::MCollider colliderB;
   ::MMIStandard::MTransform transformB;

  _MCollisionDetectionService_ComputePenetration_args__isset __isset;

  void __set_colliderA(const  ::MMIStandard::MCollider& val);

  void __set_transformA(const  ::MMIStandard::MTransform& val);

  void __set_colliderB(const  ::MMIStandard::MCollider& val);

  void __set_transformB(const  ::MMIStandard::MTransform& val);

  bool operator == (const MCollisionDetectionService_ComputePenetration_args & rhs) const
  {
    if (!(colliderA == rhs.colliderA))
      return false;
    if (!(transformA == rhs.transformA))
      return false;
    if (!(colliderB == rhs.colliderB))
      return false;
    if (!(transformB == rhs.transformB))
      return false;
    return true;
  }
  bool operator != (const MCollisionDetectionService_ComputePenetration_args &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MCollisionDetectionService_ComputePenetration_args & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};


class MCollisionDetectionService_ComputePenetration_pargs {
 public:


  virtual ~MCollisionDetectionService_ComputePenetration_pargs() noexcept;
  const  ::MMIStandard::MCollider* colliderA;
  const  ::MMIStandard::MTransform* transformA;
  const  ::MMIStandard::MCollider* colliderB;
  const  ::MMIStandard::MTransform* transformB;

  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MCollisionDetectionService_ComputePenetration_result__isset {
  _MCollisionDetectionService_ComputePenetration_result__isset() : success(false) {}
  bool success :1;
} _MCollisionDetectionService_ComputePenetration_result__isset;

class MCollisionDetectionService_ComputePenetration_result {
 public:

  MCollisionDetectionService_ComputePenetration_result(const MCollisionDetectionService_ComputePenetration_result&);
  MCollisionDetectionService_ComputePenetration_result& operator=(const MCollisionDetectionService_ComputePenetration_result&);
  MCollisionDetectionService_ComputePenetration_result() {
  }

  virtual ~MCollisionDetectionService_ComputePenetration_result() noexcept;
   ::MMIStandard::MVector3 success;

  _MCollisionDetectionService_ComputePenetration_result__isset __isset;

  void __set_success(const  ::MMIStandard::MVector3& val);

  bool operator == (const MCollisionDetectionService_ComputePenetration_result & rhs) const
  {
    if (!(success == rhs.success))
      return false;
    return true;
  }
  bool operator != (const MCollisionDetectionService_ComputePenetration_result &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MCollisionDetectionService_ComputePenetration_result & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MCollisionDetectionService_ComputePenetration_presult__isset {
  _MCollisionDetectionService_ComputePenetration_presult__isset() : success(false) {}
  bool success :1;
} _MCollisionDetectionService_ComputePenetration_presult__isset;

class MCollisionDetectionService_ComputePenetration_presult {
 public:


  virtual ~MCollisionDetectionService_ComputePenetration_presult() noexcept;
   ::MMIStandard::MVector3* success;

  _MCollisionDetectionService_ComputePenetration_presult__isset __isset;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);

};

typedef struct _MCollisionDetectionService_CausesCollision_args__isset {
  _MCollisionDetectionService_CausesCollision_args__isset() : colliderA(false), transformA(false), colliderB(false), transformB(false) {}
  bool colliderA :1;
  bool transformA :1;
  bool colliderB :1;
  bool transformB :1;
} _MCollisionDetectionService_CausesCollision_args__isset;

class MCollisionDetectionService_CausesCollision_args {
 public:

  MCollisionDetectionService_CausesCollision_args(const MCollisionDetectionService_CausesCollision_args&);
  MCollisionDetectionService_CausesCollision_args& operator=(const MCollisionDetectionService_CausesCollision_args&);
  MCollisionDetectionService_CausesCollision_args() {
  }

  virtual ~MCollisionDetectionService_CausesCollision_args() noexcept;
   ::MMIStandard::MCollider colliderA;
   ::MMIStandard::MTransform transformA;
   ::MMIStandard::MCollider colliderB;
   ::MMIStandard::MTransform transformB;

  _MCollisionDetectionService_CausesCollision_args__isset __isset;

  void __set_colliderA(const  ::MMIStandard::MCollider& val);

  void __set_transformA(const  ::MMIStandard::MTransform& val);

  void __set_colliderB(const  ::MMIStandard::MCollider& val);

  void __set_transformB(const  ::MMIStandard::MTransform& val);

  bool operator == (const MCollisionDetectionService_CausesCollision_args & rhs) const
  {
    if (!(colliderA == rhs.colliderA))
      return false;
    if (!(transformA == rhs.transformA))
      return false;
    if (!(colliderB == rhs.colliderB))
      return false;
    if (!(transformB == rhs.transformB))
      return false;
    return true;
  }
  bool operator != (const MCollisionDetectionService_CausesCollision_args &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MCollisionDetectionService_CausesCollision_args & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};


class MCollisionDetectionService_CausesCollision_pargs {
 public:


  virtual ~MCollisionDetectionService_CausesCollision_pargs() noexcept;
  const  ::MMIStandard::MCollider* colliderA;
  const  ::MMIStandard::MTransform* transformA;
  const  ::MMIStandard::MCollider* colliderB;
  const  ::MMIStandard::MTransform* transformB;

  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MCollisionDetectionService_CausesCollision_result__isset {
  _MCollisionDetectionService_CausesCollision_result__isset() : success(false) {}
  bool success :1;
} _MCollisionDetectionService_CausesCollision_result__isset;

class MCollisionDetectionService_CausesCollision_result {
 public:

  MCollisionDetectionService_CausesCollision_result(const MCollisionDetectionService_CausesCollision_result&);
  MCollisionDetectionService_CausesCollision_result& operator=(const MCollisionDetectionService_CausesCollision_result&);
  MCollisionDetectionService_CausesCollision_result() : success(0) {
  }

  virtual ~MCollisionDetectionService_CausesCollision_result() noexcept;
  bool success;

  _MCollisionDetectionService_CausesCollision_result__isset __isset;

  void __set_success(const bool val);

  bool operator == (const MCollisionDetectionService_CausesCollision_result & rhs) const
  {
    if (!(success == rhs.success))
      return false;
    return true;
  }
  bool operator != (const MCollisionDetectionService_CausesCollision_result &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MCollisionDetectionService_CausesCollision_result & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MCollisionDetectionService_CausesCollision_presult__isset {
  _MCollisionDetectionService_CausesCollision_presult__isset() : success(false) {}
  bool success :1;
} _MCollisionDetectionService_CausesCollision_presult__isset;

class MCollisionDetectionService_CausesCollision_presult {
 public:


  virtual ~MCollisionDetectionService_CausesCollision_presult() noexcept;
  bool* success;

  _MCollisionDetectionService_CausesCollision_presult__isset __isset;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);

};

class MCollisionDetectionServiceClient : virtual public MCollisionDetectionServiceIf, public MMIServiceBaseClient {
 public:
  MCollisionDetectionServiceClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> prot) :
    MMIServiceBaseClient(prot, prot) {}
  MCollisionDetectionServiceClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> iprot, std::shared_ptr< ::apache::thrift::protocol::TProtocol> oprot) :    MMIServiceBaseClient(iprot, oprot) {}
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getInputProtocol() {
    return piprot_;
  }
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getOutputProtocol() {
    return poprot_;
  }
  void ComputePenetration( ::MMIStandard::MVector3& _return, const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  void send_ComputePenetration(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  void recv_ComputePenetration( ::MMIStandard::MVector3& _return);
  bool CausesCollision(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  void send_CausesCollision(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  bool recv_CausesCollision();
};

class MCollisionDetectionServiceProcessor : public MMIServiceBaseProcessor {
 protected:
  ::std::shared_ptr<MCollisionDetectionServiceIf> iface_;
  virtual bool dispatchCall(::apache::thrift::protocol::TProtocol* iprot, ::apache::thrift::protocol::TProtocol* oprot, const std::string& fname, int32_t seqid, void* callContext);
 private:
  typedef  void (MCollisionDetectionServiceProcessor::*ProcessFunction)(int32_t, ::apache::thrift::protocol::TProtocol*, ::apache::thrift::protocol::TProtocol*, void*);
  typedef std::map<std::string, ProcessFunction> ProcessMap;
  ProcessMap processMap_;
  void process_ComputePenetration(int32_t seqid, ::apache::thrift::protocol::TProtocol* iprot, ::apache::thrift::protocol::TProtocol* oprot, void* callContext);
  void process_CausesCollision(int32_t seqid, ::apache::thrift::protocol::TProtocol* iprot, ::apache::thrift::protocol::TProtocol* oprot, void* callContext);
 public:
  MCollisionDetectionServiceProcessor(::std::shared_ptr<MCollisionDetectionServiceIf> iface) :
    MMIServiceBaseProcessor(iface),
    iface_(iface) {
    processMap_["ComputePenetration"] = &MCollisionDetectionServiceProcessor::process_ComputePenetration;
    processMap_["CausesCollision"] = &MCollisionDetectionServiceProcessor::process_CausesCollision;
  }

  virtual ~MCollisionDetectionServiceProcessor() {}
};

class MCollisionDetectionServiceProcessorFactory : public ::apache::thrift::TProcessorFactory {
 public:
  MCollisionDetectionServiceProcessorFactory(const ::std::shared_ptr< MCollisionDetectionServiceIfFactory >& handlerFactory) :
      handlerFactory_(handlerFactory) {}

  ::std::shared_ptr< ::apache::thrift::TProcessor > getProcessor(const ::apache::thrift::TConnectionInfo& connInfo);

 protected:
  ::std::shared_ptr< MCollisionDetectionServiceIfFactory > handlerFactory_;
};

class MCollisionDetectionServiceMultiface : virtual public MCollisionDetectionServiceIf, public MMIServiceBaseMultiface {
 public:
  MCollisionDetectionServiceMultiface(std::vector<std::shared_ptr<MCollisionDetectionServiceIf> >& ifaces) : ifaces_(ifaces) {
    std::vector<std::shared_ptr<MCollisionDetectionServiceIf> >::iterator iter;
    for (iter = ifaces.begin(); iter != ifaces.end(); ++iter) {
      MMIServiceBaseMultiface::add(*iter);
    }
  }
  virtual ~MCollisionDetectionServiceMultiface() {}
 protected:
  std::vector<std::shared_ptr<MCollisionDetectionServiceIf> > ifaces_;
  MCollisionDetectionServiceMultiface() {}
  void add(::std::shared_ptr<MCollisionDetectionServiceIf> iface) {
    MMIServiceBaseMultiface::add(iface);
    ifaces_.push_back(iface);
  }
 public:
  void ComputePenetration( ::MMIStandard::MVector3& _return, const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB) {
    size_t sz = ifaces_.size();
    size_t i = 0;
    for (; i < (sz - 1); ++i) {
      ifaces_[i]->ComputePenetration(_return, colliderA, transformA, colliderB, transformB);
    }
    ifaces_[i]->ComputePenetration(_return, colliderA, transformA, colliderB, transformB);
    return;
  }

  bool CausesCollision(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB) {
    size_t sz = ifaces_.size();
    size_t i = 0;
    for (; i < (sz - 1); ++i) {
      ifaces_[i]->CausesCollision(colliderA, transformA, colliderB, transformB);
    }
    return ifaces_[i]->CausesCollision(colliderA, transformA, colliderB, transformB);
  }

};

// The 'concurrent' client is a thread safe client that correctly handles
// out of order responses.  It is slower than the regular client, so should
// only be used when you need to share a connection among multiple threads
class MCollisionDetectionServiceConcurrentClient : virtual public MCollisionDetectionServiceIf, public MMIServiceBaseConcurrentClient {
 public:
  MCollisionDetectionServiceConcurrentClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> prot, std::shared_ptr<::apache::thrift::async::TConcurrentClientSyncInfo> sync) :
    MMIServiceBaseConcurrentClient(prot, prot, sync) {}
  MCollisionDetectionServiceConcurrentClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> iprot, std::shared_ptr< ::apache::thrift::protocol::TProtocol> oprot, std::shared_ptr<::apache::thrift::async::TConcurrentClientSyncInfo> sync) :    MMIServiceBaseConcurrentClient(iprot, oprot, sync) {}
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getInputProtocol() {
    return piprot_;
  }
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getOutputProtocol() {
    return poprot_;
  }
  void ComputePenetration( ::MMIStandard::MVector3& _return, const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  int32_t send_ComputePenetration(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  void recv_ComputePenetration( ::MMIStandard::MVector3& _return, const int32_t seqid);
  bool CausesCollision(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  int32_t send_CausesCollision(const  ::MMIStandard::MCollider& colliderA, const  ::MMIStandard::MTransform& transformA, const  ::MMIStandard::MCollider& colliderB, const  ::MMIStandard::MTransform& transformB);
  bool recv_CausesCollision(const int32_t seqid);
};

#ifdef _MSC_VER
  #pragma warning( pop )
#endif

} // namespace

#endif
