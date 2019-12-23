import React, { PureComponent } from 'react';
import { DataSourceHttpSettings } from '@grafana/ui';
import { DataSourcePluginOptionsEditorProps } from '@grafana/data';
import { OpcUaDataSourceOptions } from './types';

interface Props extends DataSourcePluginOptionsEditorProps<OpcUaDataSourceOptions> {}

export class ConfigEditor extends PureComponent<Props> {
  render() {
    const { options, onOptionsChange } = this.props;
    return (
      <div className="gf-form-group">
        <DataSourceHttpSettings
          defaultUrl={'opc.tcp://nodename.host.net:62550/Path/OpcUAServer'}
          dataSourceConfig={options}
          onChange={onOptionsChange}
        />
      </div>
    );
  }
}
