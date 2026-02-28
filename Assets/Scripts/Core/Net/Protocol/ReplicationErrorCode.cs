namespace OpenTTD.Core.Net.Protocol
{
    public enum ReplicationErrorCode
    {
        None = 0,
        MalformedPayload = 1,
        UnsupportedCodec = 2,
        InvalidTransferMetadata = 3,
        ReassemblyCreateFailed = 4,
        ReassemblyAddFailed = 5,
        DecodeFailed = 6,
        ApplyFailed = 7,
        LineageMismatch = 8,
        BudgetLimited = 9
    }
}
