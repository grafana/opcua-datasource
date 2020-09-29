import React, { PureComponent } from 'react';

import { Input, Button } from '@grafana/ui';
import { CascaderOption } from 'rc-cascader/lib/Cascader';
import { QualifiedName, OpcUaQuery, OpcUaDataSourceOptions, OpcUaBrowseResults, separator } from '../types';
import { QueryEditorProps } from '@grafana/data';
import { DataSource } from '../DataSource';
import { SegmentFrame } from './SegmentFrame';
import { browsePathToString, stringToBrowsePath } from '../utils/QualifiedName';
import { toCascaderOption } from '../utils/CascaderOption';
//import { BrowsePathEditor } from './BrowsePathEditor';
import { ButtonCascader } from './ButtonCascader/ButtonCascader';
import { Browser } from './Browser';

type Props = QueryEditorProps<DataSource, OpcUaQuery, OpcUaDataSourceOptions> & { nodeNameType: string };
    
type State = {
    options: CascaderOption[];
    value: string[];
    alias: string;
    nodeId: string,
    browsepath: QualifiedName[];
    browserOpened: boolean;
};

const rootNode = 'i=85';


export class NodeQueryEditor extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);

        let alias = this.props?.query?.alias;
        if (typeof alias === 'undefined')
            alias = "";

        this.state = {
            options: [],
            value: this.props.query.value || ['Select to browse OPC UA Server'],
            browsepath: this.props.query.browsepath,
            nodeId: this.props.query.nodeId,
            alias: alias,
            browserOpened: false
        }

        props.datasource.getResource('browse', { nodeId: rootNode }).then((results: OpcUaBrowseResults[]) => {
            console.log('Results', results);
            this.setState({
                options: results.map((r: OpcUaBrowseResults) => toCascaderOption(r)),
            });
        });

    }

    onChange = (selected: string[], selectedItems: CascaderOption[]) => {
        const { query, onChange, onRunQuery } = this.props;
        const value = selectedItems.map(item => (item.label ? item.label.toString() : ''));
        const nodeId = selected[selected.length - 1];

        this.setState({ value: value, nodeId: nodeId });
        onChange({
            ...query,
            value,
            nodeId,
        });
        onRunQuery();
    };


    onChangeBrowsePath = (event: React.FormEvent<HTMLInputElement>) => {
        const { query, onChange, onRunQuery } = this.props;
        var s = event?.currentTarget.value;
        let browsepath = stringToBrowsePath(s);
        this.setState({ browsepath: browsepath }, () => {

            onChange({
                ...query,
                browsepath
            });
            onRunQuery();

        });
    }


    onChangeAlias = (event: React.FormEvent<HTMLInputElement>) => {
        const { query, onChange, onRunQuery } = this.props;
        var s = event?.currentTarget.value;
        this.setState({ alias: s }, () => {
            let alias = s;
            onChange({
                ...query,
                alias
            });
            onRunQuery();

        });
    };

    getChildren = (selectedOptions: CascaderOption[]) => {
        const targetOption = selectedOptions[selectedOptions.length - 1];
        targetOption.loading = true;
        if (targetOption.value) {
            this.props.datasource
                .getResource('browse', { nodeId: targetOption.value })
                .then((results: OpcUaBrowseResults[]) => {
                    targetOption.loading = false;
                    targetOption.children = results.map(r => toCascaderOption(r));
                    this.setState({
                        options: [...this.state.options],
                    });
                });
        }
    };


    browse = (nodeId: string): Promise<OpcUaBrowseResults[]> => {
        return this.props.datasource
            .getResource('browse', { nodeId: nodeId });
    };

    toggleBrowsePathBrowser = () => {
        this.setState({ browserOpened: !this.state.browserOpened });
    }

    renderBrowsePathBrowser = (rootNodeId: OpcUaBrowseResults) => {
        
        if (this.state.browserOpened) {
            return <div data-id="Treeview-MainDiv" style={{
                border: "lightgrey 1px solid",
                borderRadius: "1px",
                cursor: "pointer",
                padding: "2px",
                position: "absolute",
                left: 30,
                top: 10,
                zIndex: 10,
            }}>
                <Browser closeBrowser={() => this.setState({ browserOpened: false })} closeOnSelect={true}
                    browse={a => this.browse(a)} datasource={this.props.datasource}
                    ignoreRootNode={true} rootNodeId={rootNodeId}
                    onNodeSelectedChanged={(node, browsepath) => { this.setState({ browsepath: browsepath }) }}></Browser></div>;
        }
        return <></>;
    }


    render() {
        const { options, value, browsepath, alias, nodeId } = this.state;
        const rootNodeId: OpcUaBrowseResults = {
            browseName: {
                name: "root", namespaceUrl: ""
            },
            displayName: "root",
            isForward: true,
            nodeClass: 0,
            nodeId: nodeId
        };
        return (
            <div style={{ padding: "4px" }}>
                <SegmentFrame label={this.props.nodeNameType} >
                    <div onBlur={e => console.log('onBlur', e)}>
                        <ButtonCascader
                            //className="query-part"
                            value={value}
                            loadData={this.getChildren}
                            options={options}
                            onChange={this.onChange}
                        >
                            {value.join(separator)}
                        </ButtonCascader>
                    </div>
                    <div>
                        <Input value={browsePathToString(browsepath)} placeholder={'browsepath'} onChange={e => this.onChangeBrowsePath(e)} width={30} />
                    </div>
                    <Button onClick={() => this.toggleBrowsePathBrowser()}>Browse</Button>
                    <div style={{ position: 'relative' }}>
                        {this.renderBrowsePathBrowser(rootNodeId)}
                    </div>
                </SegmentFrame>
                <SegmentFrame label="Alias">
                    <Input value={alias} placeholder={'alias'} onChange={e => this.onChangeAlias(e)} width={30} />
                </SegmentFrame>
            </div>
        );
    }
}

