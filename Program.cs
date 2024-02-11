using System;

namespace safex
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Provides a safe way to do the equivalent of bash parameter expansion without the risk of uninteded side-effects/");
                Console.WriteLine("");
                Console.WriteLine("  Usage: safex variableName templateString value1 [value2...]");
                Console.WriteLine("");
                Console.WriteLine(@"   Example with two parameters and special characters:");
                Console.WriteLine(@"      output =$(./ safex mysentence $'My dog\'s name is {0} and my cat\'s name is {1}' 'Fido' 'Whiskers')");
                Console.WriteLine("       eval \"$output\"");
                Console.WriteLine("       echo \"$mysentence\"");
                Console.WriteLine("");
                Console.WriteLine(@"   Example with three parameters, special characters, and the need for complex escaping:");
                Console.WriteLine(@"      output =$(./ safex anotherSentence $'Here are the names: {0}, {1}, and {2}' 'Fido' 'Whiskers' 'Tweety')");
                Console.WriteLine("       eval \"$output\"");
                Console.WriteLine("       echo \"$mysentence\"");
                Console.WriteLine("");
                Console.WriteLine(@"   Example using variables:");
                Console.WriteLine("       output =$(./ safex \"mysentence\" \"$template\" \"$param1\" \"$param2\"");
                Console.WriteLine("       eval \"$output\"");
                Console.WriteLine("       echo \"$mysentence\"");
                Console.WriteLine("");
                return;
            }

            var variableName = args[0];
            var templateString = args[1];

            // Prepare values for formatting by extracting all arguments beyond the first two
            object[] values = new object[args.Length - 2];
            for (int i = 2; i < args.Length; i++)
            {
                values[i - 2] = args[i];
            }

            string formattedString;
            try
            {
                // Apply the formatting
                formattedString = string.Format(templateString, values);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error formatting string: {ex.Message}");
                return;
            }

            // Output the Bash command for variable assignment
            // Escape single quotes to prevent issues in Bash
            formattedString = formattedString.Replace("'", "'\\''");
            Console.WriteLine($"{variableName}='{formattedString}'");
        }
    }
}
