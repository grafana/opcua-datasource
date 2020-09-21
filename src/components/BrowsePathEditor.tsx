import React, { PureComponent } from "react";
import { QualifiedName } from '../types';
import { QualifiedNameEditor } from './QualifiedNameEditor';


export interface Props {
    browsePath: QualifiedName[];
    //onAddPath();
    //onDeletePath();
}

type State = {
}

export class BrowsePathEditor extends PureComponent<Props, State> {
    constructor(props: Props) {
        super(props);

    }

    selectBrowseNamespace = () => {

    }

    onChangePath = () => {

    }

    onDelete = (id: number) => {

    }

    eachElement = (item: QualifiedName, id: number) => {
        const { namespaceUrl, name } = item;
        return <li><QualifiedNameEditor name={name} namespaceUrl={namespaceUrl} id={id} onchange={this.onChangePath} selectBrowseNamespace={this.selectBrowseNamespace} />
            <button onClick={e => this.onDelete(id)}>Delete</button>
        </li>;
    }

    
    render() {
        const { browsePath } = this.props;

        return ( 
            <div>
                {browsePath && browsePath.length > 0 && browsePath.map(this.eachElement)}
            </div>
        );
    }
}