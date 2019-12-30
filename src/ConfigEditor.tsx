import React, { PureComponent, ChangeEvent } from 'react';
import { DataSourcePluginOptionsEditorProps, DataSourceSettings, FormField } from '@grafana/ui';
import { MyDataSourceOptions } from './types';

type Settings = DataSourceSettings<MyDataSourceOptions>;

interface Props extends DataSourcePluginOptionsEditorProps<Settings> {}

interface State {}

export class ConfigEditor extends PureComponent<Props, State> {
  componentDidMount() {}

  OnUrlChanged = (event: ChangeEvent<HTMLInputElement>) => {
    console.log('Url change event', event);
    const { onOptionsChange, options } = this.props;
    const jsonData = {
      ...options.jsonData,
      url: event.target.value,
    };
    onOptionsChange({ ...options, jsonData });
  };

  render() {
    const { options } = this.props;
    const { jsonData } = options;

    return (
      <div className="gf-form-group">
        <div className="gf-form max-width-100">
          <FormField
            label="OPC UA URL"
            labelWidth={10}
            inputWidth={32}
            onChange={this.OnUrlChanged}
            value={jsonData.url || ''}
            placeholder="opc.tcp://localhost:48400/UA/ComServerWrapper"
          />
        </div>
      </div>
    );
  }
}
