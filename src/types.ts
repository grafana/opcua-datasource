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
    eventFilters: EventFilterSer[];
}

export interface QualifiedName {
    namespaceUrl: string;
    name: string;
}

export interface EventColumn {
    browsename: QualifiedName;
    alias: string;
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
    operands: FilterOperand[];
}

export interface FilterOperand {
    type: FilterOperandEnum;
    value: object;
}

export interface EventFilterSer {
    oper: FilterOperator;
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
    SimpleAttribute = 4
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
    BitwiseOr = 17
}

export class EventFilterOperatorUtil {
    public static operNames: string[] = ["==", "IsNull", ">", "<", ">=", "<=", "Like", "Not", "Between", "InList", "And", "Or", "Cast", "InView", "OfType", "RelatedTo", "BitwiseAnd", "BitwiseOr"];
    static GetString(oper: FilterOperator): string {
        return EventFilterOperatorUtil.operNames[oper];
    }

    static GetQualifiedNameString(qm: QualifiedName): string {
        if (qm.namespaceUrl != null && qm.namespaceUrl.length > 0)
            return qm.namespaceUrl + ":" + qm.name;
        else
            return qm.name;
    }



    static GetLiteralString(op: LiteralOp): string {
        return "Data type node: " + op.typeId + " Value: " + op.value;
    }

    static GetSimpleAttributeString(op: SimpleAttributeOp): string {
        let s = "Type definition node: " + op.typeId + "  BrowsePath: ";
        for (var i = 0; i < op.browsePath.length; i++) {
            s += this.GetQualifiedNameString(op.browsePath[i]);
            if (i < op.browsePath.length - 1)
                s += "/";
        }
        return s;
    }

    static GetOperandString(operand: FilterOperand): string {
        switch (operand.type) {
            case FilterOperandEnum.SimpleAttribute:
                return this.GetSimpleAttributeString(operand.value as SimpleAttributeOp);
            case FilterOperandEnum.Literal:
                return this.GetLiteralString(operand.value as LiteralOp);
        }
        return "";
    }
}


export const separator = ' / ';

/**
 * These are options configured for each DataSource instance
 */
export interface OpcUaDataSourceOptions extends DataSourceJsonData {}
