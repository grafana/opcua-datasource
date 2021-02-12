import { FilterOperand, FilterOperandEnum, FilterOperator, LiteralOp, SimpleAttributeOp } from '../types';
import { browsePathToShortString } from './QualifiedName';

export class EventFilterOperatorUtil {
    static operNames: string[] = [
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
    static GetString(oper: FilterOperator): string {
        return EventFilterOperatorUtil.operNames[oper];
    }


    static GetLiteralString(op: LiteralOp): string {
        return  op.value + ' [' + op.typeId + ']';
    }

    static GetSimpleAttributeString(op: SimpleAttributeOp): string {
        let s = '[' + op.typeId + '] ';
        s += browsePathToShortString(op.browsePath);
        return s;
    }

    static GetOperandString(operand: FilterOperand): string {
        switch (operand.type) {
            case FilterOperandEnum.SimpleAttribute:
                return this.GetSimpleAttributeString(operand.value as SimpleAttributeOp);
            case FilterOperandEnum.Literal:
                return this.GetLiteralString(operand.value as LiteralOp);
        }
        return '';
    }


}