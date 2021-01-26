import React, { PureComponent } from 'react';
import { Input } from '@grafana/ui';
import { NodePath, NSNodeId, OpcUaNodeInfo, QualifiedName } from '../types';
import { browsePathToShortString } from '../utils/QualifiedName';
import { nodeIdToShortString } from '../utils/NodeId';
import { FaEdit } from 'react-icons/fa';

export interface Props {
    node: NodePath;
    onNodeChanged(node: OpcUaNodeInfo, path: QualifiedName[]): void;
    getNamespaceIndices(): Promise<string[]>;
    getNodePath(nodeId: string): Promise<NodePath>;
    placeholder: string;
}

type State = {
    node: NodePath;
    edit: boolean;
    nsTable: string[];
    nsTableFetched: boolean,
    nsNodeId: NSNodeId | null,
    editnode: string;
    preEditNode: string;
    propsNode: string;
};

export class NodeTextEditor extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);

        console.log("nodeid to parse: " + this.props.node.node.nodeId);
        let nsNodeId: NSNodeId | null = null;
        if (this.props.node.node.nodeId !== null && this.props.node.node.nodeId.length > 0) {
            try {
                nsNodeId = JSON.parse(this.props.node.node.nodeId);
            } catch (e) {
                console.log(e);
            }
        }

        this.state = {
            edit: false,
            node: this.props.node,
            nsTable: [],
            nsTableFetched: false,
            nsNodeId: nsNodeId,
            editnode: '',
            preEditNode: '',
            propsNode: '',
        };
    }

    onUpdateNode(nodePath: NodePath): void {
        let nsNodeId: NSNodeId | null = null;
        if (nodePath.node.nodeId !== null && nodePath.node.nodeId.length > 0) {
            try {
                nsNodeId = JSON.parse(nodePath.node.nodeId);
            }
            catch (e) {
                console.log(e);
            }
        }
        let nodeId = nodeIdToShortString(nsNodeId, this.state.nsTable);
        this.setState({ node: nodePath, editnode: nodeId, preEditNode: nodeId, nsNodeId: nsNodeId },
            () => this.props.onNodeChanged(this.state.node.node, this.state.node.browsePath));
    }

    onSubmit = () => {
        this.props.getNodePath(this.state.editnode)
            .then((result) => this.onUpdateNode(result))
            .catch(r => this.setState({ editnode: this.state.preEditNode }))
            .finally(() => this.setState({ edit: false }));
    }

    onChange = (e: React.FormEvent<HTMLInputElement>) => {
        this.setState({ editnode: e.currentTarget.value });
    }


    updateNodeState() {
        if (this.state.propsNode !== this.props.node.node.nodeId) {
            this.props.getNodePath(this.props.node.node.nodeId)
                .then((result) => this.onUpdateNode(result))
                .catch(r => this.setState({ editnode: this.state.preEditNode }))
                .finally(() => this.setState({ edit: false, propsNode: this.props.node.node.nodeId}));
        }
    }

    render() {
        if (!this.state.nsTableFetched) {
            this.props.getNamespaceIndices().then(ind => {
                let nodeId = nodeIdToShortString(this.state.nsNodeId, ind);
                this.setState({ nsTable: ind, nsTableFetched: true, editnode: nodeId, preEditNode: nodeId})
            });
        }

        this.updateNodeState();

        return this.state.edit ? (
            <div>
                <Input
                    autoFocus={true}
                    placeholder={this.props.placeholder}
                    value={this.state.editnode}
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
                <div onClick={() => this.setState({ edit: true })} style={{ cursor: 'pointer', display: 'flex', alignItems:'center' }}>
                    <span placeholder={this.props.placeholder} style={{ minWidth: 200, display: 'inline-block' }}>
                        {browsePathToShortString(this.state.node.browsePath)}
                    </span>
                    <FaEdit style={{ marginLeft: 10, marginRight: 10, flexWrap:'nowrap' }} size={20}></FaEdit>
                </div>
            );
    }
}
