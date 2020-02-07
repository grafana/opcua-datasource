import { DataQueryRequest, DataQueryResponse, DataSourceApi, DataSourceInstanceSettings, DataFrame, FieldType, ArrayVector } from '@grafana/data';

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
    console.log('query options', options);

    if (!options.targets || !(options.targets.length > 0) || !options.targets[0].metric) {
      return Promise.resolve({ data: [] });
    }

    const { range } = options;
    const from = range.from.toISOString();
    const to = range.to.toISOString();
    const queries: any[] = [];

    options.targets.forEach(target => {
      if (target.metric && target.metric.hasOwnProperty('nodeId') && target.aggregate && target.aggregate.hasOwnProperty('nodeId')) {
        queries.push({
          refId: target.refId,
          intervalMs: options.intervalMs,
          maxDataPoints: target.readType === 'Processed' ? options.maxDataPoints : -1,
          datasourceId: this.id,
          call: target.readType === 'Processed' ? 'ReadDataProcessed' : 'ReadDataRaw',
          callParams: {
            nodeId: target.metric.nodeId,
            aggregate: target.aggregate.nodeId,
          },
        });
      }
    });

    if (queries.length === 0) {
      return Promise.resolve({ data: [] });
    }

    return this.backendSrv
      .datasourceRequest({
        url: '/api/tsdb/query',
        method: 'POST',
        data: {
          from,
          to,
          queries,
        },
      })
      .then((results: OpcUaResponse) => {
        console.log('results', results);
        return {
          data: Object.values(results.data.results).map((result: any) => {
            const request = options.targets.find(target => target.refId === result.refId);
            let entry: DataFrame = { fields: [], length: 0 };
            if (request && request.metric) {
              entry = {
                refId: result.refId,
                fields: [
                  {
                    name: 'Time',
                    type: FieldType.time,
                    values: new ArrayVector(result.meta.map((e: any) => new Date(e.SourceTimestamp))),
                    config: {
                      title: request.metric.displayName,
                    },
                  },
                  {
                    name: request.metric.nodeId,
                    type: FieldType.number,
                    values: new ArrayVector(result.meta.map((e: any) => e.Value)),
                    config: {
                      title: request.metric.displayName,
                    },
                  },
                ],
                length: result.meta.length,
              };
            }
            console.log('entry', entry);

            return entry;
          }),
        };
      });
  }

  browse(nodeId: string): Promise<OpcUaBrowseResults[]> {
    console.log('browsing', nodeId);
    return this.backendSrv
      .datasourceRequest({
        url: '/api/tsdb/query',
        method: 'POST',
        data: {
          from: '1m',
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
        console.log('We got results', results);
        const ret: OpcUaBrowseResults[] = (results.data.results['FlatBrowse'].meta as unknown) as OpcUaBrowseResults[];
        return ret;
      });
  }

  callFunction(call: string, nodeId = 'i=85'): Promise<any> {
    return this.backendSrv.datasourceRequest({
      url: '/api/tsdb/query',
      method: 'POST',
      data: {
        from: '1m',
        to: 'now',
        queries: [
          {
            refId: 'A',
            intervalMs: 1,
            maxDataPoints: 1,
            datasourceId: this.id,
            call,
            callParams: {
              nodeId,
            },
          },
        ],
      },
    });
  }

  getTreeData(nodeId = 'i=85'): Promise<any> {
    return this.callFunction('Browse', nodeId);
  }

  testDatasource(): Promise<any> {
    if (!this.config.url) {
      return Promise.resolve({
        status: 'warn',
        message: 'Missing URL',
      });
    }

    return this.getTreeData('i=84')
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
