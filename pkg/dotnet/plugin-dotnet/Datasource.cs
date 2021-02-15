using Google.Protobuf;
using Opc.Ua;
using Pluginv2;
using System;
using System.Text.Json;

namespace plugin_dotnet
{

    public class DashboardInfo
	{
		public string name { get; set; }

        public string[] dashKeys { get; set; }

    }

    public class EventQuery
    {
        public string eventTypeNodeId { get; set; }
        public EventColumn[] eventColumns { get; set; }
        public EventFilter[] eventFilters { get; set; }
    }

    public class QualifiedName
    {
        public string namespaceUrl { get; set; }
        public string name { get; set; }
    }

    public class EventColumn
    { 
        public QualifiedName[] browsePath{ get; set; }
        public string alias { get; set; }
    }

    public class EventFilter
    {
        public FilterOperator oper { get; set; }
        public FilterOperand[] operands { get; set; }
    }


    public class NodePath
    {
        /// <summary>
        /// Contains info about a node.
        /// </summary>
        public NodeInfo node { get; set; }
        /// <summary>
        /// Path from a predefined (depends on context) node to this node.
        /// </summary>
        public QualifiedName[] browsePath { get; set; }
    }


    public class RelativeBrowsePath
    {
        /// <summary>
        /// The node where the relative path starts
        /// </summary>
        public string startNode { get; set; }
        /// <summary>
        /// Browse path. (Reference types are omitted. Usually this means hierarchical reference type is used)
        /// </summary>
        public QualifiedName[] browsePath { get; set; }
    }



    public enum FilterOperandEnum
    { 
        Literal = 1,
        Element = 2,
        Attribute = 3,
        SimpleAttribute = 4
    }

    public class LiteralOp
    {
        public string typeId { get; set; }
        public string value { get; set; }
    }

    public class ElementOp
    { 
        public uint index { get; set; }
    }

    public class AttributeOp
    { 
        //TODO
    }


    public class SimpleAttributeOp
    {
        public string typeId { get; set; }

        public QualifiedName[] browsePath { get; set; }

        public int attributeId { get; set; }
        
        //indexRange NumericRange 
    }

    public class FilterOperand
    {
        public FilterOperandEnum type { get; set; }
        public string value { get; set; }
    }

    public class NSNodeId
    { 
        /// <summary>
        /// The Url of the namespace the node belongs to.
        /// </summary>
        public string namespaceUrl { get; set; }
        /// <summary>
        /// The string representation of the node id. The namespace index may not be valid if it is something different from 0.
        /// </summary>
        public string id { get; set; }  
    }

    public class NSExpandedNodeId
    {
        public string namespaceUrl { get; set; }
        public string id { get; set; }
    }

    public class NsNodeIdentifier
    {
        public string NamespaceUrl { get; set; }
        public IdType IdType { get; set; }
        public string Identifier { get; set; }
    }


    public class BrowseFilter
    {
        public int maxResults { get; set; }
        public string browseName { get; set; }
    }


    public class OpcUAQuery
    {
        public string refId { get; set; }
        public Int64 maxDataPoints { get; set; }
        public Int64 intervalMs { get; set; }
        public Int64 datasourceId { get; set; }
        public TimeRange timeRange { get; set; }
        public NodePath nodePath { get; set; }
        
        public string templateVariable { get; set; }

        public bool useTemplate { get; set; }

        public QualifiedName[] relativePath { get; set; }
        public string alias { get; set; }

        public string readType { get; set; }
        public object aggregate { get; set; }
        public int maxValuesPerNode { get; set; }
        public float resampleInterval { get; set; }

        public EventQuery eventQuery { get; set; }
        //public string eventTypeNodeId { get; set; }
        //public string[] eventTypes { get; set; }


        public OpcUAQuery() { }

        public OpcUAQuery(DataQuery dataQuery)
        {
            refId = dataQuery.RefId;
            maxDataPoints = dataQuery.MaxDataPoints;
            intervalMs = dataQuery.IntervalMS;
            timeRange = dataQuery.TimeRange;
            byte[] byDecoded = Convert.FromBase64String(dataQuery.Json.ToBase64());
            OpcUAQuery query = JsonSerializer.Deserialize<OpcUAQuery>(byDecoded);
            datasourceId = query.datasourceId;
            nodePath = query.nodePath;
            templateVariable = query.templateVariable;
            useTemplate = query.useTemplate;
    
            alias = query.alias;
            readType = query.readType;
            aggregate = query.aggregate;
            maxValuesPerNode = query.maxValuesPerNode;
            resampleInterval = query.resampleInterval;
            eventQuery = query.eventQuery;
            relativePath = query.relativePath;
        }

        public OpcUAQuery(string refId, Int64 maxDataPoints, Int64 intervalMs, Int64 datasourceId, string nodeId)
        {
            this.refId = refId;
            this.maxDataPoints = maxDataPoints;
            this.intervalMs = intervalMs;
            this.datasourceId = datasourceId;
            this.nodePath = nodePath;
        }
    }

    class OpcUaJsonData
    {
        public bool tlsAuth { get; set; }
        public bool tlsAuthWithCACert { get; set; }
        public bool tlsSkipVerify { get; set; }

        public OpcUaJsonData(ByteString base64encoded)
        {
            byte[] byDecoded = System.Convert.FromBase64String(base64encoded.ToString());
            OpcUaJsonData jsonData = JsonSerializer.Deserialize<OpcUaJsonData>(byDecoded);
            tlsAuth = jsonData.tlsAuth;
            tlsAuthWithCACert = jsonData.tlsAuthWithCACert;
            tlsSkipVerify = jsonData.tlsSkipVerify;
        }
    }

    public class NodeInfo 
    {
        public string displayName { get; set; }
        public QualifiedName browseName { get; set; }
        public string nodeId { get; set; }
        public uint nodeClass { get; set; }
    }

    public class BrowseResultsEntry
    {
        public string displayName { get; set; }
        public QualifiedName browseName { get; set; }
        public string nodeId { get; set; }
        public bool isForward { get; set; }
        public uint nodeClass { get; set; }
        public string typeId { get; set; }
        
        public BrowseResultsEntry() { }
        public BrowseResultsEntry(string displayName, QualifiedName browseName, string nodeId, ExpandedNodeId typeId, bool isForward, uint nodeClass)
        {
            this.displayName = displayName;
            this.browseName = browseName;
            this.nodeId = nodeId;
            this.isForward = isForward;
            this.typeId = typeId.IdType.ToString();
            this.nodeClass = nodeClass;
        }
    }

    class LogMessage
    {
        public string type { get; set; }
        public string message { get; set; }

        public LogMessage(string type, string message)
        {
            this.type = type;
            this.message = message;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize<LogMessage>(this);
        }
    }

    public class UaResult
    {
        public UaResult() : this(true)
        {
        }

        public UaResult(bool success)
        {
            this.success = success;
        }

        public bool success { get; set; }

        public string error { get; set; }
    }

    public class UaResult<T>
    {
        public UaResult()
        {
            Success = true;
        }

        public bool Success { get; set; }

        public T Value { get; set; }

        public string Error { get; set; }
    }
}