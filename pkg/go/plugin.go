package main

import (
	"fmt"
	"log"
	//"os"
	//"io"
	"time"
	"net/http"
	"net/http/pprof"

	"google.golang.org/grpc"
	"google.golang.org/grpc/keepalive"

	"github.com/grafana/grafana_plugin_model/go/datasource"
	"github.com/grafana/grafana-opcua-datasource/pkg/logger"
	hclog "github.com/hashicorp/go-hclog"
	plugin "github.com/hashicorp/go-plugin"
)

var pluginLogger = hclog.New(&hclog.LoggerOptions{
	Name:  "opcua-datasource",
	Level: hclog.LevelFromString("DEBUG"),
})


func healthcheckHandler(w http.ResponseWriter, _ *http.Request) {
	w.WriteHeader(http.StatusOK)
	fmt.Fprintln(w, "OK")
}

func registerPProfHandlers(r *http.ServeMux) {
	r.HandleFunc("/debug/pprof/", pprof.Index)
	r.HandleFunc("/debug/pprof/cmdline", pprof.Cmdline)
	r.HandleFunc("/debug/pprof/profile", pprof.Profile)
	r.HandleFunc("/debug/pprof/symbol", pprof.Symbol)
	r.HandleFunc("/debug/pprof/trace", pprof.Trace)
}

// pluginGRPCServer provides a default GRPC server with message sizes increased from 4MB to 16MB
func pluginGRPCServer(opts []grpc.ServerOption) *grpc.Server {
	sopts := grpc.KeepaliveParams(
		keepalive.ServerParameters{
			MaxConnectionIdle: 1 * time.Minute,
		},
	)

	return grpc.NewServer(sopts)
}

func main() {
	// pluginLogger := hclog.New(&hclog.LoggerOptions{
	// 	Name:  "opcua-datasource",
	// 	Level: hclog.LevelFromString("DEBUG"),
	// 	Output: func () io.Writer {
	// 		f, err := os.OpenFile("$HOME/src/grafana/data/plugins/grafana-opcua-datasource/datasource.log", os.O_RDWR|os.O_APPEND, 0644);
	// 		if err != nil {
	// 			log.Fatal("No log joy", err)
	// 		}
	// 		return f
	// 	}(),
	// });
	
	logger.SetLog(pluginLogger)
	pluginLogger.Debug("Running GRPC server")	
	m := http.NewServeMux()
	m.HandleFunc("/healthz", healthcheckHandler)
	registerPProfHandlers(m)
	go func() {
		if err := http.ListenAndServe(":6061", m); err != nil {
			log.Fatal(err)
		}
	}()
	// background pruning of column cache
	go func() {

	}()

	plugin.Serve(&plugin.ServeConfig{
		HandshakeConfig: plugin.HandshakeConfig{
			ProtocolVersion:  1,
			MagicCookieKey:   "grafana_plugin_type",
			MagicCookieValue: "datasource",
		},
		Plugins: map[string]plugin.Plugin{
			"grafana-opcua-datasource": &datasource.DatasourcePluginImpl{Plugin: &GrafanaOpcUADatasource{}},
		},
		GRPCServer: pluginGRPCServer,
	})
}
