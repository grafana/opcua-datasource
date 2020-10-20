import React, { Component } from "react";
import { OpcUaBrowseResults, QualifiedName, NodeClass } from '../types';
import { ThemeGetter } from './ThemesGetter';
import { GrafanaTheme } from '@grafana/data';
import { Paper, Table, TableHead, TableRow, TableCell, TableBody } from '@material-ui/core';
import { Button } from '@grafana/ui';
import { qualifiedNameToString, copyQualifiedName } from '../utils/QualifiedName';
import { nodeClassToString } from '../utils/Nodeclass';

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
	currentNode: OpcUaBrowseResults,
	children: OpcUaBrowseResults[],
	theme: GrafanaTheme | null,
	browsePath: OpcUaBrowseResults[],
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
			currentNode: {
				browseName: { name: "", namespaceUrl: "" }, displayName: "", isForward: false, nodeClass: -1, nodeId: ""
			},
			children: [],
			fetchedChildren: false,
			theme: null,
			browsePath: []
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
		if (this.state.currentNode.nodeId === "") {
			this.setState({ children: [], fetchedChildren: false, currentNode: rootNodeId, browsePath: [] });
		}

		let bg: string = "";
		let txt: string = "";
		//let bgBlue: string = "";
		if (this.state.theme != null) {
			bg = this.state.theme.colors.bg2;
			txt = this.state.theme.colors.text;
			//bgBlue = this.state.theme.colors.bgBlue1;
		}

		this.fetchChildren();

		return (
			<div className="panel-container">
				<ThemeGetter onTheme={this.onTheme} />
				<Paper>
					<Table>
						<TableHead style={{ backgroundColor: bg, color: txt, }}>
							<TableRow style={{ height: 20 }}>
								<TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: "nowrap" }}>Browse name</TableCell>
								<TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: "nowrap" }}>DisplayName</TableCell>
								<TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: "nowrap" }}>Node Class</TableCell>
								<TableCell style={{ color: txt, border: 0, padding: 2, whiteSpace: "nowrap" }}>Navigate</TableCell>
							</TableRow>
						</TableHead>
						<TableBody style={{ backgroundColor: bg, color: txt, }}>
							{this.state.children.map((row: OpcUaBrowseResults, index: number) => (
								<TableRow style={{ height: 14 }} key={index}>
									<TableCell style={{ color: txt, border: 0, padding: 2 }} onClick={(e) => this.onNodeChanged(index) }>
										{ qualifiedNameToString(row.browseName)}
									</TableCell>
									<TableCell style={{ color: txt, border: 0, padding: 2 }}>
										{row.displayName}
									</TableCell>
									<TableCell style={{ color: txt, border: 0, padding: 2 }}>
										{nodeClassToString(row.nodeClass as NodeClass)}
									</TableCell>
									<TableCell style={{ color: txt, border: 0, padding: 2 }}>
										<Button onClick={(e) => this.navigate(e, index)}>Navigate</Button>
									</TableCell>
								</TableRow>
							))}
						</TableBody>
					</Table>
				</Paper>
				<Button onClick={(e) => this.navigateBack(e)}>Navigate Back</Button>
			</div>
		);
	}

	onNodeChanged(index: number): void {
		if (index < this.state.children.length) {
			var n = this.state.children[index];
			this.props.onNodeSelectedChanged(n, this.getBrowsePath(n))
			if (this.props.closeOnSelect)
				this.props.closeBrowser();
		}
	}


	getBrowsePath(node: OpcUaBrowseResults ): QualifiedName[] {
		let bp = this.state.browsePath.map(a => copyQualifiedName(a.browseName)).slice();
		bp.push(copyQualifiedName(node.browseName));
		return bp;
	}



	fetchChildren() {
		if (!this.state.fetchedChildren && this.state.currentNode.nodeId.length > 0) {
			this.props.browse(this.state.currentNode.nodeId).then((response) => { this.setState({ children: response, fetchedChildren: true }) });
		}
    }

    navigate(e: React.MouseEvent<HTMLButtonElement, MouseEvent>, index: number): void {
		if (this.state.children.length > index) {
			let currentNode = this.state.children[index];
			var bp = this.state.browsePath.slice();
			bp.push(currentNode);
			this.setState({ currentNode: currentNode, fetchedChildren: false, children: [], browsePath: bp });
		}
	}

	navigateBack(e: React.MouseEvent<HTMLButtonElement, MouseEvent>): void {
		if (this.state.browsePath.length > 1) {
			var currentNode = this.state.browsePath[this.state.browsePath.length - 2];
			var bp = this.state.browsePath.slice(0, this.state.browsePath.length - 1);
			this.setState({ currentNode: currentNode, fetchedChildren: false, children: [], browsePath: bp });
		}
		else if (this.state.browsePath.length === 1) {
			var currentNode = this.props.rootNodeId;
			var bp = this.state.browsePath.slice(0, this.state.browsePath.length - 1);
			this.setState({ currentNode: currentNode, fetchedChildren: false, children: [], browsePath: bp });
		}
	}
}

