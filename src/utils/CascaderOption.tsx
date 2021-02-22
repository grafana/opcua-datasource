import { OpcUaBrowseResults } from '../types';
import { CascaderOption } from 'rc-cascader/lib/Cascader';

export function toCascaderOption(opcBrowseResult: OpcUaBrowseResults, children?: CascaderOption[]): CascaderOption {
  console.log('browse Result', opcBrowseResult);
  return {
    label: opcBrowseResult.displayName,
    value: opcBrowseResult.nodeId,
    isLeaf: !opcBrowseResult.isForward || opcBrowseResult.nodeClass === 2, //!opcBrowseResult.isForward,
  };
}
