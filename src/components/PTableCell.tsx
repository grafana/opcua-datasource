//import React, { FC } from 'react';
//import { Cell } from 'react-table';
////import { config } from '@grafana/runtime'
////import { TableFilterActionCallback } from '@grafana/runtime/types';

////import { getTextAlign } from '@grafana/ui';
////import { TableFilterActionCallback } from '@grafana/ui/types';
////import {  useStyles } from '@grafana/ui/themes';
////import { GrafanaTheme } from '@grafana/data';
////import { TableStyles } from '@grafana/ui/components/Table/styles';
//import { ColumnData, CellData } from './PTable';

//export interface Props {
//    cell: Cell;
//    column: PColumn;
//    value: PCell;
//  //tableStyles: TableStyles;
//  onCellClick?: Function;
//}

//export const PTableCell: FC<Props> = ({ cell, column, /*tableStyles,*/ onCellClick }) => {
//    const cellProps = cell.getCellProps();

//    //var style = useStyles((theme?: GrafanaTheme) => {
//    //    if (!theme) {
//    //        theme = {} as GrafanaTheme;
//    //    }
//    //});

//  let onClick: ((event: React.SyntheticEvent) => void) | undefined = undefined;

//  if (onCellClick) {
//    if (cellProps.style) {
//      cellProps.style.cursor = 'pointer';
//    }

//    onClick = () => onCellClick(cell.column.Header as string, cell.value);
//  }

//  //if (cellProps.style) {
//  //    cellProps.style.textAlign = getTextAlign(field);
//  //}

//  return (
//    <div {...cellProps} onClick={onClick} /*className={tableStyles.tableCellWrapper}>*/>
//      {cell.render('Cell', { column/*, tableStyles*/ })}
//    </div>
//  );
//};
