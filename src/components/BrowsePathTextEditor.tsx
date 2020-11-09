import React, { PureComponent } from 'react';
import { QualifiedName } from '../types';
import { Input } from '@grafana/ui';
import { browsePathToString, stringToBrowsePath } from '../utils/QualifiedName';

export interface Props {
  browsePath: QualifiedName[];
  onBrowsePathChanged(browsePath: QualifiedName[]): void;
}

type State = {
  shortenedPath: string;
  longPath: string;
  edit: boolean;
};

export class BrowsePathTextEditor extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    let browsePath = this.props.browsePath;
    if (typeof browsePath === 'undefined') {
      browsePath = [];
    }
    let shortendPath = browsePath.map(p => p.name).join('/');
    let longPath = browsePathToString(browsePath);
    this.state = {
      shortenedPath: shortendPath,
      longPath: longPath,
      edit: false,
    };
  }

    
    render() {
        let browsePath = this.props.browsePath;
        if (typeof browsePath === 'undefined')
            browsePath = [];

        if (!this.state.edit) {

            let shortendPath = browsePath.map(p => p.name).join("/");
            let longPath = browsePathToString(browsePath);
            this.setState(
                {
                    shortenedPath: shortendPath,
                    longPath: longPath
                });
        }

        return this.state.edit ? 
            (
                <div title={this.state.longPath}>
                    <Input value={this.state.longPath} onChange={e => this.onChangeLongPath(e)} placeholder={'Path'} onBlur={(e) => this.onChangeBrowsePath(e)}></Input>
                </div>
            )
            :
            ( 
                <div title={this.state.longPath}>
                    <Input value={this.state.shortenedPath} placeholder={'Path'} onClick={() => this.setState({ edit: true })}></Input>
                </div>
            );
    }

    onChangeLongPath(e: React.FormEvent<HTMLInputElement>): void {
        let s: string = e.currentTarget.value;
        this.setState({ longPath: s });
    }

    onChangeBrowsePath(e: React.FormEvent<HTMLInputElement>): void {
        let s: string = e.currentTarget.value;
        let browsePath = stringToBrowsePath(s);
        this.props.onBrowsePathChanged(browsePath);
        this.setState({ edit: false })
    }
}