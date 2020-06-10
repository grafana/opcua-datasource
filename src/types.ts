import { DataQuery, DataSourceJsonData } from '@grafana/data';

export interface OpcUaQuery extends DataQuery {
  nodeId: string;
  value: string[];
  readType: string;
  aggregate: string;
  interval: string;
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
  isForward: boolean;
  nodeClass: number;
}

export const separator = ' / ';

/**
 * These are options configured for each DataSource instance
 */
export interface OpcUaDataSourceOptions extends DataSourceJsonData {}
