import { DataQuery, DataSourceJsonData, SelectableValue } from '@grafana/data';

export interface OpcUaQuery extends DataQuery {
  metric: SelectableValue<string>;
}

export interface OpcUaResultsEntry {
  meta: string;
  series: any;
  tables: any;
}

export interface OpcUaResults {
  results: Record<string, OpcUaResultsEntry>;
}

export interface OpcUaResponse {
  data: OpcUaResults;
}

export interface OpcUaBrowseResults {
  displayName: string;
  browseName: string;
  nodeId: string;
}

/**
 * These are options configured for each DataSource instance
 */
export interface OpcUaDataSourceOptions extends DataSourceJsonData {}
