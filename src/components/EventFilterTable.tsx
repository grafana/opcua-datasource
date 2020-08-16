import React, { PureComponent } from 'react';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
//import TableHead from "@material-ui/core/TableHead";
import TableRow from '@material-ui/core/TableRow';
//import { withStyles, makeStyles } from '@material-ui/core/styles';
import { Paper } from '@material-ui/core';
import { FilterOperator, EventFilter, EventFilterOperatorUtil } from '../types';

//import { SegmentFrame } from './SegmentFrame';

//const useStyles = makeStyles({
//    table: {
//        minWidth: 650,
//        color: "white"
//    },
//});

export interface Props {
  rows: EventFilter[];
  ondelete(idx: number): void;
}

type State = {};

export class EventFilterTable extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
  }

  renderCompareOperatorRow(row: EventFilter, index: number) {
    return (
      <TableRow style={{ height: 14 }} key={index}>
        <TableCell style={{ color: 'white', border: 0, padding: 0 }}> {row.operands[0]} </TableCell>
        <TableCell align="right" style={{ color: 'white', border: 0, padding: 0 }}>
          {EventFilterOperatorUtil.GetString(row.oper)}
        </TableCell>
        <TableCell style={{ color: 'white', border: 0, padding: 0 }}> {row.operands[1]} </TableCell>
        <TableCell>
          <button style={{ backgroundColor: 'gray' }} onClick={() => this.props.ondelete(index)}>
            Delete
          </button>
        </TableCell>
      </TableRow>
    );
  }

  renderDefaultRow(row: EventFilter, index: number) {
    return (
      <TableRow style={{ height: 14 }} key={index}>
        <TableCell align="right" style={{ color: 'white', border: 0, padding: 0 }}>
          {row.oper}
        </TableCell>
        {row.operands.map((oper, idx) => (
          <TableCell style={{ color: 'white', border: 0, padding: 0 }}> {oper[idx]} </TableCell>
        ))}
      </TableRow>
    );
  }

  //<TableHead style={{ backgroundColor: 'black', color: 'white', }}>
  //    <TableRow style={{ height: 20}}>
  //        <TableCell style={{ color: 'white', border: 0, padding: 0}}>Browse Name</TableCell>
  //        <TableCell style={{ color: 'white', border: 0, padding: 0 }} align="right">Alias</TableCell>
  //        <TableCell style={{ color: 'white', border: 0, padding: 0 }} align="right"></TableCell>
  //    </TableRow>
  //</TableHead>

  render() {
    return (
      <div className="panel-container" style={{ width: '100' }}>
        <Paper>
          <Table>
            <TableBody style={{ backgroundColor: 'black', color: 'white' }}>
              {this.props.rows.map((row, index) => {
                switch (row.oper) {
                  case FilterOperator.GreaterThan:
                  case FilterOperator.GreaterThanOrEqual:
                  case FilterOperator.LessThan:
                  case FilterOperator.LessThanOrEqual:
                  case FilterOperator.Equals:
                    return this.renderCompareOperatorRow(row, index);
                }
                return this.renderDefaultRow(row, index);
              })}
            </TableBody>
          </Table>
        </Paper>
      </div>
    );
  }
}
