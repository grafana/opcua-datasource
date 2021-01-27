import { NSNodeId } from '../types';

export function nodeIdToShortString(nodeId: NSNodeId | null, nsTable: string[]): string {
    if (nodeId === null)
        return "";
    let idx = nsTable.indexOf(nodeId.namespaceUrl);
    if (idx >= 0) {
        let sepIdx = nodeId.id.indexOf(";");
        if (sepIdx >= 0 && sepIdx < (nodeId.id.length - 1)) {
            return "ns=" + idx + ";" + nodeId.id.substr(sepIdx + 1);
        }
        else { // Ns=0
            return nodeId.id;
        }
    }
    return nodeId.id;
}