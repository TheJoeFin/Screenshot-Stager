using System.Text;
using System.Text.RegularExpressions;

namespace Screenshot_Stager;
public static class StringExtensions
{
    public static readonly List<Char> ReservedChars = [
    ' ', '"', '*', '/', ':', '<', '>', '?', '\\', '|', '+',
        ',', '.', ';', '=', '[', ']', '!', '@' ];

    public static string MakePathSafe(this string stringToClean)
    {
        StringBuilder sb = new();
        sb.Append(stringToClean);

        foreach (char reservedChar in ReservedChars)
            sb.Replace(reservedChar, '-');

        return Regex.Replace(sb.ToString(), @"-+", "-");
    }
}
