//import React, { Component } from "react";
////import {
////	FaFolder,
////	FaFolderOpen,
////	FaChevronDown,
////	FaChevronRight,
////	FaChartLine,
////	FaCubes,
////	FaMinusCircle,
////} from "react-icons/fa";
//import last from "lodash/last";
//import { clone } from "./../utils/Clone";

//type Props = {
//	getChildNodes: () => void,
//	level: Number,
//	marginRight: Number,
//	node: Object,
//	nodeChain: Object,
//	onNodeSelect: () => void,
//	onToggle: () => void,
//	targetNodeChain: String,
//	type?: String,
//};

///**
// * The root component allow for display/use of subcomponents
// */
//export default class TreeNode extends Component<Props> {
//	static defaultProps = {
//		level: 0,
//	};

//	/**
//	 * Renders the component.
//	 */
//	render() {
//		const {
//			node,
//			nodeChain,
//			getChildNodes,
//			level,
//			onToggle,
//			onNodeSelect,
//		} = this.props;

//		if (node.type === "objecttype" || node.type === "unspecified") {
//			return null;
//		}

//		let children = getChildNodes(node);

//		let chainHere = clone(nodeChain);
//		chainHere.push(node);

//		const divStyle = this.getDivStyle();
//		const iconStyle = this.getNodeIconStyle();

//		return (
//			<React.Fragment>
//				<div
//					level={level}
//					onClick={
//						node.type === "file"
//							? () => onNodeSelect(node, chainHere)
//							: null
//					}
//					onMouseOver={this.handleHover}
//					onMouseOut={this.handleUnhover}
//					ref={(frag) => (this.nodeHolder = frag)}
//					style={divStyle}
//					type={node.type}
//				>
//					{/* This is the arrow on the left. Only when a folder */}
//					<span
//						onClick={() => onToggle(node)}
//						style={{ ...iconStyle, ...{ cursor: "pointer" } }}
//					>
//						{node.type === "folder" &&
//							(node.isOpen ? (
//								<FaChevronDown />
//							) : (
//								<FaChevronRight />
//							))}
//					</span>

//					<span
//						onClick={() => onToggle(node)}
//						marginRight={10}
//						style={{ ...iconStyle, ...{ cursor: "pointer" } }}
//					>
//						{node.type === "file" && <FaChartLine />}
//						{node.type === "objecttype" && <FaCubes />}
//						{node.type === "unspecified" && <FaMinusCircle />}
//						{node.type === "folder" && node.isOpen === true && (
//							<FaFolderOpen />
//						)}
//						{node.type === "folder" && !node.isOpen && <FaFolder />}
//					</span>

//					<span
//						role="button"
//						onClick={() => onNodeSelect(node, chainHere)}
//					>
//						{this.getNodeLabel(node)}
//					</span>
//				</div>

//				{node.isOpen &&
//					children.map((childNode) => (
//						<TreeNode
//							{...this.props}
//							node={childNode}
//							nodeChain={chainHere}
//							level={level + 1}
//						/>
//					))}
//			</React.Fragment>
//		);
//	}

//	/**
//	 * When the component mounts, scroll to the active node.
//	 */
//	componentDidMount() {
//		if (this.isActiveNode()) {
//			this.nodeHolder.scrollIntoView({
//				behavior: "smooth",
//				block: "end",
//				inline: "nearest",
//			});
//		}
//	}

//	/**
//	 * Checks if this is the currently active node.
//	 */
//	isActiveNode = () => {
//		const { node, nodeChain, targetNodeChain } = this.props;

//		let chainHere = clone(nodeChain);
//		chainHere.push(node);

//		if (targetNodeChain && chainHere) {
//			const lastTargetNode = targetNodeChain[targetNodeChain.length - 1];
//			const lastNode = chainHere[chainHere.length - 1];

//			if (lastTargetNode.path === lastNode.path) {
//				return true;
//			}
//		}

//		return false;
//	};

//	/**
//	 * Calculates how much to indent the node based on the level it's at.
//	 * @param {*} level The level of the node.
//	 * @param {*} type The type of node.
//	 */
//	getPaddingLeft = (level, type) => {
//		let paddingLeft = level * 20;
//		if (type === "file") paddingLeft += 20;
//		return paddingLeft;
//	};

//	/**
//	 * Gets the label for the node.
//	 * @param {*} node The node.
//	 */
//	getNodeLabel = (node) => last(node.path.split("@#£$"));

//	/**
//	 * Gets the style of the surrounding div.
//	 */
//	getDivStyle = () => {
//		const { level, type } = this.props;

//		let borderColor = null;
//		if (this.isActiveNode()) {
//			borderColor = "lightblue";
//		}

//		return {
//			alignItems: "center",
//			border: borderColor ? borderColor + " 1px solid" : null,
//			cursor: type === "file" ? "pointer" : undefined,
//			display: "flex",
//			flexDirection: "row",
//			paddingBottom: 5,
//			paddingLeft: this.getPaddingLeft(level, type),
//			paddingRight: 8,
//			paddingTop: 5,
//		};
//	};

//	/**
//	 * Gets the style of the icon for the node.
//	 */
//	getNodeIconStyle = () => {
//		const { marginRight } = this.props;

//		return {
//			fontSize: 12,
//			marginRight: marginRight || 5,
//		};
//	};

//	/**
//	 * Called when the user hovers over the node.
//	 */
//	handleHover = () => {
//		this.nodeHolder.style.background = "#333";
//	};

//	/**
//	 * Called when the user moves away from the node.
//	 */
//	handleUnhover = () => {
//		this.nodeHolder.style.background = "transparent";
//	};
//}
