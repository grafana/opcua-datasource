import React, { PureComponent} from "react";
import { SegmentFrame } from './SegmentFrame';

export interface AddEventFieldFormProps {
    add(browsename: string, alias: string): void;
}

type State = {
    browsename: string;
    alias: string;
};


export class AddEventFieldForm extends PureComponent<AddEventFieldFormProps, State> {
    constructor(props: AddEventFieldFormProps) {
        super(props);
        this.state = {
            browsename: "",
            alias: ""
        };
        this.handleSubmit = this.handleSubmit.bind(this);
        this.changeBrowseName = this.changeBrowseName.bind(this);
        this.changeAlias = this.changeAlias.bind(this);
    }

    handleSubmit(event: { preventDefault: () => void; }) {
        this.props.add(this.state.browsename, this.state.alias);
        event.preventDefault();
    }

    changeBrowseName(event: { target: any; }) {
        const target = event.target;
        const value = target.value;
       
        this.setState({
            browsename: value
        });
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
                    <SegmentFrame label="Browse name">
                        <input
                        name="browsename"
                        type="input"
                        value={this.state.browsename}
                            onChange={this.changeBrowseName} />
                    </SegmentFrame>
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
}