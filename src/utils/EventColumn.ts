import { EventColumn } from "../types";

export function copyEventColumn(r: EventColumn): EventColumn {
        return {
            browsename: {
                name: r.browsename.name,
                namespaceUrl: r.browsename.namespaceUrl
            },
            alias: r.alias
        };
    };