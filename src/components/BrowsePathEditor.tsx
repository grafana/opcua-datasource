import React, { PureComponent } from 'react';
import { QualifiedName, OpcUaBrowseResults, BrowseFilter } from '../types';
import { BrowsePathTextEditor } from './BrowsePathTextEditor';
import { BrowserDialog } from './BrowserDialog';
import { GrafanaTheme } from '@grafana/data';
import { FaSearch } from 'react-icons/fa';

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

type State = {};

export class BrowsePathEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {};
  }

  toggleBrowsePathBrowser = () => {
    if (!this.props.isBrowserOpen(this.props.id)) {
      this.props.openBrowser(this.props.id);
    } else {
      this.props.closeBrowser(this.props.id);
    }
  };

  renderBrowsePathBrowser = (rootNodeId: OpcUaBrowseResults) => {
    if (this.props.isBrowserOpen(this.props.id)) {
      return (
        <div
          data-id="Treeview-MainDiv"
          style={{
            border: 'lightgrey 1px solid',
            borderRadius: '1px',
            cursor: 'pointer',
            padding: '2px',
            position: 'fixed',
            left: 30,
            bottom: 10,
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
            onNodeSelectedChanged={(node, browsePath) => {
              this.props.onChangeBrowsePath(browsePath);
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
      <div className="gf-form-inline" style={{ flexWrap: 'nowrap', margin: 2 }}>
        <BrowsePathTextEditor
          getNamespaceIndices={() => this.props.getNamespaceIndices()}
          browsePath={this.props.browsePath}
          onBrowsePathChanged={(bp) => this.props.onChangeBrowsePath(bp)}
        />
        <div
          style={{ display: 'inline-block', marginLeft: 5, marginTop: 5, marginRight: 10, cursor: 'pointer' }}
          onClick={(e) => this.toggleBrowsePathBrowser()}
        >
          <FaSearch fill="currentColor" size={20}></FaSearch>
        </div>
        <div style={{ position: 'relative' }}>{this.renderBrowsePathBrowser(rootNodeId)}</div>
      </div>
    );
  }
}
