# grafana-opcua-datasource
An OPC UA datasource for reading from OPC UA servers (DA/HDA/AE) into Grafana directly

# Important
This datasource is pre-alpha quality. It does not even read data right now.

# Purpose
The idea was to get familiar with OPC UA, the frameworks that are out there, specifically for a GO backend, and to allow the interaction with these frameworks to provide a native Grafana OPC UA experience.

# Contributing
I'm not at the point where I can take care of any issues. If you have some, you are welcome to fork and issue a pull request. Otherwise, I suspect I will have time to return to this perhaps in the new year (2020).

# Vision
The OPC UA datasource should be
- Easy to use
- Graphically configurable
- Smart (ie: Able to use metadata from the browsing of tags to ease configuration in whatever panel the datasource is interacting with)
- Robust and Reliable (of course!)

Right now I've forked a cgooopc wrapper that for me has been by far the most mature and reliable way to interact with OPC UA servers.
