using System;

namespace SwapChains.Runtime.PersistenceData
{
    public static class GuidExtensions
    {
        public static SerializableGuid ToSerializableGuid(this Guid systemGuid)
        {
            var bytes = systemGuid.ToByteArray();
            return new SerializableGuid(
                BitConverter.ToUInt32(bytes, 0),
                BitConverter.ToUInt32(bytes, 4),
                BitConverter.ToUInt32(bytes, 8),
                BitConverter.ToUInt32(bytes, 12)
            );
        }
    }
}
