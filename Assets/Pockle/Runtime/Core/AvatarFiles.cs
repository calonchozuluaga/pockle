#nullable enable
using System;
using System.IO;

namespace Pockle.Core
{
    /// <summary>Bounded device-cache reads and durable new-file writes. Never opens paths received from a server/picker.</summary>
    public static class AvatarFiles
    {
        public const int MaximumManifestBytes = 4096;
        public static byte[]? ReadBounded(string path, int maximum)
        {
            if (maximum < 1) return null;
            using (var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                long length = input.Length;
                if (length < 1 || length > maximum) return null;
                var data = new byte[(int)length]; int offset = 0;
                while (offset < data.Length)
                {
                    int read = input.Read(data, offset, data.Length - offset); if (read == 0) return null;
                    offset += read;
                }
                return input.ReadByte() == -1 ? data : null;
            }
        }
        public static void WriteNewDurable(string path, byte[] bytes)
        {
            using (var output = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            { output.Write(bytes, 0, bytes.Length); output.Flush(true); }
        }
    }
}
