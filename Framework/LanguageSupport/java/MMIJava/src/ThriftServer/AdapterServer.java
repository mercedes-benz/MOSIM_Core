// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser


package ThriftServer;


import de.mosim.mmi.*;
import de.mosim.mmi.register.MMIAdapter;
import org.apache.thrift.protocol.TCompactProtocol;
import org.apache.thrift.server.TThreadPoolServer;
import org.apache.thrift.transport.TServerSocket;
import org.apache.thrift.transport.TServerTransport;
import org.apache.thrift.transport.TTransportException;
import org.apache.thrift.transport.TTransportFactory;


public class AdapterServer implements AutoCloseable {

    /**
     * Basic class which represents the adapter server
     */

    //  TCP Port of the server
    private final int port;

    //  The server itself
    private TThreadPoolServer server;

    //  The implementation, which is hosted by the server
    private final MMIAdapter.Iface implementation;

    //  Constructor
    public AdapterServer(int port, MMIAdapter.Iface implementation) {
        this.port = port;
        this.implementation = implementation;
    }

    //  Starts the server
    public void Start() {
        //start a ThreadPoolServer
        try {
            MMIAdapter.Processor processor = new MMIAdapter.Processor(implementation);
            TServerTransport serverTransport = new TServerSocket(port);
            this.server = new TThreadPoolServer(new TThreadPoolServer.Args(serverTransport).processor(processor).transportFactory(new TTransportFactory()).protocolFactory(new TCompactProtocol.Factory()));
            this.server.serve();
        } catch (TTransportException e) {
            //e.printStackTrace();
        }
    }

    public void close() {
        this.server.stop();
    }
}
