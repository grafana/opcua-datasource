import React, { FC } from 'react';
import { DataSourceHttpSettings, Label, RadioButtonGroup } from '@grafana/ui';
import { DataSourcePluginOptionsEditorProps } from '@grafana/data';
import { OPCTimestamp, OpcUaDataSourceOptions } from './types';

interface Props extends DataSourcePluginOptionsEditorProps<OpcUaDataSourceOptions> {}

export const ConfigEditor: FC<Props> = (props: Props) => {
  const { options, onOptionsChange } = props;
  const description =
    options.jsonData.timestamp === OPCTimestamp.Server
      ? 'Read the timestamp from the OPC Server'
      : 'Read the timestamp from the OPC Source/Device/Client';
  return (
    <div className="gf-form-group">
      <DataSourceHttpSettings
        defaultUrl={'opc.tcp://nodename.host.net:62550/Path/OpcUAServer'}
        dataSourceConfig={options}
        onChange={(o) => onOptionsChange(o)}
      />
      <Label description={description}>OPC Timestamp Source</Label>
      <RadioButtonGroup
        options={[
          { label: 'Server', value: OPCTimestamp.Server },
          { label: 'Source', value: OPCTimestamp.Source },
        ]}
        value={options.jsonData.timestamp}
        onChange={(timestamp: OPCTimestamp | undefined) =>
          onOptionsChange({ ...options, jsonData: { ...options.jsonData, timestamp: timestamp! } })
        }
      />
    </div>
  );
};
