package Utils;


import java.text.DateFormat;
import java.util.Date;
import java.util.Locale;


public class Logger {
    private static final String BLACK_BRIGHT = "\033[0;90m";  // BLACK
    private static final String RED_BRIGHT = "\033[0;91m";    // RED
    private static final String GREEN_BRIGHT = "\033[0;92m";  // GREEN
    private static final String YELLOW_BRIGHT = "\033[0;93m"; // YELLOW
    private static final String BLUE_BRIGHT = "\033[0;94m";   // BLUE
    private static final String PURPLE_BRIGHT = "\033[0;95m"; // PURPLE
    private static final String CYAN_BRIGHT = "\033[0;96m";   // CYAN
    private static final String WHITE_BRIGHT = "\033[0;97m";  // WHITE
    private static final String RESET = "\033[0m";  // Text Reset
    public static LogLevel logLevel = LogLevel.L_INFO;
    public static Boolean color = false;

    public static void printLog(LogLevel level, String message) {
        DateFormat df = DateFormat.getDateTimeInstance(DateFormat.MEDIUM, DateFormat.MEDIUM, Locale.ENGLISH);
        Date date = new Date(System.currentTimeMillis());

        if (level.ordinal() <= logLevel.ordinal()) {
            if (color) {
                switch (level) {
                    case L_ERROR:
                        System.err.println(RED_BRIGHT + df.format(date) + " ----> " + message + RESET);
                        break;
                    case L_INFO:
                        System.out.println(GREEN_BRIGHT + df.format(date) + " ----> " + message + RESET);
                        break;
                    case L_DEBUG:
                        System.out.println(CYAN_BRIGHT + df.format(date) + " ----> " + message + RESET);
                }
            } else {
                switch (level) {
                    case L_ERROR:
                        System.err.println("ERROR: " + (df.format(date) + " ----> " + message));
                        break;
                    case L_INFO:
                        System.out.println(("INFO: " + df.format(date) + " ----> " + message));
                        break;
                    case L_DEBUG:
                        System.out.println("DEBUG: " + (df.format(date) + " ----> " + message));
                }

            }

        }


    }
}
