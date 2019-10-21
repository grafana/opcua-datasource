// Copyright 2018-2019 opcua authors. All rights reserved.
// Use of this source code is governed by a MIT-style license that can be
// found in the LICENSE file.

package main

import (
	"log"

	"github.com/grafana/grafana-opcua-datasource/pkg/cgoopc"

)

var config = cgoopc.Config{
	Iface:		"opc.tcp://",
	Host:		"mi",
	Port:		48400,
	Exchange:	"opcua-dev",
	Hosts:		[]string{"mi"},
	LogLevel:	4,
	RootNode:	"|var|CODESYS Control for Raspberry Pi SL.Application.PLC_PRG.",
	Origin:		"plc",
}


func main() {
	log.Println("Starting")
	c := cgoopc.NewClient(4)
	c.Connect("opc.tcp://mi:48400/UA/ComServerWrapper")
	defer c.Delete()
	for idx, br := range c.BrowseTags() {
		log.Printf("[%d]: %v", idx, br)
	}
}

