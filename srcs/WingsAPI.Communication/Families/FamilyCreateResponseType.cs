using ProtoBuf;

namespace WingsAPI.Communication.Families
{
    [ProtoContract]
    public enum FamilyCreateResponseType
    {
        GENERIC_ERROR,

        NAME_ALREADY_TAKEN,

        SUCCESS
    }
}