import React, { PureComponent } from 'react';
import { SegmentAsync, Segment, FormField } from '@grafana/ui';
import { QueryEditorProps, SelectableValue } from '@grafana/data';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults } from './types';
import { SegmentFrame, SegmentLabel } from './components/SegmentFrame';

const selectText = 'Select <metric>';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;

export class QueryEditor extends PureComponent<Props> {
  constructor(props: Props) {
    super(props);
  }

  onChange = (variable: string, value: any) => {
    const { onChange, query, onRunQuery } = this.props;
    onChange({ ...query, [variable]: value });
    onRunQuery(); // executes the query
  };

  getTreeData = (): Promise<Array<SelectableValue<string>>> => {
    return this.props.datasource.flatBrowse().then((results: OpcUaBrowseResults[]) => {
      return results.map((item: OpcUaBrowseResults) => ({
        label: item.displayName,
        key: item.nodeId,
        description: item.nodeId,
        value: item.nodeId,
      }));
    });
  };

  render() {
    return (
      <>
        <SegmentFrame label="Tag">
          <SegmentAsync value={this.props.query.metric || selectText} loadOptions={this.getTreeData} onChange={e => this.onChange('metric', e)} />
          <SegmentLabel label={'Read Type'} />
          <Segment
            value={this.props.query.readType || 'Processed'}
            options={[
              { label: 'Raw', value: 'Raw' },
              { label: 'Processed', value: 'Processed' },
            ]}
            onChange={e => this.onChange('readType', e)}
          />
          <SegmentLabel label={'Aggregate'} />
          <Segment
            value={this.props.query.aggregate || 'Interpolative'}
            options={[{ label: 'Interpolative', value: 'Interpolative' }]}
            onChange={e => this.onChange('aggregate', e)}
          />
          <FormField label={'Interval'} value={'$__interval'} />
        </SegmentFrame>
      </>
    );
  }
}
