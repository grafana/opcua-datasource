import React, { PureComponent } from 'react';

import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { Paper } from '@material-ui/core';
import { EventColumn, QualifiedName, OpcUaBrowseResults } from './../types';
import { GrafanaTheme } from '@grafana/data';
import { BrowsePathEditor } from './BrowsePathEditor';
import { DataSource } from '../DataSource';
import { Input } from '@grafana/ui';
import { copyEventColumn } from '../utils/EventColumn';
import { renderOverlay } from '../utils/Overlay';
import { FaChevronDown, FaChevronUp, FaPlusSquare } from 'react-icons/fa';

type Props = {
  datasource: DataSource;
  eventTypeNodeId: string;
  eventFields: EventColumn[];
  onChangeAlias(alias: string, idx: number): void;
  onChangeBrowsePath(browsePath: QualifiedName[], idx: number): void;
  getNamespaceIndices(): Promise<string[]>;
  deleteField(idx: number): void;
  addField(newCol: EventColumn): void;
  moveFieldUp(index: number): void;
  moveFieldDown(index: number): void;
  theme: GrafanaTheme | null;
};

type State = {
  new: EventColumn;
  browserOpened: string | null;
};

const imageHeight = 20;
const imageWidth = 20;

export class EventFieldTable extends PureComponent<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      new: {
        alias: '',
        browsePath: [],
      },
      browserOpened: null,
    };
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

    if (typeof this.props.eventTypeNodeId === 'undefined' || this.props.eventTypeNodeId === '') {
      return <></>;
    }

    return (
      <div className="panel-container" style={{ width: '100' }}>
        {renderOverlay(
          bg,
          () => this.state.browserOpened !== null,
          () => this.closeBrowser()
        )}
        <Paper>
          <Table>
            <TableHead style={{ backgroundColor: bg, color: txt }}>
              <TableRow style={{ height: 20 }}>
                <TableCell style={{ color: txt, border: 0, padding: 0, fontSize: 20 }}>Browse Path</TableCell>
                <TableCell style={{ color: txt, border: 0, padding: 0, fontSize: 20 }}>Alias</TableCell>
                <TableCell style={{ color: txt, border: 0, padding: 0 }}></TableCell>
              </TableRow>
            </TableHead>
            <TableBody style={{ backgroundColor: bg, color: txt }}>
              {this.props.eventFields.map((row: EventColumn, index: number) => (
                <TableRow style={{ height: imageHeight }} key={index}>
                  <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                    <BrowsePathEditor
                      getNamespaceIndices={() => this.props.getNamespaceIndices()}
                      theme={this.props.theme}
                      id={index.toString()}
                      closeBrowser={(id: string) => this.closeBrowser()}
                      isBrowserOpen={(id: string) => this.state.browserOpened === id}
                      openBrowser={(id: string) => this.openBrowser(id)}
                      browse={this.browse}
                      browsePath={row.browsePath}
                      onChangeBrowsePath={(browsePath) => this.props.onChangeBrowsePath(browsePath, index)}
                      rootNodeId={this.props.eventTypeNodeId}
                    ></BrowsePathEditor>
                  </TableCell>
                  <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                    <Input
                      value={row.alias}
                      onChange={(e) => this.props.onChangeAlias(e.currentTarget.value, index)}
                    ></Input>
                  </TableCell>
                  <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                    {this.renderTrashBin(index)}
                    {this.renderMoveUpDown(index, this.props.eventFields.length, txt, bgBlue)}
                  </TableCell>
                </TableRow>
              ))}
              <TableRow style={{ height: imageHeight }}>
                <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                  <BrowsePathEditor
                    theme={this.props.theme}
                    id={'new'}
                    getNamespaceIndices={() => this.props.getNamespaceIndices()}
                    closeBrowser={(id: string) => this.closeBrowser()}
                    isBrowserOpen={(id: string) => this.state.browserOpened === id}
                    openBrowser={(id: string) => this.openBrowser(id)}
                    browse={this.browse}
                    browsePath={this.state.new.browsePath}
                    onChangeBrowsePath={(browsePath) => this.onChangeNewBrowsePath(browsePath)}
                    rootNodeId={this.props.eventTypeNodeId}
                  ></BrowsePathEditor>
                </TableCell>
                <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                  <Input
                    value={this.state.new.alias}
                    onChange={(e) => this.onChangeNewAlias(e.currentTarget.value)}
                  ></Input>
                </TableCell>
                <TableCell style={{ color: txt, border: 0, padding: 0 }}>
                  <div
                    style={{ display: 'inline-block', marginLeft: 5, cursor: 'pointer' }}
                    onClick={(e) => this.onAddField()}
                  >
                    <FaPlusSquare fill="currentColor" size={imageHeight} />
                  </div>
                </TableCell>
              </TableRow>
            </TableBody>
          </Table>
        </Paper>
      </div>
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
          onClick={() => this.props.deleteField(index)}
        >
          <path d="M10,18a1,1,0,0,0,1-1V11a1,1,0,0,0-2,0v6A1,1,0,0,0,10,18ZM20,6H16V5a3,3,0,0,0-3-3H11A3,3,0,0,0,8,5V6H4A1,1,0,0,0,4,8H5V19a3,3,0,0,0,3,3h8a3,3,0,0,0,3-3V8h1a1,1,0,0,0,0-2ZM10,5a1,1,0,0,1,1-1h2a1,1,0,0,1,1,1V6H10Zm7,14a1,1,0,0,1-1,1H8a1,1,0,0,1-1-1V8H17Zm-3-1a1,1,0,0,0,1-1V11a1,1,0,0,0-2,0v6A1,1,0,0,0,14,18Z"></path>
        </svg>
      </div>
    );
  }

  renderMoveUpDown(index: number, size: number, txtColor: string, bgColor: string) {
    if (index === 0 && index === size) {
      return <></>;
    } else if (index === 0 && index < size - 1) {
      return (
        <div
          style={{ display: 'inline-block', marginLeft: 5, cursor: 'pointer' }}
          onClick={(e) => this.props.moveFieldDown(index)}
        >
          <FaChevronDown fill="currentColor" size={imageHeight} />
        </div>
      );
    } else if (index > 0 && index < size - 1) {
      return (
        <>
          <div
            style={{ display: 'inline-block', marginLeft: 5, cursor: 'pointer' }}
            onClick={(e) => this.props.moveFieldDown(index)}
          >
            <FaChevronDown fill="currentColor" size={imageHeight} />
          </div>
          <div
            style={{ display: 'inline-block', marginLeft: 5, cursor: 'pointer' }}
            onClick={(e) => this.props.moveFieldUp(index)}
          >
            <FaChevronUp fill="currentColor" size={imageHeight} />
          </div>
        </>
      );
    } else if (index > 0 && index === size - 1) {
      return (
        <div
          style={{ display: 'inline-block', marginLeft: 5, cursor: 'pointer' }}
          onClick={(e) => this.props.moveFieldUp(index)}
        >
          <FaChevronUp fill="currentColor" size={imageHeight} />
        </div>
      );
    }
    return <></>;
  }

  openBrowser(id: string): void {
    this.setState({ browserOpened: id });
  }

  closeBrowser(): void {
    this.setState({ browserOpened: null });
  }

  onChangeNewAlias(alias: string): void {
    this.setState({ new: { alias: alias, browsePath: this.state.new.browsePath } });
  }

  onAddField = () => {
    if (this.state.new.browsePath != null && this.state.new.browsePath.length > 0) {
      this.props.addField(copyEventColumn(this.state.new));
      this.setState({ new: { alias: '', browsePath: [] } });
    }
  };

  onChangeNewBrowsePath = (browsePath: QualifiedName[]) => {
    this.setState({ new: { alias: this.state.new.alias, browsePath: browsePath } });
  };

  browse = (nodeId: string): Promise<OpcUaBrowseResults[]> => {
    return this.props.datasource.getResource('browseEventFields', { nodeId: nodeId });
  };
}
