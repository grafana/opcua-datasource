package cgoopc

import (
	"testing"
	"log"
	"time"
)

var driver *OPCUA_Driver
//var opcURL = "opc.tcp://192.168.1.205:4840"
var opcURL = "opc.tcp://172.28.1.96:4840"


var config = Config{
	Iface:		"opc.tcp://",
	Host:		"172.28.1.96",
	Port:		4840,
	Exchange:	"opcua-dev",
	Hosts:		[]string{"172.28.1.74", "172.28.1.96", "172.28.1.220"},
	LogLevel:	4,
	RootNode:	"|var|CODESYS Control for Raspberry Pi SL.Application.PLC_PRG.",
	Origin:		"plc",
}

func TestOPCUA_Driver_TryToConnect(t *testing.T) {
	driver = New_OPC_Driver(&config, opcURL, 200)
	driver.OnValueChange = test_handler

	driver.ConnectRetry()
}

func test_handler(tag TagItem) {
	log.Printf("handler: alias=%s, val=%d\n", tag.EncodedTagId, tag.Data.(int))
}

func TestOPCUA_Driver_SetSelected(t *testing.T) {

	TestOPCUA_Driver_TryToConnect(t)

	selected := []string{
		"|var|CODESYS Control for Raspberry Pi SL.Application.PLC_PRG.i",
		"|var|CODESYS Control for Raspberry Pi SL.Application.PLC_PRG.count",
	}
	driver.SetSelectedList(selected)
	time.Sleep(time.Second * 5)
}

func TestOPCUA_Driver_SetSelected2(t *testing.T) {
	TestOPCUA_Driver_SetSelected(t)

	log.Println("client unsubscribe all ...")
	driver.UnSubscribeAll()

	driver.Client.SubscriptionCreate(200.0)
	subId := driver.Client.sub.id
	if subId == 0 {
		log.Printf("[WARN] No subscriptionId was found\n")
	} else {
		log.Printf("[INFO] subscriptionId=%d\n", subId)
	}

	for _, tag := range driver.selectedTagsMap {
		log.Printf("Subscribing from selected map: ns=%d, s=%s", tag.NameSpace, tag.StringNodeId)
		err := driver.Client.Subscribe(subId, uint16(tag.NameSpace), tag.StringNodeId, default_handler_on_change)
		if err != nil {
			log.Printf("[ERROR] SubscribeSelected() err=%v\n", err)
		} else {
			log.Printf("[INFO] Subscribed")
		}
	}

	driver.Client.StartListen(200)
	time.Sleep(time.Second * 5)
}

func TestOPCUA_Driver_SetSelected3(t *testing.T) {

	TestOPCUA_Driver_SetSelected(t)

	log.Println("client unsubscribe all ...")
	driver.UnSubscribeAll()

	driver.Client.SubscriptionCreate(200.0)
	subId := driver.Client.sub.id
	if subId == 0 {
		log.Printf("[WARN] No subscriptionId was found\n")
		return
	} else {
		log.Printf("[INFO] subscriptionId=%d\n", subId)
	}

	for _, tag := range driver.selectedTagsMap {
		log.Printf("Subscribing from selected map: ns=%d, s=%s", tag.NameSpace, tag.StringNodeId)
		err := driver.Client.Subscribe(subId, uint16(tag.NameSpace), tag.StringNodeId, default_handler_on_change)
		if err != nil {
			log.Printf("[ERROR] SubscribeSelected() err=%v\n", err)
		} else {
			log.Printf("[INFO] Subscribed")
		}
	}
	driver.Client.StartListen(200)
	time.Sleep(time.Second * 5)
}

func TestOPCUA_Driver_SetSelected4(t *testing.T) {

	TestOPCUA_Driver_SetSelected(t)

	log.Println("client unsubscribe all ...")
	driver.UnSubscribeAll()

	driver.SubscribeSelected()
	time.Sleep(time.Second * 3)
}

func TestOPCUA_Driver_SetSelected_Cycle(t *testing.T) {

	TestOPCUA_Driver_SetSelected(t)

	for i := 0; i < 1000; i++ {
		log.Println("client unsubscribe all ...")
		driver.UnSubscribeAll()

		driver.SubscribeSelected()
		time.Sleep(time.Second * 5)
	}
}