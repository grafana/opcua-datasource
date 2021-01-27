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
      tabs: [
        { label: 'Traditional', active: true },
        { label: 'Tree view', active: false },
        ],
        maxValuesPerNode: props.query?.maxValuesPerNode?.toString(),
        resampleInterval: props.query?.resampleInterval?.toString(),
        theme: null,
    };
  }

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
        const { onChange, query } = this.props;
        let max = e.currentTarget.value;
        this.setState({ maxValuesPerNode: max }, () => {
            let maxNumber = parseInt(this.state.maxValuesPerNode);
            if (isNaN(maxNumber)) {
                maxNumber = 0; // Default handling. 
            }
            onChange({ ...query, maxValuesPerNode: maxNumber });
        });
    }

    onChangeResampleInterval(e: React.FormEvent<HTMLInputElement>): void {
        const { onChange, query } = this.props;
        let resampleInt = e.currentTarget.value;
        this.setState({ resampleInterval: resampleInt }, () => {
            let resampleInterval = parseInt(this.state.resampleInterval);
            if (isNaN(resampleInterval)) {
                resampleInterval = 0; // Default handling. 
            }
            onChange({ ...query, resampleInterval: resampleInterval });
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
  };

  renderReadTypes = () => {
    const { query, onRunQuery } = this.props;
    return (
      <>
        <RadioButtonGroup
          options={this.readTypeOptions}
          value={query.readType}
          onChange={e => e && this.onChangeField('readType', e)}
        />
        {this.optionalParams(query, onRunQuery)}
      </>
    );
  };

  renderNodeQueryEditor = (nodeNameType: string) => {
    const { datasource, onChange, query, onRunQuery } = this.props;
    return (
        <NodeQueryEditor
        theme={this.state.theme}
        nodeNameType={nodeNameType}
        datasource={datasource}
        onChange={onChange}
        onRunQuery={onRunQuery}
        query={query}
      ></NodeQueryEditor>
    );
  };

  renderOriginal = () => {
    const { datasource, onChange, query, onRunQuery } = this.props;
    const readTypeValue = this.readTypeValue(query.readType);
    if (readTypeValue === 'Events' || readTypeValue === 'Subscribe Events') {
        return (
            <>
                <div>{this.renderReadTypes()}</div>
                <div>{this.renderNodeQueryEditor('Event Source')}</div>
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
            <div>{this.renderReadTypes()}</div>
            <div>{this.renderNodeQueryEditor('Instance')}</div>
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
