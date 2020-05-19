import React, { PureComponent, ChangeEvent } from 'react';
import { SegmentAsync, Segment, LegacyForms } from '@grafana/ui';
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
      onChange({ ...query, readType: 'Realtime' });
    }
    this.metric = this.props.query.metric;
    this.selectedOptions = [];
  }

  onChange = (field: string, sval: SelectableValue<any>, ...args: any[]) => {
    const { onChange, query, onRunQuery } = this.props;
    console.log('change', args);
    const changes: Record<string, any> = {};

    changes[field] = sval.value;
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

  get readTypeOptions(): Array<SelectableValue<string>> {
    return [
      { label: 'Raw', value: 'ReadDataRaw' },
      { label: 'Processed', value: 'ReadDataProcessed' },
      { label: 'Realtime', value: 'ReadNode' },
    ]
  }

  readTypeValue = (readType: string): string => {
    const foundVal: SelectableValue<string> | undefined = this.readTypeOptions.find((o: SelectableValue<string>) => o.value === readType)
    if (foundVal && foundVal.label) {
      return foundVal.label;
    } else {
      return "Processed";
    }
  }

  optionalParams = (query: OpcUaQuery, onRunQuery: () => void): JSX.Element => {
    const readTypeValue = this.readTypeValue(query.readType);
    switch (readTypeValue) {
      case 'Processed': {
        return (
          <>
            <SegmentLabel label={'Aggregate'} />
            <SegmentAsync
              value={query.aggregate ? this.props.query.aggregate.displayName : selectText('aggregate')}
              loadOptions={() => this.browseNodeSV('i=2997')}
              onChange={e => this.onChange('aggregate', e)}
            />
            <LegacyForms.FormField
              label="Interval"
              value={query.interval}
              onChange={this.onChangeInterval}
              onBlur={() => onRunQuery()}
            />
          </>
        );
      }
      case 'Raw': {
        return (
          <>
            <LegacyForms.FormField
              label="Max Values"
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
            value={this.readTypeValue(query.readType)}
            options={this.readTypeOptions}
            onChange={e => this.onChange('readType', e)}
          />
          {this.optionalParams(query, onRunQuery)}
        </SegmentFrame>
      </>
    );
  }
}
