#nullable enable
using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    /// <summary>Avatar artwork is independent of inventory ownership and toy finish IDs.</summary>
    public static class AvatarCatalog
    {
        public const string DefaultId = "pip-peach";
        public const int OutputSize = 512, MaximumInputSide = 2048, MaximumInputBytes = 8 * 1024 * 1024;
        public static IReadOnlyList<string> Ids { get; } = Array.AsReadOnly(new[]
        { "pip-peach", "pip-moon", "fox", "axolotl", "firb", "coco", "snowy", "ginger", "reiny", "guin", "bundy", "wally" });
        public static bool Contains(string? id)
        { foreach (var value in Ids) if (value == id) return true; return false; }
        public static string Resource(string? id) => Contains(id) ? "UI/Avatars/" + id : "";
        public static string AccountScope(string? verifiedUserId)
        { return Guid.TryParseExact(verifiedUserId, "D", out var id) && id != Guid.Empty ? id.ToString("D") : ""; }
        public static bool PhotoId(string? value) => Guid.TryParseExact(value, "N", out var id) && id != Guid.Empty;
    }

    /// <summary>Square crop in source pixels; normalized centers use bottom-left origin like Unity textures.</summary>
    public readonly struct AvatarCrop
    {
        public double X { get; }
        public double Y { get; }
        public double Size { get; }
        private AvatarCrop(double x, double y, double size) { X = x; Y = y; Size = size; }
        public static bool TryCreate(int width, int height, double centerX, double centerY, double zoom, out AvatarCrop crop)
        {
            crop = default;
            if (width < 1 || height < 1 || width > AvatarCatalog.MaximumInputSide || height > AvatarCatalog.MaximumInputSide ||
                !Finite(centerX) || !Finite(centerY) || !Finite(zoom) || zoom < 1 || zoom > 4) return false;
            double size = Math.Min(width, height) / zoom;
            crop = new AvatarCrop(Clamp(centerX * width - size / 2, 0, width - size),
                Clamp(centerY * height - size / 2, 0, height - size), size);
            return true;
        }
        private static bool Finite(double n) => !double.IsNaN(n) && !double.IsInfinity(n);
        private static double Clamp(double n, double min, double max) => Math.Max(min, Math.Min(max, n));
    }

    /// <summary>Reject oversized or unknown encoded images before allocating the decoded Unity texture.</summary>
    public static class AvatarImageHeader
    {
        public static bool Allowed(byte[]? bytes)
        {
            if (bytes == null || bytes.Length < 24 || bytes.Length > AvatarCatalog.MaximumInputBytes) return false;
            int width = 0, height = 0;
            if (bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71 &&
                bytes[4] == 13 && bytes[5] == 10 && bytes[6] == 26 && bytes[7] == 10 &&
                bytes[12] == 73 && bytes[13] == 72 && bytes[14] == 68 && bytes[15] == 82)
            { width = Big32(bytes, 16); height = Big32(bytes, 20); }
            else if (bytes[0] == 255 && bytes[1] == 216)
            {
                int at = 2;
                while (at + 3 < bytes.Length)
                {
                    if (bytes[at++] != 255) return false;
                    while (at < bytes.Length && bytes[at] == 255) at++;
                    if (at >= bytes.Length) return false;
                    int marker = bytes[at++];
                    if (marker == 217 || marker == 218) break;
                    if (marker == 1 || marker >= 208 && marker <= 215) continue;
                    if (at + 1 >= bytes.Length) return false;
                    int length = (bytes[at] << 8) | bytes[at + 1];
                    if (length < 2 || at + length > bytes.Length) return false;
                    if (marker >= 192 && marker <= 207 && marker != 196 && marker != 200 && marker != 204)
                    {
                        if (length < 8) return false;
                        height = (bytes[at + 3] << 8) | bytes[at + 4];
                        width = (bytes[at + 5] << 8) | bytes[at + 6]; break;
                    }
                    at += length;
                }
            }
            return width > 0 && height > 0 && width <= AvatarCatalog.MaximumInputSide && height <= AvatarCatalog.MaximumInputSide;
        }
        private static int Big32(byte[] b, int i) => (b[i] << 24) | (b[i + 1] << 16) | (b[i + 2] << 8) | b[i + 3];
    }
}
