import Adapter.AdapterController;
import Adapter.JAVAMMUInstantiator;
import MMIStandard.MAdapterDescription;
import MMIStandard.MIPAddress;
import Utils.LogLevel;
import Utils.Logger;
import com.beust.jcommander.JCommander;

import java.util.List;

public class Main {

    // <summary>
    /// Entry routine
    /// </summary>
    /// <param name="args"></param>
    public static void main(String[] args) {
        //Welcome message
        System.out.print(
                "       _____ _    _____       ___       __            __           \n" +
                        "      / /   | |  / /   |     /   | ____/ /___ _____  / /____  _____\n" +
                        " __  / / /| | | / / /| |    / /| |/ __  / __ `/ __ \\/ __/ _ \\/ ___/\n" +
                        "/ /_/ / ___ | |/ / ___ |   / ___ / /_/ / /_/ / /_/ / /_/  __/ /    \n" +
                        "\\____/_/  |_|___/_/  |_|  /_/  |_\\__,_/\\__,_/ .___/\\__/\\___/_/     \n" +
                        "                                           /_/                     "
        );

        System.out.println();


        //TODO Logger=??

        //parsing and check the command line args
        Args jArgs = new Args();
        JCommander.newBuilder().addObject(jArgs).build().parse(args);
        MIPAddress adapterAddress = new MIPAddress(jArgs.getAdapterAddress(), jArgs.getAdapterPort());
        MIPAddress registerAddress = new MIPAddress(jArgs.getRegisterAddress(), jArgs.getRegisterPort());
        String mmuPath = Args.getMMUPath();

        if (jArgs.logLevel != null) {
            switch (jArgs.logLevel) {
                case "0":
                    Logger.logLevel = LogLevel.L_SILENT;
                    break;
                case "1":
                    Logger.logLevel = LogLevel.L_ERROR;
                    break;
                case "2":
                    Logger.logLevel = LogLevel.L_INFO;
                    break;
                case "3":
                    Logger.logLevel = LogLevel.L_DEBUG;
                    break;
            }
        }

        if (jArgs.color != null) {
            if (jArgs.color.equals("t") || jArgs.color.equals("true"))
                Logger.color = true;

            if (jArgs.color.equals("f") || jArgs.color.equals("false"))
                Logger.color = false;
        }


        //print connection info
        System.out.println("______________________________________________________________");
        System.out.println("Adapter is reachable at: " + adapterAddress.getAddress() + ":" + adapterAddress.getPort());
        System.out.println("Register is reachable at: " + registerAddress.getAddress() + ":" + registerAddress.getPort());
        System.out.println("MMUs will be loaded from:  " + mmuPath);
        System.out.println("______________________________________________________________");

        //create the adapter description
        MAdapterDescription adapterDescription = new MAdapterDescription("JAVAAdapter", "7999d37f-1337-45da-1001-f4adde70a55d", "JAVA", List.of(adapterAddress));
        //Create the adapterController and init the Adapter
        AdapterController adapterController = new AdapterController(adapterAddress, registerAddress, mmuPath, new JAVAMMUInstantiator(), adapterDescription, "JAVA");
        adapterController.Start();
        Runtime.getRuntime().addShutdownHook(new Thread(() -> System.out.println("shutdown")));

    }
}
