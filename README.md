# grafana-opcua-datasource
An OPC UA datasource for reading from OPC UA servers (DA/HDA/AE) into Grafana directly

## Update 15-Jun-2020
A major overhaul is complete. It's almost all in the backend where the GRPC protobuf was update to the latest version from Grafana. This has simplified the frontend and queries significantly. Controls and UI elements are updated to Grafana 7.x.x, as such, v1.0.0 and all versions going forward will be for Grafana 7.x.x+ only. 

![full dashboard](https://raw.githubusercontent.com/srclosson/grafana-opcua-datasource/master/src/img/dashboard2.png)

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
* OPCUA DA Subscriptions: These are the ones where you will not need to hit the refresh button. If you have subscribed to a datapoint at 500ms, you will get an unsolicited update every 500ms. 
* OPCUA AE
* Password authentication
* Aliasing tag names for display in the legend
* Curious about getting into being able to write values or call functions over the query editor UI.

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

# Needs/Wants:
* I'd love it if someone could sponser the following. The benefit will be that compatibility to your OPC products will be guarenteed.
  * Some OPC UA Historian
  * Some sort of OPC UA AE product

# Vision
The OPC UA datasource should be
- Easy to use
- Graphically configurable
- Smart (ie: Able to use metadata from the browsing of tags to ease configuration in whatever panel the datasource is interacting with)
- Robust and Reliable (of course!)

