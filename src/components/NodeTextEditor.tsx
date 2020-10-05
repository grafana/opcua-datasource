import React, { PureComponent } from "react";
import { Input } from '@grafana/ui';
import { OpcUaNodeInfo } from '../types';


export interface Props {
    node: OpcUaNodeInfo;
    onNodeChanged(node: OpcUaNodeInfo): void;
}

type State = {
}

export class NodeTextEditor extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);
    }

    render() {
        return (
            <div data-tip={this.props.node} style={{ width: 500 }}>
                <Input value={this.props.node.displayName}></Input>
            </div>
        );
    }
}