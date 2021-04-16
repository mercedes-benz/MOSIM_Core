<!-- SPDX-License-Identifier: MIT -->
# MOSIM_Core

![mosim](https://mosim.eu/____impro/1/onewebmedia/MOSIM%20Logo%20white%20background%20150.png?etag=%221b8a-5c57fd19%22&sourceContentType=image%2Fpng&ignoreAspectRatio&resize=150%2B84&extract=0%2B7%2B149%2B59)


This repository contains the Motion Model Interface (MMI) core framework, developed in the context of the MOSIM research project (http://mosim.eu).
The MOSIM projects aims at generating realistic human motions based on state-of-the-art motion synthesis technologies.
The framework allows to combine heterogeneous motion synthesis approaches by utilizing modular units referred as Motion Model Units (MMUs).
These units can be realized in different programming languages and engines.
The core framework utilizes Apache Thrift for communication and to automatically generate source code files for many programming languages.

![concept](https://mosim.eu/____impro/1/onewebmedia/Artchitecture.png?etag=W%2F%2217ea6-5d566ea0%22&sourceContentType=image%2Fpng&ignoreAspectRatio&resize=845%2B366&extract=20%2B22%2B801%2B334)

Besides the infrastructure of the core framework, the repository comprises an implementation of the so-called co-simulation as well as basic MMUs.

The MOSIM Framework is provided in four other repositories: The [MOSIM Services](https://github.com/Daimler/MOSIM_Services) contains the implementation of various services, assisting in MMU and Co-Simulator development. The [MOSIM Tools](https://github.com/Daimler/MOSIM_Tools) contains helpful tools, e.g. for MMUGenerator for animation based Unity MMUs. The [MOSIM Demos](https://github.com/Daimler/MOSIM_Demos) repository contains integrations to Unity 3D and Unreal Engine containing simple demonstration scenes. The [MOSIM (meta)](https://github.com/Daimler/MOSIM) repository is a meta repository, grouping all of the repositories together, providing a deploy functionality and central cloning opportunity.

## Usage

If you want to **use the framework**, we recommend using the pre-compiled environment, documented in the article [Installing the pre-compiled framework](https://github.com/Daimler/mosim_core/wiki/InstallPrecompiled).

If you plan to **develop MMUs** for the MOSIM framework, we recommend starting with the article [MMU Development](https://github.com/Daimler/mosim_core/wiki/MMUDevelopment).

If you plan to **develop Services** for the MOSIM framework, we recommend starting with the article [Service Development](https://github.com/Daimler/mosim_core/wiki/ServiceDevelopment).


If you want to **integrate the MOSIM framework** to your own Unity Scene, we recommend reading the article [Integrating the MOSIM framework](https://github.com/Daimler/mosim_core/wiki/IntegratingFramework).

If you want to **compile the MOSIM framework** from scratch, we recommend reading the article [Compling the MOSIM framework from scratch](https://github.com/Daimler/mosim_core/wiki/CompileFramework). 

The full documentation of the MOSIM framework can be found in the wiki pages for the [technical architecture](https://github.com/Daimler/mosim_core/wiki/01_home). 


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
