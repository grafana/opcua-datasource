//// @flow
//import React, { Component } from "react";
//import values from "lodash/values";
//import TreeNode from "./TreeNode";
//import last from "lodash/last";
//import { convertRemToPixels } from "../../functions/UtilityFunctions/UtilityFunctions";

//var NodeClass = {
//	DataType: 64,
//	Method: 4,
//	Object: 1,
//	ObjectType: 8,
//	ReferenceType: 32,
//	Unspecified: 0,
//	Variable: 2,
//	VariableType: 16,
//	View: 128,
//};

//type Props = {
//	datasource: any,
//	changed: () => void,
//	target: Object,
//};

//const data = {};



///**
// * Displays nodes in a tree and allows users to select an entity for display.
// */
//export class Browser extends Component<Props, State> {
//	state = {
//		nodes: data,
//	};

//	rootNode: TreeNode;

//	/**
//	 *
//	 * @param {*} props sets the data structure
//	 */
//	constructor(props) {
//		super(props);

//		let newRoot = {
//			children: [],
//			isRoot: true,
//			nodeId: "", // first is empty
//			path: "/Root" + "",
//			type: "folder",
//		};
//		this.state.nodes["/Root"] = newRoot;
//	}

//	/**
//	 * Renders the component.
//	 */
//	render() {
//		const nodeChain = [];
//		const rootNodes = this.getRootNodes();
//		this.renavigateChildNodes();

//		let { target } = this.props;
//		let targetNodeChain = null;
//		if (target) {
//			targetNodeChain = target.nodeChain;
//		}

//		return (
//			<div data-id="Treeview-MainDiv">
//				<span
//					data-id="Treeview-CloseSpan"
//					onClick={this.handleClose}
//					onMouseOver={this.handleHoverClose}
//					onMouseOut={this.handleUnhoverClose}
//					ref={(spn) => (this.closeSpan = spn)}
//					style={{
//						border: "lightgrey 1px solid",
//						borderRadius: convertRemToPixels("2rem"),
//						cursor: "pointer",
//						padding: convertRemToPixels("0.5rem"),
//						position: "absolute",
//						right: convertRemToPixels("2rem"),
//						top: 0,
//						zIndex: 1,
//					}}
//				>
//					X
//				</span>
//				<div
//					data-id="Treeview-ScrollDiv"
//					style={{
//						height: convertRemToPixels("20rem"),
//						overflowX: "hidden",
//						overflowY: "auto",
//					}}
//				>
//					{rootNodes.map((node, i) => (
//						<TreeNode
//							key={i}
//							node={node}
//							nodeChain={nodeChain}
//							getChildNodes={this.getChildNodes}
//							onToggle={this.onToggle}
//							onNodeSelect={this.onNodeSelect}
//							targetNodeChain={targetNodeChain}
//						/>
//					))}
//				</div>
//			</div>
//		);
//	}

//	/**
//	 * Gets the node(s) at the root of the tree.
//	 */
//	getRootNodes = () => {
//		const { nodes } = this.state;
//		return values(nodes).filter((node) => node.isRoot === true);
//	};

//	/**
//	 * Gets the children for a given node.
//	 */
//	getChildNodes = (node) => {
//		const { nodes } = this.state;
//		if (!node.children) return [];
//		return node.children.map((path) => nodes[path]);
//	};

//	/**
//	 * When a node is re-clicked on, the tree should renavigate to the location of the node.
//	 */
//	renavigateChildNodes = () => {
//		const { target } = this.props;
//		if (!target) {
//			return;
//		}

//		const { renavigateNodeChain } = target;
//		if (!renavigateNodeChain || renavigateNodeChain.length <= 0) {
//			return;
//		}

//		const { nodes } = this.state;
//		this.renavigateNextNode(renavigateNodeChain, nodes);

//		target.renavigateNodeChain = null;

//		const { changed } = this.props;
//		if (changed) {
//			changed();
//		}
//	};

//	/**
//	 * Saves the renavigated nodes to the state.
//	 */
//	saveRenavigatedNodes = (nodes: Array<Object>) => {
//		this.setState({ nodes });
//	};

//	/**
//	 * Takes a value from the node chain and loads its children.
//	 */
//	renavigateNextNode = (renavigateNodeChain, nodes) => {
//		if (!renavigateNodeChain || renavigateNodeChain.length <= 1) {
//			return nodes;
//		}

//		const node = renavigateNodeChain[0];

//		const that = this;

//		const { datasource } = this.props;
//		datasource
//			.getChildrenFromServer(node.nodeId)
//			.then(function (foundDataArray) {
//				// Empty parent children list
//				nodes[node.path].children = [];

//				for (const item of foundDataArray) {
//					let newPath = node.path + "@#£$" + item.displayName;
//					let newNode = {
//						children: [],
//						isRoot: false,
//						nodeId: item.nodeId,
//						path: newPath,
//						type: checkType(item.nodeClass),
//					};
//					nodes[newPath] = newNode;

//					const newChildren = nodes[node.path].children.concat(
//						newPath
//					);
//					nodes[node.path].children = newChildren;
//					nodes[node.path].isOpen = true;

//					nodes[node.path].isOpen = true;
//				}
//			})
//			.catch((error) => {
//				console.error(error);
//			})
//			.finally(() => {
//				renavigateNodeChain.splice(0, 1);

//				if (renavigateNodeChain.length > 0) {
//					nodes = that.renavigateNextNode(renavigateNodeChain, nodes);
//				}

//				this.saveRenavigatedNodes(nodes);
//			});
//	};

//	/**
//	 * Gets the child nodes for a folder from the server.
//	 */
//	retrieveChildNodes = (node) => {
//		const { nodes } = this.state;

//		const { datasource } = this.props;

//		datasource
//			.getChildrenFromServer(node.nodeId)
//			.then(function (foundDataArray) {
//				// Empty parent children list
//				nodes[node.path].children = [];

//				for (const item of foundDataArray) {
//					let newPath = node.path + "@#£$" + item.displayName;
//					let newNode = {
//						children: [],
//						isRoot: false,
//						nodeId: item.nodeId,
//						path: newPath,
//						type: checkType(item.nodeClass),
//					};
//					nodes[newPath] = newNode;

//					const newChildren = nodes[node.path].children.concat(
//						newPath
//					);
//					nodes[node.path].children = newChildren;
//				}
//			})
//			.catch((error) => {
//				console.error(error);
//			})
//			.finally(() => {
//				this.setState({ nodes });
//			});
//	};

//	/**
//	 * Toggles a folder open and closed.
//	 */
//	onToggle = (node) => {
//		const { nodes } = this.state;

//		// Load children if this is the "opening part" (not open at the moment can mean "undefined")
//		if (!nodes[node.path].isOpen) {
//			this.retrieveChildNodes(node);
//			this.setState({ nodes });
//		}

//		nodes[node.path].isOpen = !node.isOpen;
//		this.setState({ nodes });
//	};

//	/**
//	 * If the user selects a folder, expand the contents. If the user selects a file, open the entity associated with it.
//	 */
//	onNodeSelect = (node, nodeChain) => {
//		if (node.type === "folder") {
//			this.onToggle(node);
//		} else if (node.type === "file") {
//			this.onSelect(node, nodeChain);
//		}
//	};

//	/**
//	 * Called when the user closes the tree.
//	 */
//	handleClose = () => {
//		let { target } = this.props;

//		target.treeOpened = false;

//		const { changed } = this.props;
//		if (changed) {
//			changed();
//		}
//	};

//	/**
//	 * Called when the user hovers over the close button.
//	 */
//	handleHoverClose = () => {
//		this.closeSpan.style.backgroundColor = "lightgrey";
//	};

//	/**
//	 * Called when the user moves away from the close button.
//	 */
//	handleUnhoverClose = () => {
//		this.closeSpan.style.backgroundColor = "transparent";
//	};

//	/**
//	 * Called when the user selects an item from the tree.
//	 */
//	onSelect = (node, nodeChain) => {
//		let newTarget = {
//			text: node.path,
//			value: node.nodeId,
//		};

//		let { datasource, target } = this.props;
//		datasource.addTarget(newTarget);

//		target.target = node.nodeId;

//		const nodesFromPath = node.path.split("@#£$");
//		if (nodesFromPath.length > 3) {
//			target.name =
//				nodesFromPath[nodesFromPath.length - 3] +
//				"\\" +
//				last(nodesFromPath);
//		} else {
//			target.name = last(nodesFromPath);
//		}

//		target.browseName = target.name;
//		target.nodeChain = nodeChain;
//		target.path = node.path;
//		target.treeOpened = false;

//		const { changed } = this.props;
//		if (changed) {
//			changed();
//		}
//	};
//}

///**
// *
// * @param {*} nodeClass decides what type of file we have
// */
//function checkType(nodeClass) {
//	if ((nodeClass & NodeClass.Variable) === NodeClass.Variable) {
//		// 2
//		return "file";
//	}
//	if ((nodeClass & NodeClass.Object) === NodeClass.Object) {
//		// 1
//		return "folder";
//	}
//	if ((nodeClass & NodeClass.ObjectType) === NodeClass.ObjectType) {
//		// 8
//		return "objecttype";
//	}

//	return "unspecified";
//}
