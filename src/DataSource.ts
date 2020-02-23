import { DataQueryRequest, DataQueryResponse, DataSourceApi, DataSourceInstanceSettings, DataFrame, FieldType, ArrayVector } from '@grafana/data';

import { OpcUaQuery, OpcUaDataSourceOptions, OpcUaResults, OpcUaResponse, OpcUaBrowseResults } from './types';
//import { FieldType } from '@grafana/data';
import { getBackendSrv } from '@grafana/runtime';

export class DataSource extends DataSourceApi<OpcUaQuery, OpcUaDataSourceOptions> {
  config: DataSourceInstanceSettings<OpcUaDataSourceOptions>;
  browseData: any;
  constructor(instanceSettings: DataSourceInstanceSettings<OpcUaDataSourceOptions>) {
    super(instanceSettings);
    console.log('instanceSettings', instanceSettings);
    this.config = instanceSettings;
    this.browseData = [];
  }

  async query(options: DataQueryRequest<OpcUaQuery>): Promise<DataQueryResponse> {
      if (!options.targets || !(options.targets.length > 0) || !options.targets[0].metric) {
      return Promise.resolve({ data: [] });
    }

    const queries: any[] = [];

    options.targets.forEach(target => {
      if (target.metric && target.metric.hasOwnProperty('nodeId')) {
        queries.push({
          refId: target.refId,
          intervalMs: target.readType === 'Processed' ? Number(target.interval) : 0,
          maxDataPoints: target.readType === 'Processed' ? options.maxDataPoints : -1,
          datasourceId: this.id,
          call: target.readType,
          callParams: {
            nodeId: target.metric.nodeId,
            aggregate: target.readType === 'Processed' && target.aggregate.hasOwnProperty('nodeId') ? target.aggregate.nodeId : '',
          },
        });
      }
    });

    if (queries.length === 0) {
      return Promise.resolve({ data: [] });
    }
    
      return getBackendSrv()
          .post('/api/tsdb/query', {
              from: options.range?.from.valueOf().toString(),
              to: options.range?.to.valueOf().toString(),
              queries,
          })
          .then((results: OpcUaResults) => {

              return {
                  data: Object.values(results.results)
                      .filter(result => result.hasOwnProperty('meta'))
                      .map((result: any) => {
                          const request = options.targets.find(target => target.refId === result.refId);
                          let entry: DataFrame = { fields: [], length: 0 };
                          if (request && request.metric) {
                              entry = {
                                  refId: result.refId,
                                  fields: [
                                      {
                                          name: 'Time',
                                          type: FieldType.time,
                                          values: Array.isArray(result.meta)
                                              ? new ArrayVector(result.meta.map((e: any) => new Date(e.SourceTimestamp)))
                                              : new ArrayVector([new Date(result.meta.SourceTimestamp)]),
                                          config: {
                                              title: request.displayName,
                                          },
                                      },
                                      {
                                          name: request.displayName,
                                          type: FieldType.number,
                                          values: Array.isArray(result.meta)
                                              ? new ArrayVector(result.meta.map((e: any) => e.Value))
                                              : new ArrayVector([result.meta.Value]),
                                          config: {
                                              title: request.displayName,
                                          },
                                      },
                                  ],
                                  length: result.meta.length,
                              };
                          }

                          return entry;
                      }),
              };
          });
  }

  async doSingleQuery(query: OpcUaQuery): Promise<DataFrame> {
    const opts = {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(query),
    };

    console.log(this.config);
    const res = await fetch(this.config.url!, opts);
    console.log('res', res);
    return Promise.resolve({
      fields: [],
      length: 0,
    });
  }

  browse(nodeId: string): Promise<OpcUaBrowseResults[]> {
    return getBackendSrv()
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
    let queries = [
      {
        refId: 'FlatBrowse',
        intervalMs: 1,
        maxDataPoints: 1,
        datasourceId: this.id,
        call: 'FlatBrowse',
      },
    ];
    return getBackendSrv()
      .post('/api/tsdb/query', { queries })
      .then((results: OpcUaResponse) => {
        console.log('We got results', results);
        const ret: OpcUaBrowseResults[] = (results.data.results['FlatBrowse'].meta as unknown) as OpcUaBrowseResults[];
        return ret;
      });
  }

  callFunction(call: string, nodeId = 'i=85'): Promise<any> {
    let queries = [
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
    ];
    return getBackendSrv().post('/api/tsdb/query', { queries });
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
