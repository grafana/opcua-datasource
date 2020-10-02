import React, { PureComponent } from "react";
import { QualifiedName } from '../types';
import { Input } from '@grafana/ui';
import { browsePathToString, stringToBrowsePath } from '../utils/QualifiedName';


export interface Props {
    browsePath: QualifiedName[];
    onBrowsePathChanged(browsePath: QualifiedName[]): void;
}

type State = {
    shortenedPath: string,
    longPath: string,
    edit: boolean,
}

export class BrowsePathTextEditor extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);
        let browsePath = this.props.browsePath;
        if (typeof browsePath === 'undefined')
            browsePath = [];
        let shortendPath = browsePath.map(p => p.name).join("/");
        let longPath = browsePathToString(browsePath);
        this.state =
        {
            shortenedPath: shortendPath,
            longPath: longPath,
            edit: false,
        };
    }

    
    render() {
        let browsePath = this.props.browsePath;
        if (typeof browsePath === 'undefined')
            browsePath = [];
        let shortendPath = browsePath.map(p => p.name).join("/");
        let longPath = browsePathToString(browsePath);
        this.setState(
        {
            shortenedPath: shortendPath,
            longPath: longPath
        });

        return this.state.edit ? 
            (
                <div data-tip={this.state.longPath} style={{ width: 500 }}>
                    <Input value={this.state.longPath} onChange={e => this.onChangeBrowsePath(e)} placeholder={'Path'} onBlur={() => this.setState({ edit: false })}></Input>
                </div>
            )
            :
            ( 
                <div data-tip={this.state.longPath}>
                    <Input value={this.state.shortenedPath} placeholder={'Path'} onClick={() => this.setState({ edit: true })}></Input>
                </div>
            );
    }

    onChangeBrowsePath(e: React.FormEvent<HTMLInputElement>): void {
        let s: string = e.currentTarget.value;
        let browsePath = stringToBrowsePath(s);
        this.props.onBrowsePathChanged(browsePath);
    }
}