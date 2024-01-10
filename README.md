# OPC UA Datasource plugin for Grafana

## STATUS

> [!CAUTION]
> This plugin is now deprecated. This deprecation means that this plugin won't receive any feature updates or bug fixes except critical security fixes. After 6 months (End of June 2024), the plugin will reach EOL and there won't be any fixes afterwards. Existing installations of the plugin will continue to work. Reach out to [Grafana Community Forum](https://community.grafana.com/) if you need any further assistance on this plugin.

## Introduction

Utilizing the datasource plugin framework, this projects allows you to access data from OPC UA servers directly from Grafana.

![full dashboard](https://raw.githubusercontent.com/srclosson/grafana-opcua-datasource/master/src/img/dashboard2.png)

## What works

* Browsing and adding multiple servers
* Authenticated connection with certificate or no security
* Graphical query editor
* Data Access (DA)
* Historical Access (HA / HDA)
* Alarms & Conditions (AC / AE)

## What needs to be implemented

* OPCUA DA Subscriptions: These are the ones where you will not need to hit the refresh button. If you have subscribed to a data point at 500ms, you will get an unsolicited update every 500ms.
* Password authentication
* Two-way communication with the OPC UA server (currently read only)

## Description and Architecture

This plugin uses GRPC and a C# backend to communicate with Grafana directly. See `pkg/dotnet` directory for the backend component

## Building

* `yarn install` to install dependencies
* `yarn build | yarn dev` to build the plugin
* `make build` to build the backend component

Restart Grafana and you should have the datasource installed.

## Contributing

Contributions that addresses the needs above or other feature you'd like to see are most welcome. Fork the project and commit a PR with your requests.

![Prediktor](https://raw.githubusercontent.com/srclosson/grafana-opcua-datasource/master/src/img/PrediktorLogo_thumb.png) is a proud contributor
Open Software like this project and open standards like OPC UA fits perfectly with our quest to give our clients the freedom to operate. To know more about our offerings and get in touch, check out [https://prediktor.com](https://prediktor.com).

## Q&A

Q: **Can it read OPC Classic DA/HDA/AE?**

A: Yes, provided use use the OPC Foundations COMIOP wrapper, which you can find [here](https://github.com/OPCFoundation/UA-.NETStandard). You will need to configure IOP to wrap your OPC COM server. Tested against Matrikon OPC Desktop Historian and Matrikon OPC Simulation Server.
