import React, { PureComponent } from 'react';
import { Input } from '@grafana/ui';
import { QualifiedName } from '../types';

export interface Props {
  namespaceUrl: string;
  name: string;
  id: number;
  selectBrowseNamespace(id: number, current: string): void;
  onchange(id: number, t: QualifiedName): void;
}

type State = {};

export class QualifiedNameEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
  }

  onChangeNamespaceUrl = (e: React.FormEvent<HTMLInputElement>) => {
    let v = e.currentTarget.value;
    let qm: QualifiedName = { name: this.props.name, namespaceUrl: v };
    this.props.onchange(this.props.id, qm);
  };
  onChangeName = (e: React.FormEvent<HTMLInputElement>) => {
    let v = e.currentTarget.value;
    let qm: QualifiedName = { name: v, namespaceUrl: this.props.namespaceUrl };
    this.props.onchange(this.props.id, qm);
  };

  render() {
    return (
      <div>
        <Input
          value={this.props.namespaceUrl}
          placeholder={'Namespace Url'}
          onChange={(e) => this.onChangeNamespaceUrl(e)}
          width={30}
        ></Input>
        <button onClick={(e) => this.props.selectBrowseNamespace(this.props.id, this.props.namespaceUrl)}></button>
        <Input
          value={this.props.name}
          placeholder={'Name'}
          onChange={(e) => this.onChangeName(e)}
          width={30}
        ></Input>
      </div>
    );
  }
}
