<!-- SPDX-License-Identifier: MIT -->
# MOSIM_Core

![mosim](https://mosim.eu/____impro/1/onewebmedia/MOSIM%20Logo%20white%20background%20150.png?etag=%221b8a-5c57fd19%22&sourceContentType=image%2Fpng&ignoreAspectRatio&resize=150%2B84&extract=0%2B7%2B149%2B59)


## Intended Usage

This repository contains the Motion Model Interface (MMI) core framework, developed in the context of the MOSIM research project (http://mosim.eu).
The MOSIM projects aims at generating realistic human motions based on state-of-the-art motion synthesis technologies.
The framework allows to combine heterogeneous motion synthesis approaches by utilizing modular units referred as Motion Model Units (MMUs).
These units can be realized in different programming languages and engines.
The core framework utilizes Apache Thrift for communication and to automatically generate source code files for many programming languages.
A comprehensive overview of the framework is available at 
[MOSIM ITEA3 Website](https://itea3.org/project/workpackage/document/download/5770/MMU%20concept%20and%20interface%20specification.pdf).

![concept](https://mosim.eu/____impro/1/onewebmedia/Artchitecture.png?etag=W%2F%2217ea6-5d566ea0%22&sourceContentType=image%2Fpng&ignoreAspectRatio&resize=845%2B366&extract=20%2B22%2B801%2B334)

Besides the infrastructure of the core framework, the repository comprises an implementation of the so-called co-simulation as well as basic MMUs.




## Installation

The core framework consists of the so-called launcher which starts all further components, Adapters and MMUs.
The easiest way to run the system, is to use a predefined release package comprising a working environment.
However, the individual components can be manually built as well. 
In general, for each programming language the MMIStandard artificat is generated.
This artifact comprises the auto-generated code by Thrift. 
Moreover, the so called MMI[CSharp,CPP,Python,..] represents a further core library, providing functions on top of the MMIStandard artifact.
Finally, depending on the programming languages, adapters are built on top of these programming languages.
For utilizing and setting up the actual simulation in a target engine, the Framework/EngineSupport projects can be used.
In particular, Framework/EngineSupport/MMIUnity.TargetEngine provides a comprehensive library for using the MMI framework inside the Unity Engine.
Please be aware that the actual Unity.dll is not part of this repository and must be manually included.




## Contributing

We welcome any contributions.
If you want to contribute to this project, please read the [contributing guide](CONTRIBUTING.md).

## Code of Conduct

Please read our [Code of Conduct](https://github.com/Daimler/daimler-foss/blob/master/CODE_OF_CONDUCT.md) as it is our base for interaction.

## License

This project is licensed under the [MIT LICENSE](LICENSE).

## Provider Information

Please visit <https://www.daimler-tss.com/en/imprint/> for information on the provider.

Notice: Before you use the program in productive use, please take all necessary precautions,
e.g. testing and verifying the program with regard to your specific use.
The program was tested solely for our own use cases, which might differ from yours.
