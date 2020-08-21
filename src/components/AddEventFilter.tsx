import React, { PureComponent} from "react";
import { SegmentFrame } from './SegmentFrame';
import { FilterOperandEnum, FilterOperand, FilterOperator, EventFilter, EventFilterOperatorUtil, LiteralOp, SimpleAttributeOp } from '../types'; 

export interface Props {
    add(filter: EventFilter): void;
}

type State = {
    oper: FilterOperator;
    fieldName: string;
    namespaceUrl: string;
    value: string;
    typeId: string;
};


export class AddEventFilter extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            oper: FilterOperator.GreaterThan,
            fieldName: "Severity",
            namespaceUrl: "",
            typeId: "i=10",
            value: "500"
        };
        this.handleSubmit = this.handleSubmit.bind(this);
        this.changeOperator = this.changeOperator.bind(this);
    }

    handleSubmit(event: { preventDefault: () => void; }) {
        let attr: SimpleAttributeOp = { attributeId: 13, typeId: "", browsePath: [{ name: this.state.fieldName, namespaceUrl: this.state.namespaceUrl }] };
        let literal: LiteralOp = {
            typeId: this.state.typeId, value: this.state.value };
        let operands: FilterOperand[] = [{ type: FilterOperandEnum.SimpleAttribute, value: attr }, { type: FilterOperandEnum.Literal, value: literal } ];
        var evFilter: EventFilter = { oper: this.state.oper, operands: operands.slice() };
        this.props.add(evFilter);
        event.preventDefault();
    }


    changeOperator(event: { target: any; }) {
        const target = event.target;
        const value = target.value as FilterOperator;
        switch (value) {
            case FilterOperator.GreaterThan:
            case FilterOperator.GreaterThanOrEqual:
            case FilterOperator.LessThan:
            case FilterOperator.LessThanOrEqual:
            case FilterOperator.Equals:
                {
                    this.setState({ oper: value});
                }
                
        }
    }

    changeNamespaceUrl(event: { target: any; }) {
        const target = event.target;
        const value = target.value;

        this.setState({
            namespaceUrl: value
        });
    }


    changeFieldName(event: { target: any; }) {
        const target = event.target;
        const value = target.value;

        this.setState({
            fieldName: value
        });
    }

    changeValueType(event: { target: any; }) {
        const target = event.target;
        const value = target.value;

        this.setState({
            typeId: value
        });
    }

    changeValue(event: { target: any; }) {
        const target = event.target;
        const value = target.value;

        this.setState({
            value: value
        });
    }


    renderDropdown() {
        return (
            <select onSelect={this.changeOperator}>
                {
                    EventFilterOperatorUtil.operNames.map((n, idx) =>
                    {
                        return (<option value={idx}>{n}</option>);
                    })
                }
            </select>
            );
    }

    renderOperands(oper: FilterOperator) {
        switch (oper) {
            case FilterOperator.GreaterThan:
            case FilterOperator.GreaterThanOrEqual:
            case FilterOperator.LessThan:
            case FilterOperator.LessThanOrEqual:
            case FilterOperator.Equals:
                return (<><SegmentFrame label="Namespace Url">
                        <input
                            name="ns"
                            type="input"
                            value={this.state.namespaceUrl}
                        onChange={(ev) => this.changeNamespaceUrl(ev)} />
                    </SegmentFrame>
                    <SegmentFrame label="Name">
                        <input
                            name="evField"
                            type="input"
                            value={this.state.fieldName}
                            onChange={(ev) => this.changeFieldName(ev)} />
                    </SegmentFrame>
                    <SegmentFrame label="Value Type" marginLeft >
                        <input
                            name="type"
                            type="input"
                            value={this.state.typeId}
                            onChange={(ev) => this.changeValueType(ev) } />
                    </SegmentFrame>

                    <SegmentFrame label="Value" marginLeft >
                        <input
                            name="value"
                            type="input"
                            value={this.state.value}
                            onChange={(ev) => this.changeValue(ev)} />
                    </SegmentFrame></>);
        }
        return <></>;
    }

    render() {
        return (
            <div>
                <br/>
                <form onSubmit={this.handleSubmit}>
                    {this.renderDropdown()}
                    {this.renderOperands(this.state.oper)}
                    <input type="submit" value="Add Filter" />
                </form>
            </div>
        );
    }
}