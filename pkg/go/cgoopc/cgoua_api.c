#include <stdio.h>
#include "open62541.h"
#include "cgoua_api.h"
#include "_cgo_export.h"

inline int IsStringEqual(const char *a, const char *b) {
    while (*a == *b) {
        if (*a == '\0') return 1;
        a++;
        b++;
    }
    return 0;
}

size_t strlstchar(const char *str, const char ch) {
    char *chptr = strrchr(str, ch);
    return chptr - str;
}

ArrayInt
*initArrayInt(size_t initSize) {
    ArrayInt *a = malloc(sizeof(a));
    a->array = malloc(initSize * sizeof(Tuple));
    a->used = 0;
    a->size = initSize;
    return a;
}

void appendArrayInt(ArrayInt *a, int ns, int elem) {
    // resize the array if it's needed
    if (a->used == a->size) {
        size_t extension = a->size;
        if (extension > 100) {
            extension = 100;
        };
        a->size = a->size + extension;
        a->array = realloc(a->array, a->size * sizeof(Tuple));
        for (int i = a->used; i < a->size; i++) {
            a->array[i].ns = 0;
            a->array[i].val = 0;
        }
    }
    // append 'int' element
    a->array[a->used].ns = ns;
    a->array[a->used].val = elem;
    a->used++;
}

int contains(ArrayInt *a, int ns, int elem) {
    for (int i=0; i < a->used; i++) {
        if (a->array[i].val == elem && a->array[i].ns == 0 ) {
            return 1;
        }
    }
    return 0;
}

void freeArrayInt(ArrayInt *a) {
    free(a->array);
    free(a);
}

Array *initArray(size_t initSize) {
    Array *a = malloc(sizeof(*a));
    a->size = initSize;
    a->used = 0;
    a->array = (NodeData *) malloc(initSize * sizeof(NodeData));
    return a;
}

void insertArray(Array *a, NodeData element) {
    if (a->used == a->size) {
        a->size *= 2;
        a->array = (NodeData *) realloc(a->array, a->size * sizeof(NodeData));
        for (unsigned int i = a->used; i < a->size; i++) {
            memset(&a->array[i], 0, sizeof(NodeData));
        }
    }
    // Assign name
    a->array[a->used].stringNodeId = (char *) malloc(strlen(element.stringNodeId) + 1);
    strcpy(a->array[a->used].stringNodeId, element.stringNodeId);

    // Assign display name
    a->array[a->used].displayName = (char *) malloc(strlen(element.displayName) + 1);
    strcpy(a->array[a->used].displayName, element.displayName);

    // Assign ID
    a->array[a->used].nameSpace = element.nameSpace;

    // Assign type
    a->array[a->used].type = element.type;

    // Assign nodeId
    a->array[a->used].intNodeId = element.intNodeId;
    a->used++;
}

void freeArray(Array *a) {
    // Free all name variables of each array element first
    for (int i = 0; i < a->used; i++) {
        free(a->array[i].stringNodeId);
        a->array[i].stringNodeId = NULL;

        free(a->array[i].displayName);
        a->array[i].displayName = NULL;
    }
    // Now free the array
    free(a->array);
    a->array = NULL;
    a->used = 0;
    a->size = 0;
}

UA_StatusCode
readTagInt32(UA_Client *client, UA_UInt16 nsIndex, UA_Int32 numeric, char *name, UA_Int32 *ret) {
    UA_Int32 value = 0;
    UA_Variant *val = UA_Variant_new();
    UA_StatusCode retval;
    if (numeric == -1) {
        //String node ID
        retval = UA_Client_readValueAttribute(client, UA_NODEID_STRING(nsIndex, name), val);
    } else {
        //Int Node ID
        retval = UA_Client_readValueAttribute(client, UA_NODEID_NUMERIC(nsIndex, numeric), val);
    }
    if (retval == UA_STATUSCODE_GOOD && UA_Variant_isScalar(val)) {
        if (val->type == &UA_TYPES[UA_TYPES_INT32]) {
            value = *(UA_Int32 *) val->data;
            //printf("UINT32 value is: %i\n", value);
        } else if (val->type == &UA_TYPES[UA_TYPES_UINT32]) {
            value = *(UA_UInt32 *) val->data;
            //printf("UA_UInt32 value is: %i\n", value);
        } else if (val->type == &UA_TYPES[UA_TYPES_INT16]) {
            value = *(UA_Int16 *) val->data;
            //printf("UA_Int16 value is: %i\n", value);
        } else if (val->type == &UA_TYPES[UA_TYPES_UINT16]) {
            value = *(UA_UInt16 *) val->data;
            //printf("UA_UInt16 value is: %i\n", value);
        } else {
            //printf("Unknown value type to read\n");
        }
    } else {
        printf("[ERROR] C.readTagInt32: No value. Status=%s, Name=%s\n", UA_StatusCode_name(retval), name);
    }
    UA_Variant_delete(val);
    *ret = value;
    return retval;
}

UA_StatusCode
writeTagInt32(UA_Client *client, UA_UInt16 nsIndex, char *name, UA_Int32 val) {
    UA_NodeId nodeId = UA_NODEID_STRING(nsIndex, name);
    UA_Variant *newValue = UA_Variant_new();
    UA_Variant_setScalar(newValue, &val, &UA_TYPES[UA_TYPES_INT32]);
    UA_StatusCode statusCode = UA_Client_writeValueAttribute(client, nodeId, newValue);
    return statusCode;
}

// Helper to print trace info while scanning OPCUA tree
void printRef(UA_ReferenceDescription *ref, uint8_t level) {
    //printf("level=%d\n", level);
    char *levelPref[level+1];
    memset(levelPref, '_', level);
    levelPref[level] = '\0';

    //printf("%*.s", level, levelPref);
    if (ref->nodeId.nodeId.identifierType == UA_NODEIDTYPE_STRING) {
        UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "%-3d %-3d %-5d %*.s %-16.*s %-16.*s",
               ref->nodeClass,
               ref->nodeId.nodeId.namespaceIndex,
               ref->nodeId.nodeId.identifier.numeric,
               level, *levelPref,
               (int) ref->nodeId.nodeId.identifier.string.length, ref->nodeId.nodeId.identifier.string.data,
               (int) ref->displayName.text.length, ref->displayName.text.data);

    } else if (ref->nodeId.nodeId.identifierType == UA_NODEIDTYPE_NUMERIC) {
        UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "%-3d %-3d %-5d %*.s %-16.*s %-16.*s",
               ref->nodeClass,
               ref->nodeId.nodeId.namespaceIndex,
               ref->nodeId.nodeId.identifier.numeric,
               level, *levelPref,
               (int) ref->browseName.name.length, ref->browseName.name.data,
               (int) ref->displayName.text.length, ref->displayName.text.data);

    } else {
        UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "[WARN] printRef: Unknown nodeId.identiferType = %d\n", ref->nodeId.nodeId.identifierType);
    }
}

// Add 'TAG' node to result list
void putNodeToArray(UA_ReferenceDescription *ref, Array *a) {
    NodeData x;
    x.nameSpace = ref->nodeId.nodeId.namespaceIndex;
    x.displayName = (char *) malloc(strlen(ref->displayName.text.data) + 1);
    x.stringNodeId = (char *) malloc(2);
    strcpy(x.displayName, ref->displayName.text.data);

    if (ref->nodeId.nodeId.identifierType == UA_NODEIDTYPE_STRING) {
        //printRef(ref, 0); // variable found
        x.type = 3;
        x.intNodeId = -1;
        x.stringNodeId = (char *) malloc((int) ref->nodeId.nodeId.identifier.string.length + 1);
        strcpy(x.stringNodeId, ref->nodeId.nodeId.identifier.string.data);
        x.stringNodeId[ref->nodeId.nodeId.identifier.string.length] = '\0';
        x.intNodeId = ref->nodeId.nodeId.identifier.numeric;
        insertArray(a, x);
    } else if (ref->nodeId.nodeId.identifierType == UA_NODEIDTYPE_NUMERIC) {
        // SKIP 'EM !
        // printf("[WARN] putNodeToArray: Skipping UA_NODEIDTYPE_NUMERIC id = %d\n", ref->nodeId.nodeId.identifier.numeric);
    } else {
        printf("[WARN] putNodeToArray: Unknown nodeId.identiferType = %d\n", ref->nodeId.nodeId.identifierType);
    }

    if (x.stringNodeId != NULL) {
        free(x.stringNodeId);
    }
    if (x.displayName != NULL) {
        free(x.displayName);
    }
}

// Fixing wrond terminated string
// it might be a bug in the library
void fixNodeStringId(UA_NodeId *nodeId, int dn_len) {
    if (nodeId->identifier.numeric != 0) {
        size_t chankPos = strlstchar((char *)(nodeId->identifier.string.data), '.');
        if (chankPos <= 0 || chankPos > 99999) {
            return;
        }
        size_t len = nodeId->identifier.string.length;
        size_t chunkLen = len - chankPos - 1;
        if (chunkLen > dn_len) {
            len = len - (chunkLen - dn_len);
            printf("[WARN] the indentifer.string is long!\n");
            nodeId->identifier.string.length = len;
            nodeId->identifier.string.data[len] = '\0';
            printf("[WARN] fixed string = %.*s \n", (int)(nodeId->identifier.string.length), nodeId->identifier.string.data);
        }
    }
}

// Getting node by Id and scanf for it children
void browseNodeId(UA_Client *client, UA_NodeId *nodeId, Array *a, ArrayInt *numNodeIDs, uint8_t level, int debug) {
    level += 2;
    UA_BrowseRequest bReq3;
    UA_BrowseRequest_init(&bReq3);
    bReq3.requestedMaxReferencesPerNode = 0;
    bReq3.nodesToBrowse = UA_BrowseDescription_new();
    bReq3.nodesToBrowseSize = 1;
    bReq3.nodesToBrowse[0].nodeId = *nodeId;
    bReq3.nodesToBrowse[0].resultMask = UA_BROWSERESULTMASK_ALL; /* return everything */

    UA_BrowseResponse bResp3 = UA_Client_Service_browse(client, bReq3);

    for (size_t i = 0; i < bResp3.resultsSize; ++i) {
        for (size_t j = 0; j < bResp3.results[i].referencesSize; ++j) {

            UA_ReferenceDescription *ref = &(bResp3.results[i].references[j]);
            UA_UInt16 ns = ref->nodeId.nodeId.namespaceIndex;
            UA_UInt32 num = ref->nodeId.nodeId.identifier.numeric;
            UA_NodeClass ncl = ref->nodeClass;
            enum UA_NodeIdType ntyp = ref->nodeId.nodeId.identifierType;

            if (debug >= 6) {
                printRef(ref, level); // prints everything whatever it is
            }

            if (ntyp != UA_NODEIDTYPE_STRING && ntyp != UA_NODEIDTYPE_NUMERIC) {
                continue;
            }
            if (ncl == UA_NODECLASS_VARIABLE) {
                putNodeToArray(ref, a);
                continue;
            }
            else if (ncl == UA_NODECLASS_OBJECT || ncl == UA_NODECLASS_OBJECTTYPE) {
                // getting next node ID
                UA_NodeId nextId;

                // check duplicates
                if (contains(numNodeIDs, ns, num) != 0) {
                    continue;
                }
                appendArrayInt(numNodeIDs, ns, num); // append nodeId to cache

                if (ntyp == UA_NODEIDTYPE_STRING) {
                    nextId = UA_NODEID_STRING(ns, (char *)ref->nodeId.nodeId.identifier.string.data);
                    fixNodeStringId(&nextId, (int)ref->displayName.text.length);
                }
                else {
                    nextId = UA_NODEID_NUMERIC(ns, num);
                }
                browseNodeId(client, &nextId, a, numNodeIDs, level, debug);
                continue;
            } else {
                //printRef(ref);
                continue;
            }
        }
    }

}

Array *getNodesArray(UA_Client *client, int debug) {
    ArrayInt *numNodeIDs = initArrayInt(10);
    Array *a = initArray(5);
    UA_BrowseRequest bReq;
    UA_BrowseRequest_init(&bReq);
    bReq.requestedMaxReferencesPerNode = 0;
    bReq.nodesToBrowse = UA_BrowseDescription_new();
    bReq.nodesToBrowseSize = 1;
    bReq.nodesToBrowse[0].nodeId = UA_NODEID_NUMERIC(0, UA_NS0ID_OBJECTSFOLDER); /* browse objects folder */
    bReq.nodesToBrowse[0].resultMask = UA_BROWSERESULTMASK_ALL; /* return everything */
    UA_BrowseResponse bResp = UA_Client_Service_browse(client, bReq);
    if (debug >= 6) {
        printf("%-9s %-9s %-16s %-16s %-16s\n", "NODECLASS", "NAMESPACE", "NODEID", "BROWSE NAME", "DISPLAY NAME");
    }

    uint8_t level = 0;

    for (size_t i = 0; i < bResp.resultsSize; ++i) {
        for (size_t j = 0; j < bResp.results[i].referencesSize; ++j) {
            UA_ReferenceDescription *ref = &(bResp.results[i].references[j]);
            if (debug == 1) {
                printRef(ref, level);
            }

            if (ref->nodeId.nodeId.identifierType == UA_NODEIDTYPE_NUMERIC) {
                UA_NodeId nodeId = UA_NODEID_NUMERIC(ref->nodeId.nodeId.namespaceIndex,
                                                     ref->nodeId.nodeId.identifier.numeric);

                if (contains(numNodeIDs, ref->nodeId.nodeId.namespaceIndex, ref->nodeId.nodeId.identifier.numeric) == 0) {

                    appendArrayInt(numNodeIDs, ref->nodeId.nodeId.namespaceIndex, ref->nodeId.nodeId.identifier.numeric);
                    browseNodeId(client, &nodeId, a, numNodeIDs, level, debug);
                } else {
                    // do nothing, cycle ref
                }

            } else if (ref->nodeId.nodeId.identifierType == UA_NODEIDTYPE_STRING) {
                UA_NodeId nodeId = UA_NODEID_STRING(ref->nodeId.nodeId.namespaceIndex,
                                                    ref->nodeId.nodeId.identifier.string.data);
                browseNodeId(client, &nodeId, a, numNodeIDs, level, debug);
            }
        }
    }
    UA_BrowseRequest_deleteMembers(&bReq);
    UA_BrowseResponse_deleteMembers(&bResp);

    printf("check cache:\n");
    for (int i = 0; i < numNodeIDs->used; i++) {
        int ns = numNodeIDs->array[i].ns;
        int num = numNodeIDs->array[i].val;
        if (ns == 4 && num == 58) {
            printf("ns=%d, num=%d\n", ns, num);
        }
    }
    freeArrayInt(numNodeIDs);
    return a;
}

UA_Logger logger = UA_Log_Stdout;

// wrapper
UA_MonitoredItemCreateResult
createDataExchange(UA_Client *client, UA_UInt32 subId,
                   UA_TimestampsToReturn ts, const UA_MonitoredItemCreateRequest monRequest,
                   void *context, int dataChangeCallbackId,
                   int deleteItemCallbackId) {
    int *c = context;
    UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "createDataExchange called, subId=%d", subId);
    UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "createDataExchange called, fn=%d", *c);
    UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "createDataExchange called, ns=%d",
        monRequest.itemToMonitor.nodeId.namespaceIndex);
    UA_LOG_INFO(UA_Log_Stdout, UA_LOGCATEGORY_USERLAND, "createDataExchange called, stringId=%s",
        monRequest.itemToMonitor.nodeId.identifier.string.data);

    UA_MonitoredItemCreateResult *result = UA_MonitoredItemCreateResult_new();

    UA_MonitoredItemCreateResult monResponse =
        UA_Client_MonitoredItems_createDataChange(client, subId, UA_TIMESTAMPSTORETURN_BOTH, monRequest,
            context, go_handler, NULL);
    return monResponse;
}

