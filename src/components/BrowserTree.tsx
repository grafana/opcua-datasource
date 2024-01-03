import React, { Component } from 'react';
import TreeNode from './TreeNode';
import { convertRemToPixels } from '../utils/ConvertRemToPixels';
import { OpcUaBrowseResults, QualifiedName } from '../types';
import { GrafanaTheme } from '@grafana/data';

type Props = {
  browse: (nodeId: string) => Promise<OpcUaBrowseResults[]>;
  rootNodeId: OpcUaBrowseResults;
  ignoreRootNode: boolean;
  closeOnSelect: boolean;
  theme: GrafanaTheme | null;
  onNodeSelectedChanged: (nodeId: OpcUaBrowseResults, browsePath: QualifiedName[]) => void;
  closeBrowser: () => void;
};

type State = {
  fetchedChildren: boolean;
  rootNode: OpcUaBrowseResults;
  children: OpcUaBrowseResults[];
};

/**
 * Displays nodes in a tree and allows users to select an entity for display.
 */
export class BrowserTree extends Component<Props, State> {
  /**
   *
   * @param {*} props sets the data structure
   */
  constructor(props: Props) {
    super(props);
    this.state = {
      rootNode: {
        browseName: { name: '', namespaceUrl: '' },
        displayName: '',
        isForward: false,
        nodeClass: -1,
        nodeId: '',
      },
      children: [],
      fetchedChildren: false,
    };
  }

  handleClose = () => {
    this.props.closeBrowser();
  };

  handleHoverClose = () => {};

  nodeSelect = (node: OpcUaBrowseResults, browsePath: QualifiedName[]) => {
    this.props.onNodeSelectedChanged(node, browsePath);
    if (this.props.closeOnSelect) {
      this.props.closeBrowser();
    }
  };

  renderNode = (node: OpcUaBrowseResults) => {
    return (
      <TreeNode
        node={node}
        browse={this.props.browse}
        onNodeSelect={this.nodeSelect}
        level={0}
        marginRight={4}
        parentNode={null}
      />
    );
  };

  renderNodes = (rootNodeId: OpcUaBrowseResults, ignoreRoot: boolean) => {
    if (!ignoreRoot) {
      return this.renderNode(rootNodeId);
    } else {
      if (!this.state.fetchedChildren) {
        this.props
          .browse(rootNodeId.nodeId)
          .then((response) => {
            this.setState({ children: response, fetchedChildren: true });
          })
          .catch((c) => console.log(c));
        return <></>;
      } else {
        return this.state.children.map((a) => this.renderNode(a));
      }
    }
  };

  /**
   * Renders the component.
   */
  render() {
    const rootNodeId = this.props.rootNodeId;
    if (this.state.rootNode.nodeId !== rootNodeId.nodeId) {
      this.setState({ children: [], fetchedChildren: false, rootNode: rootNodeId });
    }
    let bg = '';
    if (this.props.theme != null) {
      bg = this.props.theme.colors.bg2;
    }
    return (
      <div
        style={{
          background: bg,
        }}
      >
        <div
          data-id="Treeview-ScrollDiv"
          style={{
            height: convertRemToPixels('20rem'),
            overflowX: 'hidden',
            overflowY: 'auto',
          }}
        >
          {this.renderNodes(rootNodeId, this.props.ignoreRootNode)}
        </div>
      </div>
    );
  }
}
