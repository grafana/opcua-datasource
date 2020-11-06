import React, { Component } from "react";
import { convertRemToPixels } from '../utils/ConvertRemToPixels';
import { OpcUaBrowseResults, QualifiedName, BrowseFilter } from '../types';
import { ThemeGetter } from './ThemesGetter';
import { GrafanaTheme } from '@grafana/data';
import { BrowserTree } from './BrowserTree';
import { BrowserTable } from './BrowserTable';
import { Checkbox } from '@grafana/ui';
import {
	FaWindowClose

} from "react-icons/fa";
type Props = {
	browse: (nodeId: string, browseFilter: BrowseFilter) => Promise<OpcUaBrowseResults[]>;
	rootNodeId: OpcUaBrowseResults,
	ignoreRootNode: boolean,
	closeOnSelect: boolean,
	onNodeSelectedChanged: (nodeId: OpcUaBrowseResults, browsePath: QualifiedName[]) => void;
	closeBrowser: () => void;
};

type State = {
	table: boolean,
	theme: GrafanaTheme | null,
}



/**
 * Displays nodes in a tree and allows users to select an entity for display.
 */
export class BrowserDialog extends Component<Props, State> {
	/**
	 *
	 * @param {*} props sets the data structure
	 */
	constructor(props: Props) {
		super(props);
		this.state = { table: false, theme: null };

	}

	onTheme = (theme: GrafanaTheme) => {
		if (this.state.theme == null && theme != null) {
			this.setState({ theme: theme });
		}
	}



	/**
	 * Renders the component.
	 */
	render() {

		let bg: string = "";
		if (this.state.theme != null) {
			bg = this.state.theme.colors.bg2;
		}


		return (


			<div style={{
				background: bg
			}}
			>
				<ThemeGetter onTheme={this.onTheme} />
				<span
					data-id="Treeview-CloseSpan"
					onClick={() => this.handleClose()}
					onMouseOver={this.handleHoverClose}
					onMouseOut={this.handleUnhoverClose}
					//ref={(spn) => (this.closeSpan = spn)}
					style={{
						//border: this.props.theme.colors.border1,
						cursor: "pointer",
						padding: convertRemToPixels("0.5rem"),
						float: "right"
					}}
				>
					<FaWindowClose />
				</span>
				<div style={{ height: "18px" }}>  </div>
				<div style={{ whiteSpace: 'nowrap' }}>
					<Checkbox label="Tree" checked={!this.state.table} onChange={(e) => this.changeBrowser(false)}></Checkbox>
					<Checkbox label="Table" checked={this.state.table} onChange={(e) => this.changeBrowser(true)}></Checkbox>
				</div>
				<div
					data-id="Treeview-ScrollDiv"
					style={{
						height: convertRemToPixels("20rem"),
						overflowX: "hidden",
						overflowY: "auto",
					}}
				>
					{this.renderBrowser()}
				</div>
			</div>
		);
	}

	changeBrowser(useTable: boolean): void {
		this.setState({ table: useTable });
    }


	renderTree() {
		return <BrowserTree closeBrowser={() => this.props.closeBrowser()} closeOnSelect={this.props.closeOnSelect}
			browse={(nodeId) => this.props.browse(nodeId, {
				browseName: "", maxResults: 0})}
			ignoreRootNode={true} rootNodeId={this.props.rootNodeId}
			onNodeSelectedChanged={(node, browsepath) => { this.props.onNodeSelectedChanged(node, browsepath) }}></BrowserTree>;
	}

	renderTable() {
		return <BrowserTable closeBrowser={() => this.props.closeBrowser()} closeOnSelect={this.props.closeOnSelect}
			browse={(nodeId, filter) => this.props.browse(nodeId, filter)}
			ignoreRootNode={true} rootNodeId={this.props.rootNodeId}
			onNodeSelectedChanged={(node, browsepath) => { this.props.onNodeSelectedChanged(node, browsepath) }}></BrowserTable>;
	}

	renderBrowser(): React.ReactNode {

		if (this.state.table)
			return this.renderTable();
		else
			return this.renderTree();
	}


	handleClose = () => {
		this.props.closeBrowser();
	}

	handleHoverClose = () => {
	}

	handleUnhoverClose = () => {
	}
}
