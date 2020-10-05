import React, { PureComponent} from "react";
import { SegmentFrame } from './SegmentFrame';
import { FilterOperandEnum, FilterOperand, FilterOperator, EventFilter, EventFilterOperatorUtil, LiteralOp, SimpleAttributeOp, QualifiedName, OpcUaBrowseResults, OpcUaNodeInfo, NodeClass } from '../types'; 
import { copyQualifiedName } from '../utils/QualifiedName';
import { BrowsePathEditor } from './BrowsePathEditor';
import { DataSource } from '../DataSource';
import { NodeEditor } from './NodeEditor';
import { Button } from '@grafana/ui';

export interface Props {
    datasource: DataSource,
    eventTypeNodeId: string,
    add(filter: EventFilter): void,

}

type State = {
    oper: FilterOperator,
    browsePath: QualifiedName[],
    value: string,
    typeId: OpcUaNodeInfo,
};


export class AddEventFilter extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            oper: FilterOperator.GreaterThan,
            browsePath: [],
            typeId: {
                browseName: { name: "", namespaceUrl: "" }, displayName: "", nodeClass: -1, nodeId: ""
            },
            value: "500"
        };
        this.changeOperator = this.changeOperator.bind(this);
    }

    addFilter() {
        if (this.state.browsePath.length > 0) {
            let attr: SimpleAttributeOp = { attributeId: 13, typeId: "", browsePath: this.state.browsePath.map(bp => copyQualifiedName(bp)) };
            let literal: LiteralOp = {
                typeId: this.state.typeId.nodeId, value: this.state.value
            };
            let operands: FilterOperand[] = [{ type: FilterOperandEnum.SimpleAttribute, value: attr }, { type: FilterOperandEnum.Literal, value: literal }];
            var evFilter: EventFilter = { oper: this.state.oper, operands: operands.slice() };
            this.props.add(evFilter);
        }
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
                return (<><BrowsePathEditor
                        browsePath={this.state.browsePath}
                        rootNodeId={this.props.eventTypeNodeId}
                    onChangeBrowsePath={(bp) => this.setState({ browsePath: bp })}
                    browse={(nodeId) => this.browseEventFields(nodeId)}> </BrowsePathEditor>

                    <SegmentFrame label="Value Type" marginLeft >
                        <NodeEditor browse={(node) => this.browseDataTypes(node)} onChangeNode={(node) =>  this.onChangeValueTypeNode(node)} rootNodeId="i=24"  />
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

    onChangeValueTypeNode(node: OpcUaNodeInfo): void {
        this.setState({ typeId: node });
    }

    browseDataTypes(nodeId: string): Promise<OpcUaBrowseResults[]> {
        return this.props.datasource
            .getResource('browse', { nodeId: nodeId, nodeClassMask: NodeClass.DataType });
    }

    browseEventFields(nodeId: string): Promise<OpcUaBrowseResults[]> {
        return this.props.datasource
            .getResource('browse', { nodeId: nodeId  });
    }

    render() {
        return (
            <div>
                <br/>
                {this.renderDropdown()}
                {this.renderOperands(this.state.oper)}
                <Button onClick={() => this.addFilter()}>Add Filter</Button>
            </div>
        );
    }
}