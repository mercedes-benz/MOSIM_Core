package Adapter;

import MMIStandard.MMUDescription;
import Utils.LogLevel;
import Utils.Logger;
import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.stream.JsonReader;

import java.io.*;
import java.nio.file.*;
import java.util.*;
import java.util.zip.ZipEntry;
import java.util.zip.ZipFile;
import java.util.zip.ZipInputStream;


public class FileWatcher extends Thread {
    /**
     * class which:
     * checks the given path for the descriptions
     * parses the descriptions
     * checks the descriptions if it contains a supported language
     * checks the given path for the assembly name and saves the description and the assembly path in SessionData
     */

    private final WatchService watchService;
    //	path which should be checked
    private final Path watchDir;
    //	The supported languages
    private final Set<String> languages = new HashSet<>();

    //	Basic constructor
    //	<param name="watchDir">The patch which should be checked for MMus</param>
    //	<param name="languages">The supported languages</param>
    public FileWatcher(WatchService watchService, Path watchDir, List<String> languages) {
        this.watchService = watchService;
        this.watchDir = watchDir;
        this.languages.addAll(languages);
    }

    //	Starts the check for loadable MMUs
    @Override
    public void run() {
        this.UpdateLoadableMMUs(this.watchDir.toAbsolutePath().toString());
        Logger.printLog(LogLevel.L_INFO, "Filewatcher started for dir: " + watchDir);
        WatchKey key = null;

        while (true) {
            int loaded = 0;
            try {
                key = watchService.take();
                // key.pollEvents().stream().filter(e->e.kind()==StandardWatchEventKinds.ENTRY_CREATE).filter(e-> e.context().equals("description.json")).collect(Collectors.toList());
                for (WatchEvent<?> event : key.pollEvents()) {
                    if (event.kind() == StandardWatchEventKinds.ENTRY_CREATE) // checks for creation new files in dir
                    {
                        //Logger.printLog(LogLevel.L_INFO,"New file in MMU dir" + ": " + event.context());
                        Path path = watchDir.resolve((Path) event.context());
                        if (path.toString().endsWith(".zip")) // checks for zip archives
                        {
                            File file = new File(path.toFile().toString());
                            Logger.printLog(LogLevel.L_INFO, "New file in MMU dir" + ": " + file.getName());
                            Thread.sleep(500);
                            if (this.inspectZips(file))
                                loaded++;
                        }
                    }
                }
                Logger.printLog(LogLevel.L_INFO, "Scanned for new loadable MMUS " + loaded + " loadable MMUs found");
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            if (key != null)
                key.reset();
        }
    }

    /// <summary>
    /// Returns the descriptions of all loadable MMUs
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private void UpdateLoadableMMUs(String path) {
        int loaded = 0;
        File file = new File((path));
        if (!file.exists()) {
            Logger.printLog(LogLevel.L_ERROR, "Specified MMUpath does not exist");
            Thread.currentThread().interrupt();
            return;
        }

        List<File> zipFiles = getZipArchives(file, new ArrayList<>());


        if (zipFiles != null || zipFiles.size() != 0) {
            for (File f : zipFiles) {
                if (inspectZips(f)) {
                    loaded++;
                }
            }
        }
        Logger.printLog(LogLevel.L_INFO, "Scanned for loadable MMUS " + loaded + " loadable MMUs found");
    }


  /*  //Collects recursive all folder paths
    private ArrayList<File> getFolderPaths(File file, ArrayList<File> list) {
        if (file == null || list == null || !file.isDirectory())
            return null;

        list.add(file);
        File[] fileArr = file.listFiles();
        for(File f: fileArr)
        {
            getFolderPaths(f,list);
        }
        return list;
    }*/


    //Collects recursive all folder paths
    private ArrayList<File> getZipArchives(File file, ArrayList<File> list) {
        if (file == null || list == null || !file.isDirectory())
            return null;

        for (File f : file.listFiles()) {
            if (f.getName().endsWith(".zip")) {
                list.add(f);
            }
            getZipArchives(f, list);
        }
        return list;
    }



    /*private boolean CheckFolder(File folder)
    {
        //check if there is a file called description.json
        File[] files = folder.listFiles((dir, name) -> name.contentEquals("description.json"));


        //Skip if no description file
        if (files.length == 0) {

            //System.err.println("Can not find description.json ");
            return false;
        }

        //load and parse the JSON file

        try (FileReader fileReader = new FileReader(files[0].getAbsolutePath()))
        {
            MMUDescription mmuDescription = new Gson().fromJson(new BufferedReader(fileReader), MMUDescription.class);
            if (!this.languages.contains(mmuDescription.Language)) {
                return false;
            }

            //Check if already available -> skip
            if (SessionData.MMULoadingProperties.stream().anyMatch(s -> s.y.ID.equals(mmuDescription.ID))) {
                return false;
            }

            //Determine the mmu file
            File[] mmus = folder.listFiles((dir, name) -> name.contentEquals(mmuDescription.AssemblyName));

            if (mmus.length != 0) {
                //Add the description to the dictionary
                String mmuFilePath = mmus[0].getAbsolutePath();
                SessionData.MMULoadingProperties.add(new Tuple<String, MMUDescription>(mmuFilePath, mmuDescription));
                SessionData.MMUDescriptions.add(mmuDescription);
                return true;
            } else {
                System.err.println("Cannot find corresponding assembly name. " + mmuDescription.AssemblyName + " of MMU: " + mmuDescription.Name);
            }

        } catch (IOException e) {
            e.printStackTrace();
        }
        return false;
    }*/

    //opens the zip and searches for the description.json, parses it and checks if the language is supported
    // also searches for the mmu .jar
    private boolean inspectZips(File file) {
        Gson gson = new GsonBuilder().create();
        MMUDescription mmuDescription;

        try {
            ZipFile zipFile = new ZipFile(file);

            ZipInputStream inputStream = new ZipInputStream(new BufferedInputStream(new FileInputStream(file.getAbsolutePath())));


            Map<String, ZipEntry> files = new HashMap<>();
            ZipEntry entry = inputStream.getNextEntry();
            while (entry != null) {
                files.put(entry.getName(), entry);
                entry = inputStream.getNextEntry();
            }
            inputStream.close();

            if (files.containsKey("description.json")) {
                JsonReader reader = new JsonReader(new InputStreamReader(zipFile.getInputStream(files.get("description.json"))));
                mmuDescription = gson.fromJson(reader, MMUDescription.class);
                reader.close();
                if (SessionData.MMUZipEntry.containsKey(mmuDescription.ID) || !languages.contains(mmuDescription.Language)) {
                    zipFile.close();
                    return false;
                }
            } else {
                zipFile.close();
                return false;
            }

            if (files.containsKey(mmuDescription.AssemblyName)) {
                File dir = new File(this.watchDir.toAbsolutePath().toString() + "\\temp");
                if (!dir.exists()) {
                    dir.mkdir();
                }
                File newFile = newFile(new File(dir.getAbsolutePath()), mmuDescription.ID + ".jar");
                int length;
                InputStream in = zipFile.getInputStream(zipFile.getEntry(mmuDescription.AssemblyName));
                OutputStream out = new FileOutputStream(newFile);
                byte[] buffer = new byte[1024];
                while ((length = in.read(buffer)) > 0) {
                    out.write(buffer, 0, length);
                }
                out.close();
                in.close();
                SessionData.MMUZipEntry.put(mmuDescription.ID, mmuDescription);
                SessionData.MMUDescriptions.add(mmuDescription);
                zipFile.close();
                return true;
            } else {
                zipFile.close();
                return false;
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
        return false;
    }


    private File newFile(File destinationDir, String fileName) throws IOException {
        File destFile = new File(destinationDir, fileName);

        String destDirPath = destinationDir.getCanonicalPath();
        String destFilePath = destFile.getCanonicalPath();

        if (!destFilePath.startsWith(destDirPath + File.separator)) {
            throw new IOException("Entry is outside of the target dir: " + fileName);
        }
        return destFile;
    }

}
