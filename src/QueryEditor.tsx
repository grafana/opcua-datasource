//import defaults from 'lodash/defaults';

import React, { PureComponent } from 'react';
import { QueryEditorProps } from '@grafana/ui';
import { DataSource } from './DataSource';
import { MyQuery, MyDataSourceOptions } from './types';
import { Transfer, Tree } from 'antd';
import { TransferProps, TransferItem } from 'antd/lib/transfer';
import 'QueryEditor.css';

type Props = QueryEditorProps<DataSource, MyQuery, MyDataSourceOptions>;

interface State {}

// Customize Table Transfer
const { TreeNode } = Tree;

const isChecked = (selectedKeys: any[], eventKey: any): any => {
  return selectedKeys.indexOf(eventKey) !== -1;
};

const generateTree = (treeNodes: Array<any> = [], checkedKeys: Array<any> = []) => {
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
      render={item => item.title}
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
                },
              ) => {
                if (typeof eventKey !== "undefined") {
                  onItemSelect(eventKey, !isChecked(checkedKeys, eventKey));
                }
              }}
              onSelect={(
                _,
                {
                  node: {
                    props: { eventKey },
                  },
                },
              ) => {
                if (typeof eventKey !== "undefined") {
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

export class QueryEditor extends PureComponent<Props, State> {
  state: TransferProps = {
    dataSource: [],
    targetKeys: [],
  };

  isLoading = false;

  onChange = (targetKeys: string[]) => {
    console.log("onChange", targetKeys);
    this.setState({ targetKeys });
  };

  updateDataSource = (ds: Array<TransferItem>) => {
    this.setState({
      dataSource: ds,
    })
  };

  updateTargetKeys = (tk: Array<string>) => {
    this.setState({
      targetKeys: tk,
    })
  };

  getTreeData = (): Array<TransferItem> => {
    if ((typeof this.state.dataSource === "undefined" || this.state.dataSource.length == 0) && !this.isLoading) {
      this.isLoading = true;
      this.props.datasource.getTreeData()
      .then((resp): any => {
        let keys: Array<string> =  resp.data.results['A'].tables[0].rows.map((item: any) => {
          console.log("iterating item", item);
          return item[1];
        });
        let newDatasource = keys.map((item: string) => {        
          return { 
            key: item, 
            title: item,
          };
        });

        this.updateDataSource(newDatasource);
        this.isLoading = false;
        return newDatasource;
      });
    }

    return this.state.dataSource;
  }

  render() {
    var { targetKeys } = this.state;
    
    return (
      <div className="gf-form">
        <TreeTransfer 
          showSearch={true} 
          filterOption={(inputValue, item) => item.title.includes(inputValue)}
          dataSource={this.getTreeData()} 
          targetKeys={targetKeys} 
          onChange={this.onChange} 
        />
      </div>
    );     
  }
}
