package main

import (
	"os"

	"github.com/grafana/grafana-plugin-sdk-go/backend/datasource"
	"github.com/grafana/grafana-plugin-sdk-go/backend/log"
	"github.com/grafana/opcua-datasource/pkg/opc"
)

func main() {
	if err := datasource.Manage("grafana-opcua-datasource", opc.NewOpcUADatasource, datasource.ManageOpts{}); err != nil {
		log.DefaultLogger.Error(err.Error())
		os.Exit(1)
	}
}
