import { DataSourcePlugin } from '@grafana/data';
import { DataSource } from './DataSource';
import { ConfigEditor } from './ConfigEditor';
import { QueryEditor } from './QueryEditor';
import { OpcUaQuery, OpcUaDataSourceOptions } from './types';

export const plugin = new DataSourcePlugin<DataSource, OpcUaQuery, OpcUaDataSourceOptions>(DataSource)
  .setConfigEditor(ConfigEditor)
  .setQueryEditor(QueryEditor);
