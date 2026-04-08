using System.Text.RegularExpressions;

namespace backend.Utils;

public static class MediaNaming
{
    /// <summary>
    /// Generates a Cloudinary-friendly publicId using the old naming pattern
    /// Title + '_' + Artist with spaces collapsed to underscores and unsafe
    /// characters removed. Does NOT include a file extension.
    /// Examples:
    ///  - "Price of Admission", "Turnpike Troubadours" -> "Price_of_Admission_Turnpike_Troubadours"
    /// </summary>
    public static string GeneratePublicId(string title, string artist)
    {
        string Combine(string s) => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();

        var baseName = (Combine(title) + "_" + Combine(artist)).Trim('_');

        // Replace whitespace with underscores
        baseName = Regex.Replace(baseName, @"\s+", "_");

        // Remove characters that are not letters, numbers, underscore, hyphen, or dot
        baseName = Regex.Replace(baseName, @"[^A-Za-z0-9_\-]", string.Empty);

        // Collapse multiple underscores
        baseName = Regex.Replace(baseName, @"_+", "_");

        return baseName;
    }
}
