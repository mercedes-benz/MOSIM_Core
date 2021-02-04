// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Andreas Kaiser, Felix Gaisbauer

package Adapter;

import de.mosim.mmi.*;
import ThriftClients.MMIRegisterServiceClient;
import ThriftServer.AdapterServer;
import Utils.LogLevel;
import Utils.Logger;
import de.mosim.mmi.core.MIPAddress;
import de.mosim.mmi.register.MAdapterDescription;
import de.mosim.mmi.register.MMIAdapter;
import org.apache.thrift.TException;

import java.io.IOException;
import java.nio.file.*;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

public class AdapterController implements AutoCloseable {
    //	The path of the mmus
    public static String mmuPath;
    /**
     * Basic class which:
     * creates the adapter description
     * initializes the adapter and the SessionData
     * registers the adapter at the MMIRegister
     * starts the FileWatcher
     * starts the AdapterServer
     */

    //	The helper class which instantiates the MMUs from file
    private static IMMUInstantiation MMUInstantiator;
    //	The address of the AdapterServer
    private final MIPAddress adapterAddress;
    //	The address of the MMiRegister
    private final MIPAddress registerAddress;
    //	The supported languages
    private final List<String> languages;
    //	Bool which shows, if the adapter is registered at the MMIRegister
    private boolean isRegistered;
    // Instance of the utilized server to communicate with the mmu abstraction
    private AdapterServer thriftServer;


    //	Basic constructor
    //	<param name="address">The address of the adapter</param>
    //	<param name="mmiRegisterAddress">The address of the register (where all services, adapters and mmus are registered)</param>
    //	<param name="mmuPath">The path where the MMUs are located</param>
    //	<param name="instantioator">the instantiator class which instantiates thte MMus from file</param>
    //	<param name="languages">the languages the adapter should support </param>
    //	<param name="adapterDescription">the description of the adapter</param>
    //	<param name="logLevel">the log level for the logger, default = L_Debug</param> // not implemented yet
    public AdapterController(MIPAddress address, MIPAddress registerAddress, String mmuPath, IMMUInstantiation mmuInstantiator, MAdapterDescription adapterDescription, String... languages) {
        MMUInstantiator = mmuInstantiator;
        this.adapterAddress = address;
        this.registerAddress = registerAddress;
        AdapterController.mmuPath = mmuPath;
        this.languages = List.of(languages); //Immutable !!!!!
        this.isRegistered = false;

        //init SessionData
        SessionData.registerAddress = registerAddress;
        SessionData.adapterDescription = adapterDescription;
    }

    //returns  the instantiator
    public static IMMUInstantiation getMMUInstantiator() {
        return MMUInstantiator;
    }

    //	Starts a thread for registering the adapter, for the Filewatcher and for the AdapterServer
    public void Start() {
        SessionData.startTime = System.currentTimeMillis();
        MMIAdapter.Iface adapterImplementation = new ThriftAdapterImplementation();
        ExecutorService executor = Executors.newFixedThreadPool(3);

        //create and run the filewatcher
        FileWatcher filewatcher;
        try {
            WatchService watcher = FileSystems.getDefault().newWatchService();
            Path path = Paths.get(mmuPath);
            if (Files.exists(path)) {
                path.register(watcher, StandardWatchEventKinds.ENTRY_CREATE);
                filewatcher = new FileWatcher(watcher, path, this.languages);
                filewatcher.setDaemon(false); //because of io
                executor.execute(filewatcher);
            } else {
                Logger.printLog(LogLevel.L_ERROR, "Specified MMUpath does not exist");
            }
        } catch (IOException e) {
            e.printStackTrace();
            Logger.printLog(LogLevel.L_ERROR, e.getMessage());
        }

        //Use thrift based implementation
        Logger.printLog(LogLevel.L_INFO, "Starting adapter server at " + this.adapterAddress.getAddress() + ": " + this.adapterAddress.getPort());

        //Create and start the thrift server
        this.thriftServer = new AdapterServer(this.adapterAddress.Port, adapterImplementation);

        //Start AdapterController in separate thread
            executor.execute(() -> startAdapterServer());
            executor.execute(() -> registerAdapter());
    }

    //	Registers the adapter at the MMIRegister
    private void registerAdapter() {
        while (!isRegistered) {
            try (MMIRegisterServiceClient client = new MMIRegisterServiceClient(this.registerAddress.getAddress(), this.registerAddress.getPort())) {

                isRegistered = client.Access.RegisterAdapter(SessionData.adapterDescription).isSuccessful();
                Logger.printLog(LogLevel.L_INFO, "Successfully registered at MMIRegister");
            } catch (TException e) {
                e.printStackTrace();
                Logger.printLog(LogLevel.L_ERROR, "Failed to register the adapter at MMIRegister");
                //System.err.println(e.getMessage());
            }
            try {
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    //	Creates and starts an new AdapterServer
    private void startAdapterServer() {
        MMIAdapter.Iface adapterImplementation = new ThriftAdapterImplementation();
        //this.thriftServer.Start();

        try (AdapterServer test = new AdapterServer(this.adapterAddress.Port, adapterImplementation)) {
            test.Start();
        }
    }

    @Override
    public void close()  {
        this.thriftServer.close();
    }
}
