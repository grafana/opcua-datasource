#ifndef CGOUA_API_H
#define CGOUA_API_H

#include "open62541.h"

typedef struct NodeData {
    int nameSpace;
    int type;
    int intNodeId;
    char *stringNodeId;
    char *displayName;
} NodeData;

// array of Node structs
typedef struct Array {
    NodeData *array;
    size_t used;
    size_t size;
} Array;

typedef struct Tuple {
    int ns;
    int val;
} Tuple;

typedef struct ArrayInt {
    Tuple *array;
    size_t used;
    size_t size;
} ArrayInt;

extern Array *getNodesArray(UA_Client *client, int debug);

extern UA_StatusCode readTagInt32(UA_Client *client, UA_UInt16 nsIndex, UA_Int32 numeric,  char *name, UA_Int32 *val);

extern UA_StatusCode writeTagInt32(UA_Client *client, UA_UInt16 nsIndex, char *name, UA_Int32 val);

extern UA_MonitoredItemCreateResult createDataExchange(UA_Client *client, UA_UInt32 subId,
    UA_TimestampsToReturn ts, const UA_MonitoredItemCreateRequest monRequest,
    void *context, int dataChangeCallbackId,
    int daleteItemCallbackId);

#endif
