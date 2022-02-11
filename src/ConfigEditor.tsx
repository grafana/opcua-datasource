import React, { FC } from 'react';
import { DataSourceHttpSettings, Alert } from '@grafana/ui';
import { DataSourcePluginOptionsEditorProps } from '@grafana/data';
import { OpcUaDataSourceOptions } from './types';

interface Props extends DataSourcePluginOptionsEditorProps<OpcUaDataSourceOptions> {}

export const ConfigEditor: FC<Props> = (props: Props) => {
  const { options, onOptionsChange } = props;
  return (
    <div className="gf-form-group">
      <DataSourceHttpSettings
        defaultUrl={'opc.tcp://nodename.host.net:62550/Path/OpcUAServer'}
        dataSourceConfig={options}
        onChange={onOptionsChange}
      />
      <Alert title="Additional Configuration" severity="info">
        <p>In the grafana.conf, pleasure ensure you have the following configuration specified:</p>
        <pre>
          <code>
            {/* prettier-ignore */}
            [plugin.grafana-opcua-datasource]
            <br />
            {/* prettier-ignore */}
            data_dir = &quot;/some/path/to/config/grafana-opcua-datasource&quot;
            <br />
          </code>
        </pre>
        <p>Example:</p>
        <pre>
          <code>
            {/* prettier-ignore */}
            [plugin.grafana-opcua-datasource]
            <br />
            {/* prettier-ignore */}
            data_dir = &quot;/var/lib/grafana-opcua-datasource&quot;
            <br />
          </code>
        </pre>
      </Alert>
    </div>
  );
};
