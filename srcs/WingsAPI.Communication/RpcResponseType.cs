using ProtoBuf;

namespace WingsAPI.Communication
{
    [ProtoContract]
    public enum RpcResponseType
    {
        UNKNOWN_ERROR,
        SUCCESS,
        GENERIC_SERVER_ERROR,
        MAINTENANCE_MODE,
        SESSION_NOT_FOUND,
        INVALID_SESSION_STATE,
        PULSE_FAILED,
        UPDATE_FAILED,
        UNHANDLED_ERROR
    }
}