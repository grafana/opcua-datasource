import { FilterOperand, FilterOperandEnum, FilterOperator, LiteralOp, NSNodeId, SimpleAttributeOp } from '../types';
import { nodeIdToShortString } from './NodeId';
import { browsePathToShortString } from './QualifiedName';

export class EventFilterOperatorUtil {
  static operatorNames: string[] = [
    '==',
    'IsNull',
    '>',
    '<',
    '>=',
    '<=',
    'Like',
    'Not',
    'Between',
    'InList',
    'And',
    'Or',
    'Cast',
    'InView',
    'OfType',
    'RelatedTo',
    'BitwiseAnd',
    'BitwiseOr',
  ];
  static GetString(operator: FilterOperator): string {
    return EventFilterOperatorUtil.operatorNames[operator];
  }

  static GetLiteralString(op: LiteralOp, nsTable: string[]): string {
    let nsNodeId: NSNodeId = JSON.parse(op.typeId);
    let s = nodeIdToShortString(nsNodeId, nsTable);
    return op.value + ' [' + s + ']';
  }

  static GetSimpleAttributeString(op: SimpleAttributeOp): string {
    let s = browsePathToShortString(op.browsePath);
    if (op.typeId.length > 0) {
      s += ' [' + op.typeId + ']';
    }
    return s;
  }

  static GetOperandString(operand: FilterOperand, nsTable: string[]): string {
    switch (operand.type) {
      case FilterOperandEnum.SimpleAttribute:
        return this.GetSimpleAttributeString(operand.value as SimpleAttributeOp);
      case FilterOperandEnum.Literal:
        return this.GetLiteralString(operand.value as LiteralOp, nsTable);
    }
    return '';
  }
}
