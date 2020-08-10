import { DataQuery, DataSourceJsonData } from '@grafana/data';

export interface OpcUaQuery extends DataQuery {
  nodeId: string;
  value: string[];
  readType: string;
  aggregate: OpcUaNodeDefinition;
  interval: string;
  eventQuery: EventQuery;
}

export interface EventQuery {
    eventTypeNodeId: string;
    eventTypes: string[];
    eventColumns: EventColumn[];
    eventFilters: EventFilter[];
}

export interface EventColumn {
    browseName: string;
    alias: string
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

export interface OpcUaNodeDefinition {
  name: string;
  nodeId: string;
}

export interface EventFilter {
    oper: FilterOperator;
    operands: string[];
}

export enum FilterOperator {
    Equals = 0,
    IsNull = 1,
    GreaterThan = 2,
    LessThan = 3,
    GreaterThanOrEqual = 4,
    LessThanOrEqual = 5,
    Like = 6,
    Not = 7,
    Between = 8,
    InList = 9,
    And = 10,
    Or = 11,
    Cast = 12,
    InView = 13,
    OfType = 14,
    RelatedTo = 15,
    BitwiseAnd = 16,
    BitwiseOr = 17
}

export class EventFilterOperatorUtil {
    public static operNames: string[] = ["==", "IsNull", ">", "<", ">=", "<=", "Like", "Not", "Between", "InList", "And", "Or", "Cast", "InView", "OfType", "RelatedTo", "BitwiseAnd", "BitwiseOr"];
    static GetString(oper: FilterOperator): string {
        return EventFilterOperatorUtil.operNames[oper];
    }
}


export const separator = ' / ';

/**
 * These are options configured for each DataSource instance
 */
export interface OpcUaDataSourceOptions extends DataSourceJsonData {}
