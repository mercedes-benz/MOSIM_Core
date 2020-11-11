/**
 * Autogenerated by Thrift Compiler (0.13.0)
 *
 * DO NOT EDIT UNLESS YOU ARE SURE THAT YOU KNOW WHAT YOU ARE DOING
 *  @generated
 */
#ifndef MBlendingService_H
#define MBlendingService_H

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

class MBlendingServiceIf : virtual public MMIServiceBaseIf {
 public:
  virtual ~MBlendingServiceIf() {}
  virtual void SetBlendingMask( ::MMIStandard::MBoolResponse& _return, const std::map< ::MMIStandard::MTransform, double> & mask, const std::string& avatarID) = 0;
  virtual void Blend( ::MMIStandard::MAvatarPostureValues& _return, const  ::MMIStandard::MAvatarPostureValues& startPosture, const  ::MMIStandard::MAvatarPostureValues& targetPosture, const double weight) = 0;
};

class MBlendingServiceIfFactory : virtual public MMIServiceBaseIfFactory {
 public:
  typedef MBlendingServiceIf Handler;

  virtual ~MBlendingServiceIfFactory() {}

  virtual MBlendingServiceIf* getHandler(const ::apache::thrift::TConnectionInfo& connInfo) = 0;
  virtual void releaseHandler(MMIServiceBaseIf* /* handler */) = 0;
};

class MBlendingServiceIfSingletonFactory : virtual public MBlendingServiceIfFactory {
 public:
  MBlendingServiceIfSingletonFactory(const ::std::shared_ptr<MBlendingServiceIf>& iface) : iface_(iface) {}
  virtual ~MBlendingServiceIfSingletonFactory() {}

  virtual MBlendingServiceIf* getHandler(const ::apache::thrift::TConnectionInfo&) {
    return iface_.get();
  }
  virtual void releaseHandler(MMIServiceBaseIf* /* handler */) {}

 protected:
  ::std::shared_ptr<MBlendingServiceIf> iface_;
};

class MBlendingServiceNull : virtual public MBlendingServiceIf , virtual public MMIServiceBaseNull {
 public:
  virtual ~MBlendingServiceNull() {}
  void SetBlendingMask( ::MMIStandard::MBoolResponse& /* _return */, const std::map< ::MMIStandard::MTransform, double> & /* mask */, const std::string& /* avatarID */) {
    return;
  }
  void Blend( ::MMIStandard::MAvatarPostureValues& /* _return */, const  ::MMIStandard::MAvatarPostureValues& /* startPosture */, const  ::MMIStandard::MAvatarPostureValues& /* targetPosture */, const double /* weight */) {
    return;
  }
};

typedef struct _MBlendingService_SetBlendingMask_args__isset {
  _MBlendingService_SetBlendingMask_args__isset() : mask(false), avatarID(false) {}
  bool mask :1;
  bool avatarID :1;
} _MBlendingService_SetBlendingMask_args__isset;

class MBlendingService_SetBlendingMask_args {
 public:

  MBlendingService_SetBlendingMask_args(const MBlendingService_SetBlendingMask_args&);
  MBlendingService_SetBlendingMask_args& operator=(const MBlendingService_SetBlendingMask_args&);
  MBlendingService_SetBlendingMask_args() : avatarID() {
  }

  virtual ~MBlendingService_SetBlendingMask_args() noexcept;
  std::map< ::MMIStandard::MTransform, double>  mask;
  std::string avatarID;

  _MBlendingService_SetBlendingMask_args__isset __isset;

  void __set_mask(const std::map< ::MMIStandard::MTransform, double> & val);

  void __set_avatarID(const std::string& val);

  bool operator == (const MBlendingService_SetBlendingMask_args & rhs) const
  {
    if (!(mask == rhs.mask))
      return false;
    if (!(avatarID == rhs.avatarID))
      return false;
    return true;
  }
  bool operator != (const MBlendingService_SetBlendingMask_args &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MBlendingService_SetBlendingMask_args & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};


class MBlendingService_SetBlendingMask_pargs {
 public:


  virtual ~MBlendingService_SetBlendingMask_pargs() noexcept;
  const std::map< ::MMIStandard::MTransform, double> * mask;
  const std::string* avatarID;

  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MBlendingService_SetBlendingMask_result__isset {
  _MBlendingService_SetBlendingMask_result__isset() : success(false) {}
  bool success :1;
} _MBlendingService_SetBlendingMask_result__isset;

class MBlendingService_SetBlendingMask_result {
 public:

  MBlendingService_SetBlendingMask_result(const MBlendingService_SetBlendingMask_result&);
  MBlendingService_SetBlendingMask_result& operator=(const MBlendingService_SetBlendingMask_result&);
  MBlendingService_SetBlendingMask_result() {
  }

  virtual ~MBlendingService_SetBlendingMask_result() noexcept;
   ::MMIStandard::MBoolResponse success;

  _MBlendingService_SetBlendingMask_result__isset __isset;

  void __set_success(const  ::MMIStandard::MBoolResponse& val);

  bool operator == (const MBlendingService_SetBlendingMask_result & rhs) const
  {
    if (!(success == rhs.success))
      return false;
    return true;
  }
  bool operator != (const MBlendingService_SetBlendingMask_result &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MBlendingService_SetBlendingMask_result & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MBlendingService_SetBlendingMask_presult__isset {
  _MBlendingService_SetBlendingMask_presult__isset() : success(false) {}
  bool success :1;
} _MBlendingService_SetBlendingMask_presult__isset;

class MBlendingService_SetBlendingMask_presult {
 public:


  virtual ~MBlendingService_SetBlendingMask_presult() noexcept;
   ::MMIStandard::MBoolResponse* success;

  _MBlendingService_SetBlendingMask_presult__isset __isset;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);

};

typedef struct _MBlendingService_Blend_args__isset {
  _MBlendingService_Blend_args__isset() : startPosture(false), targetPosture(false), weight(false) {}
  bool startPosture :1;
  bool targetPosture :1;
  bool weight :1;
} _MBlendingService_Blend_args__isset;

class MBlendingService_Blend_args {
 public:

  MBlendingService_Blend_args(const MBlendingService_Blend_args&);
  MBlendingService_Blend_args& operator=(const MBlendingService_Blend_args&);
  MBlendingService_Blend_args() : weight(0) {
  }

  virtual ~MBlendingService_Blend_args() noexcept;
   ::MMIStandard::MAvatarPostureValues startPosture;
   ::MMIStandard::MAvatarPostureValues targetPosture;
  double weight;

  _MBlendingService_Blend_args__isset __isset;

  void __set_startPosture(const  ::MMIStandard::MAvatarPostureValues& val);

  void __set_targetPosture(const  ::MMIStandard::MAvatarPostureValues& val);

  void __set_weight(const double val);

  bool operator == (const MBlendingService_Blend_args & rhs) const
  {
    if (!(startPosture == rhs.startPosture))
      return false;
    if (!(targetPosture == rhs.targetPosture))
      return false;
    if (!(weight == rhs.weight))
      return false;
    return true;
  }
  bool operator != (const MBlendingService_Blend_args &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MBlendingService_Blend_args & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};


class MBlendingService_Blend_pargs {
 public:


  virtual ~MBlendingService_Blend_pargs() noexcept;
  const  ::MMIStandard::MAvatarPostureValues* startPosture;
  const  ::MMIStandard::MAvatarPostureValues* targetPosture;
  const double* weight;

  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MBlendingService_Blend_result__isset {
  _MBlendingService_Blend_result__isset() : success(false) {}
  bool success :1;
} _MBlendingService_Blend_result__isset;

class MBlendingService_Blend_result {
 public:

  MBlendingService_Blend_result(const MBlendingService_Blend_result&);
  MBlendingService_Blend_result& operator=(const MBlendingService_Blend_result&);
  MBlendingService_Blend_result() {
  }

  virtual ~MBlendingService_Blend_result() noexcept;
   ::MMIStandard::MAvatarPostureValues success;

  _MBlendingService_Blend_result__isset __isset;

  void __set_success(const  ::MMIStandard::MAvatarPostureValues& val);

  bool operator == (const MBlendingService_Blend_result & rhs) const
  {
    if (!(success == rhs.success))
      return false;
    return true;
  }
  bool operator != (const MBlendingService_Blend_result &rhs) const {
    return !(*this == rhs);
  }

  bool operator < (const MBlendingService_Blend_result & ) const;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);
  uint32_t write(::apache::thrift::protocol::TProtocol* oprot) const;

};

typedef struct _MBlendingService_Blend_presult__isset {
  _MBlendingService_Blend_presult__isset() : success(false) {}
  bool success :1;
} _MBlendingService_Blend_presult__isset;

class MBlendingService_Blend_presult {
 public:


  virtual ~MBlendingService_Blend_presult() noexcept;
   ::MMIStandard::MAvatarPostureValues* success;

  _MBlendingService_Blend_presult__isset __isset;

  uint32_t read(::apache::thrift::protocol::TProtocol* iprot);

};

class MBlendingServiceClient : virtual public MBlendingServiceIf, public MMIServiceBaseClient {
 public:
  MBlendingServiceClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> prot) :
    MMIServiceBaseClient(prot, prot) {}
  MBlendingServiceClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> iprot, std::shared_ptr< ::apache::thrift::protocol::TProtocol> oprot) :    MMIServiceBaseClient(iprot, oprot) {}
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getInputProtocol() {
    return piprot_;
  }
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getOutputProtocol() {
    return poprot_;
  }
  void SetBlendingMask( ::MMIStandard::MBoolResponse& _return, const std::map< ::MMIStandard::MTransform, double> & mask, const std::string& avatarID);
  void send_SetBlendingMask(const std::map< ::MMIStandard::MTransform, double> & mask, const std::string& avatarID);
  void recv_SetBlendingMask( ::MMIStandard::MBoolResponse& _return);
  void Blend( ::MMIStandard::MAvatarPostureValues& _return, const  ::MMIStandard::MAvatarPostureValues& startPosture, const  ::MMIStandard::MAvatarPostureValues& targetPosture, const double weight);
  void send_Blend(const  ::MMIStandard::MAvatarPostureValues& startPosture, const  ::MMIStandard::MAvatarPostureValues& targetPosture, const double weight);
  void recv_Blend( ::MMIStandard::MAvatarPostureValues& _return);
};

class MBlendingServiceProcessor : public MMIServiceBaseProcessor {
 protected:
  ::std::shared_ptr<MBlendingServiceIf> iface_;
  virtual bool dispatchCall(::apache::thrift::protocol::TProtocol* iprot, ::apache::thrift::protocol::TProtocol* oprot, const std::string& fname, int32_t seqid, void* callContext);
 private:
  typedef  void (MBlendingServiceProcessor::*ProcessFunction)(int32_t, ::apache::thrift::protocol::TProtocol*, ::apache::thrift::protocol::TProtocol*, void*);
  typedef std::map<std::string, ProcessFunction> ProcessMap;
  ProcessMap processMap_;
  void process_SetBlendingMask(int32_t seqid, ::apache::thrift::protocol::TProtocol* iprot, ::apache::thrift::protocol::TProtocol* oprot, void* callContext);
  void process_Blend(int32_t seqid, ::apache::thrift::protocol::TProtocol* iprot, ::apache::thrift::protocol::TProtocol* oprot, void* callContext);
 public:
  MBlendingServiceProcessor(::std::shared_ptr<MBlendingServiceIf> iface) :
    MMIServiceBaseProcessor(iface),
    iface_(iface) {
    processMap_["SetBlendingMask"] = &MBlendingServiceProcessor::process_SetBlendingMask;
    processMap_["Blend"] = &MBlendingServiceProcessor::process_Blend;
  }

  virtual ~MBlendingServiceProcessor() {}
};

class MBlendingServiceProcessorFactory : public ::apache::thrift::TProcessorFactory {
 public:
  MBlendingServiceProcessorFactory(const ::std::shared_ptr< MBlendingServiceIfFactory >& handlerFactory) :
      handlerFactory_(handlerFactory) {}

  ::std::shared_ptr< ::apache::thrift::TProcessor > getProcessor(const ::apache::thrift::TConnectionInfo& connInfo);

 protected:
  ::std::shared_ptr< MBlendingServiceIfFactory > handlerFactory_;
};

class MBlendingServiceMultiface : virtual public MBlendingServiceIf, public MMIServiceBaseMultiface {
 public:
  MBlendingServiceMultiface(std::vector<std::shared_ptr<MBlendingServiceIf> >& ifaces) : ifaces_(ifaces) {
    std::vector<std::shared_ptr<MBlendingServiceIf> >::iterator iter;
    for (iter = ifaces.begin(); iter != ifaces.end(); ++iter) {
      MMIServiceBaseMultiface::add(*iter);
    }
  }
  virtual ~MBlendingServiceMultiface() {}
 protected:
  std::vector<std::shared_ptr<MBlendingServiceIf> > ifaces_;
  MBlendingServiceMultiface() {}
  void add(::std::shared_ptr<MBlendingServiceIf> iface) {
    MMIServiceBaseMultiface::add(iface);
    ifaces_.push_back(iface);
  }
 public:
  void SetBlendingMask( ::MMIStandard::MBoolResponse& _return, const std::map< ::MMIStandard::MTransform, double> & mask, const std::string& avatarID) {
    size_t sz = ifaces_.size();
    size_t i = 0;
    for (; i < (sz - 1); ++i) {
      ifaces_[i]->SetBlendingMask(_return, mask, avatarID);
    }
    ifaces_[i]->SetBlendingMask(_return, mask, avatarID);
    return;
  }

  void Blend( ::MMIStandard::MAvatarPostureValues& _return, const  ::MMIStandard::MAvatarPostureValues& startPosture, const  ::MMIStandard::MAvatarPostureValues& targetPosture, const double weight) {
    size_t sz = ifaces_.size();
    size_t i = 0;
    for (; i < (sz - 1); ++i) {
      ifaces_[i]->Blend(_return, startPosture, targetPosture, weight);
    }
    ifaces_[i]->Blend(_return, startPosture, targetPosture, weight);
    return;
  }

};

// The 'concurrent' client is a thread safe client that correctly handles
// out of order responses.  It is slower than the regular client, so should
// only be used when you need to share a connection among multiple threads
class MBlendingServiceConcurrentClient : virtual public MBlendingServiceIf, public MMIServiceBaseConcurrentClient {
 public:
  MBlendingServiceConcurrentClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> prot, std::shared_ptr<::apache::thrift::async::TConcurrentClientSyncInfo> sync) :
    MMIServiceBaseConcurrentClient(prot, prot, sync) {}
  MBlendingServiceConcurrentClient(std::shared_ptr< ::apache::thrift::protocol::TProtocol> iprot, std::shared_ptr< ::apache::thrift::protocol::TProtocol> oprot, std::shared_ptr<::apache::thrift::async::TConcurrentClientSyncInfo> sync) :    MMIServiceBaseConcurrentClient(iprot, oprot, sync) {}
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getInputProtocol() {
    return piprot_;
  }
  std::shared_ptr< ::apache::thrift::protocol::TProtocol> getOutputProtocol() {
    return poprot_;
  }
  void SetBlendingMask( ::MMIStandard::MBoolResponse& _return, const std::map< ::MMIStandard::MTransform, double> & mask, const std::string& avatarID);
  int32_t send_SetBlendingMask(const std::map< ::MMIStandard::MTransform, double> & mask, const std::string& avatarID);
  void recv_SetBlendingMask( ::MMIStandard::MBoolResponse& _return, const int32_t seqid);
  void Blend( ::MMIStandard::MAvatarPostureValues& _return, const  ::MMIStandard::MAvatarPostureValues& startPosture, const  ::MMIStandard::MAvatarPostureValues& targetPosture, const double weight);
  int32_t send_Blend(const  ::MMIStandard::MAvatarPostureValues& startPosture, const  ::MMIStandard::MAvatarPostureValues& targetPosture, const double weight);
  void recv_Blend( ::MMIStandard::MAvatarPostureValues& _return, const int32_t seqid);
};

#ifdef _MSC_VER
  #pragma warning( pop )
#endif

} // namespace

#endif
