import { DataQuery, DataSourceJsonData } from '@grafana/data';

export enum NodeClass {
  Unspecified = 0,
  Object = 1,
  Variable = 2,
  Method = 4,
  ObjectType = 8,
  VariableType = 16,
  ReferenceType = 32,
  DataType = 64,
  View = 128,
}

export interface NSNodeId {
  namespaceUrl: string;
  id: string;
}

export interface BrowseFilter {
  maxResults: number;
  browseName: string;
}

export interface OpcUaQuery extends DataQuery {
  useTemplate: boolean;
  templateVariable: string;
  nodePath: NodePath;
  relativePath: QualifiedName[];
  alias: string;
  readType: string;
  aggregate: OpcUaNodeDefinition;
  maxValuesPerNode: number;
  resampleInterval: number;
  eventQuery: EventQuery;
}

export interface NodePath {
  node: OpcUaNodeInfo;
  browsePath: QualifiedName[];
}

export interface EventQuery {
  eventTypeNodeId: string;
  eventColumns: EventColumn[];
  eventFilters: EventFilterSer[];
}

export interface QualifiedName {
  namespaceUrl: string;
  name: string;
}

export interface EventColumn {
  browsePath: QualifiedName[];
  alias: string;
}

export interface DashboardInfo {
  name: string;
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

export interface OpcUaNodeInfo {
  displayName: string;
  browseName: QualifiedName;
  nodeId: string;
  nodeClass: number;
}

export interface OpcUaBrowseResults extends OpcUaNodeInfo {
  isForward: boolean;
}

export interface OpcUaNodeDefinition {
  name: string;
  nodeId: string;
}

export interface EventFilter {
  operator: FilterOperator;
  operands: FilterOperand[];
}

export interface FilterOperand {
  type: FilterOperandEnum;
  value: object;
}

export interface EventFilterSer {
  operator: FilterOperator;
  operands: FilterOperandSer[];
}

export interface FilterOperandSer {
  type: FilterOperandEnum;
  value: string;
}

export enum FilterOperandEnum {
  Literal = 1,
  Element = 2,
  Attribute = 3,
  SimpleAttribute = 4,
}

export interface LiteralOp {
  typeId: string;
  value: string;
}

export interface ElementOp {
  index: number;
}

export interface AttributeOp {
  //TODO
}

export interface SimpleAttributeOp {
  typeId: string;
  browsePath: QualifiedName[];
  attributeId: number;
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
  BitwiseOr = 17,
}

export const separator = ' / ';

export enum OPCTimestamp {
  Server = 'server',
  Source = 'source',
}

/**
 * These are options configured for each DataSource instance
 */
export interface OpcUaDataSourceOptions extends DataSourceJsonData {
  timestamp: OPCTimestamp;
}
