import { DataQueryRequest, DataQueryResponse, DataSourceApi, DataSourceInstanceSettings } from '@grafana/data';

import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaResponse, OpcUaBrowseResults } from './types';
//import { FieldType } from '@grafana/data';
import { getBackendSrv, BackendSrv } from '@grafana/runtime';

export class DataSource extends DataSourceApi<OpcUaQuery, OpcUaDataSourceOptions> {
  config: DataSourceInstanceSettings<OpcUaDataSourceOptions>;
  browseData: any;
  backendSrv: BackendSrv;
  constructor(instanceSettings: DataSourceInstanceSettings<OpcUaDataSourceOptions>) {
    super(instanceSettings);
    console.log('instanceSettings', instanceSettings);
    this.backendSrv = getBackendSrv();
    this.config = instanceSettings;
    this.browseData = [];
  }

  async query(options: DataQueryRequest<OpcUaQuery>): Promise<DataQueryResponse> {
    const { range } = options;
    const from = range.from.toISOString();
    const to = range.to.toISOString();

    console.log("options", options);
    this.backendSrv
      .datasourceRequest({
        url: '/api/tsdb/query',
        method: 'POST',
        data: {
          from,
          to,
          queries: options.targets.map(target => {
            return {
              refId: target.refId,
              intervalMs: options.intervalMs,
              maxDataPoints: options.maxDataPoints,
              datasourceId: this.id,
              call: 'ReadDataProcessed',
              callParams: {
                nodeId: target.metric.description,
              }
            }
          }),
        },
      })
      .then((results: OpcUaResponse) => {
        console.log("results", results);
      });

    return Promise.resolve({ data: [] });
  }

  browse(nodeId: string): Promise<OpcUaBrowseResults[]> {
    return this.backendSrv
      .datasourceRequest({
        url: '/api/tsdb/query',
        method: 'POST',
        data: {
          from: '5m',
          to: 'now',
          queries: [
            {
              refId: 'Browse',
              intervalMs: 1,
              maxDataPoints: 1,
              datasourceId: this.id,
              call: 'Browse',
              callParams: {
                nodeId,
              },
            },
          ],
        },
      })
      .then((results: OpcUaResponse) => {
        const ret: OpcUaBrowseResults[] = (results.data.results['Browse'].meta as unknown) as OpcUaBrowseResults[];
        return ret;
      });
  }

  flatBrowse(): Promise<OpcUaBrowseResults[]> {
    return this.backendSrv
      .datasourceRequest({
        url: '/api/tsdb/query',
        method: 'POST',
        data: {
          from: '5m',
          to: 'now',
          queries: [
            {
              refId: 'FlatBrowse',
              intervalMs: 1,
              maxDataPoints: 1,
              datasourceId: this.id,
              call: 'FlatBrowse',
            },
          ],
        },
      })
      .then((results: OpcUaResponse) => {
        console.log("We got results", results);
        const ret: OpcUaBrowseResults[] = (results.data.results['FlatBrowse'].meta as unknown) as OpcUaBrowseResults[];
        return ret;
      });
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
            call: 'Browse',
          },
        ],
      },
    });
  }

  testDatasource() {
    if (!this.config.url) {
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
              call: 'Browse',
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
