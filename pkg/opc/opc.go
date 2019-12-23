package opc

import (
	"context"
	"crypto/rsa"
	"crypto/tls"
	"errors"
	"fmt"

	//"github.com/grafana/grafana-opcua-datasource/pkg/cgoopc"
	"github.com/gopcua/opcua"
	"github.com/gopcua/opcua/id"
	"github.com/gopcua/opcua/ua"
	"github.com/grafana/grafana-opcua-datasource/pkg/logger"
)

// QueryModel contains the query information from the API call that we use to make a query.
type QueryModel struct {
	DatasourceID  int    `json:"datasourceId"`
	Format        string `json:"format"`
	IntervalMs    int    `json:"intervalMs"`
	MaxDataPoints int    `json:"maxDataPoints"`
	RefID         string `json:"refId"`
	Endpoint      string `json:"endpoint"`
	Call          string `json:"call"`
}

// Client holds the client object
//var client *cgoopc.Client

var client *opcua.Client

// Connect registers a connection with an OPC UA server
func Connect(endpoint string, certificate string, key string) error {
	// logger.Print.Debug("Creating a new client")
	// client = cgoopc.NewClient(4)
	// logger.Print.Debug(fmt.Sprint("Created", client))
	// client.Connect(endpoint)
	// logger.Print.Debug(fmt.Sprint("Connected", client))
	ctx := context.Background()

	c, err := tls.X509KeyPair([]byte(certificate), []byte(key))
	if err != nil {
		return err
	}

	pk, ok := c.PrivateKey.(*rsa.PrivateKey)
	if !ok {
		return errors.New("Invalid Private Key")
	}

	cert := c.Certificate[0]
	logger.Print.Debug(fmt.Sprintf("Connecting to %s", endpoint))
	client = opcua.NewClient(endpoint,
		opcua.SecurityPolicy(ua.SecurityPolicyURINone),
		opcua.SecurityMode(ua.MessageSecurityModeSignAndEncrypt),
		opcua.Certificate(cert),
		opcua.PrivateKey(pk))

	if client == nil {
		return errors.New("Got nil for a client")
	}
	client.Connect(ctx)
	return nil
}

// GetTree returns the browse tree in flat format
func GetTree() ([]NodeDef, error) {
	defer Close()
	//return client.BrowseTags()
	// logger.Print.Debug(fmt.Sprintf("Browsing -> %v", client))
	// desc := &ua.BrowseDescription{
	// 	NodeID:          ua.NewNumericNodeID(0, id.RootFolder),
	// 	BrowseDirection: ua.BrowseDirectionForward,
	// 	ReferenceTypeID: ua.NewNumericNodeID(0, id.References),
	// 	IncludeSubtypes: true,
	// 	NodeClassMask:   uint32(ua.NodeClassAll),
	// 	ResultMask:      uint32(ua.BrowseResultMaskAll),
	// }

	// req := &ua.BrowseRequest{
	// 	View: &ua.ViewDescription{
	// 		ViewID:    ua.NewTwoByteNodeID(0),
	// 		Timestamp: time.Now(),
	// 	},
	// 	RequestedMaxReferencesPerNode: 0,
	// 	NodesToBrowse:                 []*ua.BrowseDescription{desc},
	// }
	//response, err := client.Browse(req)

	n := client.Node(ua.NewNumericNodeID(0, id.RootFolder))
	//attrs, err := n.Attributes(ua.AttributeIDNodeID)
	refs, err := n.References(id.Organizes, ua.BrowseDirectionForward, ua.NodeClassAll, true)
	//refs, err := n.ReferencedNodes(0, ua.BrowseDirectionForward, ua.NodeClassAll, true)

	// refs, err := Browse(n, "", 0)
	if err != nil {
		return nil, err
	}
	// for result := range response.Results {
	// 	logger.Print.Debug(fmt.Sprintf("Browsed -> %v", result))
	// }

	logger.Print.Debug(fmt.Sprintf("nodelist -> %v", refs))

	var nodeList []NodeDef
	return nodeList, nil
}

// Close the connection and free up memory
func Close() {
	client.Close()
}
