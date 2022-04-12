
# OPC UA Backend Component
Broken into 3 services:
- DataService: Which queries and returns data from OPC UA Servers
- ResourceService: Which handles metadata queries and browsing
- DiagnosticsService: Which handles health, and health checks. 

# Other important files:
- DataFrame.cs: Handles all the conversion between the grafana dataframe standard, and an Apache Arrow Table.

# Logging
- Logging goes through grafana logs
