
# OPC UA Backend Component
Broken into 3 services:
- DataService: Which queries and returns data from OPC UA Servers
- ResourceService: Which handles metadata queries and browsing
- DiagnosticsService: Which handles health, and health checks. 

# Other important files:
- DataFrame.cs: Handles all the conversion between the grafana dataframe standard, and an Apache Arrow Table.

# Logging
- Logging goes through grafana logs

# Dashboard Mapping
The dashboardmappings.db file can be found at:
- Linux/Macos: See the 'Configuration' section of the main README.
- Windows: '${CommonApplicationData}/Grafana/grafana-opcua-datasource'
