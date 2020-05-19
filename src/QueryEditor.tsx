import React, { PureComponent, ChangeEvent } from 'react';
import { SegmentAsync, RadioButtonGroup, Input } from '@grafana/ui';
import { CascaderOption } from 'rc-cascader/lib/Cascader';
import { QueryEditorProps, SelectableValue } from '@grafana/data';
//import { Cascader } from './components/Cascader/Cascader';
import { ButtonCascader } from './components/ButtonCascader/ButtonCascader';
//import { Cascader, CascaderOption } from './components/Cascader/Cascader';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults, separator } from './types';
import { SegmentFrame, SegmentLabel } from './components/SegmentFrame';

const rootNode = 'i=85';
const selectText = (t: string): string => `Select <${t}>`;

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;
type State = {
  options: CascaderOption[];
  value: string[];
};

export class QueryEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);

    this.state = {
      options: [],
      value: this.props.query.value || ['Select to browse OPC UA Server'],
    };

    props.datasource.browse(rootNode).then((results: OpcUaBrowseResults[]) => {
      this.setState({
        options: results.map((r: OpcUaBrowseResults) => this.toCascaderOption(r)),
      });
    });
  }

  onChangeField = (field: string, sval: SelectableValue<any> | string, ...args: any[]) => {
    const { onChange, query, onRunQuery } = this.props;
    console.log('change', args);
    const changes: Record<string, any> = {};

    if (typeof sval === 'string') {
      changes[field] = sval;
    } else {
      changes[field] = sval.value;
    }

    onChange({ ...query, ...changes });
    onRunQuery(); // executes the query
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
      this.props.datasource.browse(targetOption.value).then((results: OpcUaBrowseResults[]) => {
        targetOption.loading = false;
        targetOption.children = results.map(r => this.toCascaderOption(r));
        this.setState({
          options: [...this.state.options],
        });
      });
    }
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
              value={query.aggregate ? this.props.query.aggregate.displayName : selectText('aggregate')}
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

  render() {
    const { query, onRunQuery } = this.props;
    const { options, value } = this.state;
    console.log('QueryEditor::render');

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
  }
}
