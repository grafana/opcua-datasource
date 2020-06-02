import {
  DataQueryRequest,
  DataQueryResponse,
  //DataSourceApi,
  DataSourceInstanceSettings,
  //FieldType,
  //ArrayVector,
} from '@grafana/data';

import {
  OpcUaQuery,
  OpcUaDataSourceOptions,
  //OpcUaResults,
  //separator,
} from './types';
//import { FieldType } from '@grafana/data';
import { DataSourceWithBackend } from '@grafana/runtime';
import { Observable } from 'rxjs';

export class DataSource extends DataSourceWithBackend<OpcUaQuery, OpcUaDataSourceOptions> {
  config: DataSourceInstanceSettings<OpcUaDataSourceOptions>;
  browseData: any;
  constructor(instanceSettings: DataSourceInstanceSettings<OpcUaDataSourceOptions>) {
    super(instanceSettings);
    console.log('instanceSettings', instanceSettings);
    this.config = instanceSettings;
    this.browseData = [];
  }

  query(request: DataQueryRequest<OpcUaQuery>): Observable<DataQueryResponse> {
    return super.query(request);
  }

  getResource(path: string, params?: any): Promise<any> {
    return super.getResource(path, params);
  }
}
