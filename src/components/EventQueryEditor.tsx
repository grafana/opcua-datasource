import React, { PureComponent } from 'react';
import { CascaderOption } from 'rc-cascader/lib/Cascader';
import { EventColumn, EventFilter, QualifiedName, OpcUaBrowseResults, OpcUaQuery, OpcUaDataSourceOptions, separator} from '../types';
import { EventFieldTable } from './EventFieldTable';
import { EventFilterTable } from './EventFilterTable';
import { AddEventFilter } from './AddEventFilter';
import { SegmentFrame } from './SegmentFrame';
import { ButtonCascader } from './ButtonCascader/ButtonCascader';
import { copyEventFilter, createFilterTree, serializeEventFilter, deserializeEventFilters } from '../utils/EventFilter';
import { copyEventColumn } from '../utils/EventColumn';
import { toCascaderOption } from '../utils/CascaderOption';
import { DataSource } from '../DataSource';
import { QueryEditorProps } from '@grafana/data';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;

type State = {
   
    eventTypeNodeId: string;
    eventOptions: CascaderOption[];
    eventTypes: string[];
    eventFields: EventColumn[];
    eventFilters: EventFilter[];

};

const eventTypesNode = "i=3048";

export class EventQueryEditor extends PureComponent<Props, State> {

    constructor(props: Props) {
        super(props);
        // Better way of doing initialization from props??
        let evtype = this.props?.query?.eventQuery?.eventTypes;
        if (typeof evtype === 'undefined')
            evtype = [];

        let eventTypeNodeId = this.props.query?.eventQuery?.eventTypeNodeId;
        let removeFirstEventFilter = true; // First event filter from props is event type node.
        if (typeof eventTypeNodeId === 'undefined') {
            eventTypeNodeId = "";
            removeFirstEventFilter = false;
        }
        let evFilters = this.props.query?.eventQuery?.eventFilters;
        if (typeof evFilters === 'undefined') {
            evFilters = [];
        }
        if (evFilters.length > 0 && removeFirstEventFilter)
            evFilters = evFilters.slice(1, evFilters.length);

        this.state = {
            eventTypes: evtype,
            eventOptions: [],
            eventFields: this.buildEventFields(this.props.query?.eventQuery?.eventColumns),
            eventTypeNodeId: eventTypeNodeId,
            eventFilters: deserializeEventFilters(evFilters),
        };



        props.datasource.getResource('browseTypes', { nodeId: eventTypesNode }).then((results: OpcUaBrowseResults[]) => {
            console.log('Results', results);
            this.setState({
                eventOptions: results.map((r: OpcUaBrowseResults) => toCascaderOption(r)),
            });
        });
    }

    onChangeEventType = (selected: string[], selectedItems: CascaderOption[]) => {
        const evtTypes = selectedItems.map(item => (item.label ? item.label.toString() : ''));
        const nid = selected[selected.length - 1];
        this.setState({ eventTypeNodeId: nid, eventTypes: evtTypes }, () => this.updateEventQuery());
    };


    buildEventFields = (storedEventColumns: EventColumn[]): EventColumn[] => {
        if (typeof storedEventColumns === 'undefined') {
            return [
                { alias: "", browsePath: [{ name: "Time", namespaceUrl: "http://opcfoundation.org/UA/" }] },
                { alias: "", browsePath: [{ name: "EventId", namespaceUrl: "http://opcfoundation.org/UA/" }] },
                { alias: "", browsePath: [{ name: "EventType", namespaceUrl: "http://opcfoundation.org/UA/" }] },
                { alias: "", browsePath: [{ name: "SourceName", namespaceUrl: "http://opcfoundation.org/UA/" }] },
                { alias: "", browsePath: [{ name: "Message", namespaceUrl: "http://opcfoundation.org/UA/" }] },
                { alias: "", browsePath: [{ name: "Severity", namespaceUrl: "http://opcfoundation.org/UA/" }] }
            ];
        }
        return storedEventColumns.map(a => copyEventColumn(a));
    }


    getEventTypes = (selectedOptions: CascaderOption[]) => {
        const targetOption = selectedOptions[selectedOptions.length - 1];
        targetOption.loading = true;
        if (targetOption.value) {
            this.props.datasource
                .getResource('browseTypes', { nodeId: targetOption.value })
                .then((results: OpcUaBrowseResults[]) => {
                    targetOption.loading = false;
                    targetOption.children = results.map(r => toCascaderOption(r));
                    this.setState({
                        eventOptions: [...this.state.eventOptions],
                    });
                });
        }
    };

    handleDeleteSelectField = (idx: number) => {
        let tempArray = this.state.eventFields.map(a => copyEventColumn(a));
        tempArray.splice(idx, 1);
        this.setState({ eventFields: tempArray }, () => this.updateEventQuery());
    }

    handleDeleteEventFilter = (idx: number) => {
        let tempArray = this.state.eventFilters.map(a => copyEventFilter(a));
        tempArray.splice(idx, 1);
        this.setState({ eventFilters: tempArray }, () => this.updateEventQuery());
    }


    updateEventQuery = () => {
        const { query, onChange, onRunQuery } = this.props;

        let eventColumns = this.state.eventFields.map(c => copyEventColumn(c));
        let evtTypes = this.state.eventTypes;
        let nid = this.state.eventTypeNodeId;
        let eventFilters = createFilterTree(this.state.eventTypeNodeId, this.state.eventFilters).map(x => serializeEventFilter(x));

        let eventQuery = {
            eventTypeNodeId: nid,
            eventTypes: evtTypes,
            eventColumns: eventColumns,
            eventFilters: eventFilters
        }
        onChange({
            ...query,
            eventQuery: eventQuery
        });
        onRunQuery();
    }


    addEventFilter = (eventFilter: EventFilter) => {
        let tempArray = this.state.eventFilters.slice();
        tempArray.push(eventFilter);
        this.setState({ eventFilters: tempArray }, () => this.updateEventQuery());
    }

    renderTables() {
        const { datasource } = this.props;
        let validEventTypeNodeId: boolean = true;
        if (typeof this.state.eventTypeNodeId === 'undefined' || this.state.eventTypeNodeId === "") {
            validEventTypeNodeId = false;
        }
        if (validEventTypeNodeId) {
            return (<>
                <EventFieldTable datasource={datasource} eventTypeNodeId={this.state.eventTypeNodeId}
                    eventColumns={this.state.eventFields}
                    onChangeAlias={(alias, idx) => { this.onChangeAlias(alias, idx) }}
                    onChangeBrowsePath={(browsePath, idx) => { this.onChangeBrowsePath(browsePath, idx) }}
                    ondelete={(idx: number) => this.handleDeleteSelectField(idx)}
                    onAddColumn={(col: EventColumn) => this.onAddColumn(col)  } />
                <br />
                <EventFilterTable rows={this.state.eventFilters} ondelete={(idx: number) => { this.handleDeleteEventFilter(idx) }} />
                <br />
                <AddEventFilter add={(eventFilter: EventFilter) => { this.addEventFilter(eventFilter) }} datasource={this.props.datasource} eventTypeNodeId={this.state.eventTypeNodeId} />
            </>);
        }
        return (<></>);
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


    render() {

        return (
            <>
                <SegmentFrame label="Event Type" >
                    <ButtonCascader
                        //className="query-part"
                        value={this.state.eventTypes}
                        loadData={this.getEventTypes}
                        options={this.state.eventOptions}
                        onChange={this.onChangeEventType}
                    >
                        {this.state.eventTypes.join(separator)}
                    </ButtonCascader>
                </SegmentFrame>
                <br />
                {this.renderTables()}
            </>
        );
    }



}