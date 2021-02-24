//<<<<<<< HEAD
//import React, { PureComponent, ChangeEvent } from 'react';
//import { SegmentAsync, RadioButtonGroup, Input } from '@grafana/ui';
//import { CascaderOption } from 'rc-cascader/lib/Cascader';
//import { QueryEditorProps, SelectableValue } from '@grafana/data';
//import { ButtonCascader } from './components/ButtonCascader/ButtonCascader';
//import { DataSource } from './DataSource';
//import { EventColumn, EventFilter, OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults, separator } from './types';
//import { SegmentFrame, SegmentLabel } from './components/SegmentFrame';
//import { EventField, EventFieldTable } from './components/EventFieldTable';
//import { AddEventFieldForm } from './components/AddEventFieldForm';
//import { EventFilterTable } from './components/EventFilterTable';
//import { AddEventFilter } from './components/AddEventFilter';
//import { MultiSelect } from '@grafana/ui';
//import {} from '@emotion/core';

//const rootNode = 'i=85';
//const eventTypesNode = 'i=3048';
//const selectText = (t: string): string => `Select <${t}>`;
//const defaultTag = 'Select to browse OPC UA Server';

//type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;
//type State = {
//  options: CascaderOption[];
//  value: string[];
//  eventTypeNodeId: string;
//  eventOptions: CascaderOption[];
//  eventTypes: string[];
//  eventFields: EventField[];
//  eventFilters: EventFilter[];

//  tabs: Array<{ label: string; active: boolean }>;
//};

//=======
import React, { PureComponent } from 'react';
import { TabsBar, TabContent, Tab, RadioButtonGroup, SegmentAsync, Input } from '@grafana/ui';
import { TreeEditor } from './components/TreeEditor';
import { GrafanaTheme, QueryEditorProps, SelectableValue } from '@grafana/data';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults } from './types';
import { css } from 'emotion';
import { EventQueryEditor } from './components/EventQueryEditor';
import { NodeQueryEditor } from './components/NodeQueryEditor';
import { SegmentFrame } from './components/SegmentFrame';
import { ThemeGetter } from './components/ThemesGetter';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;
type State = {
    tabs: Array<{ label: string; active: boolean }>;
    maxValuesPerNode: string;
    resampleInterval: string;
    theme: GrafanaTheme | null;
};

const selectText = (t: string): string => `Select <${t}>`;

const tabMarginBox = css(`
{
  border-left: 1px solid #202226;
  border-right: 1px solid #202226;
  border-bottom: 1px solid #202226;
  background: #141414;
}
`);

const tabMarginHeader = css(`
  background: #141414;
`);

export class QueryEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
//<<<<<<< HEAD
//      options: [],
//      value: this.props.query.value || [defaultTag],
//      eventTypes: [],
//      eventOptions: [],
//      eventFields: this.eventFields,
//      eventTypeNodeId: '',
//      eventFilters: [],
//      tabs: this.readTypeOptions.map(o => ({
//        label: o.label || '',
//        active: o.active,
//      })),
//=======
      tabs: this.readTypeOptions.map(o => ({
        label: o.label || '',
        active: o.active,
        })),
      maxValuesPerNode: props.query?.maxValuesPerNode?.toString(),
        resampleInterval: props.query?.resampleInterval?.toString(),
        theme: null,
//>>>>>>> prediktor-opc-ae
    };
  }

//<<<<<<< HEAD
//    props.datasource.getResource('browse', { nodeId: rootNode }).then((results: OpcUaBrowseResults[]) => {
//      console.log('Results', results);
//      this.setState({
//        options: results.map((r: OpcUaBrowseResults) => this.toCascaderOption(r)),
//      });
//    });

//    props.datasource.getResource('browseTypes', { nodeId: eventTypesNode }).then((results: OpcUaBrowseResults[]) => {
//      console.log('Results', results);
//      this.setState({
//        eventOptions: results.map((r: OpcUaBrowseResults) => this.toCascaderOption(r)),
//      });
//    });
//  }

//  get eventFields(): EventField[] {
//    return [
//      { alias: '', browsename: 'Time' },
//      { alias: '', browsename: 'EventId' },
//      { alias: '', browsename: 'EventType' },
//      { alias: '', browsename: 'SourceName' },
//      { alias: '', browsename: 'Message' },
//      { alias: '', browsename: 'Severity' },
//=======
  onSelect = (val: string) => {
    console.log('onSelect', val);
  };


  get readTypeOptions(): Array<SelectableValue<string>> {
    return [
      { label: 'Raw', value: 'ReadDataRaw' },
      { label: 'Processed', value: 'ReadDataProcessed' },
      { label: 'Polling', value: 'ReadNode' },
      { label: 'Subscription', value: 'Subscribe' },
      { label: 'Events', value: 'ReadEvents' },
      { label: 'Subscribe Events', value: 'SubscribeEvents' },
      { label: 'Resource', value: 'Resource' },
//>>>>>>> prediktor-opc-ae
    ];
  }

  readTypeValue = (readType: string): string => {
    const foundVal: SelectableValue<string> | undefined = this.readTypeOptions.find(
      (o: SelectableValue<string>) => o.value === readType
    );
    if (foundVal && foundVal.label) {
      return foundVal.label;
    } else {
      return 'Processed';
    }
    };

    onChangeMaxValuesPerNode(e: React.FormEvent<HTMLInputElement>): void {
        const { onChange, query, onRunQuery } = this.props;
        let max = e.currentTarget.value;
        this.setState({ maxValuesPerNode: max }, () => {
            let maxNumber = parseInt(this.state.maxValuesPerNode);
            if (isNaN(maxNumber)) {
                maxNumber = 0; // Default handling. 
            }
            onChange({ ...query, maxValuesPerNode: maxNumber });
            onRunQuery();
        });
    }

    onChangeResampleInterval(e: React.FormEvent<HTMLInputElement>): void {
        const { onChange, query, onRunQuery } = this.props;
        let resampleInt = e.currentTarget.value;
        this.setState({ resampleInterval: resampleInt }, () => {
            let resampleInterval = parseInt(this.state.resampleInterval);
            if (isNaN(resampleInterval)) {
                resampleInterval = 0; // Default handling. 
            }
            onChange({ ...query, resampleInterval: resampleInterval });
            onRunQuery();
        });
    }



  onChangeField = (field: string, sval: SelectableValue<any> | string, ...args: any[]) => {
    const { /*datasource,*/ query, onChange, onRunQuery } = this.props;
    const {
      /*nodeId, refId*/
    } = query;

    console.log('change', field, sval, args);
    const changes: Record<string, any> = {};

    if (typeof sval === 'string') {
      changes[field] = sval;
    } else {
      changes[field] = sval.value;
    }

//<<<<<<< HEAD
//    if (changes[field] === 'Subscribe') {
//      datasource.getResource('subscribe', { nodeId, refId }).then((results: any[]) => {
//        console.log('We got subscribe results', results);
//        onChange({ ...query, ...changes });
//        onRunQuery();
//      });
//    } else {
//      onChange({ ...query, ...changes });
//      onRunQuery();
//    }
//  };

//  onChange = (selected: string[], selectedItems: CascaderOption[]) => {
//    const { query, onChange, onRunQuery } = this.props;
//    const value = selectedItems.map(item => (item.label ? item.label.toString() : ''));
//    const nodeId = selected[selected.length - 1];
//    console.log('value', value, 'nodeId', nodeId);
//    this.setState({ value });
//    onChange({
//      ...query,
//      value,
//      nodeId,
//    });
//    onRunQuery();
//  };

//  toEventColumns = (r: EventField): EventColumn => {
//    return {
//      browseName: r.browsename,
//      alias: r.alias,
//    };
//  };

//  //deep copy
//  toEventFilter = (r: EventFilter): EventFilter => {
//    return {
//      oper: r.oper,
//      operands: r.operands.slice(),
//    };
//  };

//  onChangeEventType = (selected: string[], selectedItems: CascaderOption[]) => {
//    const evtTypes = selectedItems.map(item => (item.label ? item.label.toString() : ''));
//    const nid = selected[selected.length - 1];
//    this.setState({ eventTypeNodeId: nid, eventTypes: evtTypes }, () => this.updateEventQuery());
//  };

//  onSelect = (val: string) => {
//    console.log('onSelect', val);
//  };

//  onChangeInterval = (event: ChangeEvent<HTMLInputElement>) => {
//    const { onChange, query } = this.props;
//    onChange({ ...query, interval: event.target.value });
//  };

//  toCascaderOption = (opcBrowseResult: OpcUaBrowseResults, children?: CascaderOption[]): CascaderOption => {
//    console.log('browse Result', opcBrowseResult);
//    return {
//      label: opcBrowseResult.displayName,
//      value: opcBrowseResult.nodeId,
//      isLeaf: !opcBrowseResult.isForward || opcBrowseResult.nodeClass === 2, //!opcBrowseResult.isForward,
//    };
//  };

//  getChildren = (selectedOptions: CascaderOption[]) => {
//    const targetOption = selectedOptions[selectedOptions.length - 1];
//    targetOption.loading = true;
//    if (targetOption.value) {
//      this.props.datasource
//        .getResource('browse', { nodeId: targetOption.value })
//        .then((results: OpcUaBrowseResults[]) => {
//          targetOption.loading = false;
//          targetOption.children = results.map(r => this.toCascaderOption(r));
//          this.setState({
//            options: [...this.state.options],
//          });
//        });
//    }
//=======
    //if (changes[field] === 'Subscribe') {
    //  datasource.getResource('subscribe', { nodeId, refId }).then((results: any[]) => {
    //    console.log('We got subscribe results', results);
    //    onChange({ ...query, ...changes });
    //    onRunQuery();
    //  });
    //} else {
    onChange({ ...query, ...changes });
    onRunQuery();
    //}
//>>>>>>> prediktor-opc-ae
  };

  //getEventTypes = (selectedOptions: CascaderOption[]) => {
  //  const targetOption = selectedOptions[selectedOptions.length - 1];
  //  targetOption.loading = true;
  //  if (targetOption.value) {
  //    this.props.datasource
  //      .getResource('browseTypes', { nodeId: targetOption.value })
  //      .then((results: OpcUaBrowseResults[]) => {
  //        targetOption.loading = false;
  //        targetOption.children = results.map(r => this.toCascaderOption(r));
  //        this.setState({
  //          eventOptions: [...this.state.eventOptions],
  //        });
  //      });
  //  }
  //};

  browseNodeSV = (nodeId: string): Promise<Array<SelectableValue<any>>> => {
    return this.props.datasource.getResource('browse', { nodeId }).then((results: OpcUaBrowseResults[]) => {
      return results.map((item: OpcUaBrowseResults) => {
        return {
          label: item.displayName,
          key: item.nodeId,
          description: item.displayName,
          value: {
            name: item.displayName,
            nodeId: item.nodeId,
          },
        };
      });
    });
  };

//<<<<<<< HEAD
//  get readTypeOptions(): Array<SelectableValue<string>> {
//    return [
//      { label: 'Raw', value: 'ReadDataRaw' },
//      { label: 'Processed', value: 'ReadDataProcessed' },
//      { label: 'Realtime', value: 'ReadNode' },
//      { label: 'Subscription', value: 'Subscribe' },
//      { label: 'Events', value: 'ReadEvents' },
//      { label: 'Events2', value: 'ReadEvents2' },
//    ];
//  }

//  readTypeValue = (readType: string): string => {
//    const foundVal: SelectableValue<string> | undefined = this.readTypeOptions.find(
//      (o: SelectableValue<string>) => o.value === readType
//    );
//    if (foundVal && foundVal.label) {
//      return foundVal.label;
//    } else {
//      return 'Processed';
//    }
//  };

//  handleDeleteSelectField = (idx: number) => {
//    let tempArray = this.state.eventFields.slice();
//    tempArray.splice(idx, 1);
//    this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
//  };

//  handleDeleteEventFilter = (idx: number) => {
//    let tempArray = this.state.eventFilters.slice();
//    tempArray.splice(idx, 1);
//    this.setState({ eventFilters: tempArray }, () => this.updateEventQuery());
//  };

//  updateEventQuery = () => {
//    const { query, onChange, onRunQuery } = this.props;

//    let eventColumns = this.state.eventFields.map(c => this.toEventColumns(c));
//    let evtTypes = this.state.eventTypes;
//    let nid = this.state.eventTypeNodeId;
//    let eventFilters = this.state.eventFilters.map(c => this.toEventFilter(c));
//    let eventQuery = {
//      eventTypeNodeId: nid,
//      eventTypes: evtTypes,
//      eventColumns: eventColumns,
//      eventFilters: eventFilters,
//    };
//    onChange({
//      ...query,
//      eventQuery: eventQuery,
//    });
//    onRunQuery();
//  };

//  addSelectField = (browsename: string, alias: string) => {
//    let tempArray = this.state.eventFields.slice();

//    tempArray.push({ browsename: browsename, alias: alias });
//    this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
//  };

//  addEventFilter = (eventFilter: EventFilter) => {
//    let tempArray = this.state.eventFilters.slice();
//    tempArray.push(eventFilter);
//    this.setState({ eventFilters: tempArray }, () => this.updateEventQuery());
//  };

//  renderProcessedQueryOptions = () => {
//    const { query } = this.props;
//    return (
//      <>
//        <SegmentFrame width={7} label={'Aggregate'}>
//          <SegmentAsync
//            value={query.aggregate?.name ?? selectText('aggregate')}
//            loadOptions={() => this.browseNodeSV('i=11201')}
//            onChange={e => this.onChangeField('aggregate', e)}
//          />
//        </SegmentFrame>
//      </>
//    );
//  };

//  renderRawQueryOptions = () => {
//    const { onRunQuery } = this.props;
//    return (
//      <>
//        <SegmentFrame width={7} label="Max Values">
//          <Input
//            width={10}
//            value={-1}
//            onChange={() => console.log('not implemented yet')}
//            onBlur={() => onRunQuery()}
//          />
//        </SegmentFrame>
//      </>
//    );
//  };

//  renderEventQueryOptions = () => {
//    const { query } = this.props;
//    return (
//      <>
//        <SegmentFrame width={7} label="Event Type">
//          <ButtonCascader
//            value={this.state.eventTypes}
//            loadData={this.getEventTypes}
//            options={this.state.eventOptions}
//            onChange={this.onChangeEventType}
//          >
//            {this.state.eventTypes.join(separator)}
//          </ButtonCascader>
//          <SegmentLabel label="Columns" />
//          <div className={'gf-form gf-form--grow'}>
//            <MultiSelect
//              value={this.getEventOptions(query.eventQuery?.eventColumns)}
//              options={this.getEventOptions()}
//              onChange={this.onChangeColumns}
//            />
//          </div>
//        </SegmentFrame>
//      </>
//    );
//  };

//  getEventOptions = (eventColumnsOrFields?: EventField[] | EventColumn[]): Array<SelectableValue<EventField>> => {
//    console.log('fields', eventColumnsOrFields);
//    let fields: EventField[] = this.state.eventFields;
//    if (eventColumnsOrFields && eventColumnsOrFields.length && eventColumnsOrFields[0].hasOwnProperty('displayname')) {
//      fields = eventColumnsOrFields as EventField[];
//    }
//    if (eventColumnsOrFields && eventColumnsOrFields.length && eventColumnsOrFields[0].hasOwnProperty('displayName')) {
//      fields = (eventColumnsOrFields as EventColumn[]).map((c: EventColumn) => ({
//        alias: c.alias,
//        browsename: c.browseName,
//      }));
//=======

  optionalParams = (query: OpcUaQuery, onRunQuery: () => void): JSX.Element => {
    const readTypeValue = this.readTypeValue(query.readType);
      switch (readTypeValue) {
          case 'Processed': {
              return (
                  <>
                      <SegmentFrame label={'Aggregate'}>
                          <SegmentAsync
                              value={query.aggregate?.name ?? selectText('aggregate')}
                              loadOptions={() => this.browseNodeSV('i=11201')}
                              onChange={e => this.onChangeField('aggregate', e)} />
                      </SegmentFrame>
                      <SegmentFrame label={'Resample Interval [s]'}>
                          <Input width={20}
                              value={this.state.resampleInterval}
                              onChange={(e) => this.onChangeResampleInterval(e)} />
                      </SegmentFrame>
                  </>
              );
          }
          case 'Raw': {
              return (
                  <>
                      <SegmentFrame label={'Max Values Per Node'}>
                          <Input width={20}
                              value={this.state.maxValuesPerNode}
                              onChange={(e) => this.onChangeMaxValuesPerNode(e)} />
                      </SegmentFrame>
                  </>
              );
          }

          default: {
              return <></>;
          }
      }
////>>>>>>> prediktor-opc-ae
//    }
//    return fields.map(item => ({
//      label: item.browsename,
//      value: item,
//    }));
  };

//<<<<<<< HEAD
//  onChangeColumns = (items: Array<SelectableValue<EventField>>) => {
//    console.log('onChangeColumns', items);
//    const { eventFields } = this.state;
//    items.forEach(item => {
//      if (
//        item.value &&
//        !eventFields.find(ef => ef.alias === item.value?.alias && ef.browsename === item.value?.browsename)
//      ) {
//        this.addSelectField(item.value.alias, item.value.browsename);
//      }
//    });
//  };

//  renderEvents = (): JSX.Element => {
//    const { query } = this.props;
//    const { options, value } = this.state;
//    return (
//      <>
//        <RadioButtonGroup
//          options={this.readTypeOptions}
//          value={query.readType}
//          onChange={e => e && this.onChangeField('readType', e)}
//        />
//        <br />
//        <SegmentFrame label="Event Source">
//          <div onBlur={e => console.log('onBlur', e)}>
//            <ButtonCascader
//              //className="query-part"
//              value={value}
//              loadData={this.getChildren}
//              options={options}
//              onChange={this.onChange}
//            >
//              {value.join(separator)}
//            </ButtonCascader>
//          </div>
//        </SegmentFrame>
//        <SegmentFrame label="Event Type">
//          <ButtonCascader
//            //className="query-part"
//            value={this.state.eventTypes}
//            loadData={this.getEventTypes}
//            options={this.state.eventOptions}
//            onChange={this.onChangeEventType}
//          >
//            {this.state.eventTypes.join(separator)}
//          </ButtonCascader>
//        </SegmentFrame>
//        <br />
//        <EventFieldTable rows={this.state.eventFields} ondelete={(idx: number) => this.handleDeleteSelectField(idx)} />
//        <br />
//        <AddEventFieldForm add={(browsename: string, alias: string) => this.addSelectField(browsename, alias)} />
//        <br />
//        <EventFilterTable
//          rows={this.state.eventFilters}
//          ondelete={(idx: number) => {
//            this.handleDeleteEventFilter(idx);
//          }}
//        />
//        <br />
//        <AddEventFilter
//          add={(eventFilter: EventFilter) => {
//            this.addEventFilter(eventFilter);
//          }}
//        />
//      </>
//    );
//  };

//  renderQueryOptions = () => {
//    const { query } = this.props;
//    return (
//      <>
//        <SegmentFrame width={7} label="Query Options">
//          <RadioButtonGroup
//            options={this.readTypeOptions}
//            value={query.readType}
//            onChange={readType => {
//              this.onChangeField('readType', readType!);
//            }}
//          />
//        </SegmentFrame>
//        {(() => {
//          switch (query.readType) {
//            case 'ReadDataRaw':
//              return this.renderRawQueryOptions();
//            case 'ReadDataProcessed':
//              return this.renderProcessedQueryOptions();
//            case 'ReadEvents':
//              return this.renderEvents();
//            case 'ReadEvents2':
//              return this.renderEventQueryOptions();
//            default:
//              return <></>;
//          }
//        })()}
//=======
  renderReadTypes = () => {
    const { query, onRunQuery } = this.props;
    return (
        <>
            <h2>Data Retrieval Method</h2>
            <div style={{ marginBottom: 10, marginLeft: 6 }}>
            <RadioButtonGroup
                options={this.readTypeOptions}
                value={query.readType}
                onChange={e => e && this.onChangeField('readType', e)}
                />
            </div>
            {this.optionalParams(query, onRunQuery)}
      </>
    );
  };

//<<<<<<< HEAD
//  render() {
//    const { options, value } = this.state;

//    return (
//      <>
//        <SegmentFrame width={7} label="Tag">
//          <div onBlur={e => console.log('onBlur', e)}>
//            <ButtonCascader
//              //className="query-part"
//              value={value}
//              loadData={this.getChildren}
//              options={options}
//              onChange={this.onChange}
//            >
//              {value.join(separator)}
//            </ButtonCascader>
//          </div>
//        </SegmentFrame>
//        {value[0] === defaultTag ? <></> : this.renderQueryOptions()}
//        <SegmentFrame width={7} label="Alias">
//          <Input value={undefined} placeholder={'alias'} onChange={e => this.onChangeField('alias', e)} width={60} />
//        </SegmentFrame>
//=======
  renderNodeQueryEditor = (nodeNameType: string) => {
    const { datasource, onChange, query, onRunQuery } = this.props;
      return (
          <>
              <h2>UA Node Selection</h2>
              <NodeQueryEditor
                  theme={this.state.theme}
                  nodeNameType={nodeNameType}
                  datasource={datasource}
                  onChange={onChange}
                  onRunQuery={onRunQuery}
                  query={query}></NodeQueryEditor>
          </>
    );
  };

  renderOriginal = () => {
    const { datasource, onChange, query, onRunQuery } = this.props;
    const readTypeValue = this.readTypeValue(query.readType);
    if (readTypeValue === 'Events' || readTypeValue === 'Subscribe Events') {
        return (
            <>
                <div>{this.renderNodeQueryEditor('Event Source')}</div>
                <div>{this.renderReadTypes()}</div>
                <EventQueryEditor datasource={datasource} onChange={onChange} onRunQuery={onRunQuery} query={query} theme={this.state.theme} /> { ' '}
            </>
        );
    }
    else if (readTypeValue === 'Resource')
    {
        return <div>{this.renderReadTypes()}</div>;
    }
    else
    {
        return (
            <>
                <div>{this.renderNodeQueryEditor('Instance')}</div>
                <div>{this.renderReadTypes()}</div>
            
        </>
        );
    }
  };

  renderTreeEditor = () => {
    return <TreeEditor {...this.props} />;
    };

    onTheme = (theme: GrafanaTheme) => {
        if (this.state.theme == null && theme != null) {
            this.setState({ theme: theme });
        }
    };

  render() {
    const { tabs } = this.state;

    return (
        <>
        <ThemeGetter onTheme={this.onTheme} />
        <TabsBar className={tabMarginHeader}>
          {tabs.map((tab, index) => {
            return (
              <Tab
                key={index}
                label={tab.label}
                active={tab.active}
                onChangeTab={() => {
                  this.setState({
                    ...this.state,
                    tabs: tabs.map((tab, idx) => ({ ...tab, active: idx === index })),
                  });
                }}
              />
            );
          })}
        </TabsBar>
        <TabContent className={tabMarginBox}>
          {tabs[0].active && this.renderOriginal()}
          {tabs[1].active && this.renderTreeEditor()}
        </TabContent>
      </>
    );
  }
}
