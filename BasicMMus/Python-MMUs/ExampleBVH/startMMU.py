import sys

#sys.path.append("python")
import MMIPython.PythonAdapter as PythonAdapter
from scripts.BVH_MMU import BVH_Streamer

description = ""

with open(".\\data\\description.json", "r") as f:
	description = " ".join(f.readlines())
	#print(description)
if __name__ == "__main__":
	PythonAdapter.start_adapter([(description, BVH_Streamer)])
	