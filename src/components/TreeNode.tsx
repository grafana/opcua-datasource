import React, { Component } from 'react';
import { OpcUaBrowseResults, QualifiedName } from '../types';
import { FaChevronDown, FaChevronRight } from 'react-icons/fa';

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
 * The root component allow for display/use of sub components
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
    let isOpen = this.state.isOpen;
    this.setState({ isOpen: !isOpen });
  };

  getBrowsePath = (): QualifiedName[] => {
    let browsePaths: QualifiedName[] = [];
    browsePaths.push(this.props.node.browseName);
    let parentNode = this.props.parentNode;
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
        <span onClick={() => this.onToggle()} style={{ cursor: 'pointer' }}></span>
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
      browse(this.props.node.nodeId).then((response) => {
        this.setState({ children: response, fetchedChildren: true });
      });
    }

    if (!this.state.isOpen && this.state.fetchedChildren) {
      this.setState({ children: [], fetchedChildren: false });
    }

    let divStyle = this.getDivStyle() as React.CSSProperties;

    return (
      <>
        <div style={divStyle}>
          {this.renderExpander()}
          <span role="button" onClick={() => onNodeSelect(this.props.node, this.getBrowsePath())}>
            {this.props.node.displayName}
          </span>
        </div>

        {this.state.children.map((childNode, idx) => (
          <TreeNode
            {...this.props}
            key={`TreeNode-${idx}`}
            level={this.props.level + 1}
            node={childNode}
            parentNode={this}
          />
        ))}
      </>
    );
  }

  /**
   * Calculates how much to indent the node based on the level it's at.
   * @param {*} level The level of the node.
   */
  getPaddingLeft = (level: number) => {
    let paddingLeft = level * 20;
    return paddingLeft;
  };

  /**
   * Gets the style of the surrounding div.
   */
  getDivStyle = () => {
    let borderColor = null;
    let level = this.props.level;

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
}
