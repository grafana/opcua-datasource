import React, { PureComponent } from "react";
//import { PTable, TableData/*, RowData, ColumnData*/ } from './PTable';
//import PropTypes from "prop-types";
//import { withStyles } from "@material-ui/core/styles";
//import Paper from "@material-ui/core/Paper";
import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
//import { withStyles, makeStyles } from '@material-ui/core/styles';
import { Paper } from '@material-ui/core';
import { EventColumn } from './../types';
//import { SegmentFrame } from './SegmentFrame';

//const useStyles = makeStyles({
//    table: {
//        minWidth: 650,
//        color: "white"
//    },
//});

export interface EventFieldsProps {
    rows: EventColumn[];
    ondelete(idx: number): void;
}

//export interface EventField
//{
//    browsename: string;
//    alias: string;
//}

type State = {
}

export class EventFieldTable extends PureComponent < EventFieldsProps, State > {
    constructor(props: EventFieldsProps) {
        super(props);
    }

    render() {
        return (
            <div className="panel-container" style={{ width: '100' }}>
                <Paper>
                    <Table>
                        <TableHead style={{ backgroundColor: 'black', color: 'white', }}>
                            <TableRow style={{ height: 20 }}>
                                <TableCell style={{ color: 'white', border: 0, padding: 0 }}>Namespace Url</TableCell>
                                <TableCell style={{ color: 'white', border: 0, padding: 0}}>Browse Name</TableCell>
                                <TableCell style={{ color: 'white', border: 0, padding: 0 }} align="right">Alias</TableCell>
                                <TableCell style={{ color: 'white', border: 0, padding: 0 }} align="right"></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody style={{ backgroundColor: 'black', color: 'white', }}>
                            {this.props.rows.map((row, index) => (
                                <TableRow style={{ height: 14 }} key={index}>
                                    <TableCell style={{ color: 'white', border: 0, padding: 0 }}>
                                        {row.browsename.namespaceUrl}
                                    </TableCell>

                                    <TableCell style={{ color: 'white', border: 0, padding: 0 }}>
                                        {row.browsename.name}
                                    </TableCell>
                                    <TableCell align="right" style={{ color: 'white', border: 0, padding: 0}}>{row.alias}</TableCell>
                                    <TableCell align="right" style={{ color: 'white', border: 0, padding: 0 }}>
                                        <button style={{ backgroundColor: 'gray' }} onClick={() => this.props.ondelete(index)}>Delete</button></TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </Paper>
            </div>
        );
    }
}