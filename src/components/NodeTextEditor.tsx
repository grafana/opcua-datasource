import React, { PureComponent } from 'react';
import { Input } from '@grafana/ui';
import { OpcUaNodeInfo, NodePath } from '../types';
import { browsePathToShortString } from '../utils/QualifiedName';

export interface Props {
  node: NodePath;
  //onNodeChanged(node: OpcUaNodeInfo): void;
  readNode(nodeId: string): Promise<OpcUaNodeInfo>;
  placeholder: string;
}

type State = {
  node: NodePath;
  editnode: string;
  edit: boolean;
  originalNodeId: string;
};

export class NodeTextEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      edit: false,
      node: this.props.node,
      editnode: this.props.node.node.nodeId,
      originalNodeId: this.props.node.node.nodeId,
    };
  }

  //onSubmit = () => {
  //    this.setState({ edit: false });
  //    this.props.readNode(this.state.editnode).then((result) => this.setState({ node: result }));
  //}

  //onChange = (e: React.FormEvent<HTMLInputElement>) => {
  //    this.setState({ editnode: e.currentTarget.value });
  //}

  render() {
    //if (this.state.originalNodeId != this.props.node.nodeId)
    //    this.setState({ node: this.props.node, editnode: this.props.node.nodeId, originalNodeId: this.props.node.nodeId });

    return this.state.edit ? (
      <div>
        <Input
          placeholder={this.props.placeholder}
          value={this.state.editnode} /*onBlur={() => this.onSubmit()} onChange={(e) => this.onChange(e)} */
        ></Input>
      </div>
    ) : (
      <div>
        <Input
          placeholder={this.props.placeholder}
          value={browsePathToShortString(this.props.node.browsePath)} /*onClick={() => this.setState({ edit: true })}*/
        ></Input>
      </div>
    );
  }
}
