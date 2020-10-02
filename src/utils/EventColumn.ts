import { EventColumn, QualifiedName } from "../types";
import { copyQualifiedName } from './QualifiedName';



export function copyEventColumn(r: EventColumn): EventColumn {
    var paths = r.browsePath.slice().map((bp: QualifiedName) => copyQualifiedName(bp));

    return {
        browsePath: paths,
        alias: r.alias
    };
};