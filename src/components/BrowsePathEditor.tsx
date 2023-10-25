import React from 'react';
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

export const BrowsePathEditor: React.FC<Props> = (props: Props) => {
  const toggleBrowsePathBrowser = () => {
    if (!props.isBrowserOpen(props.id)) {
      props.openBrowser(props.id);
    } else {
      props.closeBrowser(props.id);
    }
  };

  const renderBrowsePathBrowser = (rootNodeId: OpcUaBrowseResults) => {
    if (props.isBrowserOpen(props.id)) {
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
            theme={props.theme}
            closeBrowser={() => props.closeBrowser(props.id)}
            closeOnSelect={true}
            browse={(nodeId, filter) => props.browse(nodeId, filter)}
            getNamespaceIndices={() => props.getNamespaceIndices()}
            ignoreRootNode={true}
            rootNodeId={rootNodeId}
            onNodeSelectedChanged={(node, browsepath) => {
              props.onChangeBrowsePath(browsepath);
            } }
          ></BrowserDialog>
        </div>
      );
    }
    return <></>;
  }


  let rootNodeId: OpcUaBrowseResults = {
    browseName: { name: '', namespaceUrl: '' },
    displayName: '',
    isForward: true,
    nodeClass: 0,
    nodeId: props.rootNodeId,
  };

  return (
    <div className="gf-form-inline" style={{ flexWrap: 'nowrap', margin: 2 }}>
      <BrowsePathTextEditor
        getNamespaceIndices={() => props.getNamespaceIndices()}
        browsePath={props.browsePath}
        onBrowsePathChanged={(bp) => props.onChangeBrowsePath(bp)}
      />
      <div
        style={{ display: 'inline-block', marginLeft: 5, marginTop: 5, marginRight: 10, cursor: 'pointer' }}
        onClick={(e) => toggleBrowsePathBrowser()}
      >
        <FaSearch fill="currentColor" size={20}></FaSearch>
      </div>
      <div style={{ position: 'relative' }}>{renderBrowsePathBrowser(rootNodeId)}</div>
    </div>
  );
}
