import React, { PureComponent } from 'react';
import { OpcUaBrowseResults, OpcUaNodeInfo, QualifiedName, NodePath, BrowseFilter } from '../types';
import { NodeTextEditor } from './NodeTextEditor';
import { BrowserDialog } from './BrowserDialog';
import { GrafanaTheme } from '@grafana/data';
import { FaSearch } from 'react-icons/fa';

type Props = {
  rootNodeId: string;
  node: NodePath;
  theme: GrafanaTheme | null;
  browse(nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]>;
  getNamespaceIndices(): Promise<string[]>;
  onChangeNode(node: NodePath): void;
  readNode(nodeId: string): Promise<OpcUaNodeInfo>;
  getNodePath(nodeId: string, rootId: string): Promise<NodePath>;
  placeholder: string;
  openBrowser(id: string): void;
  closeBrowser(id: string): void;
  isBrowserOpen(id: string): boolean;
  id: string;
};

type State = {
  node: OpcUaNodeInfo;
};

export class NodeEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      node: {
        browseName: { name: '', namespaceUrl: '' },
        displayName: '',
        nodeClass: -1,
        nodeId: '',
      },
    };
  }

  toggleBrowser = () => {
    if (this.props.isBrowserOpen(this.props.id)) {
      this.props.closeBrowser(this.props.id);
    } else {
      this.props.openBrowser(this.props.id);
    }
  };

  renderNodeBrowser = (rootNodeId: OpcUaBrowseResults) => {
    if (this.props.isBrowserOpen(this.props.id)) {
      return (
        <div
          data-id="Treeview-MainDiv"
          style={{
            border: 'lightgrey 1px solid',
            borderRadius: '1px',
            cursor: 'pointer',
            padding: '2px',
            position: 'absolute',
            left: 30,
            top: 10,
            zIndex: 10,
          }}
        >
          <BrowserDialog
            theme={this.props.theme}
            getNamespaceIndices={() => this.props.getNamespaceIndices()}
            closeBrowser={() => this.props.closeBrowser(this.props.id)}
            closeOnSelect={true}
            browse={(nodeId, filter) => this.props.browse(nodeId, filter)}
            ignoreRootNode={true}
            rootNodeId={rootNodeId}
            onNodeSelectedChanged={(node, browsePath) => {
              this.onChangeNode(node, browsePath);
            }}
          ></BrowserDialog>
        </div>
      );
    }
    return <></>;
  };

  onChangeNode(node: OpcUaNodeInfo, path: QualifiedName[]) {
    let nodePath: NodePath = { node: node, browsePath: path };
    this.setState({ node: node }, () => this.props.onChangeNode(nodePath));
  }

  render() {
    let rootNodeId: OpcUaBrowseResults = {
      browseName: { name: '', namespaceUrl: '' },
      displayName: '',
      isForward: true,
      nodeClass: 0,
      nodeId: this.props.rootNodeId,
    };
    return (
      <div className="gf-form-inline">
        <NodeTextEditor
          getNamespaceIndices={() => this.props.getNamespaceIndices()}
          placeholder={this.props.placeholder}
          getNodePath={(s) => this.props.getNodePath(s, this.props.rootNodeId)}
          node={this.props.node}
          onNodeChanged={(n: OpcUaNodeInfo, path: QualifiedName[]) => this.onChangeNode(n, path)}
        />

        <div
          style={{ display: 'inline-block', marginLeft: 0, marginTop: 5, marginRight: 10, cursor: 'pointer' }}
          onClick={(e) => this.toggleBrowser()}
        >
          <FaSearch fill="currentColor" size={20}></FaSearch>
        </div>
        <div style={{ position: 'relative' }}>{this.renderNodeBrowser(rootNodeId)}</div>
      </div>
    );
  }
}
