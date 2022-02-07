import React, { PureComponent } from 'react';

import { Input, Checkbox } from '@grafana/ui';
import {
  QualifiedName,
  OpcUaQuery,
  OpcUaDataSourceOptions,
  OpcUaBrowseResults,
  NodeClass,
  NodePath,
  BrowseFilter,
} from '../types';
import { GrafanaTheme, QueryEditorProps } from '@grafana/data';
import { DataSource } from '../DataSource';
import { SegmentFrame } from './SegmentFrame';
//import { BrowsePathEditor } from './BrowsePathEditor';
import { BrowsePathEditor } from './BrowsePathEditor';
import { NodeEditor } from './NodeEditor';
import { renderOverlay } from '../utils/Overlay';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions> & {
  nodeNameType: string;
  theme: GrafanaTheme | null;
};

type State = {
  useTemplate: boolean;
  //templateType: NodePath;
  node: NodePath;

  alias: string;
  templateVariable: string;
  relativepath: QualifiedName[];
  browserOpened: string | null;
};

export class NodeQueryEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);

    let alias = this.props?.query?.alias;
    if (typeof alias === 'undefined') {
      alias = '';
    }

    let tempVar = this.props?.query?.templateVariable;
    if (typeof tempVar === 'undefined' || tempVar.length === 0) {
      tempVar = '';
    }

    let nodePath = this.props?.query?.nodePath;
    if (typeof nodePath === 'undefined') {
      nodePath = {
        browsePath: [],
        node: { nodeId: '', browseName: { name: '', namespaceUrl: '' }, displayName: '', nodeClass: -1 },
      };
    }

    this.state = {
      useTemplate: this.props.query.useTemplate,
      templateVariable: tempVar,
      relativepath: this.props.query.relativePath,
      node: nodePath,
      alias: alias,
      browserOpened: null,
    };
  }

  onChangeRelativePath = (relativePath: QualifiedName[]) => {
    const { query, onChange, onRunQuery } = this.props;
    this.setState({ relativepath: relativePath }, () => {
      onChange({
        ...query,
        relativePath,
      });
      onRunQuery();
    });
  };

  onChangeAlias = (event: React.FormEvent<HTMLInputElement>) => {
    const { query, onChange, onRunQuery } = this.props;
    var s = event?.currentTarget.value;
    this.setState({ alias: s }, () => {
      let alias = s;
      onChange({
        ...query,
        alias,
      });
      onRunQuery();
    });
  };

  browse = (nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]> => {
    var filter = JSON.stringify(browseFilter);
      let res: Promise<OpcUaBrowseResults[]> = this.props.datasource.getResource('browse', { nodeId: nodeId, browseFilter: filter });
      return res.then((children) => this.removeDuplicates(children));
  };

  removeDuplicates(brRes: OpcUaBrowseResults[]): OpcUaBrowseResults[] {
    var encounteredSet = new Set();
    const uniqueBrs = brRes.filter((val) => {
      if (encounteredSet.has(val.nodeId))
        return false;

      encounteredSet.add(val.nodeId);
        return true;
    });
    return uniqueBrs;
  }

  browseTypes = (nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]> => {
    var filter = JSON.stringify(browseFilter);
      let res: Promise<OpcUaBrowseResults[]> = this.props.datasource.getResource('browse', {
        nodeId: nodeId,
        nodeClassMask: NodeClass.ObjectType | NodeClass.VariableType,
        browseFilter: filter,
      });
      return res.then((children) => this.removeDuplicates(children));
  };

  getNodePath(nodeId: string, rootId: string): Promise<NodePath> {
    return this.props.datasource.getResource('getNodePath', { nodeId: nodeId, rootId: rootId });
  }

  renderTemplateOrNodeBrowser() {
    const { useTemplate } = this.state;
    if (!useTemplate) {
      return (
        <div onBlur={(e) => console.log('onBlur', e)}>
          <NodeEditor
            id={'instanceeditor'}
            closeBrowser={(id: string) => this.setState({ browserOpened: null })}
            isBrowserOpen={(id: string) => this.state.browserOpened === id}
            openBrowser={(id: string) => this.setState({ browserOpened: id })}
            getNamespaceIndices={() => this.getNamespaceIndices()}
            theme={this.props.theme}
            rootNodeId="i=85"
            placeholder="Instance"
            node={this.state.node}
            getNodePath={(nodeId, rootId) => this.getNodePath(nodeId, rootId)}
            readNode={(n) => this.readNode(n)}
            browse={(nodeId, filter) => this.browse(nodeId, filter)}
            onChangeNode={(nodepath) => this.onChangeNode(nodepath)}
          ></NodeEditor>
        </div>
      );
    } else {
      return (
        <div>
          <NodeEditor
            id={'typeeditor'}
            closeBrowser={(id: string) => this.setState({ browserOpened: null })}
            isBrowserOpen={(id: string) => this.state.browserOpened === id}
            openBrowser={(id: string) => this.setState({ browserOpened: id })}
            getNamespaceIndices={() => this.getNamespaceIndices()}
            theme={this.props.theme}
            rootNodeId="i=88"
            placeholder="Type"
            node={this.state.node}
            getNodePath={(nodeId, rootId) => this.getNodePath(nodeId, rootId)}
            readNode={(n) => this.readNode(n)}
            browse={(nodeId, filter) => this.browseTypes(nodeId, filter)}
            onChangeNode={(nodepath) => this.onChangeTemplateType(nodepath)}
          ></NodeEditor>
        </div>
      );
    }
  }
  onChangeNode(nodepath: NodePath): void {
    const { onChange, query, onRunQuery } = this.props;
    this.setState({ node: nodepath }, () => {
      onChange({ ...query, nodePath: nodepath });
      onRunQuery();
    });
  }

  onChangeTemplateType(node: NodePath): void {
    const { onChange, query, onRunQuery } = this.props;
    this.setState({ node: node }, () => {
      onChange({ ...query, nodePath: node });
      onRunQuery();
    });
  }

  readNode(nodeId: string): Promise<import('../types').OpcUaNodeInfo> {
    return this.props.datasource.getResource('readNode', { nodeId: nodeId });
  }

  getNamespaceIndices = (): Promise<string[]> => {
    return this.props.datasource.getResource('getNamespaceIndices');
  };

  renderTemplateVariable() {
    const { templateVariable } = this.state;
    if (this.state.useTemplate) {
      return (
        <SegmentFrame label="Template variable">
          <Input
            css=""
            value={templateVariable}
            placeholder="Template variable"
            onChange={(e) => this.onChangeTemplateVariable(e)}
            width={30}
          />
        </SegmentFrame>
      );
    }
    return <></>;
  }

  renderAlias() {
    return (
      <SegmentFrame label="Alias">
        <Input
          css=""
          value={this.state.alias}
          placeholder={'alias'}
          onChange={(e) => this.onChangeAlias(e)}
          width={30}
        />
      </SegmentFrame>
    );
  }

  renderBrowsePathEditor() {
    const { relativepath, node } = this.state;
    let browseNodeId: string = node.node.nodeId;

    if (this.state.useTemplate) {
      return (
        <SegmentFrame label={'Relative Path'}>
          <BrowsePathEditor
            theme={this.props.theme}
            id={'browsePath'}
            closeBrowser={(id: string) => this.setState({ browserOpened: null })}
            isBrowserOpen={(id: string) => this.state.browserOpened === id}
            openBrowser={(id: string) => this.setState({ browserOpened: id })}
            getNamespaceIndices={() => this.getNamespaceIndices()}
            browsePath={relativepath}
            browse={(nodeId, filter) => this.browse(nodeId, filter)}
            onChangeBrowsePath={(relativePath) => this.onChangeRelativePath(relativePath)}
            rootNodeId={browseNodeId}
          />
        </SegmentFrame>
      );
    }
    return <></>;
  }

  render() {
    let nodeNameType: string = this.props.nodeNameType;
    if (this.state.useTemplate) {
      nodeNameType = 'Type';
    }
    let bg = '';
    if (this.props.theme !== null) {
      bg = this.props.theme.colors.bg2;
    }
    return (
      <div style={{ margin: '4px' }}>
        {renderOverlay(
          bg,
          () => this.state.browserOpened !== null,
          () => this.setState({ browserOpened: null })
        )}
        <span style={{ marginRight: 10 }}>
          <Checkbox
            css=""
            label="Instance"
            checked={!this.state.useTemplate}
            onChange={(e) => this.changeUseTemplate(!e.currentTarget.checked)}
          ></Checkbox>
        </span>
        <span style={{ marginRight: 10 }}>
          <Checkbox
            css=""
            label="Type"
            checked={this.state.useTemplate}
            onChange={(e) => this.changeUseTemplate(e.currentTarget.checked)}
          ></Checkbox>
        </span>
        <SegmentFrame label={nodeNameType}>{this.renderTemplateOrNodeBrowser()}</SegmentFrame>
        {this.renderBrowsePathEditor()}
        {this.renderAlias()}
        {this.renderTemplateVariable()}
      </div>
    );
  }

  onChangeTemplateVariable(e: React.FormEvent<HTMLInputElement>): void {
    const { onChange, query, onRunQuery } = this.props;
    let tempVar: string = e.currentTarget.value;
    this.setState(
      {
        templateVariable: tempVar,
      },
      () => {
        onChange({ ...query, templateVariable: tempVar });
        onRunQuery();
      }
    );
  }

  changeUseTemplate(checked: boolean): void {
    const { onChange, query, onRunQuery } = this.props;
    this.setState({ useTemplate: checked }, () => {
      onChange({ ...query, useTemplate: checked });
      onRunQuery();
    });
  }
}
