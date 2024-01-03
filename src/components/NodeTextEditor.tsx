import React, { PureComponent } from 'react';
import { Input } from '@grafana/ui';
import { NodePath, NSNodeId, OpcUaNodeInfo, QualifiedName } from '../types';
import { browsePathToShortString } from '../utils/QualifiedName';
import { areNodesEqual, nodeIdToShortString } from '../utils/NodeId';
import { FaEdit } from 'react-icons/fa';

export interface Props {
  node: NodePath;
  onNodeChanged(node: OpcUaNodeInfo, path: QualifiedName[]): void;
  getNamespaceIndices(): Promise<string[]>;
  getNodePath(nodeId: string): Promise<NodePath>;
  placeholder: string;
}

type State = {
  node: NodePath | null;
  edit: boolean;
  nsTable: string[];
  nsTableFetched: boolean;
  editNode: string;
  preEditNode: string;
};

export class NodeTextEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      edit: false,
      node: null,
      nsTable: [],
      nsTableFetched: false,
      editNode: '',
      preEditNode: '',
    };
  }

  getNodeId(nId: string | null): NSNodeId | null {
    if (nId === null) {
      return null;
    }
    let nsNodeId: NSNodeId | null = null;
    if (nId !== null && nId.length > 0) {
      try {
        nsNodeId = JSON.parse(nId);
      } catch (e) {
        console.log(e);
      }
    }
    return nsNodeId;
  }

  onUpdateNode(nodePath: NodePath): void {
    let nsNodeId = this.getNodeId(nodePath.node.nodeId);
    if (nsNodeId != null) {
      this.setState({ edit: false }, () => this.props.onNodeChanged(nodePath.node, nodePath.browsePath));
    } else {
      this.setState({ edit: false, editNode: this.state.preEditNode });
    }
  }

  onSubmit = () => {
    let currentNodeId = this.getNodeId(this.state.node != null ? this.state.node.node.nodeId : null);
    let s = nodeIdToShortString(currentNodeId, this.state.nsTable);
    if (s !== this.state.editNode) {
      this.props
        .getNodePath(this.state.editNode)
        .then((result) => this.onUpdateNode(result))
        .catch((r) => this.setState({ editNode: this.state.preEditNode, edit: false }));
    } else {
      this.setState({ edit: false });
    }
  };

  onChange = (e: React.FormEvent<HTMLInputElement>) => {
    this.setState({ editNode: e.currentTarget.value });
  };

  render() {
    if (!this.state.nsTableFetched) {
      this.props.getNamespaceIndices().then((ind) => {
        this.setState({ nsTable: ind, nsTableFetched: true });
      });
      return <></>;
    }

    if (this.state.node === null || !areNodesEqual(this.props.node.node, this.state.node.node)) {
      let nodeId = this.getNodeId(this.props.node.node.nodeId);
      let editNode = nodeIdToShortString(nodeId, this.state.nsTable);
      this.setState({ node: this.props.node, editNode, preEditNode: editNode });
    }

    return this.state.edit ? (
      <div>
        <Input
          autoFocus={true}
          placeholder={this.props.placeholder}
          value={this.state.editNode}
          onBlur={() => this.onSubmit()}
          onChange={(e) => this.onChange(e)}
          onKeyPress={(k) => {
            if (k.key === 'Enter') {
              this.onSubmit();
            }
          }}
        ></Input>
      </div>
    ) : (
      <div
        onClick={() => this.setState({ edit: true })}
        style={{ cursor: 'pointer', display: 'flex', alignItems: 'center' }}
      >
        <span placeholder={this.props.placeholder} style={{ minWidth: 200, display: 'inline-block' }}>
          {browsePathToShortString(this.state.node != null ? this.state.node.browsePath : null)}
        </span>
        <FaEdit style={{ marginLeft: 10, marginRight: 10, flexWrap: 'nowrap' }} size={20}></FaEdit>
      </div>
    );
  }
}
