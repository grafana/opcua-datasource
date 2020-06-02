// Created by cgo -godefs - DO NOT EDIT
// cgo -godefs generated/open62541_types_map.go

package cgoopc

type UA_StatusCode uint32

const (
	UA_STATUSCODE_GOOD                                    = UA_StatusCode(0x00)
	UA_STATUSCODE_BADUNEXPECTEDERROR                      = UA_StatusCode(0x80010000)
	UA_STATUSCODE_BADINTERNALERROR                        = UA_StatusCode(0x80020000)
	UA_STATUSCODE_BADOUTOFMEMORY                          = UA_StatusCode(0x80030000)
	UA_STATUSCODE_BADRESOURCEUNAVAILABLE                  = UA_StatusCode(0x80040000)
	UA_STATUSCODE_BADCOMMUNICATIONERROR                   = UA_StatusCode(0x80050000)
	UA_STATUSCODE_BADENCODINGERROR                        = UA_StatusCode(0x80060000)
	UA_STATUSCODE_BADDECODINGERROR                        = UA_StatusCode(0x80070000)
	UA_STATUSCODE_BADENCODINGLIMITSEXCEEDED               = UA_StatusCode(0x80080000)
	UA_STATUSCODE_BADREQUESTTOOLARGE                      = UA_StatusCode(0x80b80000)
	UA_STATUSCODE_BADRESPONSETOOLARGE                     = UA_StatusCode(0x80b90000)
	UA_STATUSCODE_BADUNKNOWNRESPONSE                      = UA_StatusCode(0x80090000)
	UA_STATUSCODE_BADTIMEOUT                              = UA_StatusCode(0x800a0000)
	UA_STATUSCODE_BADSERVICEUNSUPPORTED                   = UA_StatusCode(0x800b0000)
	UA_STATUSCODE_BADSHUTDOWN                             = UA_StatusCode(0x800c0000)
	UA_STATUSCODE_BADSERVERNOTCONNECTED                   = UA_StatusCode(0x800d0000)
	UA_STATUSCODE_BADSERVERHALTED                         = UA_StatusCode(0x800e0000)
	UA_STATUSCODE_BADNOTHINGTODO                          = UA_StatusCode(0x800f0000)
	UA_STATUSCODE_BADTOOMANYOPERATIONS                    = UA_StatusCode(0x80100000)
	UA_STATUSCODE_BADTOOMANYMONITOREDITEMS                = UA_StatusCode(0x80db0000)
	UA_STATUSCODE_BADDATATYPEIDUNKNOWN                    = UA_StatusCode(0x80110000)
	UA_STATUSCODE_BADCERTIFICATEINVALID                   = UA_StatusCode(0x80120000)
	UA_STATUSCODE_BADSECURITYCHECKSFAILED                 = UA_StatusCode(0x80130000)
	UA_STATUSCODE_BADCERTIFICATETIMEINVALID               = UA_StatusCode(0x80140000)
	UA_STATUSCODE_BADCERTIFICATEISSUERTIMEINVALID         = UA_StatusCode(0x80150000)
	UA_STATUSCODE_BADCERTIFICATEHOSTNAMEINVALID           = UA_StatusCode(0x80160000)
	UA_STATUSCODE_BADCERTIFICATEURIINVALID                = UA_StatusCode(0x80170000)
	UA_STATUSCODE_BADCERTIFICATEUSENOTALLOWED             = UA_StatusCode(0x80180000)
	UA_STATUSCODE_BADCERTIFICATEISSUERUSENOTALLOWED       = UA_StatusCode(0x80190000)
	UA_STATUSCODE_BADCERTIFICATEUNTRUSTED                 = UA_StatusCode(0x801a0000)
	UA_STATUSCODE_BADCERTIFICATEREVOCATIONUNKNOWN         = UA_StatusCode(0x801b0000)
	UA_STATUSCODE_BADCERTIFICATEISSUERREVOCATIONUNKNOWN   = UA_StatusCode(0x801c0000)
	UA_STATUSCODE_BADCERTIFICATEREVOKED                   = UA_StatusCode(0x801d0000)
	UA_STATUSCODE_BADCERTIFICATEISSUERREVOKED             = UA_StatusCode(0x801e0000)
	UA_STATUSCODE_BADUSERACCESSDENIED                     = UA_StatusCode(0x801f0000)
	UA_STATUSCODE_BADIDENTITYTOKENINVALID                 = UA_StatusCode(0x80200000)
	UA_STATUSCODE_BADIDENTITYTOKENREJECTED                = UA_StatusCode(0x80210000)
	UA_STATUSCODE_BADSECURECHANNELIDINVALID               = UA_StatusCode(0x80220000)
	UA_STATUSCODE_BADINVALIDTIMESTAMP                     = UA_StatusCode(0x80230000)
	UA_STATUSCODE_BADNONCEINVALID                         = UA_StatusCode(0x80240000)
	UA_STATUSCODE_BADSESSIONIDINVALID                     = UA_StatusCode(0x80250000)
	UA_STATUSCODE_BADSESSIONCLOSED                        = UA_StatusCode(0x80260000)
	UA_STATUSCODE_BADSESSIONNOTACTIVATED                  = UA_StatusCode(0x80270000)
	UA_STATUSCODE_BADSUBSCRIPTIONIDINVALID                = UA_StatusCode(0x80280000)
	UA_STATUSCODE_BADREQUESTHEADERINVALID                 = UA_StatusCode(0x802a0000)
	UA_STATUSCODE_BADTIMESTAMPSTORETURNINVALID            = UA_StatusCode(0x802b0000)
	UA_STATUSCODE_BADREQUESTCANCELLEDBYCLIENT             = UA_StatusCode(0x802c0000)
	UA_STATUSCODE_GOODSUBSCRIPTIONTRANSFERRED             = UA_StatusCode(0x002d0000)
	UA_STATUSCODE_GOODCOMPLETESASYNCHRONOUSLY             = UA_StatusCode(0x002e0000)
	UA_STATUSCODE_GOODOVERLOAD                            = UA_StatusCode(0x002f0000)
	UA_STATUSCODE_GOODCLAMPED                             = UA_StatusCode(0x00300000)
	UA_STATUSCODE_BADNOCOMMUNICATION                      = UA_StatusCode(0x80310000)
	UA_STATUSCODE_BADWAITINGFORINITIALDATA                = UA_StatusCode(0x80320000)
	UA_STATUSCODE_BADNODEIDINVALID                        = UA_StatusCode(0x80330000)
	UA_STATUSCODE_BADNODEIDUNKNOWN                        = UA_StatusCode(0x80340000)
	UA_STATUSCODE_BADATTRIBUTEIDINVALID                   = UA_StatusCode(0x80350000)
	UA_STATUSCODE_BADINDEXRANGEINVALID                    = UA_StatusCode(0x80360000)
	UA_STATUSCODE_BADINDEXRANGENODATA                     = UA_StatusCode(0x80370000)
	UA_STATUSCODE_BADDATAENCODINGINVALID                  = UA_StatusCode(0x80380000)
	UA_STATUSCODE_BADDATAENCODINGUNSUPPORTED              = UA_StatusCode(0x80390000)
	UA_STATUSCODE_BADNOTREADABLE                          = UA_StatusCode(0x803a0000)
	UA_STATUSCODE_BADNOTWRITABLE                          = UA_StatusCode(0x803b0000)
	UA_STATUSCODE_BADOUTOFRANGE                           = UA_StatusCode(0x803c0000)
	UA_STATUSCODE_BADNOTSUPPORTED                         = UA_StatusCode(0x803d0000)
	UA_STATUSCODE_BADNOTFOUND                             = UA_StatusCode(0x803e0000)
	UA_STATUSCODE_BADOBJECTDELETED                        = UA_StatusCode(0x803f0000)
	UA_STATUSCODE_BADNOTIMPLEMENTED                       = UA_StatusCode(0x80400000)
	UA_STATUSCODE_BADMONITORINGMODEINVALID                = UA_StatusCode(0x80410000)
	UA_STATUSCODE_BADMONITOREDITEMIDINVALID               = UA_StatusCode(0x80420000)
	UA_STATUSCODE_BADMONITOREDITEMFILTERINVALID           = UA_StatusCode(0x80430000)
	UA_STATUSCODE_BADMONITOREDITEMFILTERUNSUPPORTED       = UA_StatusCode(0x80440000)
	UA_STATUSCODE_BADFILTERNOTALLOWED                     = UA_StatusCode(0x80450000)
	UA_STATUSCODE_BADSTRUCTUREMISSING                     = UA_StatusCode(0x80460000)
	UA_STATUSCODE_BADEVENTFILTERINVALID                   = UA_StatusCode(0x80470000)
	UA_STATUSCODE_BADCONTENTFILTERINVALID                 = UA_StatusCode(0x80480000)
	UA_STATUSCODE_BADFILTEROPERATORINVALID                = UA_StatusCode(0x80c10000)
	UA_STATUSCODE_BADFILTEROPERATORUNSUPPORTED            = UA_StatusCode(0x80c20000)
	UA_STATUSCODE_BADFILTEROPERANDCOUNTMISMATCH           = UA_StatusCode(0x80c30000)
	UA_STATUSCODE_BADFILTEROPERANDINVALID                 = UA_StatusCode(0x80490000)
	UA_STATUSCODE_BADFILTERELEMENTINVALID                 = UA_StatusCode(0x80c40000)
	UA_STATUSCODE_BADFILTERLITERALINVALID                 = UA_StatusCode(0x80c50000)
	UA_STATUSCODE_BADCONTINUATIONPOINTINVALID             = UA_StatusCode(0x804a0000)
	UA_STATUSCODE_BADNOCONTINUATIONPOINTS                 = UA_StatusCode(0x804b0000)
	UA_STATUSCODE_BADREFERENCETYPEIDINVALID               = UA_StatusCode(0x804c0000)
	UA_STATUSCODE_BADBROWSEDIRECTIONINVALID               = UA_StatusCode(0x804d0000)
	UA_STATUSCODE_BADNODENOTINVIEW                        = UA_StatusCode(0x804e0000)
	UA_STATUSCODE_BADSERVERURIINVALID                     = UA_StatusCode(0x804f0000)
	UA_STATUSCODE_BADSERVERNAMEMISSING                    = UA_StatusCode(0x80500000)
	UA_STATUSCODE_BADDISCOVERYURLMISSING                  = UA_StatusCode(0x80510000)
	UA_STATUSCODE_BADSEMPAHOREFILEMISSING                 = UA_StatusCode(0x80520000)
	UA_STATUSCODE_BADREQUESTTYPEINVALID                   = UA_StatusCode(0x80530000)
	UA_STATUSCODE_BADSECURITYMODEREJECTED                 = UA_StatusCode(0x80540000)
	UA_STATUSCODE_BADSECURITYPOLICYREJECTED               = UA_StatusCode(0x80550000)
	UA_STATUSCODE_BADTOOMANYSESSIONS                      = UA_StatusCode(0x80560000)
	UA_STATUSCODE_BADUSERSIGNATUREINVALID                 = UA_StatusCode(0x80570000)
	UA_STATUSCODE_BADAPPLICATIONSIGNATUREINVALID          = UA_StatusCode(0x80580000)
	UA_STATUSCODE_BADNOVALIDCERTIFICATES                  = UA_StatusCode(0x80590000)
	UA_STATUSCODE_BADIDENTITYCHANGENOTSUPPORTED           = UA_StatusCode(0x80c60000)
	UA_STATUSCODE_BADREQUESTCANCELLEDBYREQUEST            = UA_StatusCode(0x805a0000)
	UA_STATUSCODE_BADPARENTNODEIDINVALID                  = UA_StatusCode(0x805b0000)
	UA_STATUSCODE_BADREFERENCENOTALLOWED                  = UA_StatusCode(0x805c0000)
	UA_STATUSCODE_BADNODEIDREJECTED                       = UA_StatusCode(0x805d0000)
	UA_STATUSCODE_BADNODEIDEXISTS                         = UA_StatusCode(0x805e0000)
	UA_STATUSCODE_BADNODECLASSINVALID                     = UA_StatusCode(0x805f0000)
	UA_STATUSCODE_BADBROWSENAMEINVALID                    = UA_StatusCode(0x80600000)
	UA_STATUSCODE_BADBROWSENAMEDUPLICATED                 = UA_StatusCode(0x80610000)
	UA_STATUSCODE_BADNODEATTRIBUTESINVALID                = UA_StatusCode(0x80620000)
	UA_STATUSCODE_BADTYPEDEFINITIONINVALID                = UA_StatusCode(0x80630000)
	UA_STATUSCODE_BADSOURCENODEIDINVALID                  = UA_StatusCode(0x80640000)
	UA_STATUSCODE_BADTARGETNODEIDINVALID                  = UA_StatusCode(0x80650000)
	UA_STATUSCODE_BADDUPLICATEREFERENCENOTALLOWED         = UA_StatusCode(0x80660000)
	UA_STATUSCODE_BADINVALIDSELFREFERENCE                 = UA_StatusCode(0x80670000)
	UA_STATUSCODE_BADREFERENCELOCALONLY                   = UA_StatusCode(0x80680000)
	UA_STATUSCODE_BADNODELETERIGHTS                       = UA_StatusCode(0x80690000)
	UA_STATUSCODE_UNCERTAINREFERENCENOTDELETED            = UA_StatusCode(0x40bc0000)
	UA_STATUSCODE_BADSERVERINDEXINVALID                   = UA_StatusCode(0x806a0000)
	UA_STATUSCODE_BADVIEWIDUNKNOWN                        = UA_StatusCode(0x806b0000)
	UA_STATUSCODE_BADVIEWTIMESTAMPINVALID                 = UA_StatusCode(0x80c90000)
	UA_STATUSCODE_BADVIEWPARAMETERMISMATCH                = UA_StatusCode(0x80ca0000)
	UA_STATUSCODE_BADVIEWVERSIONINVALID                   = UA_StatusCode(0x80cb0000)
	UA_STATUSCODE_UNCERTAINNOTALLNODESAVAILABLE           = UA_StatusCode(0x40c00000)
	UA_STATUSCODE_GOODRESULTSMAYBEINCOMPLETE              = UA_StatusCode(0x00ba0000)
	UA_STATUSCODE_BADNOTTYPEDEFINITION                    = UA_StatusCode(0x80c80000)
	UA_STATUSCODE_UNCERTAINREFERENCEOUTOFSERVER           = UA_StatusCode(0x406c0000)
	UA_STATUSCODE_BADTOOMANYMATCHES                       = UA_StatusCode(0x806d0000)
	UA_STATUSCODE_BADQUERYTOOCOMPLEX                      = UA_StatusCode(0x806e0000)
	UA_STATUSCODE_BADNOMATCH                              = UA_StatusCode(0x806f0000)
	UA_STATUSCODE_BADMAXAGEINVALID                        = UA_StatusCode(0x80700000)
	UA_STATUSCODE_BADHISTORYOPERATIONINVALID              = UA_StatusCode(0x80710000)
	UA_STATUSCODE_BADHISTORYOPERATIONUNSUPPORTED          = UA_StatusCode(0x80720000)
	UA_STATUSCODE_BADINVALIDTIMESTAMPARGUMENT             = UA_StatusCode(0x80bd0000)
	UA_STATUSCODE_BADWRITENOTSUPPORTED                    = UA_StatusCode(0x80730000)
	UA_STATUSCODE_BADTYPEMISMATCH                         = UA_StatusCode(0x80740000)
	UA_STATUSCODE_BADMETHODINVALID                        = UA_StatusCode(0x80750000)
	UA_STATUSCODE_BADARGUMENTSMISSING                     = UA_StatusCode(0x80760000)
	UA_STATUSCODE_BADTOOMANYSUBSCRIPTIONS                 = UA_StatusCode(0x80770000)
	UA_STATUSCODE_BADTOOMANYPUBLISHREQUESTS               = UA_StatusCode(0x80780000)
	UA_STATUSCODE_BADNOSUBSCRIPTION                       = UA_StatusCode(0x80790000)
	UA_STATUSCODE_BADSEQUENCENUMBERUNKNOWN                = UA_StatusCode(0x807a0000)
	UA_STATUSCODE_BADMESSAGENOTAVAILABLE                  = UA_StatusCode(0x807b0000)
	UA_STATUSCODE_BADINSUFFICIENTCLIENTPROFILE            = UA_StatusCode(0x807c0000)
	UA_STATUSCODE_BADSTATENOTACTIVE                       = UA_StatusCode(0x80bf0000)
	UA_STATUSCODE_BADTCPSERVERTOOBUSY                     = UA_StatusCode(0x807d0000)
	UA_STATUSCODE_BADTCPMESSAGETYPEINVALID                = UA_StatusCode(0x807e0000)
	UA_STATUSCODE_BADTCPSECURECHANNELUNKNOWN              = UA_StatusCode(0x807f0000)
	UA_STATUSCODE_BADTCPMESSAGETOOLARGE                   = UA_StatusCode(0x80800000)
	UA_STATUSCODE_BADTCPNOTENOUGHRESOURCES                = UA_StatusCode(0x80810000)
	UA_STATUSCODE_BADTCPINTERNALERROR                     = UA_StatusCode(0x80820000)
	UA_STATUSCODE_BADTCPENDPOINTURLINVALID                = UA_StatusCode(0x80830000)
	UA_STATUSCODE_BADREQUESTINTERRUPTED                   = UA_StatusCode(0x80840000)
	UA_STATUSCODE_BADREQUESTTIMEOUT                       = UA_StatusCode(0x80850000)
	UA_STATUSCODE_BADSECURECHANNELCLOSED                  = UA_StatusCode(0x80860000)
	UA_STATUSCODE_BADSECURECHANNELTOKENUNKNOWN            = UA_StatusCode(0x80870000)
	UA_STATUSCODE_BADSEQUENCENUMBERINVALID                = UA_StatusCode(0x80880000)
	UA_STATUSCODE_BADPROTOCOLVERSIONUNSUPPORTED           = UA_StatusCode(0x80be0000)
	UA_STATUSCODE_BADCONFIGURATIONERROR                   = UA_StatusCode(0x80890000)
	UA_STATUSCODE_BADNOTCONNECTED                         = UA_StatusCode(0x808a0000)
	UA_STATUSCODE_BADDEVICEFAILURE                        = UA_StatusCode(0x808b0000)
	UA_STATUSCODE_BADSENSORFAILURE                        = UA_StatusCode(0x808c0000)
	UA_STATUSCODE_BADOUTOFSERVICE                         = UA_StatusCode(0x808d0000)
	UA_STATUSCODE_BADDEADBANDFILTERINVALID                = UA_StatusCode(0x808e0000)
	UA_STATUSCODE_UNCERTAINNOCOMMUNICATIONLASTUSABLEVALUE = UA_StatusCode(0x408f0000)
	UA_STATUSCODE_UNCERTAINLASTUSABLEVALUE                = UA_StatusCode(0x40900000)
	UA_STATUSCODE_UNCERTAINSUBSTITUTEVALUE                = UA_StatusCode(0x40910000)
	UA_STATUSCODE_UNCERTAININITIALVALUE                   = UA_StatusCode(0x40920000)
	UA_STATUSCODE_UNCERTAINSENSORNOTACCURATE              = UA_StatusCode(0x40930000)
	UA_STATUSCODE_UNCERTAINENGINEERINGUNITSEXCEEDED       = UA_StatusCode(0x40940000)
	UA_STATUSCODE_UNCERTAINSUBNORMAL                      = UA_StatusCode(0x40950000)
	UA_STATUSCODE_GOODLOCALOVERRIDE                       = UA_StatusCode(0x00960000)
	UA_STATUSCODE_BADREFRESHINPROGRESS                    = UA_StatusCode(0x80970000)
	UA_STATUSCODE_BADCONDITIONALREADYDISABLED             = UA_StatusCode(0x80980000)
	UA_STATUSCODE_BADCONDITIONALREADYENABLED              = UA_StatusCode(0x80cc0000)
	UA_STATUSCODE_BADCONDITIONDISABLED                    = UA_StatusCode(0x80990000)
	UA_STATUSCODE_BADEVENTIDUNKNOWN                       = UA_StatusCode(0x809a0000)
	UA_STATUSCODE_BADEVENTNOTACKNOWLEDGEABLE              = UA_StatusCode(0x80bb0000)
	UA_STATUSCODE_BADDIALOGNOTACTIVE                      = UA_StatusCode(0x80cd0000)
	UA_STATUSCODE_BADDIALOGRESPONSEINVALID                = UA_StatusCode(0x80ce0000)
	UA_STATUSCODE_BADCONDITIONBRANCHALREADYACKED          = UA_StatusCode(0x80cf0000)
	UA_STATUSCODE_BADCONDITIONBRANCHALREADYCONFIRMED      = UA_StatusCode(0x80d00000)
	UA_STATUSCODE_BADCONDITIONALREADYSHELVED              = UA_StatusCode(0x80d10000)
	UA_STATUSCODE_BADCONDITIONNOTSHELVED                  = UA_StatusCode(0x80d20000)
	UA_STATUSCODE_BADSHELVINGTIMEOUTOFRANGE               = UA_StatusCode(0x80d30000)
	UA_STATUSCODE_BADNODATA                               = UA_StatusCode(0x809b0000)
	UA_STATUSCODE_BADBOUNDNOTFOUND                        = UA_StatusCode(0x80d70000)
	UA_STATUSCODE_BADBOUNDNOTSUPPORTED                    = UA_StatusCode(0x80d80000)
	UA_STATUSCODE_BADDATALOST                             = UA_StatusCode(0x809d0000)
	UA_STATUSCODE_BADDATAUNAVAILABLE                      = UA_StatusCode(0x809e0000)
	UA_STATUSCODE_BADENTRYEXISTS                          = UA_StatusCode(0x809f0000)
	UA_STATUSCODE_BADNOENTRYEXISTS                        = UA_StatusCode(0x80a00000)
	UA_STATUSCODE_BADTIMESTAMPNOTSUPPORTED                = UA_StatusCode(0x80a10000)
	UA_STATUSCODE_GOODENTRYINSERTED                       = UA_StatusCode(0x00a20000)
	UA_STATUSCODE_GOODENTRYREPLACED                       = UA_StatusCode(0x00a30000)
	UA_STATUSCODE_UNCERTAINDATASUBNORMAL                  = UA_StatusCode(0x40a40000)
	UA_STATUSCODE_GOODNODATA                              = UA_StatusCode(0x00a50000)
	UA_STATUSCODE_GOODMOREDATA                            = UA_StatusCode(0x00a60000)
	UA_STATUSCODE_BADAGGREGATELISTMISMATCH                = UA_StatusCode(0x80d40000)
	UA_STATUSCODE_BADAGGREGATENOTSUPPORTED                = UA_StatusCode(0x80d50000)
	UA_STATUSCODE_BADAGGREGATEINVALIDINPUTS               = UA_StatusCode(0x80d60000)
	UA_STATUSCODE_BADAGGREGATECONFIGURATIONREJECTED       = UA_StatusCode(0x80da0000)
	UA_STATUSCODE_GOODDATAIGNORED                         = UA_StatusCode(0x00d90000)
	UA_STATUSCODE_GOODCOMMUNICATIONEVENT                  = UA_StatusCode(0x00a70000)
	UA_STATUSCODE_GOODSHUTDOWNEVENT                       = UA_StatusCode(0x00a80000)
	UA_STATUSCODE_GOODCALLAGAIN                           = UA_StatusCode(0x00a90000)
	UA_STATUSCODE_GOODNONCRITICALTIMEOUT                  = UA_StatusCode(0x00aa0000)
	UA_STATUSCODE_BADINVALIDARGUMENT                      = UA_StatusCode(0x80ab0000)
	UA_STATUSCODE_BADCONNECTIONREJECTED                   = UA_StatusCode(0x80ac0000)
	UA_STATUSCODE_BADDISCONNECT                           = UA_StatusCode(0x80ad0000)
	UA_STATUSCODE_BADCONNECTIONCLOSED                     = UA_StatusCode(0x80ae0000)
	UA_STATUSCODE_BADINVALIDSTATE                         = UA_StatusCode(0x80af0000)
	UA_STATUSCODE_BADENDOFSTREAM                          = UA_StatusCode(0x80b00000)
	UA_STATUSCODE_BADNODATAAVAILABLE                      = UA_StatusCode(0x80b10000)
	UA_STATUSCODE_BADWAITINGFORRESPONSE                   = UA_StatusCode(0x80b20000)
	UA_STATUSCODE_BADOPERATIONABANDONED                   = UA_StatusCode(0x80b30000)
	UA_STATUSCODE_BADEXPECTEDSTREAMTOBLOCK                = UA_StatusCode(0x80b40000)
	UA_STATUSCODE_BADWOULDBLOCK                           = UA_StatusCode(0x80b50000)
	UA_STATUSCODE_BADSYNTAXERROR                          = UA_StatusCode(0x80b60000)
	UA_STATUSCODE_BADMAXCONNECTIONSREACHED                = UA_StatusCode(0x80b70000)
)

type UA_Boolean bool
type UA_SByte int8
type UA_Byte uint8
type UA_Int16 int16
type UA_UInt16 uint16
type UA_Int32 int32
type UA_UInt32 uint32
type UA_Int64 int64
type UA_UInt64 uint64
type UA_Float float32
type UA_Double float64
type UA_String struct {
	Length uint64
	Data   *uint8
}
type UA_DateTime int64
type UA_DateTimeStruct struct {
	NanoSec  uint16
	MicroSec uint16
	MilliSec uint16
	Sec      uint16
	Min      uint16
	Hour     uint16
	Day      uint16
	Month    uint16
	Year     uint16
}
type UA_Guid struct {
	Data1 uint32
	Data2 uint16
	Data3 uint16
	Data4 [8]uint8
}
type UA_ByteString struct {
	Length uint64
	Data   *uint8
}
type UA_XmlElement struct {
	Length uint64
	Data   *uint8
}
type UA_NodeId struct {
	NamespaceIndex uint16
	Pad_cgo_0      [2]byte
	IdentifierType uint32
	Identifier     [16]byte
}
type UA_ExpandedNodeId struct {
	NodeId       UA_NodeId
	NamespaceUri UA_ByteString
	ServerIndex  uint32
	Pad_cgo_0    [4]byte
}
type UA_QualifiedName struct {
	NamespaceIndex uint16
	Pad_cgo_0      [6]byte
	Name           UA_ByteString
}
type UA_LocalizedText struct {
	Locale UA_ByteString
	Text   UA_ByteString
}
type UA_DataType struct {
	TypeId      UA_NodeId
	MemSize     uint16
	TypeIndex   uint16
	MembersSize uint8
	Pad_cgo_0   [3]byte
	Members     *UA_DataTypeMember
}
type UA_ExtensionObject struct {
	Encoding  uint32
	Pad_cgo_0 [4]byte
	Content   [40]byte
}
type UA_Variant struct {
	Type                *UA_DataType
	StorageType         uint32
	Pad_cgo_0           [4]byte
	ArrayLength         uint64
	Data                *byte
	ArrayDimensionsSize uint64
	ArrayDimensions     *uint32
}
type UA_NumericRangeDimension struct {
	Min uint32
	Max uint32
}
type UA_NumericRange struct {
	DimensionsSize uint64
	Dimensions     *UA_NumericRangeDimension
}
type UA_DataValue struct {
	Pad_cgo_0         [8]byte
	Value             UA_Variant
	Status            uint32
	Pad_cgo_1         [4]byte
	SourceTimestamp   int64
	SourcePicoseconds uint16
	Pad_cgo_2         [6]byte
	ServerTimestamp   int64
	ServerPicoseconds uint16
	Pad_cgo_3         [6]byte
}
type UA_DiagnosticInfo struct {
	Pad_cgo_0           [4]byte
	SymbolicId          int32
	NamespaceUri        int32
	LocalizedText       int32
	Locale              int32
	Pad_cgo_1           [4]byte
	AdditionalInfo      UA_ByteString
	InnerStatusCode     uint32
	Pad_cgo_2           [4]byte
	InnerDiagnosticInfo *UA_DiagnosticInfo
}
type UA_DataTypeMember struct {
	MemberTypeIndex uint16
	Padding         uint8
	Pad_cgo_0       [1]byte
}
type UA_AttributeId uint32

const (
	UA_ATTRIBUTEID_NODEID                  = UA_AttributeId(1)
	UA_ATTRIBUTEID_NODECLASS               = UA_AttributeId(2)
	UA_ATTRIBUTEID_BROWSENAME              = UA_AttributeId(3)
	UA_ATTRIBUTEID_DISPLAYNAME             = UA_AttributeId(4)
	UA_ATTRIBUTEID_DESCRIPTION             = UA_AttributeId(5)
	UA_ATTRIBUTEID_WRITEMASK               = UA_AttributeId(6)
	UA_ATTRIBUTEID_USERWRITEMASK           = UA_AttributeId(7)
	UA_ATTRIBUTEID_ISABSTRACT              = UA_AttributeId(8)
	UA_ATTRIBUTEID_SYMMETRIC               = UA_AttributeId(9)
	UA_ATTRIBUTEID_INVERSENAME             = UA_AttributeId(10)
	UA_ATTRIBUTEID_CONTAINSNOLOOPS         = UA_AttributeId(11)
	UA_ATTRIBUTEID_EVENTNOTIFIER           = UA_AttributeId(12)
	UA_ATTRIBUTEID_VALUE                   = UA_AttributeId(13)
	UA_ATTRIBUTEID_DATATYPE                = UA_AttributeId(14)
	UA_ATTRIBUTEID_VALUERANK               = UA_AttributeId(15)
	UA_ATTRIBUTEID_ARRAYDIMENSIONS         = UA_AttributeId(16)
	UA_ATTRIBUTEID_ACCESSLEVEL             = UA_AttributeId(17)
	UA_ATTRIBUTEID_USERACCESSLEVEL         = UA_AttributeId(18)
	UA_ATTRIBUTEID_MINIMUMSAMPLINGINTERVAL = UA_AttributeId(19)
	UA_ATTRIBUTEID_HISTORIZING             = UA_AttributeId(20)
	UA_ATTRIBUTEID_EXECUTABLE              = UA_AttributeId(21)
	UA_ATTRIBUTEID_USEREXECUTABLE          = UA_AttributeId(22)
)

type UA_AccessLevelMask uint32

const (
	UA_ACCESSLEVELMASK_READ           = UA_AccessLevelMask(0x01)
	UA_ACCESSLEVELMASK_WRITE          = UA_AccessLevelMask(0x02)
	UA_ACCESSLEVELMASK_HISTORYREAD    = UA_AccessLevelMask(0x4)
	UA_ACCESSLEVELMASK_HISTORYWRITE   = UA_AccessLevelMask(0x08)
	UA_ACCESSLEVELMASK_SEMANTICCHANGE = UA_AccessLevelMask(0x10)
)

type UA_IdType uint32

const (
	UA_IDTYPE_NUMERIC = UA_IdType(0)
	UA_IDTYPE_STRING  = UA_IdType(1)
	UA_IDTYPE_GUID    = UA_IdType(2)
	UA_IDTYPE_OPAQUE  = UA_IdType(3)
)

type UA_NodeClass uint32

const (
	UA_NODECLASS_UNSPECIFIED   = UA_NodeClass(0)
	UA_NODECLASS_OBJECT        = UA_NodeClass(1)
	UA_NODECLASS_VARIABLE      = UA_NodeClass(2)
	UA_NODECLASS_METHOD        = UA_NodeClass(4)
	UA_NODECLASS_OBJECTTYPE    = UA_NodeClass(8)
	UA_NODECLASS_VARIABLETYPE  = UA_NodeClass(16)
	UA_NODECLASS_REFERENCETYPE = UA_NodeClass(32)
	UA_NODECLASS_DATATYPE      = UA_NodeClass(64)
	UA_NODECLASS_VIEW          = UA_NodeClass(128)
)

type UA_ReferenceNode struct {
	ReferenceTypeId UA_NodeId
	IsInverse       bool
	Pad_cgo_0       [7]byte
	TargetId        UA_ExpandedNodeId
}
type UA_Argument struct {
	Name                UA_ByteString
	DataType            UA_NodeId
	ValueRank           int32
	Pad_cgo_0           [4]byte
	ArrayDimensionsSize uint64
	ArrayDimensions     *uint32
	Description         UA_LocalizedText
}
type UA_ApplicationType uint32

const (
	UA_APPLICATIONTYPE_SERVER          = UA_ApplicationType(0)
	UA_APPLICATIONTYPE_CLIENT          = UA_ApplicationType(1)
	UA_APPLICATIONTYPE_CLIENTANDSERVER = UA_ApplicationType(2)
	UA_APPLICATIONTYPE_DISCOVERYSERVER = UA_ApplicationType(3)
)

type UA_ApplicationDescription struct {
	ApplicationUri      UA_ByteString
	ProductUri          UA_ByteString
	ApplicationName     UA_LocalizedText
	ApplicationType     uint32
	Pad_cgo_0           [4]byte
	GatewayServerUri    UA_ByteString
	DiscoveryProfileUri UA_ByteString
	DiscoveryUrlsSize   uint64
	DiscoveryUrls       *UA_ByteString
}
type UA_RequestHeader struct {
	AuthenticationToken UA_NodeId
	Timestamp           int64
	RequestHandle       uint32
	ReturnDiagnostics   uint32
	AuditEntryId        UA_ByteString
	TimeoutHint         uint32
	Pad_cgo_0           [4]byte
	AdditionalHeader    UA_ExtensionObject
}
type UA_ResponseHeader struct {
	Timestamp          int64
	RequestHandle      uint32
	ServiceResult      uint32
	ServiceDiagnostics UA_DiagnosticInfo
	StringTableSize    uint64
	StringTable        *UA_ByteString
	AdditionalHeader   UA_ExtensionObject
}
type UA_ServiceFault struct {
	ResponseHeader UA_ResponseHeader
}
type UA_FindServersRequest struct {
	RequestHeader  UA_RequestHeader
	EndpointUrl    UA_ByteString
	LocaleIdsSize  uint64
	LocaleIds      *UA_ByteString
	ServerUrisSize uint64
	ServerUris     *UA_ByteString
}
type UA_FindServersResponse struct {
	ResponseHeader UA_ResponseHeader
	ServersSize    uint64
	Servers        *UA_ApplicationDescription
}
type UA_MessageSecurityMode uint32

const (
	UA_MESSAGESECURITYMODE_INVALID        = UA_MessageSecurityMode(0)
	UA_MESSAGESECURITYMODE_NONE           = UA_MessageSecurityMode(1)
	UA_MESSAGESECURITYMODE_SIGN           = UA_MessageSecurityMode(2)
	UA_MESSAGESECURITYMODE_SIGNANDENCRYPT = UA_MessageSecurityMode(3)
)

type UA_UserTokenType uint32

const (
	UA_USERTOKENTYPE_ANONYMOUS   = UA_UserTokenType(0)
	UA_USERTOKENTYPE_USERNAME    = UA_UserTokenType(1)
	UA_USERTOKENTYPE_CERTIFICATE = UA_UserTokenType(2)
	UA_USERTOKENTYPE_ISSUEDTOKEN = UA_UserTokenType(3)
)

type UA_UserTokenPolicy struct {
	PolicyId          UA_ByteString
	TokenType         uint32
	Pad_cgo_0         [4]byte
	IssuedTokenType   UA_ByteString
	IssuerEndpointUrl UA_ByteString
	SecurityPolicyUri UA_ByteString
}
type UA_EndpointDescription struct {
	EndpointUrl            UA_ByteString
	Server                 UA_ApplicationDescription
	ServerCertificate      UA_ByteString
	SecurityMode           uint32
	Pad_cgo_0              [4]byte
	SecurityPolicyUri      UA_ByteString
	UserIdentityTokensSize uint64
	UserIdentityTokens     *UA_UserTokenPolicy
	TransportProfileUri    UA_ByteString
	SecurityLevel          uint8
	Pad_cgo_1              [7]byte
}
type UA_GetEndpointsRequest struct {
	RequestHeader   UA_RequestHeader
	EndpointUrl     UA_ByteString
	LocaleIdsSize   uint64
	LocaleIds       *UA_ByteString
	ProfileUrisSize uint64
	ProfileUris     *UA_ByteString
}
type UA_GetEndpointsResponse struct {
	ResponseHeader UA_ResponseHeader
	EndpointsSize  uint64
	Endpoints      *UA_EndpointDescription
}
type UA_SecurityTokenRequestType uint32

const (
	UA_SECURITYTOKENREQUESTTYPE_ISSUE = UA_SecurityTokenRequestType(0)
	UA_SECURITYTOKENREQUESTTYPE_RENEW = UA_SecurityTokenRequestType(1)
)

type UA_ChannelSecurityToken struct {
	ChannelId       uint32
	TokenId         uint32
	CreatedAt       int64
	RevisedLifetime uint32
	Pad_cgo_0       [4]byte
}
type UA_OpenSecureChannelRequest struct {
	RequestHeader         UA_RequestHeader
	ClientProtocolVersion uint32
	RequestType           uint32
	SecurityMode          uint32
	Pad_cgo_0             [4]byte
	ClientNonce           UA_ByteString
	RequestedLifetime     uint32
	Pad_cgo_1             [4]byte
}
type UA_OpenSecureChannelResponse struct {
	ResponseHeader        UA_ResponseHeader
	ServerProtocolVersion uint32
	Pad_cgo_0             [4]byte
	SecurityToken         UA_ChannelSecurityToken
	ServerNonce           UA_ByteString
}
type UA_CloseSecureChannelRequest struct {
	RequestHeader UA_RequestHeader
}
type UA_CloseSecureChannelResponse struct {
	ResponseHeader UA_ResponseHeader
}
type UA_SignedSoftwareCertificate struct {
	CertificateData UA_ByteString
	Signature       UA_ByteString
}
type UA_SignatureData struct {
	Algorithm UA_ByteString
	Signature UA_ByteString
}
type UA_CreateSessionRequest struct {
	RequestHeader           UA_RequestHeader
	ClientDescription       UA_ApplicationDescription
	ServerUri               UA_ByteString
	EndpointUrl             UA_ByteString
	SessionName             UA_ByteString
	ClientNonce             UA_ByteString
	ClientCertificate       UA_ByteString
	RequestedSessionTimeout float64
	MaxResponseMessageSize  uint32
	Pad_cgo_0               [4]byte
}
type UA_CreateSessionResponse struct {
	ResponseHeader                 UA_ResponseHeader
	SessionId                      UA_NodeId
	AuthenticationToken            UA_NodeId
	RevisedSessionTimeout          float64
	ServerNonce                    UA_ByteString
	ServerCertificate              UA_ByteString
	ServerEndpointsSize            uint64
	ServerEndpoints                *UA_EndpointDescription
	ServerSoftwareCertificatesSize uint64
	ServerSoftwareCertificates     *UA_SignedSoftwareCertificate
	ServerSignature                UA_SignatureData
	MaxRequestMessageSize          uint32
	Pad_cgo_0                      [4]byte
}
type UA_UserIdentityToken struct {
	PolicyId UA_ByteString
}
type UA_AnonymousIdentityToken struct {
	PolicyId UA_ByteString
}
type UA_UserNameIdentityToken struct {
	PolicyId            UA_ByteString
	UserName            UA_ByteString
	Password            UA_ByteString
	EncryptionAlgorithm UA_ByteString
}
type UA_ActivateSessionRequest struct {
	RequestHeader                  UA_RequestHeader
	ClientSignature                UA_SignatureData
	ClientSoftwareCertificatesSize uint64
	ClientSoftwareCertificates     *UA_SignedSoftwareCertificate
	LocaleIdsSize                  uint64
	LocaleIds                      *UA_ByteString
	UserIdentityToken              UA_ExtensionObject
	UserTokenSignature             UA_SignatureData
}
type UA_ActivateSessionResponse struct {
	ResponseHeader      UA_ResponseHeader
	ServerNonce         UA_ByteString
	ResultsSize         uint64
	Results             *uint32
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_CloseSessionRequest struct {
	RequestHeader       UA_RequestHeader
	DeleteSubscriptions bool
	Pad_cgo_0           [7]byte
}
type UA_CloseSessionResponse struct {
	ResponseHeader UA_ResponseHeader
}
type UA_NodeAttributesMask uint32

const (
	UA_NODEATTRIBUTESMASK_NONE                    = UA_NodeAttributesMask(0)
	UA_NODEATTRIBUTESMASK_ACCESSLEVEL             = UA_NodeAttributesMask(1)
	UA_NODEATTRIBUTESMASK_ARRAYDIMENSIONS         = UA_NodeAttributesMask(2)
	UA_NODEATTRIBUTESMASK_BROWSENAME              = UA_NodeAttributesMask(4)
	UA_NODEATTRIBUTESMASK_CONTAINSNOLOOPS         = UA_NodeAttributesMask(8)
	UA_NODEATTRIBUTESMASK_DATATYPE                = UA_NodeAttributesMask(16)
	UA_NODEATTRIBUTESMASK_DESCRIPTION             = UA_NodeAttributesMask(32)
	UA_NODEATTRIBUTESMASK_DISPLAYNAME             = UA_NodeAttributesMask(64)
	UA_NODEATTRIBUTESMASK_EVENTNOTIFIER           = UA_NodeAttributesMask(128)
	UA_NODEATTRIBUTESMASK_EXECUTABLE              = UA_NodeAttributesMask(256)
	UA_NODEATTRIBUTESMASK_HISTORIZING             = UA_NodeAttributesMask(512)
	UA_NODEATTRIBUTESMASK_INVERSENAME             = UA_NodeAttributesMask(1024)
	UA_NODEATTRIBUTESMASK_ISABSTRACT              = UA_NodeAttributesMask(2048)
	UA_NODEATTRIBUTESMASK_MINIMUMSAMPLINGINTERVAL = UA_NodeAttributesMask(4096)
	UA_NODEATTRIBUTESMASK_NODECLASS               = UA_NodeAttributesMask(8192)
	UA_NODEATTRIBUTESMASK_NODEID                  = UA_NodeAttributesMask(16384)
	UA_NODEATTRIBUTESMASK_SYMMETRIC               = UA_NodeAttributesMask(32768)
	UA_NODEATTRIBUTESMASK_USERACCESSLEVEL         = UA_NodeAttributesMask(65536)
	UA_NODEATTRIBUTESMASK_USEREXECUTABLE          = UA_NodeAttributesMask(131072)
	UA_NODEATTRIBUTESMASK_USERWRITEMASK           = UA_NodeAttributesMask(262144)
	UA_NODEATTRIBUTESMASK_VALUERANK               = UA_NodeAttributesMask(524288)
	UA_NODEATTRIBUTESMASK_WRITEMASK               = UA_NodeAttributesMask(1048576)
	UA_NODEATTRIBUTESMASK_VALUE                   = UA_NodeAttributesMask(2097152)
	UA_NODEATTRIBUTESMASK_ALL                     = UA_NodeAttributesMask(4194303)
	UA_NODEATTRIBUTESMASK_BASENODE                = UA_NodeAttributesMask(1335396)
	UA_NODEATTRIBUTESMASK_OBJECT                  = UA_NodeAttributesMask(1335524)
	UA_NODEATTRIBUTESMASK_OBJECTTYPEORDATATYPE    = UA_NodeAttributesMask(1337444)
	UA_NODEATTRIBUTESMASK_VARIABLE                = UA_NodeAttributesMask(4026999)
	UA_NODEATTRIBUTESMASK_VARIABLETYPE            = UA_NodeAttributesMask(3958902)
	UA_NODEATTRIBUTESMASK_METHOD                  = UA_NodeAttributesMask(1466724)
	UA_NODEATTRIBUTESMASK_REFERENCETYPE           = UA_NodeAttributesMask(1371236)
	UA_NODEATTRIBUTESMASK_VIEW                    = UA_NodeAttributesMask(1335532)
)

type UA_NodeAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
}
type UA_ObjectAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
	EventNotifier       uint8
	Pad_cgo_1           [7]byte
}
type UA_VariableAttributes struct {
	SpecifiedAttributes     uint32
	Pad_cgo_0               [4]byte
	DisplayName             UA_LocalizedText
	Description             UA_LocalizedText
	WriteMask               uint32
	UserWriteMask           uint32
	Value                   UA_Variant
	DataType                UA_NodeId
	ValueRank               int32
	Pad_cgo_1               [4]byte
	ArrayDimensionsSize     uint64
	ArrayDimensions         *uint32
	AccessLevel             uint8
	UserAccessLevel         uint8
	Pad_cgo_2               [6]byte
	MinimumSamplingInterval float64
	Historizing             bool
	Pad_cgo_3               [7]byte
}
type UA_MethodAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
	Executable          bool
	UserExecutable      bool
	Pad_cgo_1           [6]byte
}
type UA_ObjectTypeAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
	IsAbstract          bool
	Pad_cgo_1           [7]byte
}
type UA_VariableTypeAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
	Value               UA_Variant
	DataType            UA_NodeId
	ValueRank           int32
	Pad_cgo_1           [4]byte
	ArrayDimensionsSize uint64
	ArrayDimensions     *uint32
	IsAbstract          bool
	Pad_cgo_2           [7]byte
}
type UA_ReferenceTypeAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
	IsAbstract          bool
	Symmetric           bool
	Pad_cgo_1           [6]byte
	InverseName         UA_LocalizedText
}
type UA_DataTypeAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
	IsAbstract          bool
	Pad_cgo_1           [7]byte
}
type UA_ViewAttributes struct {
	SpecifiedAttributes uint32
	Pad_cgo_0           [4]byte
	DisplayName         UA_LocalizedText
	Description         UA_LocalizedText
	WriteMask           uint32
	UserWriteMask       uint32
	ContainsNoLoops     bool
	EventNotifier       uint8
	Pad_cgo_1           [6]byte
}
type UA_AddNodesItem struct {
	ParentNodeId       UA_ExpandedNodeId
	ReferenceTypeId    UA_NodeId
	RequestedNewNodeId UA_ExpandedNodeId
	BrowseName         UA_QualifiedName
	NodeClass          uint32
	Pad_cgo_0          [4]byte
	NodeAttributes     UA_ExtensionObject
	TypeDefinition     UA_ExpandedNodeId
}
type UA_AddNodesResult struct {
	StatusCode  uint32
	Pad_cgo_0   [4]byte
	AddedNodeId UA_NodeId
}
type UA_AddNodesRequest struct {
	RequestHeader  UA_RequestHeader
	NodesToAddSize uint64
	NodesToAdd     *UA_AddNodesItem
}
type UA_AddNodesResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *UA_AddNodesResult
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_AddReferencesItem struct {
	SourceNodeId    UA_NodeId
	ReferenceTypeId UA_NodeId
	IsForward       bool
	Pad_cgo_0       [7]byte
	TargetServerUri UA_ByteString
	TargetNodeId    UA_ExpandedNodeId
	TargetNodeClass uint32
	Pad_cgo_1       [4]byte
}
type UA_AddReferencesRequest struct {
	RequestHeader       UA_RequestHeader
	ReferencesToAddSize uint64
	ReferencesToAdd     *UA_AddReferencesItem
}
type UA_AddReferencesResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *uint32
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_DeleteNodesItem struct {
	NodeId                 UA_NodeId
	DeleteTargetReferences bool
	Pad_cgo_0              [7]byte
}
type UA_DeleteNodesRequest struct {
	RequestHeader     UA_RequestHeader
	NodesToDeleteSize uint64
	NodesToDelete     *UA_DeleteNodesItem
}
type UA_DeleteNodesResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *uint32
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_DeleteReferencesItem struct {
	SourceNodeId        UA_NodeId
	ReferenceTypeId     UA_NodeId
	IsForward           bool
	Pad_cgo_0           [7]byte
	TargetNodeId        UA_ExpandedNodeId
	DeleteBidirectional bool
	Pad_cgo_1           [7]byte
}
type UA_DeleteReferencesRequest struct {
	RequestHeader          UA_RequestHeader
	ReferencesToDeleteSize uint64
	ReferencesToDelete     *UA_DeleteReferencesItem
}
type UA_DeleteReferencesResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *uint32
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_BrowseDirection uint32

const (
	UA_BROWSEDIRECTION_FORWARD = UA_BrowseDirection(0)
	UA_BROWSEDIRECTION_INVERSE = UA_BrowseDirection(1)
	UA_BROWSEDIRECTION_BOTH    = UA_BrowseDirection(2)
)

type UA_ViewDescription struct {
	ViewId      UA_NodeId
	Timestamp   int64
	ViewVersion uint32
	Pad_cgo_0   [4]byte
}
type UA_BrowseDescription struct {
	NodeId          UA_NodeId
	BrowseDirection uint32
	Pad_cgo_0       [4]byte
	ReferenceTypeId UA_NodeId
	IncludeSubtypes bool
	Pad_cgo_1       [3]byte
	NodeClassMask   uint32
	ResultMask      uint32
	Pad_cgo_2       [4]byte
}
type UA_BrowseResultMask uint32

const (
	UA_BROWSERESULTMASK_NONE              = UA_BrowseResultMask(0)
	UA_BROWSERESULTMASK_REFERENCETYPEID   = UA_BrowseResultMask(1)
	UA_BROWSERESULTMASK_ISFORWARD         = UA_BrowseResultMask(2)
	UA_BROWSERESULTMASK_NODECLASS         = UA_BrowseResultMask(4)
	UA_BROWSERESULTMASK_BROWSENAME        = UA_BrowseResultMask(8)
	UA_BROWSERESULTMASK_DISPLAYNAME       = UA_BrowseResultMask(16)
	UA_BROWSERESULTMASK_TYPEDEFINITION    = UA_BrowseResultMask(32)
	UA_BROWSERESULTMASK_ALL               = UA_BrowseResultMask(63)
	UA_BROWSERESULTMASK_REFERENCETYPEINFO = UA_BrowseResultMask(3)
	UA_BROWSERESULTMASK_TARGETINFO        = UA_BrowseResultMask(60)
)

type UA_ReferenceDescription struct {
	ReferenceTypeId UA_NodeId
	IsForward       bool
	Pad_cgo_0       [7]byte
	NodeId          UA_ExpandedNodeId
	BrowseName      UA_QualifiedName
	DisplayName     UA_LocalizedText
	NodeClass       uint32
	Pad_cgo_1       [4]byte
	TypeDefinition  UA_ExpandedNodeId
}
type UA_BrowseResult struct {
	StatusCode        uint32
	Pad_cgo_0         [4]byte
	ContinuationPoint UA_ByteString
	ReferencesSize    uint64
	References        *UA_ReferenceDescription
}
type UA_BrowseRequest struct {
	RequestHeader                 UA_RequestHeader
	View                          UA_ViewDescription
	RequestedMaxReferencesPerNode uint32
	Pad_cgo_0                     [4]byte
	NodesToBrowseSize             uint64
	NodesToBrowse                 *UA_BrowseDescription
}
type UA_BrowseResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *UA_BrowseResult
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_BrowseNextRequest struct {
	RequestHeader             UA_RequestHeader
	ReleaseContinuationPoints bool
	Pad_cgo_0                 [7]byte
	ContinuationPointsSize    uint64
	ContinuationPoints        *UA_ByteString
}
type UA_BrowseNextResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *UA_BrowseResult
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_RelativePathElement struct {
	ReferenceTypeId UA_NodeId
	IsInverse       bool
	IncludeSubtypes bool
	Pad_cgo_0       [6]byte
	TargetName      UA_QualifiedName
}
type UA_RelativePath struct {
	ElementsSize uint64
	Elements     *UA_RelativePathElement
}
type UA_BrowsePath struct {
	StartingNode UA_NodeId
	RelativePath UA_RelativePath
}
type UA_BrowsePathTarget struct {
	TargetId           UA_ExpandedNodeId
	RemainingPathIndex uint32
	Pad_cgo_0          [4]byte
}
type UA_BrowsePathResult struct {
	StatusCode  uint32
	Pad_cgo_0   [4]byte
	TargetsSize uint64
	Targets     *UA_BrowsePathTarget
}
type UA_TranslateBrowsePathsToNodeIdsRequest struct {
	RequestHeader   UA_RequestHeader
	BrowsePathsSize uint64
	BrowsePaths     *UA_BrowsePath
}
type UA_TranslateBrowsePathsToNodeIdsResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *UA_BrowsePathResult
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_RegisterNodesRequest struct {
	RequestHeader       UA_RequestHeader
	NodesToRegisterSize uint64
	NodesToRegister     *UA_NodeId
}
type UA_RegisterNodesResponse struct {
	ResponseHeader        UA_ResponseHeader
	RegisteredNodeIdsSize uint64
	RegisteredNodeIds     *UA_NodeId
}
type UA_UnregisterNodesRequest struct {
	RequestHeader         UA_RequestHeader
	NodesToUnregisterSize uint64
	NodesToUnregister     *UA_NodeId
}
type UA_UnregisterNodesResponse struct {
	ResponseHeader UA_ResponseHeader
}
type UA_QueryDataDescription struct {
	RelativePath UA_RelativePath
	AttributeId  uint32
	Pad_cgo_0    [4]byte
	IndexRange   UA_ByteString
}
type UA_NodeTypeDescription struct {
	TypeDefinitionNode UA_ExpandedNodeId
	IncludeSubTypes    bool
	Pad_cgo_0          [7]byte
	DataToReturnSize   uint64
	DataToReturn       *UA_QueryDataDescription
}
type UA_FilterOperator uint32

const (
	UA_FILTEROPERATOR_EQUALS             = UA_FilterOperator(0)
	UA_FILTEROPERATOR_ISNULL             = UA_FilterOperator(1)
	UA_FILTEROPERATOR_GREATERTHAN        = UA_FilterOperator(2)
	UA_FILTEROPERATOR_LESSTHAN           = UA_FilterOperator(3)
	UA_FILTEROPERATOR_GREATERTHANOREQUAL = UA_FilterOperator(4)
	UA_FILTEROPERATOR_LESSTHANOREQUAL    = UA_FilterOperator(5)
	UA_FILTEROPERATOR_LIKE               = UA_FilterOperator(6)
	UA_FILTEROPERATOR_NOT                = UA_FilterOperator(7)
	UA_FILTEROPERATOR_BETWEEN            = UA_FilterOperator(8)
	UA_FILTEROPERATOR_INLIST             = UA_FilterOperator(9)
	UA_FILTEROPERATOR_AND                = UA_FilterOperator(10)
	UA_FILTEROPERATOR_OR                 = UA_FilterOperator(11)
	UA_FILTEROPERATOR_CAST               = UA_FilterOperator(12)
	UA_FILTEROPERATOR_INVIEW             = UA_FilterOperator(13)
	UA_FILTEROPERATOR_OFTYPE             = UA_FilterOperator(14)
	UA_FILTEROPERATOR_RELATEDTO          = UA_FilterOperator(15)
	UA_FILTEROPERATOR_BITWISEAND         = UA_FilterOperator(16)
	UA_FILTEROPERATOR_BITWISEOR          = UA_FilterOperator(17)
)

type UA_QueryDataSet struct {
	NodeId             UA_ExpandedNodeId
	TypeDefinitionNode UA_ExpandedNodeId
	ValuesSize         uint64
	Values             *UA_Variant
}
type UA_ContentFilterElement struct {
	FilterOperator     uint32
	Pad_cgo_0          [4]byte
	FilterOperandsSize uint64
	FilterOperands     *UA_ExtensionObject
}
type UA_ContentFilter struct {
	ElementsSize uint64
	Elements     *UA_ContentFilterElement
}
type UA_ContentFilterElementResult struct {
	StatusCode                 uint32
	Pad_cgo_0                  [4]byte
	OperandStatusCodesSize     uint64
	OperandStatusCodes         *uint32
	OperandDiagnosticInfosSize uint64
	OperandDiagnosticInfos     *UA_DiagnosticInfo
}
type UA_ContentFilterResult struct {
	ElementResultsSize         uint64
	ElementResults             *UA_ContentFilterElementResult
	ElementDiagnosticInfosSize uint64
	ElementDiagnosticInfos     *UA_DiagnosticInfo
}
type UA_ParsingResult struct {
	StatusCode              uint32
	Pad_cgo_0               [4]byte
	DataStatusCodesSize     uint64
	DataStatusCodes         *uint32
	DataDiagnosticInfosSize uint64
	DataDiagnosticInfos     *UA_DiagnosticInfo
}
type UA_QueryFirstRequest struct {
	RequestHeader         UA_RequestHeader
	View                  UA_ViewDescription
	NodeTypesSize         uint64
	NodeTypes             *UA_NodeTypeDescription
	Filter                UA_ContentFilter
	MaxDataSetsToReturn   uint32
	MaxReferencesToReturn uint32
}
type UA_QueryFirstResponse struct {
	ResponseHeader      UA_ResponseHeader
	QueryDataSetsSize   uint64
	QueryDataSets       *UA_QueryDataSet
	ContinuationPoint   UA_ByteString
	ParsingResultsSize  uint64
	ParsingResults      *UA_ParsingResult
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
	FilterResult        UA_ContentFilterResult
}
type UA_QueryNextRequest struct {
	RequestHeader            UA_RequestHeader
	ReleaseContinuationPoint bool
	Pad_cgo_0                [7]byte
	ContinuationPoint        UA_ByteString
}
type UA_QueryNextResponse struct {
	ResponseHeader           UA_ResponseHeader
	QueryDataSetsSize        uint64
	QueryDataSets            *UA_QueryDataSet
	RevisedContinuationPoint UA_ByteString
}
type UA_TimestampsToReturn uint32

const (
	UA_TIMESTAMPSTORETURN_SOURCE  = UA_TimestampsToReturn(0)
	UA_TIMESTAMPSTORETURN_SERVER  = UA_TimestampsToReturn(1)
	UA_TIMESTAMPSTORETURN_BOTH    = UA_TimestampsToReturn(2)
	UA_TIMESTAMPSTORETURN_NEITHER = UA_TimestampsToReturn(3)
)

type UA_ReadValueId struct {
	NodeId       UA_NodeId
	AttributeId  uint32
	Pad_cgo_0    [4]byte
	IndexRange   UA_ByteString
	DataEncoding UA_QualifiedName
}
type UA_ReadRequest struct {
	RequestHeader      UA_RequestHeader
	MaxAge             float64
	TimestampsToReturn uint32
	Pad_cgo_0          [4]byte
	NodesToReadSize    uint64
	NodesToRead        *UA_ReadValueId
}
type UA_ReadResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *UA_DataValue
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_WriteValue struct {
	NodeId      UA_NodeId
	AttributeId uint32
	Pad_cgo_0   [4]byte
	IndexRange  UA_ByteString
	Value       UA_DataValue
}
type UA_WriteRequest struct {
	RequestHeader    UA_RequestHeader
	NodesToWriteSize uint64
	NodesToWrite     *UA_WriteValue
}
type UA_WriteResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *uint32
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_CallMethodRequest struct {
	ObjectId           UA_NodeId
	MethodId           UA_NodeId
	InputArgumentsSize uint64
	InputArguments     *UA_Variant
}
type UA_CallMethodResult struct {
	StatusCode                       uint32
	Pad_cgo_0                        [4]byte
	InputArgumentResultsSize         uint64
	InputArgumentResults             *uint32
	InputArgumentDiagnosticInfosSize uint64
	InputArgumentDiagnosticInfos     *UA_DiagnosticInfo
	OutputArgumentsSize              uint64
	OutputArguments                  *UA_Variant
}
type UA_CallRequest struct {
	RequestHeader     UA_RequestHeader
	MethodsToCallSize uint64
	MethodsToCall     *UA_CallMethodRequest
}
type UA_CallResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *UA_CallMethodResult
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_MonitoringMode uint32

const (
	UA_MONITORINGMODE_DISABLED  = UA_MonitoringMode(0)
	UA_MONITORINGMODE_SAMPLING  = UA_MonitoringMode(1)
	UA_MONITORINGMODE_REPORTING = UA_MonitoringMode(2)
)

type UA_MonitoringParameters struct {
	ClientHandle     uint32
	Pad_cgo_0        [4]byte
	SamplingInterval float64
	Filter           UA_ExtensionObject
	QueueSize        uint32
	DiscardOldest    bool
	Pad_cgo_1        [3]byte
}
type UA_MonitoredItemCreateRequest struct {
	ItemToMonitor       UA_ReadValueId
	MonitoringMode      uint32
	Pad_cgo_0           [4]byte
	RequestedParameters UA_MonitoringParameters
}
type UA_MonitoredItemCreateResult struct {
	StatusCode              uint32
	MonitoredItemId         uint32
	RevisedSamplingInterval float64
	RevisedQueueSize        uint32
	Pad_cgo_0               [4]byte
	FilterResult            UA_ExtensionObject
}
type UA_CreateMonitoredItemsRequest struct {
	RequestHeader      UA_RequestHeader
	SubscriptionId     uint32
	TimestampsToReturn uint32
	ItemsToCreateSize  uint64
	ItemsToCreate      *UA_MonitoredItemCreateRequest
}
type UA_CreateMonitoredItemsResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *UA_MonitoredItemCreateResult
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_CreateSubscriptionRequest struct {
	RequestHeader               UA_RequestHeader
	RequestedPublishingInterval float64
	RequestedLifetimeCount      uint32
	RequestedMaxKeepAliveCount  uint32
	MaxNotificationsPerPublish  uint32
	PublishingEnabled           bool
	Priority                    uint8
	Pad_cgo_0                   [2]byte
}
type UA_CreateSubscriptionResponse struct {
	ResponseHeader            UA_ResponseHeader
	SubscriptionId            uint32
	Pad_cgo_0                 [4]byte
	RevisedPublishingInterval float64
	RevisedLifetimeCount      uint32
	RevisedMaxKeepAliveCount  uint32
}
type UA_SetPublishingModeRequest struct {
	RequestHeader       UA_RequestHeader
	PublishingEnabled   bool
	Pad_cgo_0           [7]byte
	SubscriptionIdsSize uint64
	SubscriptionIds     *uint32
}
type UA_SetPublishingModeResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *uint32
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_NotificationMessage struct {
	SequenceNumber       uint32
	Pad_cgo_0            [4]byte
	PublishTime          int64
	NotificationDataSize uint64
	NotificationData     *UA_ExtensionObject
}
type UA_SubscriptionAcknowledgement struct {
	SubscriptionId uint32
	SequenceNumber uint32
}
type UA_PublishRequest struct {
	RequestHeader                    UA_RequestHeader
	SubscriptionAcknowledgementsSize uint64
	SubscriptionAcknowledgements     *UA_SubscriptionAcknowledgement
}
type UA_PublishResponse struct {
	ResponseHeader               UA_ResponseHeader
	SubscriptionId               uint32
	Pad_cgo_0                    [4]byte
	AvailableSequenceNumbersSize uint64
	AvailableSequenceNumbers     *uint32
	MoreNotifications            bool
	Pad_cgo_1                    [7]byte
	NotificationMessage          UA_NotificationMessage
	ResultsSize                  uint64
	Results                      *uint32
	DiagnosticInfosSize          uint64
	DiagnosticInfos              *UA_DiagnosticInfo
}
type UA_DeleteSubscriptionsRequest struct {
	RequestHeader       UA_RequestHeader
	SubscriptionIdsSize uint64
	SubscriptionIds     *uint32
}
type UA_DeleteSubscriptionsResponse struct {
	ResponseHeader      UA_ResponseHeader
	ResultsSize         uint64
	Results             *uint32
	DiagnosticInfosSize uint64
	DiagnosticInfos     *UA_DiagnosticInfo
}
type UA_BuildInfo struct {
	ProductUri       UA_ByteString
	ManufacturerName UA_ByteString
	ProductName      UA_ByteString
	SoftwareVersion  UA_ByteString
	BuildNumber      UA_ByteString
	BuildDate        int64
}
type UA_ServerState uint32

const (
	UA_SERVERSTATE_RUNNING            = UA_ServerState(0)
	UA_SERVERSTATE_FAILED             = UA_ServerState(1)
	UA_SERVERSTATE_NOCONFIGURATION    = UA_ServerState(2)
	UA_SERVERSTATE_SUSPENDED          = UA_ServerState(3)
	UA_SERVERSTATE_SHUTDOWN           = UA_ServerState(4)
	UA_SERVERSTATE_TEST               = UA_ServerState(5)
	UA_SERVERSTATE_COMMUNICATIONFAULT = UA_ServerState(6)
	UA_SERVERSTATE_UNKNOWN            = UA_ServerState(7)
)

type UA_ServerStatusDataType struct {
	StartTime           int64
	CurrentTime         int64
	State               uint32
	Pad_cgo_0           [4]byte
	BuildInfo           UA_BuildInfo
	SecondsTillShutdown uint32
	Pad_cgo_1           [4]byte
	ShutdownReason      UA_LocalizedText
}
type UA_Connection struct {
	State             uint32
	LocalConf         UA_ConnectionConfig
	RemoteConf        UA_ConnectionConfig
	Pad_cgo_0         [4]byte
	Channel           *UA_SecureChannel
	Sockfd            int32
	Pad_cgo_1         [4]byte
	Handle            *byte
	IncompleteMessage UA_ByteString
	GetSendBuffer     *[0]byte
	ReleaseSendBuffer *[0]byte
	Send              *[0]byte
	Recv              *[0]byte
	ReleaseRecvBuffer *[0]byte
	Close             *[0]byte
}
type UA_Server struct{}
type UA_Job struct {
	Type      uint32
	Pad_cgo_0 [4]byte
	Job       [24]byte
}
type UA_ConnectionState uint32

const (
	UA_CONNECTION_OPENING = iota
	UA_CONNECTION_ESTABLISHED
	UA_CONNECTION_CLOSED
)

type UA_ConnectionConfig struct {
	ProtocolVersion uint32
	SendBufferSize  uint32
	RecvBufferSize  uint32
	MaxMessageSize  uint32
	MaxChunkCount   uint32
}
type UA_SecureChannel struct{}
type UA_LogLevel uint32

const (
	UA_LOGLEVEL_TRACE = iota
	UA_LOGLEVEL_DEBUG
	UA_LOGLEVEL_INFO
	UA_LOGLEVEL_WARNING
	UA_LOGLEVEL_ERROR
	UA_LOGLEVEL_FATAL
)

type UA_LogCategory uint32

const (
	UA_LOGCATEGORY_NETWORK = iota
	UA_LOGCATEGORY_SECURECHANNEL
	UA_LOGCATEGORY_SESSION
	UA_LOGCATEGORY_SERVER
	UA_LOGCATEGORY_CLIENT
	UA_LOGCATEGORY_USERLAND
)

type UA_ServerNetworkLayer struct {
	Handle        *byte
	DiscoveryUrl  UA_ByteString
	Start         *[0]byte
	GetJobs       *[0]byte
	Stop          *[0]byte
	DeleteMembers *[0]byte
}
type UA_UsernamePasswordLogin struct {
	Username UA_ByteString
	Password UA_ByteString
}
type UA_ServerConfig struct {
	NThreads                    uint16
	Pad_cgo_0                   [6]byte
	Logger                      *[0]byte
	BuildInfo                   UA_BuildInfo
	ApplicationDescription      UA_ApplicationDescription
	ServerCertificate           UA_ByteString
	NetworkLayersSize           uint64
	NetworkLayers               *UA_ServerNetworkLayer
	EnableAnonymousLogin        bool
	EnableUsernamePasswordLogin bool
	Pad_cgo_1                   [6]byte
	UsernamePasswordLoginsSize  uint64
}
type UA_DataSource struct {
	Handle *byte
	Read   *[0]byte
	Write  *[0]byte
}
type UA_ValueCallback struct {
	Handle  *byte
	OnRead  *[0]byte
	OnWrite *[0]byte
}
type UA_ObjectLifecycleManagement struct {
	Constructor *[0]byte
	Destructor  *[0]byte
}
type UA_InstantiationCallback struct {
	Method *[0]byte
	Handle *byte
}
type UA_ExternalNodeStore struct {
	EnsHandle                     *byte
	AddNodes                      *[0]byte
	DeleteNodes                   *[0]byte
	WriteNodes                    *[0]byte
	ReadNodes                     *[0]byte
	BrowseNodes                   *[0]byte
	TranslateBrowsePathsToNodeIds *[0]byte
	AddReferences                 *[0]byte
	DeleteReferences              *[0]byte
	Destroy                       *[0]byte
}
type UA_Client struct{}

type UA_ClientConfig struct {
	Timeout               uint32
	SecureChannelLifeTime uint32
	Logger				  *[0]byte
	LocalConnectionConfig UA_ConnectionConfig
}
type UA_SubscriptionSettings struct {
	RequestedPublishingInterval float64
	RequestedLifetimeCount      uint32
	RequestedMaxKeepAliveCount  uint32
	MaxNotificationsPerPublish  uint32
	PublishingEnabled           bool
	Priority                    uint8
	Pad_cgo_0                   [2]byte
}
