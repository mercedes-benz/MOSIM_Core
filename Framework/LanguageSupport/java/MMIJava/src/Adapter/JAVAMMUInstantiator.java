package Adapter;

import MMIStandard.MMUDescription;
import Utils.LogLevel;
import Utils.Logger;

import java.io.BufferedInputStream;
import java.io.File;
import java.io.FileInputStream;
import java.lang.reflect.Modifier;
import java.net.URL;
import java.net.URLClassLoader;
import java.util.jar.JarEntry;
import java.util.jar.JarInputStream;

public class JAVAMMUInstantiator implements IMMUInstantiation {
    /*
		 Class which instantiates basic java MMUs
	*/

    // gets an MMUDescription with the id of the MMU it searches for the extracted jar file
    // .jar file is named like the mmu id
    // instantiates the mmu and adds it to the SessionContent
    @Override
    public MotionModelUnitBase InstantiateMMU(MMUDescription mmuDescription) {

        //Check if the MMU supports the specified language
        /*if (!mmuDescription.Language.equals("JAVA"))
            return null;
         */
        if(AdapterController.mmuPath!=null)
        {
            String filePath = AdapterController.mmuPath + "\\temp\\" + mmuDescription.getID() + ".jar";

            try {
                JarInputStream inputStream = new JarInputStream(new BufferedInputStream(new FileInputStream(filePath)));
                URLClassLoader cl = URLClassLoader.newInstance(new URL[]{new File(filePath).toURI().toURL()});
                MotionModelUnitBase mmu = null;
                JarEntry entry = inputStream.getNextJarEntry();
                while (entry != null) {
                    if (entry.getName().endsWith(".class")) {
                        String className = entry.getName().substring(0, entry.getName().length() - 6);
                        className = className.replace('/', '.');
                        Class c = cl.loadClass(className);

                        if (!c.isInterface() || !Modifier.isAbstract(c.getModifiers())) {
                            Logger.printLog(LogLevel.L_DEBUG, "Class type found -> try to instantiate MMU " + entry.getName());
                            if (MotionModelUnitBase.class.isAssignableFrom(c)) {
                                //Class[] cArg = new Class[2]; //Our constructor has 2 arguments
                                //cArg[0] = String.class; //First argument is of *object* String
                                //cArg[1] = int.class; //Second argument is of *object* type int
                                mmu = (MotionModelUnitBase) c.getDeclaredConstructor().newInstance();
                                break;
                            }
                        }
                    }
                    entry = inputStream.getNextJarEntry();
                }

                if (mmu != null) {
                    return mmu;
                }
                inputStream.close();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        return null;
    }
}
