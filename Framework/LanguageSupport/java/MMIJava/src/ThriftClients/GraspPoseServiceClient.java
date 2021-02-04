// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser

package ThriftClients;

import de.mosim.mmi.services.MGraspPoseService;
import org.apache.thrift.protocol.TCompactProtocol;
import org.apache.thrift.protocol.TProtocol;
import org.apache.thrift.transport.TSocket;
import org.apache.thrift.transport.TTransport;
import org.apache.thrift.transport.TTransportException;

public class GraspPoseServiceClient implements AutoCloseable {

    private final String address;
    private final int port;
    /// <summary>
    /// The access of the client
    /// </summary>
    public MGraspPoseService.Client Access;
    private TTransport transport;

    public GraspPoseServiceClient(String address, int port) {
        this.address = address;
        this.port = port;

        this.initialize(true);
    }

    public GraspPoseServiceClient(String address, int port, boolean autoOPen) {
        this.address = address;
        this.port = port;

        this.initialize(autoOPen);
    }

    private void initialize(boolean autoOPen) {
        this.transport = new TSocket(this.address, this.port);
        TProtocol protocol = new TCompactProtocol(transport);
        this.Access = new MGraspPoseService.Client(protocol);

        if (autoOPen)
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
