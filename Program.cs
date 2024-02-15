using System;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace safex
{
    class Program
    {

        //public static (int, int) FindMinMaxParmNums(string my_str)
        //{
        //    // Regular expression to match numbers between curly brackets
        //    Regex regex = new Regex(@"\{(\d+)\}");
        //    MatchCollection matches = regex.Matches(my_str);

        //    if (matches.Count == 0)
        //    {
        //        // Return -1 if there are no matches
        //        return (-1,-1);
        //    }

        //    int maxVal = int.MinValue;
        //    int minVal = int.MaxValue;

        //    foreach (Match match in matches)
        //    {
        //        // Extract the number from each match and convert it to an integer
        //        int num = int.Parse(match.Groups[1].Value);

        //        // Update maxVal if the current number is greater
        //        if (num > maxVal)
        //        {
        //            maxVal = num;
        //        }
        //        if (num < minVal)
        //        {
        //            minVal = num;
        //        }
        //    }

        //    return (minVal, maxVal);
        //}

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

            if (numArgs < 2)
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
                return 1;
            }

            string templateString = args[0];
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
