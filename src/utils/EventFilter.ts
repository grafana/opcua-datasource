import {
  FilterOperand,
  FilterOperandSer,
  EventFilter,
  EventFilterSer,
  LiteralOp,
  FilterOperator,
  FilterOperandEnum,
  ElementOp,
} from '../types';

function serializeEventOperand(filterOperand: FilterOperand): FilterOperandSer {
  return { type: filterOperand.type, value: JSON.stringify(filterOperand.value) };
}

export function serializeEventFilter(eventFilter: EventFilter): EventFilterSer {
  return {
    oper: eventFilter.oper,
    operands: eventFilter.operands.map((evf) => serializeEventOperand(evf)),
  };
}

export function deserializeEventFilter(eventFilter: EventFilterSer): EventFilter {
  return {
    oper: eventFilter.oper,
    operands: eventFilter.operands.map((evf) => deserializeEventOperand(evf)),
  };
}

export function deserializeEventFilters(eventFilters: EventFilterSer[]): EventFilter[] {
  if (typeof eventFilters !== 'undefined') {
    return eventFilters.map((a) => deserializeEventFilter(a));
  }
  return [];
}

function deserializeEventOperand(filterOperand: FilterOperandSer): FilterOperand {
  return { type: filterOperand.type, value: JSON.parse(filterOperand.value) };
}

export function createFilterTree(eventTypesNode: string, eventFilters: EventFilter[]): EventFilter[] {
  var eventFilterTree: EventFilter[] = [];
  if (eventTypesNode != null) {
    var literal: LiteralOp = { typeId: 'i=17', value: eventTypesNode };

    let filterEventType: EventFilter = {
      oper: FilterOperator.OfType,
      operands: [
        {
          type: FilterOperandEnum.Literal,
          value: literal,
        },
      ],
    };
    eventFilterTree.push(filterEventType);
  }

  var rootIdx = 0;
  for (var i = 0; i < eventFilters.length; i++) {
    eventFilterTree.push(eventFilters[i]);
    var left: ElementOp = { index: rootIdx };
    var right: ElementOp = {
      index: eventFilterTree.length - 1,
    };
    var and: EventFilter = {
      oper: FilterOperator.And,
      operands: [
        { type: FilterOperandEnum.Element, value: left },
        { type: FilterOperandEnum.Element, value: right },
      ],
    };
    eventFilterTree.push(and);
    rootIdx = eventFilterTree.length - 1;
  }
  return eventFilterTree;
}

//deep copy
export function copyEventFilter(r: EventFilter): EventFilter {
  return {
    oper: r.oper,
    operands: r.operands.slice(),
  };
}
