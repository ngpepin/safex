using System;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace safex
{
    class Program
    {

        public static (int ParmNumMin, int ParmNumMax, bool[] IsParmPresent) AnalyzeStringForParams(string input)
        {
            Regex regex = new Regex(@"\{(\d+)\}");
            MatchCollection matches = regex.Matches(input);
            List<int> foundNumbers = new List<int>();

            foreach (Match match in matches)
            {
                foundNumbers.Add(int.Parse(match.Groups[1].Value));
            }

            if (foundNumbers.Count == 0)
            {
                return (-1, -1, new bool[0]); // No numbers found
            }

            foundNumbers.Sort(); // Ensure the numbers are in ascending order
            int min = foundNumbers[0];
            int max = foundNumbers[^1]; // ^1 is the C# 8.0 way to get the last element

            bool[] isParmPresent = new bool[max + 1]; // +1 because we're using 0-based indexing
            foreach (var number in foundNumbers)
            {
                isParmPresent[number] = true;
            }

            return (min, max, isParmPresent);
        }

        public static (int ParmNumMin, int ParmNumMax, string[] FilledArguments) FillArguments(string input, string[] args)
        {
            var (ParmNumMin, ParmNumMax, IsParmPresent) = AnalyzeStringForParams(input);
            
            if (ParmNumMin == -1 || args.Length == 0)
            {

                return (-1, -1, new string[0]); // No parameters found or no arguments provided
            }

           //}

            // Initialize the array to hold the arguments, ensuring all positions are initially null
            string[] filledArguments = new string[ParmNumMax + 1]; // Ensure size to cover all indices up to ParmNumMax
            for (int i = 0; i <= ParmNumMax; i++)
            {
                filledArguments[i] = ""; // Initialize with null or empty string
            }


            // Directly map found numbers to their corresponding arguments, if any
            Regex regex = new Regex(@"\{(\d+)\}");
            MatchCollection matches = regex.Matches(input);
            foreach (Match match in matches)
            {
                int paramIndex = int.Parse(match.Groups[1].Value);
                // Only assign argument if it's within the bounds of provided args
                if (paramIndex < args.Length)
                {
                    filledArguments[paramIndex] = args[paramIndex];
                }
            }


            return (ParmNumMin, ParmNumMax, filledArguments);
        }

       
        static int Main(string[] args)
        {

            int numArgs = args.Length;

            if (numArgs == 0)
            {
                Console.WriteLine(@"Safex provides a safe way to do the equivalent of bash parameter expansion without the risk of undesired interpretation by the shell of string contents.");
                Console.WriteLine("");
                Console.WriteLine(@"- It essentially allows literals to be injected into literals (avoiding altogether the need for double quoting), and thus reduces the need to escape and take other convoluted");
                Console.WriteLine(@"precautions in order to protect text from interference by the shell. This is why the word "safe" appears in the name: in this case it means 'safe from interpretation'");
                Console.WriteLine(@"by the shell causing wayward expansions, variable and command substitutions, globbing, brace expansions, tilde expansions, case modification, etc.");
                Console.WriteLine("");
                Console.WriteLine(@"- Safex makes string manipulation and parameter passing more similar to the dominant paradigm used in programming languages. It follows the familiar"); 
                Console.WriteLine(@"'<Template with Embedded Arguments Placeholders {1} {2} {3}>', '<Value of Argument {1}>', '<Value of {2}>', '<Value of {3}>'...'");
                Console.WriteLine(@"pattern seen in C#'s 'Console.WriteLine' and common to many proramming languages.");
                Console.WriteLine("");              
                Console.WriteLine(@"- Safex can also be used to rearrange the order of parameters to allow more frequent use of bash aliases, especially when expressions require");  
                Console.WriteLine(@"that the text or parameters provided after alias (i.e., 'myalias <myarg> <mytext>') be located somewhere else than at the very end of the line.");
                Console.WriteLine("");       
                Console.WriteLine(@"- Note that more recent versions of Safex can return an eval command string that is executed by a bash wrapper function withing a bash script."); 
                Console.WriteLine(@"This is quite powerful, though clearly not at all 'safe'! Much of the documentation herein needs to be updated to reflect this change (which");
                Console.WriteLine(@"should be controlled by a command line switch to make the behaviour optional).");
                Console.WriteLine("");    
                Console.WriteLine("  Usage: safex templateString value1 [value2...]");
                Console.WriteLine("");
                Console.WriteLine(@" Example with two parameters and special characters:");
                Console.WriteLine(@"      output =$(./ safex $'My dog\'s name is {0} and my cat\'s name is {1}' 'Fido' 'Whiskers')");
                Console.WriteLine("       eval \"$output\"");
                Console.WriteLine("");    
                Console.WriteLine("");
                Console.WriteLine(@"   Example with three parameters, special characters, and the need for complex escaping:");
                Console.WriteLine(@"      output =$(./ safex $'Here are the names: {0}, {1}, and {2}' 'Fido' 'Whiskers' 'Tweety')");
                Console.WriteLine("       eval \"$output\"");
                Console.WriteLine("");    
                Console.WriteLine("");
                Console.WriteLine(@"   Example using variables:");
                Console.WriteLine("       output =$(./ safex \"$template\" \"$param1\" \"$param2\"");
                Console.WriteLine("       eval \"$output\"");
                Console.WriteLine("");
                Console.WriteLine("    To execute the returned string:");
                Console.WriteLine("       local temp_file =$(mktemp)");
                Console.WriteLine("       usr/bin/ safex \"${args[@]}\" > temp_file");
                Console.WriteLine("       echo -en \"executing: \"; cat temp_file\"; echo");
                Console.WriteLine("       sudo /usr/bin/unbuffer bash -c 'USER=root; ' \"$(cat temp_file)\" | more");
                Console.WriteLine("");
                return 1;
            }

            string templateString = args[0];
           // string[] commandLineArgs = args.Length > 1 ? args[1..] : new string[0];
            string[] commandLineArgs = args.Length > 1 ? args[1..] : new string[0];
            var (ParmNumMin, ParmNumMax, filledArguments) = FillArguments(templateString, commandLineArgs);

            if (ParmNumMin == -1)
            {
                Console.WriteLine(@"Error: no template parameters, e.g., '{0}', provided.");
                return 1;
            }

            string formattedString;
            try
            {
                // Apply the formatting
                formattedString = string.Format(templateString, filledArguments);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error creating response: {ex.Message}");
                return 1;
            }

            formattedString = formattedString.Replace("'", "'\\''");
            Console.WriteLine(formattedString);
            
            return 0;

        }
    }
}
