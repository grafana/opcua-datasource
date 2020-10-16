import React, { PureComponent } from "react";
import { Input } from '@grafana/ui';
import { OpcUaNodeInfo } from '../types';


export interface Props {
    node: OpcUaNodeInfo;
    onNodeChanged(node: OpcUaNodeInfo): void;
    readNode(nodeId: string): Promise<OpcUaNodeInfo>;
}

type State = {
    node: OpcUaNodeInfo,
    editnode: string,
    edit: boolean,
    originalNodeId: string
}

export class NodeTextEditor extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = { edit: false, node: this.props.node, editnode: this.props.node.nodeId, originalNodeId: this.props.node.nodeId};
    }

    onSubmit = () => {
        this.setState({ edit: false });
        this.props.readNode(this.state.editnode).then((result) => this.setState({ node: result }));
    }

    onChange = (e: React.FormEvent<HTMLInputElement>) => {
        this.setState({ editnode: e.currentTarget.value });
    }

    render() {
        if (this.state.originalNodeId != this.props.node.nodeId)
            this.setState({ node: this.props.node, editnode: this.props.node.nodeId, originalNodeId: this.props.node.nodeId });

        return (this.state.edit) ?
            (
                <div>
                    <Input value={this.state.editnode} onBlur={() => this.onSubmit()} onChange={(e) => this.onChange(e)}></Input>
                </div>
            ) :
            (
                <div>
                    <Input value={this.props.node.displayName} /*onClick={() => this.setState({ edit: true })}*/ ></Input>
                </div>
            );
        
    }
}