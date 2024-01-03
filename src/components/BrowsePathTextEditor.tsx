import React, { PureComponent } from 'react';
import { QualifiedName } from '../types';
import { Input } from '@grafana/ui';
import { browsePathToString, stringToBrowsePath } from '../utils/QualifiedName';

export interface Props {
  browsePath: QualifiedName[];
  onBrowsePathChanged(browsePath: QualifiedName[]): void;
  getNamespaceIndices(): Promise<string[]>;
}

type State = {
  shortenedPath: string;
  indexedPath: string;
  nsTable: string[];
  nsTableFetched: boolean;
  edit: boolean;
};

export class BrowsePathTextEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    let browsePath = this.props.browsePath;
    if (typeof browsePath === 'undefined') {
      browsePath = [];
    }
    let shortenedPath = browsePath.map((p) => p.name).join('/');

    this.state = {
      shortenedPath,
      indexedPath: '',
      nsTable: [],
      nsTableFetched: false,
      edit: false,
    };
  }

  render() {
    if (!this.state.nsTableFetched) {
      this.props.getNamespaceIndices().then((nsTable) => {
        let indexedPath = browsePathToString(browsePath, this.state.nsTable);
        this.setState({ nsTable: nsTable, nsTableFetched: true, indexedPath: indexedPath });
      });
    }

    let browsePath = this.props.browsePath;
    if (typeof browsePath === 'undefined') {
      browsePath = [];
    }

    if (!this.state.edit) {
      let shortenedPath = browsePath.map((p) => p.name).join('/');
      let indexedPath = browsePathToString(browsePath, this.state.nsTable);
      this.setState({
        shortenedPath,
        indexedPath: indexedPath,
      });
    }

    return this.state.edit ? (
      <Input
        title={this.state.indexedPath}
        value={this.state.indexedPath}
        onChange={(e) => this.onChangeIndexedPath(e)}
        placeholder={'Path'}
        onBlur={(e) => this.onChangeBrowsePath()}
        onKeyPress={(k) => {
          if (k.key === 'Enter') {
            this.onChangeBrowsePath();
          }
        }}
      ></Input>
    ) : (
      <Input
        title={this.state.indexedPath}
        value={this.state.shortenedPath}
        placeholder={'Path'}
        onClick={() => this.setState({ edit: true })}
      ></Input>
    );
  }

  onChangeIndexedPath(e: React.FormEvent<HTMLInputElement>): void {
    let s: string = e.currentTarget.value;
    this.setState({ indexedPath: s });
  }

  onChangeBrowsePath(): void {
    //let s: string = e.currentTarget.value;
    let browsePath = stringToBrowsePath(this.state.indexedPath, this.state.nsTable);
    this.props.onBrowsePathChanged(browsePath);
    this.setState({ edit: false });
  }
}
