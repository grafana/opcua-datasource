import React, { PureComponent, ChangeEvent } from 'react';
import { TabsBar, TabContent, Tab, RadioButtonGroup, Input, SegmentAsync } from '@grafana/ui';
import { TreeEditor } from './components/TreeEditor';
import { QueryEditorProps, SelectableValue } from '@grafana/data';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults } from './types';
import { css } from 'emotion';
import { EventQueryEditor } from './components/EventQueryEditor';
import { NodeQueryEditor } from './components/NodeQueryEditor';
import { SegmentLabel } from './components/SegmentFrame';


type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;
type State = {
    tabs: Array<{ label: string; active: boolean }>;
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
            tabs: [
                { label: 'Traditional', active: true },
                { label: 'Tree view', active: false },
            ],
        };
    }

    onSelect = (val: string) => {
        console.log('onSelect', val);
    };

    onChangeInterval = (event: ChangeEvent<HTMLInputElement>) => {
        const { onChange, query } = this.props;
        onChange({ ...query, interval: event.target.value });
    };




    get readTypeOptions(): Array<SelectableValue<string>> {
        return [
            { label: 'Raw', value: 'ReadDataRaw' },
            { label: 'Processed', value: 'ReadDataProcessed' },
            { label: 'Realtime', value: 'ReadNode' },
            { label: 'Subscription', value: 'Subscribe' },
            { label: 'Events', value: 'ReadEvents' },
            { label: 'Subscribe Events', value: 'SubscribeEvents' }
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

    onChangeField = (field: string, sval: SelectableValue<any> | string, ...args: any[]) => {
        const { /*datasource,*/ query, onChange, onRunQuery } = this.props;
        const { /*nodeId, refId*/ } = query;

        console.log('change', field, sval, args);
        const changes: Record<string, any> = {};

        if (typeof sval === 'string') {
            changes[field] = sval;
        } else {
            changes[field] = sval.value;
        }

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
    };

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


    optionalParams = (query: OpcUaQuery, onRunQuery: () => void): JSX.Element => {

        const readTypeValue = this.readTypeValue(query.readType);
        switch (readTypeValue) {
            case 'Processed': {
                return (
                    <>
                        <SegmentLabel label={'Aggregate'} marginLeft />
                        <SegmentAsync
                            value={query.aggregate?.name ?? selectText('aggregate')}
                            loadOptions={() => this.browseNodeSV('i=11201')}
                            onChange={e => this.onChangeField('aggregate', e)}
                        />
                    </>
                );
            }
            case 'Raw': {
                return (
                    <>
                        <SegmentLabel label="Max Values" marginLeft />
                        <Input
                            width={10}
                            value={-1}
                            onChange={() => console.log('not implemented yet')}
                            onBlur={() => onRunQuery()}
                        />
                    </>
                );
            }


            default: {
                return <></>;
            }
        }
    };


    renderReadTypes = () => {
        const { query, onRunQuery } = this.props;
        return <><RadioButtonGroup
            options={this.readTypeOptions}
            value={query.readType}
            onChange={e => e && this.onChangeField('readType', e)}
        />
            {this.optionalParams(query, onRunQuery)}
            </>;
    }

    renderNodeQueryEditor = (nodeNameType: string) => {
        const { datasource, onChange, query, onRunQuery } = this.props;
        return <NodeQueryEditor nodeNameType={nodeNameType} datasource={datasource} onChange={onChange} onRunQuery={onRunQuery} query={query} ></NodeQueryEditor>
    }

    renderOriginal = () => {
        const { datasource, onChange, query, onRunQuery } = this.props;
        const readTypeValue = this.readTypeValue(query.readType);
        if (readTypeValue === "Events" || readTypeValue === "Subscribe Events") {
            
            return (<>
                <div>{this.renderReadTypes()}</div>
                <div>{this.renderNodeQueryEditor("Event Source")}</div>
                <EventQueryEditor datasource={datasource} onChange={onChange} onRunQuery={onRunQuery} query={query} /> </>);
        }
        else {
            return (<>
                <div>{this.renderReadTypes()}</div>
                <div>{this.renderNodeQueryEditor("Data Value")}</div>
            </>);
        }
    };

    renderTreeEditor = () => {
        return <TreeEditor {...this.props} />;
    };

    render() {
        const { tabs } = this.state;
        console.log('QueryEditor::render');

        return (
            <>
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
