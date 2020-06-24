import React, { PureComponent, ChangeEvent } from 'react';
import { SegmentAsync, RadioButtonGroup, Input, TabsBar, TabContent, Tab } from '@grafana/ui';
import { CascaderOption } from 'rc-cascader/lib/Cascader';
import { TreeEditor } from './components/TreeEditor';
import { QueryEditorProps, SelectableValue } from '@grafana/data';
//import { Cascader } from './components/Cascader/Cascader';
import { ButtonCascader } from './components/ButtonCascader/ButtonCascader';
//import { Cascader, CascaderOption } from './components/Cascader/Cascader';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults, separator } from './types';
import { SegmentFrame, SegmentLabel } from './components/SegmentFrame';
import { css } from 'emotion';

const rootNode = 'i=85';
const selectText = (t: string): string => `Select <${t}>`;

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;
type State = {
  options: CascaderOption[];
  value: string[];
  tabs: Array<{ label: string; active: boolean }>;
};

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
      options: [],
      value: this.props.query.value || ['Select to browse OPC UA Server'],
      tabs: [
        { label: 'Traditional', active: true },
        { label: 'Tree view', active: false },
      ],
    };

    props.datasource.getResource('browse', { nodeId: rootNode }).then((results: OpcUaBrowseResults[]) => {
      console.log('Results', results);
      this.setState({
        options: results.map((r: OpcUaBrowseResults) => this.toCascaderOption(r)),
      });
    });
  }

  onChangeField = (field: string, sval: SelectableValue<any> | string, ...args: any[]) => {
    const { datasource, query, onChange, onRunQuery } = this.props;
    const { nodeId, refId } = query;

    console.log('change', field, sval, args);
    const changes: Record<string, any> = {};

    if (typeof sval === 'string') {
      changes[field] = sval;
    } else {
      changes[field] = sval.value;
    }

    if (changes[field] === 'Subscribe') {
      datasource.getResource('subscribe', { nodeId, refId }).then((results: any[]) => {
        console.log('We got subscribe results', results);
        onChange({ ...query, ...changes });
        onRunQuery();
      });
    } else {
      onChange({ ...query, ...changes });
      onRunQuery();
    }
  };

  onChange = (selected: string[], selectedItems: CascaderOption[]) => {
    const { query, onChange, onRunQuery } = this.props;
    const value = selectedItems.map(item => (item.label ? item.label.toString() : ''));
    const nodeId = selected[selected.length - 1];
    this.setState({ value });
    onChange({
      ...query,
      value,
      nodeId,
    });
      onRunQuery();
  };

  onSelect = (val: string) => {
    console.log('onSelect', val);
  };

  onChangeInterval = (event: ChangeEvent<HTMLInputElement>) => {
    const { onChange, query } = this.props;
    onChange({ ...query, interval: event.target.value });
  };

  toCascaderOption = (opcBrowseResult: OpcUaBrowseResults, children?: CascaderOption[]): CascaderOption => {
    console.log('browse Result', opcBrowseResult);
    return {
      label: opcBrowseResult.displayName,
      value: opcBrowseResult.nodeId,
      isLeaf: !opcBrowseResult.isForward || opcBrowseResult.nodeClass === 2, //!opcBrowseResult.isForward,
    };
  };

  getChildren = (selectedOptions: CascaderOption[]) => {
    const targetOption = selectedOptions[selectedOptions.length - 1];
    targetOption.loading = true;
    if (targetOption.value) {
      this.props.datasource
        .getResource('browse', { nodeId: targetOption.value })
        .then((results: OpcUaBrowseResults[]) => {
          targetOption.loading = false;
          targetOption.children = results.map(r => this.toCascaderOption(r));
          this.setState({
            options: [...this.state.options],
          });
        });
    }
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

  get readTypeOptions(): Array<SelectableValue<string>> {
    return [
      { label: 'Raw', value: 'ReadDataRaw' },
      { label: 'Processed', value: 'ReadDataProcessed' },
      { label: 'Realtime', value: 'ReadNode' },
      { label: 'Subscription', value: 'Subscribe' },
      { label: 'Events', value: 'ReadEvents' }
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

  optionalParams = (query: OpcUaQuery, onRunQuery: () => void): JSX.Element => {
    const readTypeValue = this.readTypeValue(query.readType);
    switch (readTypeValue) {
      case 'Processed': {
        return (
          <>
            <SegmentLabel label={'Aggregate'} marginLeft />
            <SegmentAsync
              value={query.aggregate?.name ?? selectText('aggregate')}
              loadOptions={() => this.browseNodeSV('i=2997')}
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

  renderOriginal = () => {
    const { query, onRunQuery } = this.props;
    const { options, value } = this.state;

    return (
      <>
        <SegmentFrame label="Tag">
          <div onBlur={e => console.log('onBlur', e)}>
            <ButtonCascader
              //className="query-part"
              value={value}
              loadData={this.getChildren}
              options={options}
              onChange={this.onChange}
            >
              {value.join(separator)}
            </ButtonCascader>
          </div>

          <RadioButtonGroup
            options={this.readTypeOptions}
            value={query.readType}
            onChange={e => e && this.onChangeField('readType', e)}
          />
          {this.optionalParams(query, onRunQuery)}
        </SegmentFrame>
        <SegmentFrame label="Alias">
          <Input value={undefined} placeholder={'alias'} onChange={e => this.onChangeField('alias', e)} width={30} />
        </SegmentFrame>
      </>
    );
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
