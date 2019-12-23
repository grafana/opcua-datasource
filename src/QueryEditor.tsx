//import defaults from 'lodash/defaults';

import React, { PureComponent } from 'react';
import { QueryEditorProps } from '@grafana/data';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults } from './types';
import { Transfer, Tree } from 'antd';
import { TransferProps, TransferItem } from 'antd/lib/transfer';
import 'QueryEditor.css';

const rootNode = 'i=84';
const loadingText = 'Loading Data...';
const loadingItem: TransferItem = {
  key: loadingText,
  title: loadingText,
  children: [],
};

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;

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

const TreeTransfer = ({ dataSource, targetKeys, ...restProps }: TransferProps) => {
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
      render={item => item.title || 'Unknown Title'}
      showSelectAll={false}
    >
      {({ direction, onItemSelect, selectedKeys }) => {
        if (direction === 'left') {
          const checkedKeys = [...selectedKeys, ...targetKeys];
          return (
            <Tree
              blockNode
              checkable
              checkStrictly
              defaultExpandAll
              checkedKeys={checkedKeys}
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

        return '';
      }}
    </Transfer>
  );
};

// const treeData: Array<TransferItem> = [
//   { key: '0-0', title: '0-0' },
//   {
//     key: '0-1',
//     title: '0-1',
//     children: [{ key: '0-1-0', title: '0-1-0' }, { key: '0-1-1', title: '0-1-1' }],
//   },
//   { key: '0-2', title: '0-3' },
// ];

interface State {
  dataSource: TransferItem[];
  targetKeys: string[];
  listStyle: any;
}

export class QueryEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      dataSource: this.getTreeData(rootNode),
      targetKeys: [],
      listStyle: {},
    };
  }

  onChange = (targetKeys: string[]) => {
    console.log('onChange', targetKeys);
    this.setState({ ...this.state, targetKeys });
  };

  onSelectChange = (sourceSelectedKeys: string[], targetSelectedKeys: string[]) => {
    console.log('Source', sourceSelectedKeys, 'target', targetSelectedKeys);
  };

  updateDataSource = (ds: TransferItem[]) => {
    this.setState({
      dataSource: ds,
    });
  };

  updateTargetKeys = (tk: string[]) => {
    this.setState({
      targetKeys: tk,
    });
  };

  getTreeData = (nodeId: string): TransferItem[] => {
    this.props.datasource.browse(nodeId).then((results: OpcUaBrowseResults[]) => {
      this.updateDataSource(
        results.map((item: OpcUaBrowseResults) => {
          return {
            key: item.nodeId,
            title: item.displayName,
            children: [loadingItem],
          };
        })
      );
    });

    return [loadingItem];
  };

  render() {
    const { targetKeys, dataSource, listStyle } = this.state;

    return (
      <div className="gf-form">
        <TreeTransfer
          showSearch={true}
          filterOption={(inputValue, item) => (item && item.title ? item.title.includes(inputValue) : false)}
          dataSource={dataSource}
          targetKeys={targetKeys}
          onChange={this.onChange}
          onSelectChange={this.onSelectChange}
          listStyle={listStyle}
        />
      </div>
    );
  }
}
