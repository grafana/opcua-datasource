import { DataQueryRequest, DataQueryResponse, DataSourceInstanceSettings } from '@grafana/data';

import { OpcUaQuery, OpcUaDataSourceOptions } from './types';
import { DataSourceWithBackend, getTemplateSrv } from '@grafana/runtime';
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

  getTemplateVariable(tempVar: string): string {
    if (typeof tempVar === 'undefined' || tempVar.length === 0) {
      return '$ObjectId';
    }
    return tempVar;
  }

  applyTemplateVariables(query: OpcUaQuery): OpcUaQuery {
    let templateSrv = getTemplateSrv();
    if (query.useTemplate) {
      query.nodePath.node.nodeId = templateSrv.replace(this.getTemplateVariable(query.templateVariable));
    }
    return query;
  }

  getResource(path: string, params?: any): Promise<any> {
    return super.getResource(path, params);
  }
}
