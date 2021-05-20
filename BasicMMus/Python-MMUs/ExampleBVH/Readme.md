# Example BVH Streamer MMU

This is an example MMU streaming the content of a single example BVH file directly to the MOSIM framework. 

## Preparation

### Retargeting 
In order to prepare the MMU, the retargeting configuration was generated for the specific BVH skeleton and stored in `data/`. Consult the Retargeting Configurator documentation for more information. 


### BVH Loader
To load the BVH, we utilize the animation utilies library developed by Erik Herrmann (DFKI), Han Du (DFKI), Martin Manns (Daimler AG / University of Siegen) and Markus Bauer (Daimler AG). The project can be found in https://github.com/eherr/anim_utils. 

### python environment
This example relies on several packages to be installed to the local pip environment. 
1. MMIStandard (python) and MMIPython from the core repository can be installed with the deploy script in the Framework/LanguageSupport/python folder. 
```
cd ..\..\..\Framework\LanguageSupport\python\
.\deploy.bat pip
cd ..\..\..\BasicMMus\Python-MMUs\ExampleBVH\;
```

2. Clone and install the anim_utils package
```
cd external
git clone https://github.com/eherr/anim_utils
pip install .\anim_utils
cd ../
```

