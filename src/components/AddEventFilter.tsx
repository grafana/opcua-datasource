import React, { PureComponent} from "react";
import { SegmentFrame } from './SegmentFrame';
import { FilterOperandEnum, FilterOperand, FilterOperator, EventFilter, EventFilterOperatorUtil, LiteralOp, SimpleAttributeOp, QualifiedName, OpcUaBrowseResults, OpcUaNodeInfo, NodeClass, NodePath, BrowseFilter } from '../types'; 
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
    typeId: NodePath,
};


export class AddEventFilter extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = {
            oper: FilterOperator.GreaterThan,
            browsePath: [],
            typeId: {
                browsePath: [], node:
                {
                    browseName: { name: "", namespaceUrl: "" }, displayName: "", nodeClass: -1, nodeId: ""
                }
            },
            value: "500"
        };
        this.changeOperator = this.changeOperator.bind(this);
    }

    addFilter() {
        if (this.state.browsePath.length > 0 && this.state.typeId.node.nodeId.trim() !== "") {
            let attr: SimpleAttributeOp = { attributeId: 13, typeId: "", browsePath: this.state.browsePath.map(bp => copyQualifiedName(bp)) };
            let literal: LiteralOp = {
                typeId: this.state.typeId.node.nodeId, value: this.state.value
            };
            let operands: FilterOperand[] = [{ type: FilterOperandEnum.SimpleAttribute, value: attr }, { type: FilterOperandEnum.Literal, value: literal }];
            var evFilter: EventFilter = { oper: this.state.oper, operands: operands.slice() };
            this.props.add(evFilter);
        }
    }


    changeOperator(event: { target: any; }) {
        const target = event.target;
        const value = target.value as FilterOperator;
        this.setState({ oper: value });
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
            <SegmentFrame label="Operator" marginLeft >
                <select onSelect={this.changeOperator}>
                    {
                        EventFilterOperatorUtil.operNames.map((n, idx) =>
                        {
                            return (<option value={idx}>{n}</option>);
                        })
                    }
                </select>
                </SegmentFrame>
            );
    }

    renderOperandsBeforeOperator(oper: FilterOperator) {
        switch (oper) {
            case FilterOperator.GreaterThan:
            case FilterOperator.GreaterThanOrEqual:
            case FilterOperator.LessThan:
            case FilterOperator.LessThanOrEqual:
            case FilterOperator.Equals:
                return (
                    <SegmentFrame label="Event Field" marginLeft >
                        <BrowsePathEditor
                            browsePath={this.state.browsePath}
                            rootNodeId={this.props.eventTypeNodeId}
                            onChangeBrowsePath={(bp) => this.setState({ browsePath: bp })}
                            browse={(nodeId) => this.browseEventFields(nodeId)}> </BrowsePathEditor>
                    </SegmentFrame>
                    );
        }
        return <></>;

    }

    renderOperandsAfterOperator(oper: FilterOperator) {
        switch (oper) {
            case FilterOperator.GreaterThan:
            case FilterOperator.GreaterThanOrEqual:
            case FilterOperator.LessThan:
            case FilterOperator.LessThanOrEqual:
            case FilterOperator.Equals:
                return (<>
                    <SegmentFrame label="Value Type" marginLeft >
                        <NodeEditor browse={(node, filter) => this.browseDataTypes(node, filter)}
                            readNode={(nodeid) => this.readNode(nodeid)}
                            onChangeNode={(node) => this.onChangeValueTypeNode(node)}
                            rootNodeId="i=24"
                            placeholder="Value type node"
                            node={this.state.typeId}
                        />
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

    onChangeValueTypeNode(node: NodePath): void {
        this.setState({ typeId: node });
    }

    browseDataTypes(nodeId: string, browseFilter: BrowseFilter): Promise<OpcUaBrowseResults[]> {
        return this.props.datasource
            .getResource('browse', { nodeId: nodeId, nodeClassMask: NodeClass.DataType });
    }

    readNode(nodeId: string): Promise<OpcUaNodeInfo> {
        return this.props.datasource
            .getResource('readNode', { nodeId: nodeId});
    }


    browseEventFields(nodeId: string): Promise<OpcUaBrowseResults[]> {
        return this.props.datasource
            .getResource('browse', { nodeId: nodeId  });
    }

    render() {
        return (
            <div>
                <br />
                {this.renderOperandsBeforeOperator(this.state.oper)}
                {this.renderDropdown()}
                {this.renderOperandsAfterOperator(this.state.oper)}
                <Button onClick={() => this.addFilter()}>Add Filter</Button>
            </div>
        );
    }
}