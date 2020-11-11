import subprocess
import shutil, os, glob
import argparse
import sys

THRIFT_FILE = "thrift-0.13.0.exe"
SUPPORTED_LANGUAGES = ["py", "csharp", "cpp", "java", "php"]

def CompileThrift(language):
  if(language == "py"):
    for f in glob.glob("mmi/*.thrift"):
      if(os.path.basename(f) == "mmi.thrift"):
        continue
      print(f, os.path.basename(f))
      cmd = "%s -gen %s -r %s"%(THRIFT_FILE, language, f)
      output = subprocess.check_output(cmd)
  else:
    cmd = "%s -gen %s -r mmi/mmi.thrift"%(THRIFT_FILE, language)
    output = subprocess.check_output(cmd)
  return output

def GitCheckout(branch, new = False):
  cmd = "git checkout %s%s"%("-b " if new else "", branch)
  output = subprocess.check_output(cmd)
  if not new:
    cmd = "git pull"
    out2 = subprocess.check_output(cmd)
    output += out2
  return output
  
def GitPull():
  cmd = "git pull"
  output = subprocess.check_output(cmd)
  return output
 
def GitAdd(files):
  cmd = "git add %s"%files
  output = subprocess.check_output(cmd)
  return output

def GitCommit(message):
  cmd = 'git commit -m "%s"'%message
  output = subprocess.check_output(cmd)
  return output
  
def GitPush(branch, new_branch = True):
  cmd = "git push %s origin %s"%("-u" if new_branch else "", branch)
  output = subprocess.check_output(cmd)
  return output


def MoveFiles(src, dest):
  if os.path.isdir(src):
    if os.path.isdir(dest) == False:
      os.makedirs(dest)
    for filepath in glob.glob(src + "\*"):
      shutil.move(filepath, dest)
    shutil.rmtree(src)
  else:
    if os.path.exists(dest):
      print("%s already exists"%dest)
    else:
      shutil.move(src, dest)
    
  
def CleanseFolder(directory, keep):
  for item in os.listdir(directory):
    path = os.path.join(directory, item)
    if not keep in item and not ".git" in item:
      if os.path.isfile(path):
        os.remove(path)
      elif os.path.isdir(path):
        shutil.rmtree(path)
      else:
        print("A simlink or something called {} was not deleted.".format(item))



"""
  Deploys a new version (string) for a certain language and adds the version_text (string) to the README file. 
"""
def DeployNewLanguage(version, language, version_text):
  remoteExisting = False
  version_branch = "compiled/%s/%s"%(language, version)
  print("\n")
  print("-------------------------------------------")
  print("Starting to deploy: %s"%language)
  GitPull()
  GitCheckout("master")
  CompileThrift(language)
  print("Compiled thrift files.")
  try:
    GitCheckout("origin/"+version_branch, False)
    remoteExisting = True
  except:
    try:
      GitCheckout(version_branch, True)
    except:
      GitCheckout(version_branch, False)
  print("Backing up thrift executable")
  MoveFiles("./%s"%THRIFT_FILE, os.path.join("..", THRIFT_FILE))
  CleanseFolder("./", "gen-%s"%language)
  MoveFiles("./gen-%s"%language, "./")
  with open("README.MD", "w") as f:
    f.write("This is an auto-generated new version branch for version %s\n"%version)
    f.write("\n")
    f.write(version_text)
    f.close()
  GitAdd(".")
  GitCommit("New auto-generated version created.")
  if remoteExisting:
    GitPush(version_branch, False)
  else:
    GitPush(version_branch)
  print("Completed new version branch")
  GitCheckout("master")
  MoveFiles(os.path.join("..", THRIFT_FILE), os.path.join(".", THRIFT_FILE))
  print("Restored initial state")
  
  
  
def DeployNewVersion(version, version_text = ""):
  if not os.path.isfile("./%s"%THRIFT_FILE):
    print("ERROR. Please ensure, that there is a compiled %s in this folder"%THRIFT_FILE)
    return -1
  for lang in SUPPORTED_LANGUAGES:
    DeployNewLanguage(version, lang, version_text)


def main():
  parser = argparse.ArgumentParser(description="Deployment script of the MMI Framework. It can be used to generate the thrift source files and move them to specific version branches. I am assuming, that the thrift file is located in the same folder as this file with the name %s. In case of deployment, please copy this programm first to the parent folder and execute from the parent folder, as git commands do not work otherwise. The git repository is considered to be in cloned in a folder named thrift"%THRIFT_FILE)
  parser.add_argument("-v", default ="",type=str, help="Version number (required)")
  parser.add_argument("-m", default = "", type=str, help="Optional Text to be appended to the README files")
  parser.add_argument("-s", default = "", type=str, help="Only deploy a single language", choices=SUPPORTED_LANGUAGES)
  parser.add_argument("-l", action='store_true', help="Compile thrift files locally without generating new version branches")
  
  options = parser.parse_args()
  
  
  if len(sys.argv) <= 1:
    parser.print_help()
    return
  if not os.path.exists(THRIFT_FILE) and not os.path.exists(os.path.join("thrift", THRIFT_FILE)):
    print("Please ensure, that the thrift file is located adjescent to this file with the name %s or inside the thrift repository folder"%(THRIFT_FILE))
    sys.exit(1)
  if options.l:
    print("Compile thrift files locally to this directory")
    if options.s != "":
      CompileThrift(options.s)
    else:
      for lang in SUPPORTED_LANGUAGES:
        print("\n-------------\nCompiled %s\n-------------"%lang)
        CompileThrift(lang)
    return
  """
  elif options.v == "":
    print("Please provide a propper version string")
  else:
    if os.path.isdir("mmi"):
      print("Please copy the script and start it from the parent folder in order to propperly execute the deployment")
      sys.exit(1)
    if os.path.isdir("thrift"):
      os.chdir("thrift")
    else:
      print("Please ensure, that the thrift repository is located in the folder thrift inside this directory")
      sys.exit(1)
    if options.s != "":
      DeployNewLanguage(options.v, options.s, options.m)
    else:
      DeployNewVersion(options.v, options.m)
  """
      
if __name__ == "__main__":
    main()

#DeployNewVersion("0.0.1", "This is a test version")

  
  

    

