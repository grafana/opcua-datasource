import React, { PureComponent } from 'react';
import { SegmentFrame } from './SegmentFrame';
import {
  FilterOperandEnum,
  FilterOperand,
  FilterOperator,
  EventFilter,
  LiteralOp,
  SimpleAttributeOp,
  QualifiedName,
  OpcUaBrowseResults,
  OpcUaNodeInfo,
  NodeClass,
  NodePath,
  BrowseFilter,
} from '../types';
import { copyQualifiedName } from '../utils/QualifiedName';
import { BrowsePathEditor } from './BrowsePathEditor';
import { DataSource } from '../DataSource';
import { NodeEditor } from './NodeEditor';
import { Button } from '@grafana/ui';
import { GrafanaTheme } from '@grafana/data';
import { renderOverlay } from '../utils/Overlay';
import { EventFilterOperatorUtil } from '../utils/Operands';

export interface Props {
  datasource: DataSource;
  eventTypeNodeId: string;
  theme: GrafanaTheme | null;
  add(filter: EventFilter): void;
  getNamespaceIndices(): Promise<string[]>;
  translateBrowsePathToNode(rootId: string, browsePath: QualifiedName[]): Promise<OpcUaNodeInfo>;
  getDataType(nodeId: string): Promise<NodePath>;
}

type State = {
  operator: FilterOperator;
  browsePath: QualifiedName[];
  value: string;
  typeId: NodePath;
  browserOpened: string | null;
};

export class AddEventFilter extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      operator: FilterOperator.GreaterThan,
      browsePath: [],
      typeId: {
        browsePath: [],
        node: {
          browseName: { name: '', namespaceUrl: '' },
          displayName: '',
          nodeClass: -1,
          nodeId: '',
        },
      },
      value: '500',
      browserOpened: null,
    };
  }

  addFilter() {
    if (this.state.browsePath.length > 0 && this.state.typeId.node.nodeId.trim() !== '') {
      let attr: SimpleAttributeOp = {
        attributeId: 13,
        typeId: '',
        browsePath: this.state.browsePath.map((bp) => copyQualifiedName(bp)),
      };
      let literal: LiteralOp = {
        typeId: this.state.typeId.node.nodeId,
        value: this.state.value,
      };
      let operands: FilterOperand[] = [
        { type: FilterOperandEnum.SimpleAttribute, value: attr },
        { type: FilterOperandEnum.Literal, value: literal },
      ];
      let evFilter: EventFilter = { operator: this.state.operator, operands: operands.slice() };
      this.props.add(evFilter);
    }
  }

  changeOperator(event: { target: any }) {
    const target = event.target;
    const value = parseInt(target.value, 10) as FilterOperator;
    this.setState({ operator: value });
  }

  changeValueType(event: { target: any }) {
    const target = event.target;
    const value = target.value;

    this.setState({
      typeId: value,
    });
  }

  changeValue(event: { target: any }) {
    const target = event.target;
    const value = target.value;

    this.setState({
      value: value,
    });
  }

  renderDropdown() {
    return (
      <SegmentFrame label="Operator" marginLeft>
        <select onChange={(e) => this.changeOperator(e)} defaultValue={this.state.operator}>
          {EventFilterOperatorUtil.operatorNames.map((n, idx) => {
            return (
              <option key={`EventFilterOption-${idx}`} value={idx}>
                {n}
              </option>
            );
          })}
        </select>
      </SegmentFrame>
    );
  }

  browsePathChanged(bp: QualifiedName[]) {
    this.setState({ browsePath: bp }, () => this.onBrowsePathChanged());
  }

  onBrowsePathChanged() {
    this.props
      .translateBrowsePathToNode(this.props.eventTypeNodeId, this.state.browsePath)
      .then((node) => this.props.getDataType(node.nodeId))
      .then((dt) => this.setState({ typeId: dt }))
      .catch((err) => console.log(err));
  }

  renderOperandsBeforeOperator(operator: FilterOperator) {
    switch (operator) {
      case FilterOperator.GreaterThan:
      case FilterOperator.GreaterThanOrEqual:
      case FilterOperator.LessThan:
      case FilterOperator.LessThanOrEqual:
      case FilterOperator.Equals:
        return (
          <SegmentFrame label="Event Field" marginLeft>
            <BrowsePathEditor
              getNamespaceIndices={() => this.props.getNamespaceIndices()}
              theme={this.props.theme}
              id={'browser'}
              closeBrowser={(id: string) => this.setState({ browserOpened: null })}
              isBrowserOpen={(id: string) => this.state.browserOpened === id}
              openBrowser={(id: string) => this.setState({ browserOpened: id })}
              browsePath={this.state.browsePath}
              rootNodeId={this.props.eventTypeNodeId}
              onChangeBrowsePath={(bp) => this.browsePathChanged(bp)}
              browse={(nodeId) => this.browseEventFields(nodeId)}
            >
              {' '}
            </BrowsePathEditor>
          </SegmentFrame>
        );
    }
    return <></>;
  }

  renderOperandsAfterOperator(operator: FilterOperator) {
    switch (operator) {
      case FilterOperator.GreaterThan:
      case FilterOperator.GreaterThanOrEqual:
      case FilterOperator.LessThan:
      case FilterOperator.LessThanOrEqual:
      case FilterOperator.Equals:
        return (
          <>
            <SegmentFrame label="Value Type" marginLeft>
              <NodeEditor
                id={'nodeEditor'}
                closeBrowser={(id: string) => this.setState({ browserOpened: null })}
                isBrowserOpen={(id: string) => this.state.browserOpened === id}
                openBrowser={(id: string) => this.setState({ browserOpened: id })}
                getNamespaceIndices={() => this.props.getNamespaceIndices()}
                theme={this.props.theme}
                browse={(node, filter) => this.browseDataTypes(node, filter)}
                getNodePath={(nodeId, rootId) => this.getNodePath(nodeId, rootId)}
                readNode={(nodeId) => this.readNode(nodeId)}
                onChangeNode={(node) => this.onChangeValueTypeNode(node)}
                rootNodeId="i=24"
                placeholder="Value type node"
                node={this.state.typeId}
              />
            </SegmentFrame>
            <SegmentFrame label="Value" marginLeft>
              <input name="value" type="input" value={this.state.value} onChange={(ev) => this.changeValue(ev)} />
            </SegmentFrame>
          </>
        );
    }
    return <></>;
  }

  onChangeValueTypeNode(node: NodePath): void {
    this.setState({ typeId: node });
  }

  browseDataTypes(nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]> {
    return this.props.datasource.getResource('browse', { nodeId: nodeId, nodeClassMask: NodeClass.DataType });
  }

  readNode(nodeId: string): Promise<OpcUaNodeInfo> {
    return this.props.datasource.getResource('readNode', { nodeId: nodeId });
  }

  getNodePath(nodeId: string, rootId: string): Promise<NodePath> {
    return this.props.datasource.getResource('getNodePath', { nodeId: nodeId, rootId: rootId });
  }

  browseEventFields(nodeId: string): Promise<OpcUaBrowseResults[]> {
    return this.props.datasource.getResource('browse', { nodeId: nodeId });
  }

  render() {
    let bg = '';
    if (this.props.theme !== null) {
      bg = this.props.theme.colors.bg2;
    }
    return (
      <div>
        {renderOverlay(
          bg,
          () => this.state.browserOpened !== null,
          () => this.setState({ browserOpened: null })
        )}
        <br />
        {this.renderOperandsBeforeOperator(this.state.operator)}
        {this.renderDropdown()}
        {this.renderOperandsAfterOperator(this.state.operator)}
        <Button onClick={() => this.addFilter()}>Add Filter</Button>
      </div>
    );
  }
}
