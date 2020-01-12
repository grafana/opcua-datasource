import React, { PureComponent } from 'react';
import { SegmentAsync, Segment, FormField } from '@grafana/ui';
import { QueryEditorProps, SelectableValue } from '@grafana/data';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults } from './types';
import { SegmentFrame, SegmentLabel } from './components/SegmentFrame';

//const rootNode = 'i=84';
//const loadingText = 'Loading...';
const initialValue = 'Select...';
const S = (s: any): SelectableValue<any> => {
  return {label: s, value: s};
}

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;

interface State {}

export class QueryEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
  }

  onComponentDidMount() {}

  onChange = (metric?: SelectableValue<string>) => {
    if (metric) {
      const { onChange, query, onRunQuery } = this.props;
      onChange({ ...query, metric });
      onRunQuery(); // executes the query
    }
  };

  onChangeReadType = (readType?: SelectableValue<string>) => {
    if (readType) {
      console.log("readType", readType);
    }
  };

  getTreeData = (query?: string): Promise<Array<SelectableValue<SelectableValue<string>>>> => {
    return this.props.datasource.flatBrowse().then((results: OpcUaBrowseResults[]) => {
      console.log(results);
      return results.map((item: OpcUaBrowseResults) => {
        return {
          label: item.displayName,
          key: item.nodeId,
          description: item.nodeId,
        };
      });
    });
  };

  get value(): SelectableValue<string> {
    const { query } = this.props;
    return query.metric || { label: initialValue, value: initialValue };
  }

  render() {
    return (
      <>
        <SegmentFrame label={'Tag'}>
          <SegmentAsync value={this.value} loadOptions={this.getTreeData} onChange={this.onChange} />
          <SegmentLabel label={'Read Type'}/>
          <Segment value={S("Raw")} options={[S("Raw"), S("Processed")]} onChange={this.onChangeReadType} />
          <SegmentLabel label={'Aggregate'}/>
          <SegmentAsync value={{key:"Select <Aggregate>"}} loadOptions={this.getTreeData} onChange={this.onChange} />
          <FormField label={"Interval"} value={"$__interval"}/>
        </SegmentFrame>
      </>
    );
  }
}
