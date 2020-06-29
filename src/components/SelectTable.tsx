import React from "react";
//import PropTypes from "prop-types";
//import { withStyles } from "@material-ui/core/styles";
import Table from "@material-ui/core/Table";
import TableBody from "@material-ui/core/TableBody";
import TableCell from "@material-ui/core/TableCell";
import TableHead from "@material-ui/core/TableHead";
import TableRow from "@material-ui/core/TableRow";
import { SegmentFrame } from './SegmentFrame';

export interface EventFieldsProps {
    rows: Array<EventField>;
    ondelete: Function;
}

export interface EventField
{
    browsename: string;
    alias: string;
}

export function SelectTable({ rows, ondelete} : EventFieldsProps) {
    return (
        <SegmentFrame label = "Event Fields">

      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Field</TableCell>
            <TableCell>Alias</TableCell>
           <TableCell>Delete</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
            {rows.map((row, idx) => (
            <TableRow key={row.browsename}>
              <TableCell>{row.browsename}</TableCell>
              <TableCell>{row.alias}</TableCell>
              <TableCell><button onClick = {() => ondelete(idx)}>Delete</button></TableCell>
            </TableRow>
          ))}
        </TableBody>
          </Table>
        </SegmentFrame>
  );
}