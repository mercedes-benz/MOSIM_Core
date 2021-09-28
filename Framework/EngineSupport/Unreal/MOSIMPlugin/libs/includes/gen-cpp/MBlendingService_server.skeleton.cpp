// This autogenerated skeleton file illustrates how to build a server.
// You should copy it to another filename to avoid overwriting it.

#include "MBlendingService.h"
#include <thrift/protocol/TBinaryProtocol.h>
#include <thrift/server/TSimpleServer.h>
#include <thrift/transport/TServerSocket.h>
#include <thrift/transport/TBufferTransports.h>

using namespace ::apache::thrift;
using namespace ::apache::thrift::protocol;
using namespace ::apache::thrift::transport;
using namespace ::apache::thrift::server;

using namespace  ::MMIStandard;

class MBlendingServiceHandler : virtual public MBlendingServiceIf {
 public:
  MBlendingServiceHandler() {
    // Your initialization goes here
  }

  void SetBlendingMask( ::MMIStandard::MBoolResponse& _return, const std::map< ::MMIStandard::MJointType::type, double> & mask, const std::string& avatarID) {
    // Your implementation goes here
    printf("SetBlendingMask\n");
  }

  void Blend( ::MMIStandard::MAvatarPostureValues& _return, const  ::MMIStandard::MAvatarPostureValues& startPosture, const  ::MMIStandard::MAvatarPostureValues& targetPosture, const double weight) {
    // Your implementation goes here
    printf("Blend\n");
  }

};

int main(int argc, char **argv) {
  int port = 9090;
  ::std::shared_ptr<MBlendingServiceHandler> handler(new MBlendingServiceHandler());
  ::std::shared_ptr<TProcessor> processor(new MBlendingServiceProcessor(handler));
  ::std::shared_ptr<TServerTransport> serverTransport(new TServerSocket(port));
  ::std::shared_ptr<TTransportFactory> transportFactory(new TBufferedTransportFactory());
  ::std::shared_ptr<TProtocolFactory> protocolFactory(new TBinaryProtocolFactory());

  TSimpleServer server(processor, serverTransport, transportFactory, protocolFactory);
  server.serve();
  return 0;
}

