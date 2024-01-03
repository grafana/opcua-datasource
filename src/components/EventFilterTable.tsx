import React, { PureComponent } from 'react';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
//import TableHead from "@material-ui/core/TableHead";
import TableRow from '@material-ui/core/TableRow';
//import { withStyles, makeStyles } from '@material-ui/core/styles';
import { Paper, TableHead } from '@material-ui/core';
import { FilterOperator, EventFilter } from '../types';
import { GrafanaTheme } from '@grafana/data';
import { EventFilterOperatorUtil } from '../utils/Operands';

export interface Props {
  rows: EventFilter[];
  theme: GrafanaTheme | null;
  onDelete(idx: number): void;
  getNamespaceIndices(): Promise<string[]>;
}

type State = {
  nsTable: string[];
  nsTableFetched: boolean;
};

const imageHeight = 20;
const imageWidth = 20;

export class EventFilterTable extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { nsTable: [], nsTableFetched: false };
  }

  renderCompareOperatorRow(row: EventFilter, index: number, bg: string, txt: string) {
    return (
      <TableRow style={{ height: 14 }} key={index}>
        <TableCell style={{ color: txt, border: 0, padding: 0 }}>
          {EventFilterOperatorUtil.GetOperandString(row.operands[0], this.state.nsTable)}
        </TableCell>
        <TableCell style={{ color: txt, border: 0, padding: 0 }}>
          {EventFilterOperatorUtil.GetString(row.operator)}
        </TableCell>
        <TableCell style={{ color: txt, border: 0, padding: 0 }}>
          {EventFilterOperatorUtil.GetOperandString(row.operands[1], this.state.nsTable)}
        </TableCell>
        <TableCell style={{ color: txt, border: 0, padding: 0 }}>{this.renderTrashBin(index)}</TableCell>
      </TableRow>
    );
  }

  renderTrashBin(index: number) {
    // Courtesy of OG.
    return (
      <div style={{ display: 'inline-block', marginLeft: '5', cursor: 'pointer' }}>
        <svg
          xmlns="http://www.w3.org/2000/svg"
          width={imageWidth}
          height={imageHeight}
          viewBox="0 0 24 24"
          fill="currentColor"
          onClick={() => this.props.onDelete(index)}
        >
          <path d="M10,18a1,1,0,0,0,1-1V11a1,1,0,0,0-2,0v6A1,1,0,0,0,10,18ZM20,6H16V5a3,3,0,0,0-3-3H11A3,3,0,0,0,8,5V6H4A1,1,0,0,0,4,8H5V19a3,3,0,0,0,3,3h8a3,3,0,0,0,3-3V8h1a1,1,0,0,0,0-2ZM10,5a1,1,0,0,1,1-1h2a1,1,0,0,1,1,1V6H10Zm7,14a1,1,0,0,1-1,1H8a1,1,0,0,1-1-1V8H17Zm-3-1a1,1,0,0,0,1-1V11a1,1,0,0,0-2,0v6A1,1,0,0,0,14,18Z"></path>
        </svg>
      </div>
    );
  }

  renderDefaultRow(row: EventFilter, index: number, txt: string) {
    return (
      <TableRow style={{ height: 14 }} key={index}>
        <TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
          {row.operator}
        </TableCell>
        {row.operands.map((operand, idx) => (
          <TableCell key={`EventFilterTable-${idx}`} style={{ color: txt, border: 0, padding: 0 }}>
            {' '}
            {EventFilterOperatorUtil.GetOperandString(operand, this.state.nsTable)}{' '}
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
    if (!this.state.nsTableFetched) {
      this.props.getNamespaceIndices().then((nsTable) => this.setState({ nsTable: nsTable, nsTableFetched: true }));
      return <></>;
    }

    return (
      <div className="panel-container" style={{ width: '100' }}>
        <Paper>
          <Table>
            <TableHead style={{ backgroundColor: bg, color: txt }}>
              <TableRow style={{ height: 20 }}>
                <TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: 'nowrap' }}>Operand</TableCell>
                <TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: 'nowrap' }}>Operator</TableCell>
                <TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: 'nowrap' }}>Operand</TableCell>
                <TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: 'nowrap' }}></TableCell>
              </TableRow>
            </TableHead>
            <TableBody style={{ backgroundColor: bg, color: txt }}>
              {this.props.rows.map((row, index) => {
                switch (row.operator) {
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
