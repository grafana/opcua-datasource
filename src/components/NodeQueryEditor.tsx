import React, { PureComponent } from 'react';

import { Input } from '@grafana/ui';
import {
  QualifiedName,
  OpcUaQuery,
  OpcUaDataSourceOptions,
  OpcUaBrowseResults,
  NodeClass,
  NodePath,
  BrowseFilter,
} from '../types';
import { QueryEditorProps } from '@grafana/data';
import { DataSource } from '../DataSource';
import { SegmentFrame } from './SegmentFrame';
//import { BrowsePathEditor } from './BrowsePathEditor';
import { BrowsePathEditor } from './BrowsePathEditor';
import { Checkbox } from '@grafana/ui';
import { NodeEditor } from './NodeEditor';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions> & { nodeNameType: string };

type State = {
  useTemplate: boolean;
  //templateType: NodePath;
  node: NodePath;
  alias: string;
  templateVariable: string;
  relativepath: QualifiedName[];
  browserOpened: boolean;
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
        node: { nodeId: 'i=85', browseName: { name: '', namespaceUrl: '' }, displayName: '', nodeClass: -1 },
      };
    }

    this.state = {
      useTemplate: this.props.query.useTemplate,
      templateVariable: tempVar,
      relativepath: this.props.query.relativePath,
      node: nodePath,
      alias: alias,
      browserOpened: false,
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
    return this.props.datasource.getResource('browse', { nodeId: nodeId, browseFilter: filter });
  };

  browseTypes = (nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]> => {
    var filter = JSON.stringify(browseFilter);
    return this.props.datasource.getResource('browse', {
      nodeId: nodeId,
      nodeClassMask: NodeClass.ObjectType | NodeClass.VariableType,
      browseFilter: filter,
    });
  };

  renderTemplateOrNodeBrowser() {
    const { useTemplate } = this.state;
    if (!useTemplate) {
      return (
        <div onBlur={e => console.log('onBlur', e)}>
          <NodeEditor
            rootNodeId="i=85"
            placeholder="Type of template"
            node={this.state.node}
            readNode={n => this.readNode(n)}
            browse={(nodeId, filter) => this.browse(nodeId, filter)}
            onChangeNode={nodepath => this.onChangeNode(nodepath)}
          ></NodeEditor>
        </div>
      );
    } else {
      return (
        <div>
          <NodeEditor
            rootNodeId="i=88"
            placeholder="Type of template"
            node={this.state.node}
            readNode={n => this.readNode(n)}
            browse={(nodeId, filter) => this.browseTypes(nodeId, filter)}
            onChangeNode={nodepath => this.onChangeTemplateType(nodepath)}
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
    const { onChange, query } = this.props;
    this.setState({ node: node }, () => onChange({ ...query, nodePath: node }));
  }

  readNode(nodeId: string): Promise<import('../types').OpcUaNodeInfo> {
    return this.props.datasource.getResource('readNode', { nodeId: nodeId });
  }

  renderTemplateVariable() {
    const { templateVariable } = this.state;
    if (this.state.useTemplate) {
      return (
        <SegmentFrame label="Template variable">
          <Input
            value={templateVariable}
            placeholder="Template variable"
            onChange={e => this.onChangeTemplateVariable(e)}
            width={30}
          />
        </SegmentFrame>
      );
    }
    return <></>;
  }

  render() {
    const { relativepath, alias, node } = this.state;
    let browseNodeId: string = node.node.nodeId;
    let nodeNameType: string = this.props.nodeNameType;
    if (this.state.useTemplate) {
      browseNodeId = this.state.node.node.nodeId;
      nodeNameType = 'Template type';
    }

    return (
      <div style={{ padding: '4px' }}>
        <Checkbox
          label="Use template"
          checked={this.state.useTemplate}
          onChange={e => this.changeUseTemplate(e)}
        ></Checkbox>
        <SegmentFrame label={nodeNameType}>
          {this.renderTemplateOrNodeBrowser()}
          <div>
            <BrowsePathEditor
              browsePath={relativepath}
              browse={(nodeId, filter) => this.browse(nodeId, filter)}
              onChangeBrowsePath={relativePath => this.onChangeRelativePath(relativePath)}
              rootNodeId={browseNodeId}
            />
          </div>
        </SegmentFrame>
        {this.renderTemplateVariable()}
        <SegmentFrame label="Alias">
          <Input value={alias} placeholder={'alias'} onChange={e => this.onChangeAlias(e)} width={30} />
        </SegmentFrame>
      </div>
    );
  }

  onChangeTemplateVariable(e: React.FormEvent<HTMLInputElement>): void {
    const { onChange, query } = this.props;
    let tempVar: string = e.currentTarget.value;
    this.setState(
      {
        templateVariable: tempVar,
      },
      () => onChange({ ...query, templateVariable: tempVar })
    );
  }

  changeUseTemplate(e: React.FormEvent<HTMLInputElement>): void {
    const { onChange, query } = this.props;
    var checked = e.currentTarget.checked;
    this.setState({ useTemplate: checked }, () => onChange({ ...query, useTemplate: checked }));
  }
}
