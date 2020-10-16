import React, { Component } from "react";
import { OpcUaBrowseResults, QualifiedName } from '../types';
import { ThemeGetter } from './ThemesGetter';
import { GrafanaTheme } from '@grafana/data';
import { Paper, Table, TableHead, TableRow, TableCell, TableBody } from '@material-ui/core';

type Props = {
	browse: (nodeId: string) => Promise<OpcUaBrowseResults[]>;
	rootNodeId: OpcUaBrowseResults,
	ignoreRootNode: boolean,
	closeOnSelect: boolean,
	onNodeSelectedChanged: (nodeId: OpcUaBrowseResults, browsePath: QualifiedName[]) => void;
	closeBrowser: () => void;
};

type State = {
	fetchedChildren: boolean,
	rootNode: OpcUaBrowseResults,
	children: OpcUaBrowseResults[],
	theme: GrafanaTheme | null;
}



/**
 * Displays nodes in a tree and allows users to select an entity for display.
 */
export class BrowserTable extends Component<Props, State> {
	/**
	 *
	 * @param {*} props sets the data structure
	 */
	constructor(props: Props) {
		super(props);
		this.state = {
			rootNode: {
				browseName: { name: "", namespaceUrl: "" }, displayName: "", isForward: false, nodeClass: -1, nodeId: ""
			},
			children: [],
			fetchedChildren: false,
			theme: null
		}
	}

	handleClose = () => {
		this.props.closeBrowser();
	}

	handleHoverClose = () => {
	}

	handleUnhoverClose = () => {
	}

	nodeSelect = (node: OpcUaBrowseResults, browsePath: QualifiedName[]) => {
		this.props.onNodeSelectedChanged(node, browsePath);
		if (this.props.closeOnSelect)
			this.props.closeBrowser();
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
		const rootNodeId = this.props.rootNodeId;
		if (this.state.rootNode.nodeId != rootNodeId.nodeId) {
			this.setState({ children: [], fetchedChildren: false, rootNode: rootNodeId });
		}

		let bg: string = "";
		let txt: string = "";
		//let bgBlue: string = "";
		if (this.state.theme != null) {
			bg = this.state.theme.colors.bg2;
			txt = this.state.theme.colors.text;
			//bgBlue = this.state.theme.colors.bgBlue1;
		}
		return (
			<div className="panel-container">
				<ThemeGetter onTheme={this.onTheme} />
				<Paper>
					<Table>
						<TableHead style={{ backgroundColor: bg, color: txt, }}>
							<TableRow style={{ height: 20 }}>
								<TableCell style={{ color: txt, border: 0, padding: 0 }}>Browse name</TableCell>
								<TableCell style={{ color: txt, border: 0, padding: 0 }} align="left">DisplayName</TableCell>
								<TableCell style={{ color: txt, border: 0, padding: 0 }} align="right">Node Class</TableCell>
							</TableRow>
						</TableHead>
						<TableBody style={{ backgroundColor: bg, color: txt, }}>
							{this.state.children.map((row: OpcUaBrowseResults, index: number) => (
								<TableRow style={{ height: 14 }} key={index}>
									<TableCell style={{ color: txt, border: 0, padding: 0 }}>
									</TableCell>
									<TableCell align="left" style={{ color: txt, border: 0, padding: 0 }}>
									</TableCell>
									<TableCell align="right" style={{ color: txt, border: 0, padding: 0 }}>
									</TableCell>
								</TableRow>
							))}
						</TableBody>
					</Table>
				</Paper>
			</div>
		);
	}
}

