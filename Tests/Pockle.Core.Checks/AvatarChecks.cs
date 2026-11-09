using System;
using System.Collections.Generic;
using System.IO;
using Pockle.Core;
internal static class AvatarChecks
{
    public static void Run()
    {
        void Check(bool value, string message) { if (!value) throw new Exception("Avatar: " + message); }
        Check(AvatarCatalog.Ids.Count == 12 && new HashSet<string>(AvatarCatalog.Ids).Count == 12, "Twelve unique choices.");
        foreach (string id in AvatarCatalog.Ids) Check(AvatarCatalog.Resource(id) == "UI/Avatars/" + id, "Stable resource route.");
        Check(!AvatarCatalog.Contains("../private") && AvatarCatalog.Resource("../../secret") == "", "No arbitrary resource paths.");
        Check(AvatarCatalog.AccountScope(null) == "" && AvatarCatalog.AccountScope("Collector") == "" && AvatarCatalog.AccountScope("../player") == "", "Only verified UUIDs can name account scopes.");
        Check(AvatarCatalog.AccountScope("00000000-0000-0000-0000-000000000000") == "", "Empty UUID is not an owner.");
        Check(AvatarCatalog.AccountScope("01234567-89AB-CDEF-0123-456789ABCDEF") == "01234567-89ab-cdef-0123-456789abcdef", "Canonical account scope.");
        Check(!AvatarCatalog.PhotoId("../photo") && AvatarCatalog.PhotoId("0123456789abcdef0123456789abcdef"), "Opaque photo filenames.");
        Check(AvatarCrop.TryCreate(1200, 800, .5, .5, 1, out var crop) && crop.X == 200 && crop.Y == 0 && crop.Size == 800, "Landscape center crop.");
        Check(AvatarCrop.TryCreate(800, 1200, 9, -9, 2, out crop) && crop.X == 400 && crop.Y == 0 && crop.Size == 400, "Crop clamps to source.");
        Check(!AvatarCrop.TryCreate(800, 1200, double.NaN, .5, 1, out _) && !AvatarCrop.TryCreate(800, 1200, .5, .5, 0, out _), "Reject invalid transforms.");
        Check(!AvatarCrop.TryCreate(5000, 1200, .5, .5, 1, out _), "Bounded decode.");
        var png = new byte[24]; byte[] magic = {137,80,78,71,13,10,26,10}; Array.Copy(magic,png,8);
        png[12]=73;png[13]=72;png[14]=68;png[15]=82; png[18]=2;png[22]=2;
        Check(AvatarImageHeader.Allowed(png), "Valid 512px PNG header.");
        png[17]=1; Check(!AvatarImageHeader.Allowed(png), "Oversized PNG rejected before decode.");
        Check(!AvatarImageHeader.Allowed(new byte[24]) && !AvatarImageHeader.Allowed(new byte[AvatarCatalog.MaximumInputBytes+1]), "Unknown and oversized input rejected.");
        var jpeg = new byte[24]; jpeg[0]=255;jpeg[1]=216;jpeg[2]=255;jpeg[3]=192;jpeg[5]=17;jpeg[6]=8;jpeg[7]=2;jpeg[9]=2;
        Check(AvatarImageHeader.Allowed(jpeg), "JPEG dimension bounds.");
        jpeg[4]=255; Check(!AvatarImageHeader.Allowed(jpeg), "Truncated JPEG metadata rejected.");
        string folder = Path.Combine(Path.GetTempPath(), "PockleAvatarChecks-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        try
        {
            string file = Path.Combine(folder, "bounded"); AvatarFiles.WriteNewDurable(file, new byte[] { 1, 2, 3 });
            Check(AvatarFiles.ReadBounded(file, 3)?.Length == 3 && AvatarFiles.ReadBounded(file, 2) == null, "File reads enforce exact byte bounds.");
            bool protectedOriginal = false; try { AvatarFiles.WriteNewDurable(file, new byte[] { 9 }); } catch (IOException) { protectedOriginal = true; }
            Check(protectedOriginal && AvatarFiles.ReadBounded(file, 3)?[0] == 1, "New immutable writes cannot overwrite existing photo bytes.");
            File.WriteAllBytes(file, new byte[AvatarFiles.MaximumManifestBytes + 1]);
            Check(AvatarFiles.ReadBounded(file, AvatarFiles.MaximumManifestBytes) == null, "Corrupt oversized manifest is not read into memory.");
        }
        finally { Directory.Delete(folder, true); }
        Console.WriteLine("PASS: avatar catalog, account scopes, bounded headers and crop geometry.");
    }
}
