import React, { PureComponent } from "react";
import { OpcUaBrowseResults, OpcUaNodeInfo } from '../types';
import { Button } from '@grafana/ui';
import { Browser } from './Browser';
import { NodeTextEditor } from './NodeTextEditor';

type Props = {
    rootNodeId: string,
    browse(nodeId: string): Promise<OpcUaBrowseResults[]>;
    onChangeNode(nodeId: OpcUaNodeInfo): void;
};


type State = {
    node: OpcUaNodeInfo,
    browserOpened: boolean,
}

export class NodeEditor extends PureComponent<Props, State> {

    constructor(props: Props) {
        super(props);
        this.state = {
            browserOpened: false, node: {
                browseName: { name: "", namespaceUrl: "" }, displayName: "", nodeClass: -1, nodeId: ""
            }
        };
    }

    toggleBrowser = () => {
        this.setState({ browserOpened: !this.state.browserOpened });
    }

    renderBrowsePathBrowser = (rootNodeId: OpcUaBrowseResults) => {
        if (this.state.browserOpened) {
            return <div data-id="Treeview-MainDiv" style={{
                border: "lightgrey 1px solid",
                borderRadius: "1px",
                cursor: "pointer",
                padding: "2px",
                position: "absolute",
                left: 30,
                top: 10,
                zIndex: 10,
            }}>
                <Browser closeBrowser={() => this.setState({ browserOpened: false })} closeOnSelect={true}
                    browse={a => this.props.browse(a)}
                    ignoreRootNode={true} rootNodeId={rootNodeId}
                    onNodeSelectedChanged={(node, browsepath) => { this.onChangeNode(node) }}></Browser></div>;
        }
        return <></>;
    }


    onChangeNode(node: OpcUaBrowseResults) {
        this.setState({ node: node }, () => this.props.onChangeNode(node));
    }


    render() {
        let rootNodeId: OpcUaBrowseResults = {
            browseName: { name: "", namespaceUrl: "" }, displayName: "", isForward: true, nodeClass: 0, nodeId: this.props.rootNodeId
        };
        return <div className="gf-form-inline"><NodeTextEditor node={this.state.node} onNodeChanged={this.onChangeNode} />
            <Button onClick={() => this.toggleBrowser()}>Browse</Button>
            <div style={{ position: 'relative' }}>
                {this.renderBrowsePathBrowser(rootNodeId)}
            </div></div>;

    }
}


