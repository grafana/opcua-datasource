import React, { PureComponent } from 'react';
import { QualifiedName, OpcUaBrowseResults, BrowseFilter } from '../types';
import { Button } from '@grafana/ui';
import { BrowsePathTextEditor } from './BrowsePathTextEditor';
import { BrowserDialog } from './BrowserDialog';
import { GrafanaTheme } from '@grafana/data';

type Props = {
  browsePath: QualifiedName[];
  rootNodeId: string;
  theme: GrafanaTheme | null;
  browse(nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]>;
  openBrowser(id: string): void;
  closeBrowser(id: string): void;
  isBrowserOpen(id: string): boolean;
  onChangeBrowsePath(browsePath: QualifiedName[]): void;
  getNamespaceIndices(): Promise<string[]>;
  id: string;
};

type State = {
 
};

export class BrowsePathEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { };
 }

  toggleBrowsePathBrowser = () => {
    if (!this.props.isBrowserOpen(this.props.id))
        this.props.openBrowser(this.props.id);
    else
        this.props.closeBrowser(this.props.id);
  };

  renderBrowsePathBrowser = (rootNodeId: OpcUaBrowseResults) => {
    if (this.props.isBrowserOpen(this.props.id)) {
      return (
        <div
          data-id="Treeview-MainDiv"
          style={{
            border: "lightgrey 1px solid",
            borderRadius: "1px",
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
              closeBrowser={() => this.props.closeBrowser(this.props.id)}
            closeOnSelect={true}
                  browse={(nodeId, filter) => this.props.browse(nodeId, filter)}
                  getNamespaceIndices={() => this.props.getNamespaceIndices()}
            ignoreRootNode={true}
            rootNodeId={rootNodeId}
            onNodeSelectedChanged={(node, browsepath) => {
              this.props.onChangeBrowsePath(browsepath);
            }}
          ></BrowserDialog>
        </div>
      );
    }
    return <></>;
  };

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
        <BrowsePathTextEditor getNamespaceIndices={() => this.props.getNamespaceIndices()} browsePath={this.props.browsePath} onBrowsePathChanged={(bp) => this.props.onChangeBrowsePath(bp)} />
        <Button onClick={() => this.toggleBrowsePathBrowser()}>Browse</Button>
        <div style={{ position: 'relative' }}>{this.renderBrowsePathBrowser(rootNodeId)}</div>
      </div>
    );
  }
}
