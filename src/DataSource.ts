import { DataQueryRequest, DataQueryResponse, DataSourceInstanceSettings } from '@grafana/data';

import { OpcUaQuery, OpcUaDataSourceOptions } from './types';
import { DataSourceWithBackend } from '@grafana/runtime';
import { Observable } from 'rxjs';

export class DataSource extends DataSourceWithBackend<OpcUaQuery, OpcUaDataSourceOptions> {
  config: DataSourceInstanceSettings<OpcUaDataSourceOptions>;
  constructor(instanceSettings: DataSourceInstanceSettings<OpcUaDataSourceOptions>) {
    super(instanceSettings);
    this.config = instanceSettings;
  }

  query(request: DataQueryRequest<OpcUaQuery>): Observable<DataQueryResponse> {
    return super.query(request);
  }

  getResource(path: string, params?: any): Promise<any> {
    return super.getResource(path, params);
  }
}
