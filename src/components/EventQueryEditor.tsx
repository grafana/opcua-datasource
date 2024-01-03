import React, { PureComponent } from 'react';
import {
  EventColumn,
  EventFilter,
  QualifiedName,
  OpcUaBrowseResults,
  OpcUaQuery,
  OpcUaDataSourceOptions,
  NodePath,
  BrowseFilter,
  NodeClass,
  OpcUaNodeInfo,
} from '../types';
import { EventFieldTable } from './EventFieldTable';
import { EventFilterTable } from './EventFilterTable';
import { AddEventFilter } from './AddEventFilter';
import { SegmentFrame } from './SegmentFrame';
import { copyEventFilter, createFilterTree, serializeEventFilter, deserializeEventFilters } from '../utils/EventFilter';
import { copyEventColumn } from '../utils/EventColumn';
import { DataSource } from '../DataSource';
import { GrafanaTheme, QueryEditorProps } from '@grafana/data';
import { NodeEditor } from './NodeEditor';
import { renderOverlay } from '../utils/Overlay';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions> & { theme: GrafanaTheme | null };

type State = {
  eventFields: EventColumn[];
  eventFilters: EventFilter[];
  browserOpened: string | null;
  node: NodePath;
};

const eventTypesNode = 'i=3048';

export class EventQueryEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    // Better way of doing initialization from props??

    let eventTypeNodeId = this.props.query?.eventQuery?.eventTypeNodeId;
    let removeFirstEventFilter = true; // First event filter from props is event type node.
    if (typeof eventTypeNodeId === 'undefined') {
      eventTypeNodeId = '';
      removeFirstEventFilter = false;
    }
    let evFilters = this.props.query?.eventQuery?.eventFilters;
    if (typeof evFilters === 'undefined') {
      evFilters = [];
    }

    // Remove event type filter.
    let deserializedEventFilters = deserializeEventFilters(evFilters);
    if (deserializedEventFilters.length > 0 && removeFirstEventFilter) {
      if (deserializedEventFilters.length > 2) {
        deserializedEventFilters.splice(2, 1);
      }
      deserializedEventFilters.splice(0, 1);
    }
    let nodePath: NodePath = {
      browsePath: [],
      node: { browseName: { name: '', namespaceUrl: '' }, displayName: '', nodeClass: -1, nodeId: eventTypeNodeId },
    };
    this.state = {
      eventFields: this.buildEventFields(this.props.query?.eventQuery?.eventColumns),
      eventFilters: deserializedEventFilters,
      browserOpened: null,
      node: nodePath,
    };

    if (eventTypeNodeId !== '') {
      this.getNodePath(eventTypeNodeId, eventTypesNode).then((r) => this.setState({ node: r }));
    }
  }

  browseTypes = (nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]> => {
    let filter = JSON.stringify(browseFilter);
    return this.props.datasource.getResource('browse', {
      nodeId: nodeId,
      nodeClassMask: NodeClass.ObjectType | NodeClass.VariableType,
      browseFilter: filter,
    });
  };

  buildEventFields = (storedEventColumns: EventColumn[]): EventColumn[] => {
    if (typeof storedEventColumns === 'undefined') {
      return [
        {
          alias: 'Active',
          browsePath: [
            { name: 'ActiveState', namespaceUrl: 'http://opcfoundation.org/UA/' },
            { name: 'Id', namespaceUrl: 'http://opcfoundation.org/UA/' },
          ],
        },
        {
          alias: 'Acked',
          browsePath: [
            { name: 'AckedState', namespaceUrl: 'http://opcfoundation.org/UA/' },
            { name: 'Id', namespaceUrl: 'http://opcfoundation.org/UA/' },
          ],
        },
        { alias: '', browsePath: [{ name: 'Time', namespaceUrl: 'http://opcfoundation.org/UA/' }] },
        { alias: '', browsePath: [{ name: 'EventId', namespaceUrl: 'http://opcfoundation.org/UA/' }] },
        { alias: '', browsePath: [{ name: 'EventType', namespaceUrl: 'http://opcfoundation.org/UA/' }] },
        { alias: '', browsePath: [{ name: 'SourceName', namespaceUrl: 'http://opcfoundation.org/UA/' }] },
        { alias: '', browsePath: [{ name: 'Message', namespaceUrl: 'http://opcfoundation.org/UA/' }] },
        { alias: '', browsePath: [{ name: 'Severity', namespaceUrl: 'http://opcfoundation.org/UA/' }] },
      ];
    }
    return storedEventColumns.map((a) => copyEventColumn(a));
  };

  handleDeleteSelectField = (idx: number) => {
    let tempArray = this.state.eventFields.map((a) => copyEventColumn(a));
    tempArray.splice(idx, 1);
    this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
  };

  handleDeleteEventFilter = (idx: number) => {
    let tempArray = this.state.eventFilters.map((a) => copyEventFilter(a));
    tempArray.splice(idx, 1);
    this.setState({ eventFilters: tempArray }, () => this.updateEventQuery());
  };

  updateEventQuery = () => {
    const { query, onChange, onRunQuery } = this.props;

    let eventColumns = this.state.eventFields.map((c) => copyEventColumn(c));
    let nid = this.state.node?.node.nodeId;
    let eventFilters = createFilterTree(nid, this.state.eventFilters).map((x) => serializeEventFilter(x));

    let eventQuery = {
      eventTypeNodeId: nid,
      eventColumns: eventColumns,
      eventFilters: eventFilters,
    };
    onChange({
      ...query,
      eventQuery: eventQuery,
    });
    onRunQuery();
  };

  addEventFilter = (eventFilter: EventFilter) => {
    let tempArray = this.state.eventFilters.slice();
    tempArray.push(eventFilter);
    this.setState({ eventFilters: tempArray }, () => this.updateEventQuery());
  };

  getNamespaceIndices = (): Promise<string[]> => {
    return this.props.datasource.getResource('getNamespaceIndices');
  };

  renderTables() {
    const { datasource } = this.props;
    let validEventTypeNodeId = true;
    if (this.state.node.node.nodeId === '') {
      validEventTypeNodeId = false;
    }
    if (validEventTypeNodeId) {
      return (
        <>
          <h2>Event Columns</h2>
          <EventFieldTable
            theme={this.props.theme}
            getNamespaceIndices={() => this.getNamespaceIndices()}
            datasource={datasource}
            eventTypeNodeId={this.state.node.node.nodeId}
            eventFields={this.state.eventFields}
            onChangeAlias={(alias, idx) => {
              this.onChangeAlias(alias, idx);
            }}
            onChangeBrowsePath={(browsePath, idx) => {
              this.onChangeBrowsePath(browsePath, idx);
            }}
            deleteField={(idx: number) => this.handleDeleteSelectField(idx)}
            addField={(col: EventColumn) => this.onAddColumn(col)}
            moveFieldUp={(idx: number) => this.moveFieldUp(idx)}
            moveFieldDown={(idx: number) => this.moveFieldDown(idx)}
          />
          <br />
          <h2>Event Filters</h2>
          <EventFilterTable
            getNamespaceIndices={() => this.getNamespaceIndices()}
            theme={this.props.theme}
            rows={this.state.eventFilters}
            onDelete={(idx: number) => {
              this.handleDeleteEventFilter(idx);
            }}
          />
          <br />
          <AddEventFilter
            theme={this.props.theme}
            getNamespaceIndices={() => this.getNamespaceIndices()}
            translateBrowsePathToNode={(startNode, bp) => this.translateBrowsePathToNode(startNode, bp)}
            getDataType={(dt) => this.getDataType(dt)}
            add={(eventFilter: EventFilter) => {
              this.addEventFilter(eventFilter);
            }}
            datasource={this.props.datasource}
            eventTypeNodeId={this.state.node.node.nodeId}
          />
        </>
      );
    }
    return <></>;
  }

  moveFieldDown(idx: number): void {
    if (idx < this.state.eventFields.length - 1) {
      let tempArray = this.state.eventFields.slice();
      const tmp = tempArray[idx];
      tempArray[idx] = tempArray[idx + 1];
      tempArray[idx + 1] = tmp;
      this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
    }
  }

  moveFieldUp(idx: number): void {
    if (idx > 0) {
      let tempArray = this.state.eventFields.slice();
      const tmp = tempArray[idx];
      tempArray[idx] = tempArray[idx - 1];
      tempArray[idx - 1] = tmp;
      this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
    }
  }

  onChangeBrowsePath(browsePath: QualifiedName[], idx: number) {
    let tempArray = this.state.eventFields.slice();
    tempArray[idx] = { alias: tempArray[idx].alias, browsePath: browsePath };
    this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
  }

  onChangeAlias(alias: string, idx: number) {
    let tempArray = this.state.eventFields.slice();
    tempArray[idx] = { alias: alias, browsePath: tempArray[idx].browsePath };
    this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
  }

  onAddColumn(col: EventColumn): void {
    let tempArray = this.state.eventFields.slice();
    tempArray.push(col);
    this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
  }

  getNodePath(nodeId: string, rootId: string): Promise<NodePath> {
    return this.props.datasource.getResource('getNodePath', { nodeId: nodeId, rootId: rootId });
  }

  readNode(nodeId: string): Promise<import('../types').OpcUaNodeInfo> {
    return this.props.datasource.getResource('readNode', { nodeId: nodeId });
  }

  translateBrowsePathToNode(startNode: string, browsePath: QualifiedName[]): Promise<OpcUaNodeInfo> {
    return this.props.datasource.postResource('translateBrowsePathToNode', {
      startNode: startNode,
      browsePath: browsePath,
    });
  }

  getDataType(nodeId: string): Promise<NodePath> {
    return this.props.datasource.getResource('getDataType', { nodeId: nodeId });
  }

  render() {
    let bg = '';
    if (this.props.theme !== null) {
      bg = this.props.theme.colors.bg2;
    }
    return (
      <>
        {renderOverlay(
          bg,
          () => this.state.browserOpened !== null,
          () => this.setState({ browserOpened: null })
        )}
        <SegmentFrame label="Event Type">
          <NodeEditor
            id={'eventTypeEditor'}
            closeBrowser={(id: string) => this.setState({ browserOpened: null })}
            isBrowserOpen={(id: string) => this.state.browserOpened === id}
            openBrowser={(id: string) => this.setState({ browserOpened: id })}
            getNamespaceIndices={() => this.getNamespaceIndices()}
            theme={this.props.theme}
            rootNodeId={eventTypesNode}
            placeholder="Event Type"
            node={this.state.node}
            getNodePath={(nodeId, rootId) => this.getNodePath(nodeId, rootId)}
            readNode={(n) => this.readNode(n)}
            browse={(nodeId, filter) => this.browseTypes(nodeId, filter)}
            onChangeNode={(nodePath) => this.setState({ node: nodePath }, () => this.updateEventQuery())}
          ></NodeEditor>
        </SegmentFrame>
        <br />
        {this.renderTables()}
      </>
    );
  }
}
