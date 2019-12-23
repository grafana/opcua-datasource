package opc

import (
	"fmt"
	"strconv"

	"github.com/gopcua/opcua"
	"github.com/gopcua/opcua/id"
	"github.com/gopcua/opcua/ua"

	"github.com/grafana/grafana-opcua-datasource/pkg/logger"
)

type NodeDef struct {
	NodeID      *ua.NodeID
	NodeClass   ua.NodeClass
	BrowseName  string
	Description string
	AccessLevel ua.AccessLevelType
	Path        string
	DataType    string
	Writable    bool
	Unit        string
	Scale       string
	Min         string
	Max         string
}

func (n NodeDef) Records() []string {
	return []string{n.BrowseName, n.DataType, n.NodeID.String(), n.Unit, n.Scale, n.Min, n.Max, strconv.FormatBool(n.Writable), n.Description}
}

func join(a, b string) string {
	if a == "" {
		return b
	}
	return a + "." + b
}

// Browse all the nodes
func Browse(n *opcua.Node, path string, level int) ([]NodeDef, error) {
	logger.Print.Debug(fmt.Sprintf("node:%s path:%q level:%d\n", n, path, level))
	if level > 10 {
		return nil, nil
	}

	attrs, err := n.Attributes(ua.AttributeIDNodeClass, ua.AttributeIDBrowseName, ua.AttributeIDDescription, ua.AttributeIDAccessLevel, ua.AttributeIDDataType)
	if err != nil {
		return nil, err
	}

	var def = NodeDef{
		NodeID: n.ID,
	}

	switch err := attrs[0].Status; err {
	case ua.StatusOK:
		def.NodeClass = ua.NodeClass(attrs[0].Value.Int())
	default:
		return nil, err
	}

	switch err := attrs[1].Status; err {
	case ua.StatusOK:
		def.BrowseName = attrs[1].Value.String()
	default:
		return nil, err
	}

	// switch err := attrs[2].Status; err {
	// case ua.StatusOK:
	// 	if attrs[2].Value != nil {
	// 		def.Description = attrs[2].Value.String()
	// 	}
	// case ua.StatusBadAttributeIDInvalid:
	// 	// ignore
	// default:
	// 	return nil, err
	// }

	// switch err := attrs[3].Status; err {
	// case ua.StatusOK:
	// 	def.AccessLevel = ua.AccessLevelType(attrs[3].Value.Int())
	// 	def.Writable = def.AccessLevel&ua.AccessLevelTypeCurrentWrite == ua.AccessLevelTypeCurrentWrite
	// case ua.StatusBadAttributeIDInvalid:
	// 	// ignore
	// default:
	// 	return nil, err
	// }

	// switch err := attrs[4].Status; err {
	// case ua.StatusOK:
	// 	switch v := attrs[4].Value.NodeID().IntID(); v {
	// 	case id.DateTime:
	// 		def.DataType = "time.Time"
	// 	case id.Boolean:
	// 		def.DataType = "bool"
	// 	case id.SByte:
	// 		def.DataType = "int8"
	// 	case id.Int16:
	// 		def.DataType = "int16"
	// 	case id.Int32:
	// 		def.DataType = "int32"
	// 	case id.Byte:
	// 		def.DataType = "byte"
	// 	case id.UInt16:
	// 		def.DataType = "uint16"
	// 	case id.UInt32:
	// 		def.DataType = "uint32"
	// 	case id.UtcTime:
	// 		def.DataType = "time.Time"
	// 	case id.String:
	// 		def.DataType = "string"
	// 	case id.Float:
	// 		def.DataType = "float32"
	// 	case id.Double:
	// 		def.DataType = "float64"
	// 	default:
	// 		def.DataType = attrs[4].Value.NodeID().String()
	// 	}
	// case ua.StatusBadAttributeIDInvalid:
	// 	// ignore
	// default:
	// 	return nil, err
	// }

	def.Path = join(path, def.BrowseName)
	logger.Print.Debug(fmt.Sprintf("%d: def.Path:%s def.NodeClass:%s\n", level, def.Path, def.NodeClass))

	var nodes []NodeDef
	if def.NodeClass == ua.NodeClassVariable {
		nodes = append(nodes, def)
	}

	browseChildren := func(refType uint32) error {
		refs, err := n.ReferencedNodes(refType, ua.BrowseDirectionForward, ua.NodeClassAll, true)
		if err != nil {
			logger.Print.Debug(fmt.Sprintf("References: %d: %s", refType, err))
		}
		logger.Print.Debug(fmt.Sprintf("found %d child refs for %v", len(refs), n))
		for _, rn := range refs {
			children, err := Browse(rn, def.Path, level+1)
			if err != nil {
				logger.Print.Debug(fmt.Sprintf("browse children: %s", err))
			}
			nodes = append(nodes, children...)
		}
		return nil
	}

	if err := browseChildren(id.HasComponent); err != nil {
		return nil, err
	}
	if err := browseChildren(id.Organizes); err != nil {
		return nil, err
	}
	if err := browseChildren(id.HasProperty); err != nil {
		return nil, err
	}
	return nodes, nil
}
