// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser

package ThriftClients;

import de.mosim.mmi.register.MMIRegisterService;
import org.apache.thrift.protocol.TCompactProtocol;
import org.apache.thrift.protocol.TProtocol;
import org.apache.thrift.transport.TSocket;
import org.apache.thrift.transport.TTransport;
import org.apache.thrift.transport.TTransportException;

public class MMIRegisterServiceClient implements AutoCloseable {


    private final String address;
    private final int port;
    /// <summary>
    /// The access of the client
    /// </summary>
    public MMIRegisterService.Client Access;
    private TTransport transport;

    public MMIRegisterServiceClient(String address, int port) {
        this.address = address;
        this.port = port;

        this.initialize(true);
    }

    public MMIRegisterServiceClient(String address, int port, boolean autoOpen) {
        this.address = address;
        this.port = port;

        this.initialize(autoOpen);
    }

    private void initialize(boolean autoOpen) {
        this.transport = new TSocket(this.address, this.port);
        TProtocol protocol = new TCompactProtocol(transport);
        this.Access = new MMIRegisterService.Client(protocol);

        if (autoOpen)
            this.start();
    }

    /// <summary>
    /// Starts the client
    /// </summary>
    public void start() {
        try {
            if (!this.transport.isOpen())
                transport.open();
        } catch (TTransportException e) {
            e.printStackTrace();
        }
    }

    @Override
    public void close() {
        transport.close();
    }

}
