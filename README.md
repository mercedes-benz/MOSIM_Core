<!-- SPDX-License-Identifier: MIT -->
# :warning: This project is no longer maintained and is archived.

# MOSIM_Core

![mosim](https://mosim.eu/____impro/1/onewebmedia/MOSIM%20Logo%20white%20background%20150.png?etag=%221b8a-5c57fd19%22&sourceContentType=image%2Fpng&ignoreAspectRatio&resize=150%2B84&extract=0%2B7%2B149%2B59)


This repository contains the Motion Model Interface (MMI) core framework, developed in the context of the MOSIM research project (http://mosim.eu).
The MOSIM Framework is an open modular framework for efficient and interactive simulation and analysis of realistic human motions for professional applications. It is a distributed system, using modular motion units (MMUs) in different programming languages and using different tools to simulate poses, which are merged in a separate co-simulator and displayed in a separate target engine. Additional services can be implemented and used, to perform frequent utility tasks. A high-level task-editor and behavior execution system allow complex agent behavior which can be configured by domain-specific experts without requiring a deep understanding of motion synthesis. 

![concept](https://mosim.eu/____impro/1/onewebmedia/Artchitecture.png?etag=W%2F%2217ea6-5d566ea0%22&sourceContentType=image%2Fpng&ignoreAspectRatio&resize=845%2B366&extract=20%2B22%2B801%2B334)

Besides the infrastructure of the core framework, the repository comprises an implementation of the so-called co-simulation as well as basic MMUs.

The MOSIM Framework is provided in four other repositories: The [MOSIM Services](https://github.com/Daimler/mosim_services) contains the implementation of various services, assisting in MMU and Co-Simulator development. The [MOSIM Tools](https://github.com/Daimler/mosim_tools) contains helpful tools, e.g. for MMUGenerator for animation based Unity MMUs. 
The [MOSIM Demos](https://github.com/Daimler/unitydemo) repository contains integrations to Unity 3D containing simple demonstration scenes.
The [MOSIM (meta)](https://github.com/Daimler/mosim) repository is a meta repository, grouping all of the repositories together, providing a deploy functionality and central cloning opportunity.

## Documentation

This wiki contains documentation of the framework and several tutorials for its usage. If you are new to the framework, we recommend the following introductory articles:

  * [What is the MOSIM Framework?](./Introduction)
  * [Components of the Framework](./Components-of-the-Framework)
  * [Repository Structure](https://github.com/Daimler/MOSIM_Core/wiki/Repository-Structure)
  * [RPCs with Apache Thrift](./RPCs-with-Apache-Thrift)

To work with the MOSIM Framework, you require at least two components:
1. the Framework containing the launcher, mmus, services and adapters
2. a target-engine project to visualize results and control the framework

Additional components, such as the high-level task-editor and the behavior execution model are optional components, improving the agent simulation and allowing for more complex behavior. 

### Setting up the MOSIM Framework

We release a stable version of the framework, which can be "installed" and used on Windows 10 systems. For more information, please read the documentation on [installing the released framework](https://github.com/Daimler/mosim_core/wiki/InstallPrecompiled). 

We provided an exemplary target-engine project in Unity in a separate [demos repository](https://github.com/Daimler/MOSIM_Demos) and recommend starting with this project if you are new to the framework. For more information, please consider the documentation on [Framework Integration](https://github.com/Daimler/mosim_core/wiki/IntegratingFramework).

If you want to receive the latest version of the framework, you can clone and deploy the [MOSIM Meta repository](https://github.com/Daimler/MOSIM). For more information on the deployment procedure, please consider the documentation on [manual compilation](https://github.com/Daimler/mosim_core/wiki/CompileFramework). 

### Developing MMUs

If you are a MMU developer and want to create new motion models, we do recommend to first reading the [prerequisites for MMU development](https://github.com/Daimler/MOSIM_Core/wiki/What-do-I-need-for-MMU-development%3F). In addition, we provided several articles on MMU development: 

* [Prerequisites for MMU Development](https://github.com/Daimler/MOSIM_Core/wiki/What-do-I-need-for-MMU-development%3F)
* [From MoCap to MMU](https://github.com/Daimler/MOSIM_Core/wiki/From-MoCap-to-MMU)
* [Introduction to Unity Animators](https://github.com/Daimler/MOSIM_Core/wiki/Introduction-to-Unity-Animators)
* [MMU Generator](https://github.com/Daimler/MOSIM_Tools/wiki/MMU-Generator)
* [MMUs in Unity](https://github.com/Daimler/MOSIM_Core/wiki/MMUs-in-Unity)
* [MMUs in Python](https://github.com/Daimler/MOSIM_Core/wiki/MMUs-in-Python)
* [MMUs in C#](https://github.com/Daimler/MOSIM_Core/wiki/MMUs-in-C%23)

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
