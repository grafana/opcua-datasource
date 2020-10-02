import React, { PureComponent} from "react";
import { SegmentFrame } from './SegmentFrame';
import { QualifiedName } from '../types';
import { Button } from '@grafana/ui';

export interface AddEventFieldFormProps {
    add(browsePath: QualifiedName[], alias: string): void;
}

type State = {
    browsePath: QualifiedName[];
    alias: string;
};


export class AddEventFieldForm extends PureComponent<AddEventFieldFormProps, State> {
    constructor(props: AddEventFieldFormProps) {
        super(props);
        this.state = {
            browsePath: [],
            alias: ""
        };
        this.changeAlias = this.changeAlias.bind(this);
    }


    handleSubmit(event: { preventDefault: () => void; }) {
        this.props.add(this.state.browsePath, this.state.alias);
        event.preventDefault();
    }

    changeAlias(event: { target: any; }) {
        const target = event.target;
        const value = target.value;

        this.setState({
            alias: value
        });
    }

    render() {
        
        return (
            <div>
                <br/>
                <form onSubmit={this.handleSubmit}>

                    <Button onClick={() => this.toggleBrowsePathBrowser()}>Browse</Button>
                    <SegmentFrame label="Alias" marginLeft >
                      <input
                        name="alias"
                        type="input"
                        value={this.state.alias}
                        onChange={this.changeAlias} />
                      </SegmentFrame>
                    <input type="submit" value="Add Column" />
                </form>
            </div>
        );
    }
    onChangeBrowsePath(bp: QualifiedName[]): void {
        throw new Error("Method not implemented.");
    }


    toggleBrowsePathBrowser() {
        throw new Error("Method not implemented.");
    }
}