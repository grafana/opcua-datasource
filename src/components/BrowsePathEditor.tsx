import React, { PureComponent } from "react";
import { QualifiedName, OpcUaBrowseResults } from '../types';
import { Button } from '@grafana/ui';
import { BrowsePathTextEditor } from './BrowsePathTextEditor';
import { BrowserDialog } from './BrowserDialog';

type Props = {
    browsePath: QualifiedName[],
    rootNodeId: string,
    browse(nodeId: string): Promise<OpcUaBrowseResults[]>;
    onChangeBrowsePath(browsePath: QualifiedName[]): void;
};


type State = {
    browserOpened: boolean,
}

export class BrowsePathEditor extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = { browserOpened: false };
    }
    toggleBrowsePathBrowser = () => {
        this.setState({ browserOpened: !this.state.browserOpened });
    }

    renderBrowsePathBrowser = (rootNodeId: OpcUaBrowseResults) => {
        if (this.state.browserOpened) {
            return <div data-id="Treeview-MainDiv" style={{
                //border: "lightgrey 1px solid",
                //borderRadius: "1px",
                cursor: "pointer",
                padding: "2px",
                position: "absolute",
                left: 30,
                top: 10,
                zIndex: 10,
            }}>
                <BrowserDialog closeBrowser={() => this.setState({ browserOpened: false })} closeOnSelect={true}
                    browse={a => this.props.browse(a)}
                    ignoreRootNode={true} rootNodeId={rootNodeId}
                    onNodeSelectedChanged={(node, browsepath) => { this.props.onChangeBrowsePath(browsepath) }}></BrowserDialog></div>;
        }
        return <></>;
    }


    render() {
        let rootNodeId: OpcUaBrowseResults = {
            browseName: { name: "", namespaceUrl: "" }, displayName: "", isForward: true, nodeClass: 0, nodeId: this.props.rootNodeId
        };
        return <div className="gf-form-inline"><BrowsePathTextEditor browsePath={this.props.browsePath} onBrowsePathChanged={this.props.onChangeBrowsePath} />
            <Button onClick={() => this.toggleBrowsePathBrowser()}>Browse</Button>
            <div style={{ position: 'relative' }}>
                {this.renderBrowsePathBrowser(rootNodeId)}
            </div></div>;

    }
}


