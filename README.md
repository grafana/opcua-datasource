# grafana-opcua-datasource
An OPC UA datasource for reading from OPC UA servers (DA/HDA/AE) into Grafana directly

# Important
Currently beta quality
## What works
* Browsing servers
* Graphical query editor
* HDA queries
* Multiple servers/datasources
* Authentication with certificates
* No security connections (but probably not useful)

## What needs to be implemented
* OPCUA DA requests (subscriptions etc)
* OPCUA AE
* Password authentication

# Description and Architecture
This plugin uses GRPC and a C# backend to communicated to the grafana backend directly. See `pkg/dotnet` directory for the backend component

# Building
* `yarn install` to install dependencies
* `yarn build | yarn dev` to build the plugin
* `make build` to build the backend component

Restart Grafana and you should have the datasource installed.

# Contributing
See the list of features above. Happy with any contributions including QA/testing

# Q&A
Q: **Can it read OPC Classic DA/HDA/AE?**
A: Yes, provided use use the OPC Foundations COMIOP wrapper, which you can find [here](https://github.com/OPCFoundation/UA-.NETStandard). You will need to configure IOP to wrap your OPC COM server. Tested against Matrikon OPC Desktop Historian and Matrikon OPC Simulation Server.

# Vision
The OPC UA datasource should be
- Easy to use
- Graphically configurable
- Smart (ie: Able to use metadata from the browsing of tags to ease configuration in whatever panel the datasource is interacting with)
- Robust and Reliable (of course!)

