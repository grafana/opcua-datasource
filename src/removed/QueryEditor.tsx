import defaults from 'lodash/defaults';

import React, { PureComponent, ChangeEvent } from 'react';
import { FormField } from '@grafana/ui';
import { QueryEditorProps } from '@grafana/data';
import { DataSource } from './DataSource';
import { OpcUaQuery, OpcUaDataSourceOptions } from './types';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions>;

interface State {}

export class QueryEditor extends PureComponent<Props, State> {
  onComponentDidMount() {}

  onQueryTextChange = (event: ChangeEvent<HTMLInputElement>) => {
    const { onChange, query } = this.props;
    onChange({ ...query, queryText: event.target.value });
  };

  render() {
    return <div className="gf-form"></div>;
  }
}
