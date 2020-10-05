import React, { PureComponent } from "react";
import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
import { Paper } from '@material-ui/core';
import { EventColumn, QualifiedName, OpcUaBrowseResults } from './../types';
import { ThemeGetter } from './ThemesGetter';
import { GrafanaTheme } from '@grafana/data';
import { BrowsePathEditor } from './BrowsePathEditor';
import { DataSource } from '../DataSource';
import { Button, Input } from '@grafana/ui';
import { copyEventColumn } from '../utils/EventColumn';


type Props = {
    datasource: DataSource,
    eventTypeNodeId: string,
    eventColumns: EventColumn[],
    onChangeAlias(alias: string, idx: number): void,
    onChangeBrowsePath(browsePath: QualifiedName[], idx: number): void,
    ondelete(idx: number): void;
    onAddColumn(newCol: EventColumn): void;
};


type State = {
    theme: GrafanaTheme | null,
    new: EventColumn,
}

export class EventFieldTable extends PureComponent < Props, State > {
    constructor(props: Props) {
        super(props);
        this.state = {
            theme: null,
            new: {
                alias: "", browsePath: []
            }
        };
    }

    onTheme = (theme: GrafanaTheme) => {
        if (this.state.theme == null && theme != null) {
            this.setState({ theme: theme });
        }
    }


    render() {
        let bg: string = "";
        let txt: string = "";
        let bgBlue: string = "";
        if (this.state.theme != null) {
            bg = this.state.theme.colors.bg2;
            txt = this.state.theme.colors.text;
            bgBlue = this.state.theme.colors.bgBlue1;
        }

        if (typeof this.props.eventTypeNodeId === 'undefined' || this.props.eventTypeNodeId === "")
            return (<></>);

        return (
            <div className="panel-container" style={{ width: '100' }}>
                <ThemeGetter onTheme={this.onTheme} />
                <Paper>
                    <Table>
                        <TableHead style={{ backgroundColor: bg, color: txt, }}>
                            <TableRow style={{ height: 20 }}>
                                <TableCell style={{ color: txt, border: 0, padding: 0 }}>Browse Path</TableCell>
                                <TableCell style={{ color: txt, border: 0, padding: 0 }} align="right">Alias</TableCell>
                                <TableCell style={{ color: txt, border: 0, padding: 0 }} align="right"></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody style={{ backgroundColor: bg, color: txt, }}>
                            {this.props.eventColumns.map((row: EventColumn, index: number) => (
                                <TableRow style={{ height: 14 }} key={index}>
                                    <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                                        <BrowsePathEditor browse={this.browse} browsePath={row.browsePath} 
                                            onChangeBrowsePath={(browsePath) => this.props.onChangeBrowsePath(browsePath, index)} rootNodeId={this.props.eventTypeNodeId} ></BrowsePathEditor>
                                    </TableCell>
                                    <TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
                                        <Input value={row.alias} onChange={e => this.props.onChangeAlias(e.currentTarget.value, index)}></Input>
                                    </TableCell>
                                    <TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
                                        <Button style={{ backgroundColor: bgBlue }} onClick={() => this.props.ondelete(index)}>Delete</Button></TableCell>
                                </TableRow>
                            ))}
                            <TableRow style={{ height: 14 }}>
                                <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                                    <BrowsePathEditor browse={this.browse} browsePath={this.state.new.browsePath}
                                        onChangeBrowsePath={(browsePath) => this.onChangeNewBrowsePath(browsePath)} rootNodeId={this.props.eventTypeNodeId} ></BrowsePathEditor>
                                </TableCell>
                                <TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
                                    <Input value={this.state.new.alias} onChange={e => this.onChangeNewAlias(e.currentTarget.value)}></Input>
                                </TableCell>
                                <TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
                                    <Button style={{ backgroundColor: bgBlue }} onClick={this.onAddColumn}>Add</Button></TableCell>
                            </TableRow>
                            
                        </TableBody>
                    </Table>
                </Paper>
            </div>
        );
    }

    onChangeNewAlias(alias: string): void {
        this.setState({ new: { alias: alias, browsePath: this.state.new.browsePath } });
    }

    onAddColumn = () => {
        if (this.state.new.browsePath != null && this.state.new.browsePath.length > 0) {
            this.props.onAddColumn(copyEventColumn(this.state.new));
            this.setState({ new: { alias: "", browsePath: [] } });
        }
    }


    onChangeNewBrowsePath = (browsePath: QualifiedName[]) => {
        this.setState({ new: { alias: this.state.new.alias, browsePath: browsePath } });
    }

    browse = (nodeId: string): Promise<OpcUaBrowseResults[]> => {
        return this.props.datasource
            .getResource('browse', { nodeId: nodeId });
    };
}