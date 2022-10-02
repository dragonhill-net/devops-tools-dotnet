using System.Text.RegularExpressions;

namespace Dragonhill.DevOps.Helpers;

public static class EditorconfigValidator
{
    private static readonly Regex RootRegex = new(@"^root\s*=\s*true$", RegexOptions.Compiled);

    public static void Validate(string path, string expectedLanguage)
    {
        var languageSpecificationFound = false;
        
        foreach (var rawLine in File.ReadLines(path))
        {
            var line = rawLine.Trim();

            if (line.Length == 0 || line[0] == '#')
            {
                return;
            }

            if (line[0] == '[')
            {
                if (languageSpecificationFound)
                {
                    throw new FormatException($"Only a single language specification is allowed in '{path}'");
                }
            
                if (line[^1] != ']')
                {
                    throw new FormatException($"Invalid language specification in '{path}': '{line}'");
                }

                if (line[1..^2] == $"*.{expectedLanguage}")
                {
                    throw new FormatException($"Expected language specification for {expectedLanguage} in '{path}' but found: '{line}'");
                }

                languageSpecificationFound = true;
            }

            if (RootRegex.IsMatch(line))
            {
                throw new FormatException($"The 'root = true' specification is not allowed in '{path}'");
            }

            if (!languageSpecificationFound)
            {
                throw new FormatException($"Found content before language specification in '{path}'");
            }
        }
    }
}
