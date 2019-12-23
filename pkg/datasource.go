package main

import (
	"fmt"
	//"strings"
	"encoding/json"

	"github.com/grafana/grafana-opcua-datasource/pkg/cgoopc"
	"github.com/grafana/grafana-opcua-datasource/pkg/logger"
	"github.com/grafana/grafana-opcua-datasource/pkg/opc"
	"github.com/grafana/grafana_plugin_model/go/datasource"
	plugin "github.com/hashicorp/go-plugin"
	"golang.org/x/net/context"
)

// GrafanaOpcUADatasource stores reference to plugin and logger
type GrafanaOpcUADatasource struct {
	plugin.NetRPCUnsupportedPlugin
}

func init() {

}

// Query all the data
func (ds *GrafanaOpcUADatasource) Query(ctx context.Context, tsdbReq *datasource.DatasourceRequest) (*datasource.DatasourceResponse, error) {
	logger.Print.Debug("Query", "datasource", tsdbReq.Datasource.Name, "TimeRange", tsdbReq.TimeRange)
	logger.Print.Debug(fmt.Sprintf("Query looks like ------> %v", tsdbReq))
	logger.Print.Debug(fmt.Sprintf("context: %v", ctx))
	response, err := ds.CreateMetricRequest(tsdbReq)

	return response, err
}

// CreateMetricRequest is what this does
func (ds *GrafanaOpcUADatasource) CreateMetricRequest(tsdbReq *datasource.DatasourceRequest) (*datasource.DatasourceResponse, error) {
	queries, err := parseJSONQueries(tsdbReq)
	if err != nil {
		return nil, err
	}

	tsdbRes := &datasource.DatasourceResponse{}
	for idx, q := range queries {
		logger.Print.Debug(fmt.Sprintf("Handling idx(%d), q=%v", idx, q))
		switch q.Call {
		case "GetTree":
			err := opc.Connect(tsdbReq.Datasource.Url, tsdbReq.Datasource.DecryptedSecureJsonData["tlsClientCert"], tsdbReq.Datasource.DecryptedSecureJsonData["tlsClientKey"])
			if err != nil {
				logger.Print.Debug(fmt.Sprintf("Could not connect %v", err))
			}
			_, err = opc.GetTree()
			if err != nil {
				logger.Print.Debug(fmt.Sprintf("Could not browse: %s", err))
			}
			// tsdbRes.Results = make([]*datasource.QueryResult, len(tsdbReq.Queries))
			// tsdbRes.Results[0] = &datasource.QueryResult{
			// 	RefId:  q.RefID,
			// 	Tables: opcTableToQueryResultTable(opcTree),
			// }
			break
		}

		if err != nil {
			logger.Print.Debug(fmt.Sprintf("Error: %v", err))
			return tsdbRes, err
		}
	}

	return tsdbRes, nil
}

func opcTableToQueryResultTable(opcTable []cgoopc.TagItem) []*datasource.Table {
	var rows []*datasource.TableRow
	for _, opcItem := range opcTable {
		logger.Print.Debug(fmt.Sprintf("Handling opcItem [%v] [%v] [%v]", opcItem.StringNodeId, opcItem.DisplayName, opcItem.Quality))
		newRow := &datasource.TableRow{
			Values: []*datasource.RowValue{
				&datasource.RowValue{Int64Value: int64(opcItem.NameSpace), Kind: datasource.RowValue_TYPE_INT64},
				&datasource.RowValue{StringValue: opcItem.StringNodeId, Kind: datasource.RowValue_TYPE_STRING},
				&datasource.RowValue{StringValue: opcItem.DisplayName, Kind: datasource.RowValue_TYPE_STRING},
				&datasource.RowValue{Int64Value: int64(opcItem.Data.(int)), Kind: datasource.RowValue_TYPE_INT64},
				&datasource.RowValue{BoolValue: opcItem.ReadOnly, Kind: datasource.RowValue_TYPE_BOOL},
				&datasource.RowValue{Int64Value: int64(opcItem.MonitoringId), Kind: datasource.RowValue_TYPE_INT64},
				&datasource.RowValue{StringValue: opcItem.EncodedTagId, Kind: datasource.RowValue_TYPE_STRING},
				&datasource.RowValue{StringValue: opcItem.CustomAlias, Kind: datasource.RowValue_TYPE_STRING},
				&datasource.RowValue{BoolValue: opcItem.HasCustomAlias, Kind: datasource.RowValue_TYPE_BOOL},
				&datasource.RowValue{Int64Value: int64(opcItem.Quality), Kind: datasource.RowValue_TYPE_INT64},
			},
		}

		rows = append(rows, newRow)
	}

	table := &datasource.Table{
		Columns: []*datasource.TableColumn{
			&datasource.TableColumn{Name: "NameSpace"},
			&datasource.TableColumn{Name: "StringNodeId"},
			&datasource.TableColumn{Name: "DisplayName"},
			&datasource.TableColumn{Name: "Data"},
			&datasource.TableColumn{Name: "ReadOnly"},
			&datasource.TableColumn{Name: "MonitoringId"},
			&datasource.TableColumn{Name: "EncodedTagId"},
			&datasource.TableColumn{Name: "CustomAlias"},
			&datasource.TableColumn{Name: "HasCustomAlias"},
			&datasource.TableColumn{Name: "Quality"},
		},
		Rows: rows,
	}

	return []*datasource.Table{table}
}

func parseJSONQueries(tsdbReq *datasource.DatasourceRequest) ([]*opc.QueryModel, error) {
	opcQueries := make([]*opc.QueryModel, 0)
	for _, query := range tsdbReq.Queries {
		q := &opc.QueryModel{}
		err := json.Unmarshal([]byte(query.ModelJson), &q)
		if err != nil {
			return nil, err
		}

		opcQueries = append(opcQueries, q)
	}
	return opcQueries, nil
}
