// Package cgoopc provides Go OPCUA client API and driver to browse/read/write/subscribe OPCUA tags.
package cgoopc

// open62541 library wrapper in Go

/*
#cgo CFLAGS: -std=c99 -I .
#include <stdlib.h>
#include "cgoua_api.h"
*/
import "C"
import (
	"errors"
	"fmt"
	"log"
	"math"
	"sync"
	"time"
	"unsafe"
)

type DataHandler func(monId uint32, value interface{}, status uint32)
type TagSubscribeHandler func(tag *TagItem)

var mu sync.Mutex
var staticHandlerOnChange DataHandler

type gNodeData struct {
	NameSpace    UA_UInt16
	Type         UA_Int32
	IntNodeId    UA_Int32
	StringNodeId *C.char
	DisplayName  *C.char
}

type gNodeArray struct {
	NodeData *gNodeData
	Used     UA_Int64
	Size     UA_Int64
}

func (a *gNodeArray) printTagList() {
	nodeData := (*gNodeData)(unsafe.Pointer(a.NodeData))
	for i := 0; i < (int)(a.Size)-1; i++ {
		nodeData.Print()
		nodeData = (*gNodeData)(unsafe.Pointer(uintptr(unsafe.Pointer(nodeData)) + unsafe.Sizeof(*nodeData)))
	}
}

func (a *gNodeArray) toList() []TagItem {
	alloc := (int)(a.Used)
	j := 0

	n := (*gNodeData)(unsafe.Pointer(a.NodeData))
	// get the size of result
	for i := 0; i < alloc; i++ {
		if len(C.GoString(n.StringNodeId)) != 0 {
			j++
		}
		n = (*gNodeData)(unsafe.Pointer(uintptr(unsafe.Pointer(n)) + unsafe.Sizeof(*n)))
	}
	result := make([]TagItem, j, j)
	k := 0
	nodeData := (*gNodeData)(unsafe.Pointer(a.NodeData))

	for i := 0; i < alloc; i++ {
		if len(C.GoString(nodeData.StringNodeId)) != 0 {
			result[k].DisplayName = C.GoString(nodeData.DisplayName)
			result[k].StringNodeId = C.GoString(nodeData.StringNodeId)
			result[k].Data = 0
			result[k].NameSpace = nodeData.NameSpace
			//result[k].CLink = nodeData
			k++
		}
		nodeData = (*gNodeData)(unsafe.Pointer(uintptr(unsafe.Pointer(nodeData)) + unsafe.Sizeof(*nodeData)))
	}
	return result
}

func (n *gNodeData) Print() {
	if n.IsNumberValue() {
		log.Printf("ns=%d, t=%d, %d, %s, %s\n", (int)(n.NameSpace), (int)(n.Type), (int)(n.IntNodeId),
			C.GoString(n.StringNodeId), C.GoString(n.DisplayName))
	}
}

func (n *gNodeData) IsNumberValue() bool {
	return (int)(n.Type) >= 0 && (int)(n.Type) <= 6
}

type TagItem struct {
	NameSpace      UA_UInt16
	StringNodeId   string
	DisplayName    string
	Data           interface{} //always int or int32
	ReadOnly       bool
	MonitoringId   UA_UInt32
	EncodedTagId   string
	CustomAlias    string
	HasCustomAlias bool
	Quality        uint32
}

func (t *TagItem) Hash() string {
	return fmt.Sprintf("%d+%s", t.NameSpace, t.StringNodeId)
}

type tagMap map[string]TagItem

type monitoringMap map[uint32]TagItem

type subscription struct {
	id          int
	pubInterval float64
	tags        tagMap
	monitors    monitoringMap
}

type Client struct {
	ua_client      *UA_Client
	sub            subscription
	stop           chan int
	listening      bool
	url            string
	logLevel       LogLevel
	OnTagSubscribe *TagSubscribeHandler
}

func NewClient(debug LogLevel) *Client {
	clt := C.UA_Client_new(C.UA_ClientConfig_default)
	log.Printf("[INFO] OPCUA client created. logLevel==%v\n", debug)
	c := Client{
		ua_client:      (*UA_Client)(unsafe.Pointer(clt)),
		sub:            subscription{},
		stop:           make(chan int, 1),
		listening:      false,
		url:            "",
		logLevel:       debug,
		OnTagSubscribe: nil,
	}
	return &c
}

// Connect connecting the client to OPC UA server.
// It is getting ConnInfo string like "opc.tcp://host:port" and returns status code following open62541 OPC UA standard.
func (c *Client) Connect(ConnInfo string) (s UA_StatusCode) {
	if ConnInfo != "" {
		c.url = ConnInfo
	}
	ci := C.CString(c.url)
	defer C.free(unsafe.Pointer(ci))
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	stat := C.UA_Client_connect(clt, ci)
	return *(*UA_StatusCode)(unsafe.Pointer(&stat))
}

func (c *Client) ReConnect(retries int) (int, error) {
	var result int
	forever := false
	if retries == 0 {
		forever = true
	}
	i := 0
	for i < retries || forever {
		log.Printf("[INFO] reconnecting %d ......................................................................\n", i)

		connectStatus := c.Connect("")
		result = int(connectStatus)

		log.Printf("[INFO] connectStatus=%d\n", int(connectStatus))
		if connectStatus == C.UA_STATUSCODE_GOOD {
			return UA_CONNECTION_ESTABLISHED, nil
		}
		time.Sleep(time.Millisecond * 100)
		if !forever {
			i++
		}
	}
	return result, errors.New(fmt.Sprintf("connection was not established, status=%d", result))
}

func (c *Client) Disconnect() (s UA_StatusCode) {
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	stat := C.UA_Client_disconnect(clt)
	return *(*UA_StatusCode)(unsafe.Pointer(&stat))
}

func (c *Client) Delete() {
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	C.UA_Client_delete(clt)
}

func (c *Client) BrowseTags() []TagItem {
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	pNodeArray := C.getNodesArray(clt, C.int(c.logLevel))
	nodeArray := (*gNodeArray)(unsafe.Pointer(pNodeArray))
	return nodeArray.toList()
}

func (c *Client) ReadInt32(ns UA_UInt16, id string) (val UA_Int32, err UA_StatusCode) {
	cid := C.CString(id)
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	val = 0
	err = (UA_StatusCode)(C.readTagInt32(clt, (C.UA_UInt16)(ns), -1, cid, (*C.UA_Int32)(unsafe.Pointer(&val))))
	return val, err
}

func (c *Client) WriteInt32(ns UA_UInt16, id string, val UA_Int32) UA_StatusCode {
	cid := C.CString(id)
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	err := (UA_StatusCode)(C.writeTagInt32(clt, (C.UA_UInt16)(ns), cid, (C.UA_Int32)(val)))
	return err
}

func (c *Client) SubscriptionCreate(pubInterval float64) (int, error) {
	req := C.UA_CreateSubscriptionRequest_default()
	req.requestedPublishingInterval = C.UA_Double(pubInterval)

	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	resp := C.UA_Client_Subscriptions_create(clt, req, nil, nil, nil)

	if resp.responseHeader.serviceResult != C.UA_STATUSCODE_GOOD {
		e := fmt.Sprintf("[ERROR] responseHeader.serviceResult=%s, code=%d",
			C.GoString(C.UA_StatusCode_name(resp.responseHeader.serviceResult)), resp.responseHeader.serviceResult)
		log.Println(e)
		return 0, errors.New(e)
	}
	sub := subscription{
		id:          (int)(resp.subscriptionId),
		pubInterval: pubInterval,
		tags:        make(map[string]TagItem, 0),
		monitors:    make(map[uint32]TagItem, 0),
	}
	c.sub = sub
	return sub.id, nil
}

func (c *Client) SubscriptionDelete() UA_StatusCode {
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	status := C.UA_Client_Subscriptions_deleteSingle(clt, C.UA_UInt32(c.sub.id))
	if status != C.UA_STATUSCODE_GOOD {
		log.Printf("[ERROR] unsubscribe failed, status=%d", status)
	} else {
		log.Printf("[INFO] unsubscribed subId=%d", c.sub.id)
	}
	return UA_StatusCode(status)
}

func (c *Client) StartListen(timeout UA_UInt16) {
	if timeout == 0 {
		timeout = 200
	}
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))
	c.listening = true
	go func(client *C.UA_Client, t UA_UInt16) {
		log.Printf("[DEBUG] listening started")
		for {
			select {
			case <-c.stop:
				c.listening = false
				log.Printf("[DEBUG] listening stopped")
				log.Println("[DEBUG] exiting from go routine loop of StartListen()")
				return
			default:
				C.UA_Client_runAsync(client, C.UA_UInt16(t))
			}
		}
	}(clt, timeout)
}

// StopListen stops ping server loop
func (c *Client) StopListen() {
	if c.listening {
		log.Println("[DEBUG] StopListen() sendind '1' signal to c.stop channel")
		c.stop <- 1
	}
}

// Subscribe to one tag by subscription id, namespace ans string ID. Works only for nodes with string type ID
func (c *Client) Subscribe(subId int, ns uint16, id string, handler DataHandler) error {

	nodeId := C.UA_NODEID_STRING(C.UA_UInt16(ns), C.CString(id))
	monRequest := C.UA_MonitoredItemCreateRequest_default(nodeId)
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))

	staticHandlerOnChange = handler

	var i = 0 // stub for handler function Id (future)
	monResponse := C.createDataExchange(clt, C.UA_UInt32(subId), C.UA_TIMESTAMPSTORETURN_BOTH,
		monRequest, (unsafe.Pointer(&i)), C.int(1), C.int(2))

	if monResponse.statusCode == C.UA_STATUSCODE_GOOD {
		log.Println("")
		log.Printf("[INFO] DataExchande created, status=%s\n", C.GoString(C.UA_StatusCode_name(monResponse.statusCode)))
		log.Printf("[INFO] DataExchange, id=%v\n", UA_Int32(monResponse.monitoredItemId))

		//encodedId := b64.StdEncoding.EncodeToString([]byte(id))
		//encodedId := hex.EncodeToString([]byte(id))
		t := TagItem{
			NameSpace:    UA_UInt16(ns),
			StringNodeId: id,
			DisplayName:  "",
			Data:         nil,
			ReadOnly:     false,
			MonitoringId: UA_UInt32(monResponse.monitoredItemId),
			EncodedTagId: "",
			Quality:      0,
		}
		mu.Lock()
		c.sub.tags[t.Hash()] = t
		c.sub.monitors[uint32(t.MonitoringId)] = t
		mu.Unlock()

		if c.OnTagSubscribe != nil {
			(*c.OnTagSubscribe)(&t)
		}
	} else {
		e := fmt.Sprintf("[ERROR] Subscribe failed, status=%d, %s\n", monResponse.statusCode, C.GoString(C.UA_StatusCode_name(monResponse.statusCode)))
		log.Println(e)
		return errors.New(e)
	}
	return nil
}

// Unsubscribe removes subscription for individual tag by namespace, string ID, subscription ID.
func (c *Client) Unsubscribe(ns UA_UInt16, id string, subId int) UA_StatusCode {
	tagHash := fmt.Sprintf("%d+%s", ns, id)
	tag := c.sub.tags[tagHash]
	return c.UnsubscribeTag(tag, subId)
}

// UnsubscribeTag removes subscription for individual tag by tag object and subscription ID.
func (c *Client) UnsubscribeTag(tag TagItem, subId int) UA_StatusCode {
	clt := (*C.UA_Client)(unsafe.Pointer(c.ua_client))

	log.Printf("[DEBUG] try to delete subId=%d, monId=%d", subId, tag.MonitoringId)
	status := C.UA_Client_MonitoredItems_deleteSingle(clt, C.UA_UInt32(subId), C.UA_UInt32(tag.MonitoringId))
	log.Printf("[DEBUG] delete status=%d", status)

	mu.Lock()
	defer mu.Unlock()
	delete(c.sub.tags, tag.Hash())
	delete(c.sub.monitors, uint32(tag.MonitoringId))

	return UA_StatusCode(status)
}

// go_handler is a static default OnChange event handler exported to C.
//export go_handler
func go_handler(clt *C.UA_Client, subID C.UA_UInt32, subContext unsafe.Pointer, monId C.UA_UInt32, monContext unsafe.Pointer, value *C.UA_DataValue) {
	monitoringID := uint32(monId)

	var val interface{}

	if value == nil || value.value._type == nil || value.value.data == nil {
		log.Print("[ERROR] Received nil value\n")
		return
	}

	// TODO: Use C.UA_NodeId to detect data type instead of type name
	// TODO: Handle missing types

	// We attempt to figure out the incoming data type using the TypeName field
	tName := C.GoString((*C.char)(unsafe.Pointer(value.value._type.typeName)))
	switch tName {
	case "Double":
		val = *(*float64)(value.value.data)
	case "Float":
		val = *(*float32)(value.value.data)
	case "Int64":
		val = *(*int64)(value.value.data)
	case "UInt64":
		val = *(*uint64)(value.value.data)
	case "Int32":
		val = *(*int32)(value.value.data)
	case "UInt32":
		val = *(*uint32)(value.value.data)
	case "Int16":
		val = *(*int16)(value.value.data)
	case "UInt16":
		val = *(*uint16)(value.value.data)
	case "Boolean":
		val = *(*bool)(value.value.data)
	case "Byte":
		val = *(*uint8)(value.value.data)
	case "SByte":
		val = *(*int8)(value.value.data)
	case "DateTime":
		val = *(*int64)(value.value.data)
	case "ByteString":
		fallthrough
	case "String":
		sv := (*C.UA_String)(value.value.data)
		cBytes := unsafe.Pointer(sv.data)

		if sv.length > C.ulong(math.MaxInt32) {
			log.Printf("[ERROR] Received string with size = %d longer than max size for UA_String\n", sv.length)
			val = *(*int)(value.value.data)
			break
		}

		// TODO: Can we avoid casting from C.ulong to C.int??
		bytes := C.GoBytes(cBytes, C.int(sv.length))

		val = string(bytes)
	default:
		// TODO: Should we really send int on unknown types?
		val = *(*int)(value.value.data)
	}

	status := (uint32)(value.status)

	if staticHandlerOnChange == nil {
		staticHandlerOnChange = default_handler_on_change
	}
	go staticHandlerOnChange(monitoringID, val, status)
}

//export default_handler_on_change
func default_handler_on_change(monID uint32, value interface{}, status uint32) {
	log.Printf("[INFO] CHANGE EVENT monitoringID = %d, value = %d, status = %d\n", monID, value, status)
}
