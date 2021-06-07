# grafana-opcua-datasource
An OPC UA datasource for reading from OPC UA servers (DA/HDA/AE) into Grafana directly

To build on any platform:
* `yarn build`

![full dashboard](https://raw.githubusercontent.com/srclosson/grafana-opcua-datasource/master/src/img/dashboard2.png)

# Important
Currently used in production and under active maintenance

## What works
* Browsing servers
* Graphical query editor
* HDA queries
* Multiple servers/datasources
* Authenticated connection with certificate or no security
* Alarms & Events

## What needs to be implemented
* OPCUA DA Subscriptions: These are the ones where you will not need to hit the refresh button. If you have subscribed to a datapoint at 500ms, you will get an unsolicited update every 500ms. 
* Password authentication
* Two-way communication with the OPC UA server (currently read only)

# Description and Architecture
This plugin uses GRPC and a C# backend to communicate with the grafana backend directly. See `pkg/dotnet` directory for the backend component

# Building
* `yarn install` to install dependencies
* `yarn build | yarn dev` to build the plugin
* `make build` to build the backend component

Restart Grafana and you should have the datasource installed.

# Contributing
See the list of features above. Happy with any contributions including QA/testing

![Prediktor](https://raw.githubusercontent.com/srclosson/grafana-opcua-datasource/master/src/img/PrediktorLogo_thumb.png) is a proud contributor
Open Software like this project and open standards like OPC UA fits perfectly with our quest to give our clients the freedom to operate. To know more about our offerings and get in touch, check out https://prediktor.com.

* [Prediktor](https://prediktor.com) has a public test OPC UA Server running at opc.tcp://opcua.prediktor.com:4880.
It provides a small information model with Realtime/Historic data and Alarms&Events. Feel free to use it

* You can find complementary OPC UA Grafana panel plugins at https://github.com/PrediktorAS/grafana

# Q&A
Q: **Can it read OPC Classic DA/HDA/AE?**
A: Yes, provided use use the OPC Foundations COMIOP wrapper, which you can find [here](https://github.com/OPCFoundation/UA-.NETStandard). You will need to configure IOP to wrap your OPC COM server. Tested against Matrikon OPC Desktop Historian and Matrikon OPC Simulation Server.

# Vision
The OPC UA datasource should be
- Easy to use
- Graphically configurable
- Smart (ie: Able to use metadata from the browsing of tags to ease configuration in whatever panel the datasource is interacting with)
- Robust and Reliable (of course!)

