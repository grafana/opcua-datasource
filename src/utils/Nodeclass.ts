import { NodeClass } from '../types';

export function nodeClassToString(nodeClass: NodeClass): string {
  switch (nodeClass) {
    case NodeClass.DataType:
      return 'DataType';
    case NodeClass.Method:
      return 'Method';
    case NodeClass.Object:
      return 'Object';
    case NodeClass.ObjectType:
      return 'ObjectType';
    case NodeClass.ReferenceType:
      return 'ReferenceType';
    case NodeClass.Unspecified:
      return 'Unspecified';
    case NodeClass.Variable:
      return 'Variable';
    case NodeClass.VariableType:
      return 'VariableType';
    case NodeClass.View:
      return 'View';
      return 'Unknown';
  }
}
