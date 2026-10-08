using System.Globalization;
using System.Text;

namespace Pockle.Core
{
    public static class ProfileName
    {
        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Collector";
            var clean = new StringBuilder();
            foreach (char character in value)
            {
                var category = char.GetUnicodeCategory(character);
                if (char.IsControl(character) || category == UnicodeCategory.LineSeparator || category == UnicodeCategory.ParagraphSeparator ||
                    category == UnicodeCategory.Format && character != '\u200d' && character != '\u200c') continue;
                clean.Append(character);
            }
            string trimmed = clean.ToString().Trim();
            if (trimmed.Length == 0) return "Collector";
            int[] starts = StringInfo.ParseCombiningCharacters(trimmed);
            return starts.Length > 20 ? trimmed.Substring(0, starts[20]) : trimmed;
        }
    }
}
