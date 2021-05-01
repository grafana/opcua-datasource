//import defaults from 'lodash/defaults';

import React, { PureComponent } from 'react';
import { QueryEditorProps } from '@grafana/data';
import { DataSource } from '../DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults } from '../types';
import { Transfer, Tree } from 'antd';
import { TransferProps, TransferItem } from 'antd/lib/transfer';
import './TreeEditorDark.less';
const rootNode = 'i=85';
const loadingText = 'Loading Data...';
const loadingItem: TransferItem = {
  key: loadingText,
  title: loadingText,
  children: [],
};

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;

interface TreeTransferProps extends TransferProps {
  onExpand: (nodeId: string) => void;
}

// Customize Table Transfer
const { TreeNode } = Tree;

const isChecked = (selectedKeys: any[], eventKey: any): any => {
  return selectedKeys.indexOf(eventKey) !== -1;
};

const generateTree = (treeNodes: any[] = [], checkedKeys: any[] = []) => {
  return treeNodes.map(({ children, ...props }) => (
    <TreeNode {...props} disabled={checkedKeys.includes(props.key)} key={props.key}>
      {generateTree(children, checkedKeys)}
    </TreeNode>
  ));
};

const TreeTransfer = ({ dataSource, targetKeys, ...restProps }: TreeTransferProps) => {
  const transferDataSource: TransferItem[] = [];
  function flatten(list: TransferItem[] = []) {
    list.forEach((item: TransferItem) => {
      transferDataSource.push(item);
      flatten(item.children);
    });
  }
  flatten(dataSource);

  return (
    <Transfer
      {...restProps}
      targetKeys={targetKeys}
      dataSource={transferDataSource}
      className="tree-transfer"
      render={(item) => item.title || 'Unknown Title'}
      showSelectAll={false}
    >
      {({ direction, onItemSelect, selectedKeys }) => {
        if (direction === 'left') {
          const checkedKeys = [...selectedKeys, ...(targetKeys || [])];
          return (
            <Tree
              blockNode
              checkable
              checkStrictly
              defaultExpandAll
              checkedKeys={checkedKeys}
              onExpand={(_, ae) => {
                if (ae.expanded && ae.node.props.eventKey) {
                  restProps.onExpand(ae.node.props.eventKey);
                }
              }}
              onCheck={(
                _,
                {
                  node: {
                    props: { eventKey },
                  },
                }
              ) => {
                if (typeof eventKey !== 'undefined') {
                  onItemSelect(eventKey, !isChecked(checkedKeys, eventKey));
                }
              }}
              onSelect={(
                _,
                {
                  node: {
                    props: { eventKey },
                  },
                }
              ) => {
                if (typeof eventKey !== 'undefined') {
                  onItemSelect(eventKey, !isChecked(checkedKeys, eventKey));
                }
              }}
            >
              {generateTree(dataSource, targetKeys)}
            </Tree>
          );
        }

        return null;
      }}
    </Transfer>
  );
};

interface State {
  dataSource: TransferItem[];
  targetKeys: string[];
  listStyle: any;
}

export class TreeEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);

    //const { query } = this.props;
    this.state = {
      dataSource: [loadingItem],
      targetKeys: [],
      listStyle: {},
    };

    this.getTreeData(rootNode).then((results: TransferItem[]) => {
      this.setState({
        dataSource: results,
        //targetKeys: query.value ? [query.value.join(separator)] : [],
      });
    });
  }

  onChange = (targetKeys: string[], direction: string, moveKeys: string[]) => {
    console.log('onChange', targetKeys, direction, moveKeys);
    this.setState({ ...this.state, targetKeys });
  };

  onSelectChange = (sourceSelectedKeys: string[], targetSelectedKeys: string[]) => {
    console.log('Source', sourceSelectedKeys, 'target', targetSelectedKeys);
  };

  findNode = (nodeId: string, dataSource: TransferItem[]): TransferItem | undefined => {
    for (const item of dataSource) {
      if (item.key === nodeId) {
        return item;
      } else {
        const found = item.hasOwnProperty('children') ? this.findNode(nodeId, item.children) : false;
        if (found) {
          return found;
        }
      }
    }

    return undefined;
  };

  onExpand = (nodeId: string) => {
    const { dataSource } = this.state;
    const entry = this.findNode(nodeId, dataSource);
    if (entry) {
      this.getTreeData(nodeId).then((children: TransferItem[]) => {
        entry.children = children;
        this.updateDataSource([...dataSource]);
      });
    }
  };

  updateDataSource = (ds: TransferItem[]) => {
    console.log('updating ds', ds);
    this.setState({
      dataSource: ds,
    });
  };

  updateTargetKeys = (tk: string[]) => {
    this.setState({
      targetKeys: tk,
    });
  };

  getTreeData = (nodeId: string): Promise<TransferItem[]> => {
    return this.props.datasource.getResource('browse', { nodeId }).then((results: OpcUaBrowseResults[]) =>
      results.map((item: OpcUaBrowseResults) => {
        if (item.nodeClass !== 2 && item.isForward) {
          return {
            key: item.nodeId,
            title: item.displayName,
            children: [loadingItem],
            checkable: true,
          };
        } else {
          console.log('item', item);
          return {
            key: item.nodeId,
            title: item.displayName,
            checkable: true,
          };
        }
      })
    );
  };

  render() {
    const { targetKeys, dataSource, listStyle } = this.state;
    console.log('treeview::render', targetKeys, dataSource);

    return (
      <div className="gf-form-inline">
        <TreeTransfer
          showSearch={false}
          dataSource={dataSource}
          targetKeys={targetKeys}
          onChange={this.onChange}
          onSelectChange={this.onSelectChange}
          listStyle={listStyle}
          onExpand={this.onExpand}
          style={{ width: '100%' }}
        />
      </div>
    );
  }
}
