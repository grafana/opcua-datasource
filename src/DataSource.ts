import defaults from 'lodash/defaults';

import { DataQueryRequest, DataQueryResponse, DataSourceApi, DataSourceInstanceSettings } from '@grafana/ui';

import { MyQuery, MyDataSourceOptions, defaultQuery } from './types';
import { MutableDataFrame, FieldType } from '@grafana/data';

export class DataSource extends DataSourceApi<MyQuery, MyDataSourceOptions> {
  jsonData: MyDataSourceOptions;
  browseData: any;
  constructor(instanceSettings: DataSourceInstanceSettings<MyDataSourceOptions>, private backendSrv: any) {
    super(instanceSettings);
    console.log('instanceSettings', instanceSettings);
    this.backendSrv = backendSrv;
    this.jsonData = instanceSettings.jsonData;
    this.browseData = [];
  }

  query(options: DataQueryRequest<MyQuery>): Promise<DataQueryResponse> {
    const { range } = options;
    const from = range.from.valueOf();
    const to = range.to.valueOf();

    // Return a constant for each query
    const data = options.targets.map(target => {
      const query = defaults(target, defaultQuery);
      return new MutableDataFrame({
        refId: query.refId,
        fields: [
          { name: 'Time', values: [from, to], type: FieldType.time },
          { name: 'Value', values: [query.constant, query.constant], type: FieldType.number },
        ],
      });
    });

    return Promise.resolve({ data });
  }

  getTreeData(): Promise<any> {
    return this.backendSrv.datasourceRequest({
      url: '/api/tsdb/query',
      method: 'POST',
      data: {
        from: '5m',
        to: 'now',
        queries: [
          {
            refId: 'A',
            intervalMs: 1,
            maxDataPoints: 1,
            datasourceId: this.id,
            format: 'table',
            call: 'GetTree',
            endpoint: this.jsonData.url,
          },
        ],
      },
    });
  }

  testDatasource() {
    this.jsonData.call = 'GetEndpoints';
    if (!this.jsonData.url) {
      return Promise.resolve({
        status: 'warn',
        message: 'Missing URL',
      });
    }

    return this.backendSrv
      .datasourceRequest({
        url: '/api/tsdb/query',
        method: 'POST',
        data: {
          from: '5m',
          to: 'now',
          queries: [
            {
              refId: 'A',
              intervalMs: 1,
              maxDataPoints: 1,
              datasourceId: this.id,
              format: 'table',
              call: 'GetTree',
              endpoint: this.jsonData.url,
            },
          ],
        },
      })
      .then((resp: DataQueryResponse) => {
        // Save the browse for future reference
        console.log('We got browseData', resp);
        this.browseData = resp;
        return Promise.resolve({
          status: 'success',
          message: 'Connection successful',
        });
      })
      .catch((err: any) => {
        console.log('We caught error', err);
        throw err;
      });
  }
}
