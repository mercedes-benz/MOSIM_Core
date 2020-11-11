import com.beust.jcommander.Parameter;

public class Args {

    @Parameter(names = {"-a", "-aaddress"}, description = "The address of the adapters tcp server.", required = true)
    private static String aAddress;

    @Parameter(names = {"-r", "-raddress"}, description = "The address of the register which holds the central information.", required = true)
    private static String rAddress;

    @Parameter(names = {"-m", "-mmupath"}, description = "The path of the mmu folder.", required = true)
    private static String mmuPath;

    @Parameter(names = {"-l", "-logLevel"}, description = "The log level 0:SILENT, 1: ERROR, 2: INFO, 3: DEBUG", required = false)
    public String logLevel;

    @Parameter(names = {"-c", "-color"}, description = "If color for debug is enabled", required = false)
    public String color;


    public static String getMMUPath() {
        return mmuPath;
    }

    public String getAdapterAddress() {
        String[] adr = aAddress.split(":");
        return adr[0];
    }

    public int getAdapterPort() {
        String[] adr = aAddress.split(":");
        return Integer.parseInt(adr[1]);
    }

    public String getRegisterAddress() {
        String[] adr = rAddress.split(":");
        return adr[0];
    }

    public int getRegisterPort() {
        String[] adr = rAddress.split(":");
        return Integer.parseInt(adr[1]);
    }
}
