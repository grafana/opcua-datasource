import React, { PureComponent, ChangeEvent } from 'react';
import { SegmentAsync, Segment, FormField } from '@grafana/ui';
import { Cascader, CascaderOption } from './components/Cascader';
import { QueryEditorProps, SelectableValue } from '@grafana/data';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults } from './types';
import { SegmentFrame, SegmentLabel } from './components/SegmentFrame';

const separator = ' / ';
const rootNode = 'i=85';
const selectText = (t: string): string => `Select <${t}>`;
const loadingOption: CascaderOption<OpcUaBrowseResults> = {
  label: 'Loading Options...',
  value: {
    displayName: 'Loading...',
    browseName: 'Loading...',
    nodeId: 'Loading...',
  },
};

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;

export class QueryEditor extends PureComponent<Props> {
  metric: OpcUaBrowseResults;
  selectedOptions: Array<CascaderOption<OpcUaBrowseResults>>;
  constructor(props: Props) {
    super(props);
    const { onChange, query } = this.props;

    if (!query.readType) {
      onChange({ ...query, readType: 'Processed' });
    }
    this.metric = this.props.query.metric;
    this.selectedOptions = [];
  }

  onChange = (...args: any[]) => {
    const { onChange, query, onRunQuery } = this.props;
    console.log('change', args);
    const changes: Record<string, any> = {};
    for (let i = 0; i < args.length; i += 2) {
      const variable: string = args[i];
      const value: any = args[i + 1];
      changes[variable] = value;
    }
    onChange({ ...query, ...changes });
    onRunQuery(); // executes the query
  };

  onSelect = (browseResults: OpcUaBrowseResults[], selectedOptions: Array<CascaderOption<OpcUaBrowseResults>>) => {
    this.metric = browseResults[browseResults.length - 1];
    this.selectedOptions = selectedOptions;
  };

  onChangeInterval = (event: ChangeEvent<HTMLInputElement>) => {
    const { onChange, query } = this.props;
    onChange({ ...query, interval: event.target.value });
  };

  onCascadeClose = () => {
    const displayName = this.selectedOptions.map(o => o.label).join(separator);
    this.onChange('metric', this.metric, 'displayName', displayName);
  };

  browseNode = (queryItem?: OpcUaBrowseResults): Promise<Array<CascaderOption<OpcUaBrowseResults>>> => {
    return this.props.datasource.browse(queryItem ? queryItem.nodeId : rootNode).then((results: OpcUaBrowseResults[]) => {
      return results.map((item: OpcUaBrowseResults) => ({
        label: item.displayName,
        value: item,
        items: [loadingOption],
        title: item.nodeId,
      }));
    });
  };

  browseNodeSV = (nodeId: string): Promise<Array<SelectableValue<any>>> => {
    return this.props.datasource.browse(nodeId).then((results: OpcUaBrowseResults[]) => {
      return results.map((item: OpcUaBrowseResults) => ({
        label: item.displayName,
        key: item.nodeId,
        description: item.displayName,
        value: item,
      }));
    });
  };

  render() {
    const { query, onRunQuery } = this.props;
    return (
      <>
        <SegmentFrame label="Tag">
          <Cascader
            initialValue={query.displayName ? query.displayName : ''}
            loadData={this.browseNode}
            onSelect={this.onSelect}
            onCascadeClose={this.onCascadeClose}
            separator={separator}
          />
          <SegmentLabel label={'Read Type'} />
          <Segment<any>
            value={query.readType ? this.props.query.readType : 'Processed'}
            options={[
              { label: 'Raw', value: 'Raw' },
              { label: 'Processed', value: 'Processed' },
            ]}
            onChange={e => e.value && this.onChange('readType', e.value)}
          />
          <SegmentLabel label={'Aggregate'} />
          <SegmentAsync
            value={query.aggregate ? this.props.query.aggregate.displayName : selectText('aggregate')}
            loadOptions={() => this.browseNodeSV('i=2997')}
            onChange={e => e.value && this.onChange('aggregate', e.value)}
          />
          <FormField label={'Interval'} value={query.interval} onChange={this.onChangeInterval} onBlur={() => onRunQuery()} />
        </SegmentFrame>
      </>
    );
  }
}
