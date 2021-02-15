import React, { PureComponent } from 'react';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
//import TableHead from "@material-ui/core/TableHead";
import TableRow from '@material-ui/core/TableRow';
//import { withStyles, makeStyles } from '@material-ui/core/styles';
import { Paper } from '@material-ui/core';
import { FilterOperator, EventFilter } from '../types';
import { GrafanaTheme } from '@grafana/data';
import { Button } from '@grafana/ui';
import { EventFilterOperatorUtil } from '../utils/Operands';

export interface Props {
    rows: EventFilter[];
    theme: GrafanaTheme | null;
    ondelete(idx: number): void;
    getNamespaceIndices(): Promise<string[]>;
}

type State = {
    nsTable: string[],
    nsTableFetched: boolean
};

export class EventFilterTable extends PureComponent<Props, State> {
  constructor(props: Props) {
      super(props);
      this.state = { nsTable: [], nsTableFetched: false };
  }


  renderCompareOperatorRow(row: EventFilter, index: number, bg: string, txt: string) {
    return (
      <TableRow style={{ height: 14 }} key={index}>
        <TableCell style={{ color: txt, border: 0, padding: 0 }}>
          {' '}
                {EventFilterOperatorUtil.GetOperandString(row.operands[0], this.state.nsTable)}{' '}
        </TableCell>
        <TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
          {EventFilterOperatorUtil.GetString(row.oper)}
        </TableCell>
        <TableCell style={{ color: txt, border: 0, padding: 0 }}>
          {' '}
                {EventFilterOperatorUtil.GetOperandString(row.operands[1], this.state.nsTable)}{' '}
        </TableCell>
        <TableCell>
          <Button style={{ backgroundColor: bg }} onClick={() => this.props.ondelete(index)}>
            Delete
          </Button>
        </TableCell>
      </TableRow>
    );
  }

  renderDefaultRow(row: EventFilter, index: number, txt: string) {
    return (
      <TableRow style={{ height: 14 }} key={index}>
        <TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
          {row.oper}
        </TableCell>
        {row.operands.map((oper, idx) => (
          <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                {' '}
                {EventFilterOperatorUtil.GetOperandString(oper, this.state.nsTable)}{' '}
          </TableCell>
        ))}
      </TableRow>
    );
  }


  render() {
    let bg = '';
    let txt = '';
    let bgBlue = '';
    if (this.props.theme != null) {
        bg = this.props.theme.colors.bg2;
        txt = this.props.theme.colors.text;
        bgBlue = this.props.theme.colors.bgBlue1;
      }
      if (!this.state.nsTableFetched)
      {
          this.props.getNamespaceIndices().then(nsTable => this.setState({ nsTable: nsTable, nsTableFetched: true }));
          return <></>;
      }
      
    return (
      <div className="panel-container" style={{ width: '100' }}>
        <Paper>
          <Table>
            <TableBody style={{ backgroundColor: bg, color: txt }}>
              {this.props.rows.map((row, index) => {
                switch (row.oper) {
                  case FilterOperator.GreaterThan:
                  case FilterOperator.GreaterThanOrEqual:
                  case FilterOperator.LessThan:
                  case FilterOperator.LessThanOrEqual:
                  case FilterOperator.Equals:
                    return this.renderCompareOperatorRow(row, index, bgBlue, txt);
                }
                return this.renderDefaultRow(row, index, txt);
              })}
            </TableBody>
          </Table>
        </Paper>
      </div>
    );
  }
}
