//import React, { PureComponent } from 'react';
////import { PTableCell, Props as TableCellProps } from './PTableCell';
////import { Icon, CustomScrollbar } from '@grafana/ui';

//export interface ColumnData {
//    header: string;
//}

//export interface CellData {
//    value: any;
//}

//export interface RowData {
//    data: Array<CellData>;
//}

//export interface TableData {
//    columns: ColumnData[],
//    rows: RowData[]
//}

//export interface Props {
//    tableData: TableData;
//    width: number;
//    height: number;
//    /** Minimal column width specified in pixels */
//    //columnMinWidth?: number;
//    //noHeader?: boolean;
//    //resizable?: boolean;
//    //onCellClick?: TableFilterActionCallback;
//}

//type State = {
//    tableData: TableData;
//}

//export class PTable extends PureComponent<Props, State> {
//    constructor(props: Props) {
//        super(props);
//        this.state = { tableData: props.tableData };
//    }

//    renderHeaderCell = (column: ColumnData, /*tableStyles: TableStyles,*/ index: number) => {
//        //headerProps.style.position = 'absolute';
//        //headerProps.style.textAlign = "center";

//        return (
//            <>
//            <div /*className={tableStyles.headerCell}*/ >
//                {column.header}
//                </div>
//                </>
//        );
//    }

//    renderHeaderRow = (columns: ColumnData[]) => {
//        var res = columns.map((column: ColumnData, index: number) => {
//            (<div>{this.renderHeaderCell(column, /*tableStyles,*/ index)} </div>);
//        });
//        return res;
//    }

//    renderRow = (columns: ColumnData[], row: RowData) => {
//        return row.data.map((cell: CellData, index: number) => {
//            this.renderCell(columns[index], cell);
//        });
//    }

//    renderCell = (column: ColumnData, cell: CellData) => {
//        return <div> {cell.value}</div>;
//    }

//    render() {
//      return (
//        <div>
//              <div>
//                  {this.renderHeaderRow(this.props.tableData.columns)}
//                </div>

//              {this.props.tableData.rows.map((row: RowData, index: number) => {
//                  this.renderRow(this.props.tableData.columns, row);
//              })}
//            </div>
//        );
//    }

////function getCellComponent(column: PColumn) {
////    return pwithTableStyles(DefaultCell, getTextColorStyle);
////}

////function getTextColorStyle(props: TableCellProps) {
////    const { /*column, cell,*/ tableStyles } = props;

////    if (!column.display) {
////        return tableStyles;
////    }

////    const displayValue = column.display(cell.value);
////    if (!displayValue.color) {
////        return tableStyles;
////    }

////    const extendedStyle = css`
////    color: ${displayValue.color};
////  `;

////    return {
////        ...tableStyles,
////        tableCell: cx(tableStyles.tableCell, extendedStyle),
////    };
////}

////function getColumns(pcolumns: PColumn[]): Column[] {
////    const columns: any[] = [];

////    for (const [fieldIndex/*, column*/] of pcolumns.entries()) {
////        //const Cell = getCellComponent(column);
////        columns.push({

////            id: fieldIndex.toString(),
////            Header: pcolumns[fieldIndex],
////            //accessor: (row: any, i: number) => {
////            //    return field.values.get(i);
////            //},
////            //sortType: selectSortType(field.type),
////            //width: fieldTableOptions.width,
////            minWidth: 50,
////        });
////    }

////    // divide up the rest of the space
////    //const sharedWidth = availableWidth / fieldCountWithoutWidth;
////    //for (const column of columns) {
////    //    if (!column.width) {
////    //        column.width = Math.max(sharedWidth, columnMinWidth);
////    //    }
////    //}

////    return columns;
////}

//}
