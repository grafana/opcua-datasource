import React, { Component } from 'react';
import { OpcUaBrowseResults, QualifiedName } from '../types';
import {
  //FaFolder,
  //FaFolderOpen,
  FaChevronDown,
  FaChevronRight,
  //FaChartLine,
  //FaCubes,
  //FaMinusCircle,
} from 'react-icons/fa';
//import last from "lodash/last";
//import { clone } from "./../utils/Clone";

type Props = {
  browse: (nodeId: string) => Promise<OpcUaBrowseResults[]>;
  node: OpcUaBrowseResults;
  parentNode: TreeNode | null;
  onNodeSelect: (node: OpcUaBrowseResults, browsePath: QualifiedName[]) => void;
  level: number;
  marginRight: number;
};

type State = {
  isOpen: boolean;
  fetchedChildren: boolean;
  node: OpcUaBrowseResults;
  children: OpcUaBrowseResults[];
};

/**
 * The root component allow for display/use of subcomponents
 */
export default class TreeNode extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      isOpen: false,
      fetchedChildren: false,
      node: {
        browseName: { name: '', namespaceUrl: '' },
        displayName: '',
        isForward: false,
        nodeClass: -1,
        nodeId: '',
      },
      children: [],
    };

    this.setState({ isOpen: false });
    this.setState({});
  }

  static defaultProps = {
    level: 0,
  };

  onToggle = () => {
    var isOpen = this.state.isOpen;
    this.setState({ isOpen: !isOpen });
  };

  getBrowsePath = (): QualifiedName[] => {
    var browsePaths: QualifiedName[] = [];
    browsePaths.push(this.props.node.browseName);
    var parentNode = this.props.parentNode;
    while (parentNode != null && typeof parentNode !== 'undefined') {
      browsePaths.push(parentNode.props.node.browseName);
      parentNode = parentNode.props.parentNode;
    }
    return browsePaths.reverse();
  };

  renderExpander = () => {
    const iconStyle = this.getNodeIconStyle();
    if (this.state.fetchedChildren && this.state.children.length === 0) {
      return <></>;
    }
    return (
      <>
        <span onClick={() => this.onToggle()} style={{ ...iconStyle, ...{ cursor: 'pointer' } }}>
          {this.state.isOpen ? <FaChevronDown /> : <FaChevronRight />}
        </span>
        <span
          onClick={() => this.onToggle()}
          //marginRight={10}
          style={{ cursor: 'pointer' }}
        ></span>
      </>
    );
  };

  /**
   * Renders the component.
   */
  render() {
    const { browse, onNodeSelect } = this.props;

    if (this.state.node.nodeId !== this.props.node.nodeId) {
      this.setState({ children: [], fetchedChildren: false, node: this.props.node });
    }

    if (this.state.isOpen && !this.state.fetchedChildren) {
      browse(this.props.node.nodeId).then(response => {
        this.setState({ children: response, fetchedChildren: true });
      });
    }

    if (!this.state.isOpen && this.state.fetchedChildren) {
      this.setState({ children: [], fetchedChildren: false });
    }

    var divStyle = this.getDivStyle() as React.CSSProperties;

    return (
      <>
        <div
          //onClick={() => onNodeSelect(this.props.node, this.getBrowsePath())}
          //onMouseOver={this.handleHover}
          //onMouseOut={this.handleUnhover}
          //ref={(frag) => (this.nodeHolder = frag)}
          style={divStyle}
          //type={node.type}
        >
          {this.renderExpander()}
          <span role="button" onClick={() => onNodeSelect(this.props.node, this.getBrowsePath())}>
            {this.props.node.displayName}
          </span>
        </div>

        {this.state.children.map(childNode => (
          <TreeNode {...this.props} level={this.props.level + 1} node={childNode} parentNode={this} />
        ))}
      </>
    );
  }

  ///**
  // * When the component mounts, scroll to the active node.
  // */
  //componentDidMount() {
  //	if (this.isActiveNode()) {
  //		this.nodeHolder.scrollIntoView({
  //			behavior: "smooth",
  //			block: "end",
  //			inline: "nearest",
  //		});
  //	}
  //}

  ///**
  // * Checks if this is the currently active node.
  // */
  //isActiveNode = () => {
  //	const { node, nodeChain, targetNodeChain } = this.props;

  //	let chainHere = clone(nodeChain);
  //	chainHere.push(node);

  //	if (targetNodeChain && chainHere) {
  //		const lastTargetNode = targetNodeChain[targetNodeChain.length - 1];
  //		const lastNode = chainHere[chainHere.length - 1];

  //		if (lastTargetNode.path === lastNode.path) {
  //			return true;
  //		}
  //	}

  //	return false;
  //};

  /**
   * Calculates how much to indent the node based on the level it's at.
   * @param {*} level The level of the node.
   */
  getPaddingLeft = (level: number) => {
    let paddingLeft = level * 20;
    //if (type === "file") paddingLeft += 20;
    return paddingLeft;
  };

  ///**
  // * Gets the label for the node.
  // * @param {*} node The node.
  // */
  //getNodeLabel = (node) => last(node.path.split("@#�$"));

  ///**
  // * Gets the style of the surrounding div.
  // */
  getDivStyle = () => {
    let borderColor = null;
    let level = this.props.level;
    //if (this.isActiveNode()) {
    //	borderColor = "lightblue";
    //}

    return {
      alignItems: 'center',
      border: borderColor ? borderColor + ' 1px solid' : null,
      cursor: /*type === "file" ?*/ 'pointer' /*: undefined*/,
      display: 'flex',
      flexDirection: 'row',
      paddingBottom: 5,
      paddingLeft: this.getPaddingLeft(level),
      paddingRight: 8,
      paddingTop: 5,
    };
  };

  /**
   * Gets the style of the icon for the node.
   */
  getNodeIconStyle = () => {
    const { marginRight } = this.props;

    return {
      fontSize: 12,
      marginRight: marginRight || 5,
    };
  };

  ///**
  // * Called when the user hovers over the node.
  // */
  //handleHover = () => {
  //	this.nodeHolder.style.background = "#333";
  //};

  ///**
  // * Called when the user moves away from the node.
  // */
  //handleUnhover = () => {
  //	this.nodeHolder.style.background = "transparent";
  //};
}
