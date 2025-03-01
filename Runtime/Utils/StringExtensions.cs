using UnityEngine;

namespace Contracts.Scripting
{
    public static class StringExtensions
    {
        public static string CamelToTitle(this string camel)
        {
            if (string.IsNullOrEmpty(camel))
            {
                return string.Empty;
            }

            // Determine the required number of spaces by counting all upper case characters beyond the start of the string.
            var spaceCount = 0;
            for (var camelIndex = 1; camelIndex < camel.Length; camelIndex++)
            {
                if (char.IsUpper(camel[camelIndex]))
                {
                    ++spaceCount;
                }
            }

            // Allocate space for the string plus the required number of spaces.
            var titleChars = new char[camel.Length + spaceCount];

            // Add the first character, forcing it to upper case as the string may be lowerCamelCase.
            titleChars[0] = char.ToUpper(camel[0]);

            // Add all remaining characters, inserting spaces before upper case characters.
            for (int camelIndex = 1, titleIndex = 1; camelIndex < camel.Length; ++camelIndex, ++titleIndex)
            {
                if (char.IsUpper(camel[camelIndex]))
                {
                    titleChars[titleIndex] = ' ';
                    ++titleIndex;
                }
                titleChars[titleIndex] = camel[camelIndex];
            }

            return new string(titleChars);
        }
    }
}
