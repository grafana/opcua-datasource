package opc

import (
	"fmt"

	"github.com/grafana/grafana-opcua-datasource/pkg/cgoopc"
	"github.com/grafana/grafana-opcua-datasource/pkg/logger"
)

// QueryModel contains the query information from the API call that we use to make a query.
type QueryModel struct {
	DatasourceID	int `json:"datasourceId"`
	Format	string `json:"format"`
	IntervalMs int `json:"intervalMs"`
	MaxDataPoints int `json:"maxDataPoints"`
	RefID	string `json:"refId"`
	Endpoint    string `json:"endpoint"`
	Call   string `json:"call"`
}

// Client holds the client object
var client *cgoopc.Client

// Connect registers a connection with an OPC UA server
func Connect(endpoint string) {
	logger.Print.Debug("Creating a new client")
	client = cgoopc.NewClient(4)
	logger.Print.Debug(fmt.Sprint("Created", client))
	client.Connect(endpoint)
	logger.Print.Debug(fmt.Sprintf("Connected", client))
}

// GetTree returns the browse tree in flat format
func GetTree() []cgoopc.TagItem {
	logger.Print.Debug(fmt.Sprintf("Browsing", client))
	treeItems := client.BrowseTags()
	logger.Print.Debug(fmt.Sprintf("Browsed", client))
	return treeItems
}

// Close the connection and free up memory
func Close () {
	client.Delete()
}