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
    operator: eventFilter.operator,
    operands: eventFilter.operands.map((evf) => serializeEventOperand(evf)),
  };
}

export function deserializeEventFilter(eventFilter: EventFilterSer): EventFilter {
  return {
    operator: eventFilter.operator,
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
  let eventFilterTree: EventFilter[] = [];
  if (eventTypesNode != null) {
    let literal: LiteralOp = { typeId: 'i=17', value: eventTypesNode };

    let filterEventType: EventFilter = {
      operator: FilterOperator.OfType,
      operands: [
        {
          type: FilterOperandEnum.Literal,
          value: literal,
        },
      ],
    };
    eventFilterTree.push(filterEventType);
  }

  let rootIdx = 0;
  for (let i = 0; i < eventFilters.length; i++) {
    eventFilterTree.push(eventFilters[i]);
    let left: ElementOp = { index: rootIdx };
    let right: ElementOp = {
      index: eventFilterTree.length - 1,
    };
    let and: EventFilter = {
      operator: FilterOperator.And,
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
    operator: r.operator,
    operands: r.operands.slice(),
  };
}
