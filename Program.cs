﻿/*
- It essentially allows literals to be injected into literals (avoiding altogether the need for double quoting), and thus reduces the need to escape and take other convoluted
precautions in order to protect text from interference by the shell. This is why the word 'safe' appears in the name: in this case it means 'safe from interpretation'
by the shell causing wayward expansions, variable and command substitutions, globbing, brace expansions, tilde expansions, case modification, etc.

- Safex makes string manipulation and parameter passing more similar to the dominant paradigm used in programming languages. It follows the familiar
'<Template with Embedded Arguments Placeholders {1} {2} {3}>', '<Value of Argument {1}>', '<Value of {2}>', '<Value of {3}>'...
pattern seen in C#'s 'Console.WriteLine' and common to many programming languages.

- Safex can also be used to rearrange the order of parameters to allow more frequent use of bash aliases, especially when expressions require
that the text or parameters provided after an alias (i.e., 'myalias <myarg> <mytext>') be located somewhere else than at the very end of the line.

- Note that more recent versions of Safex can return an eval command string that is executed by a bash wrapper function within a bash script.
This is quite powerful, though clearly not at all 'safe'! Much of the documentation herein needs to be updated to reflect this change (which
should be controlled by a command line switch to make the behaviour optional).

  Usage: safex templateString value1 [value2...]

      1- Example with two parameters, one containg a special character:
      
      safex 'My dog is named is {0} and my cat is named is {1}' 'Fido' 'Whi$ker$'
      
      output: My dog is named is Fido and my cat is named is Whi$ker$

      2- Example with four parameters, two are literals [{1},{2}], one uses variable substitution {3}, an parameter {0} 
      is used to hold a single quote:

      HN='Hammy'
      safex 'My dog{0}s name is {1}, my cat{0}s name is {2} and I call my hamster "{3}"' \' 'Fido' 'Whi$ker$' "$HN" 
      
      output: My dog'\''s name is Fido, my cat'\''s name is Whi$ker$ and I call my hamster "Hammy"

      3- Example using command expansion:
      TBC

*/


using System;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;

namespace safex
{
    class Program
    {

        public static (int ParmNumMin, int ParmNumMax, bool[] IsParmPresent) AnalyzeStringForParams(string input)
        {
            // Create a regular expression to find all instances of {numbers} in the input string
            Regex regex = new Regex(@"\{(\d+)\}");
            MatchCollection matches = regex.Matches(input);
            List<int> foundNumbers = new List<int>();

            // Extract the parameter placeholder {numbers} from the string
            foreach (Match match in matches)
            {
                foundNumbers.Add(int.Parse(match.Groups[1].Value));
            }

            // If no parameter {numbers} are found, return a tuple with -1 for min and max
            if (foundNumbers.Count == 0)
            {
                return (-1, -1, new bool[0]); 
            }

            // Sort the numbers and find the min and max
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

        // FillArguments takes a string and an array of arguments and fills in the placeholders in the string with the argument values
        public static (int ParmNumMin, int ParmNumMax, string[] FilledArguments) FillArguments(string input, string[] args)
        {
            var (ParmNumMin, ParmNumMax, IsParmPresent) = AnalyzeStringForParams(input);
            
            if (ParmNumMin == -1 || args.Length == 0)
            {

                return (-1, -1, new string[0]); // No parameters found or no arguments provided
            }

           //}

            // Initialize the array to hold the arguments, ensuring all positions are initially null
            // <nil> is used to indicate that a position is null or empty (i.e., no argument value provided)
            string[] filledArguments = new string[ParmNumMax + 1]; // Ensure size to cover all indices up to ParmNumMax
            for (int i = 0; i <= ParmNumMax; i++)
            {
                filledArguments[i] = "<nil>"; // Initialize with null or empty string
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

            // If no arguments are provided, print the help message
            if (numArgs == 0)
            {
                Console.WriteLine(@"Safex provides a safe way to do the equivalent of bash parameter expansion without the risk of undesired interpretation by the shell of string contents.");
                Console.WriteLine("");
                Console.WriteLine("  Usage: safex templateString value1 [value2...]");
                Console.WriteLine("");
                //Console.WriteLine(@" Example with two parameters and special characters:");
                //Console.WriteLine(@"      output =$(./ safex $'My dog\'s name is {0} and my cat\'s name is {1}' 'Fido' 'Whiskers')");
                //Console.WriteLine("       eval \"$output\"");
                //Console.WriteLine("");    
                //Console.WriteLine("");
                //Console.WriteLine(@"   Example with three parameters, special characters, and the need for complex escaping:");
                //Console.WriteLine(@"      output =$(./ safex $'Here are the names: {0}, {1}, and {2}' 'Fido' 'Whiskers' 'Tweety')");
                //Console.WriteLine("       eval \"$output\"");
                //Console.WriteLine("");    
                //Console.WriteLine("");
                //Console.WriteLine(@"   Example using variables:");
                //Console.WriteLine("       output =$(./ safex \"$template\" \"$param1\" \"$param2\"");
                //Console.WriteLine("       eval \"$output\"");
                //Console.WriteLine("");
                //Console.WriteLine("    To execute the returned string:");
                //Console.WriteLine("       local temp_file =$(mktemp)");
                //Console.WriteLine("       usr/bin/ safex \"${args[@]}\" > temp_file");
                //Console.WriteLine("       echo -en \"executing: \"; cat temp_file\"; echo");
                //Console.WriteLine("       sudo /usr/bin/unbuffer bash -c 'USER=root; ' \"$(cat temp_file)\" | more");
                Console.WriteLine("");
                return 1;
            }

            // Extract the template string and the command line arguments
            string templateString = args[0];
            string[] commandLineArgs = args.Length > 1 ? args[1..] : new string[0];
            var (ParmNumMin, ParmNumMax, filledArguments) = FillArguments(templateString, commandLineArgs);

            // If no parameters are found, print an error message
            if (ParmNumMin == -1)
            {
                Console.WriteLine(@"Error: no template parameters, e.g., '{1}', provided.");
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

            // Escape single quotes
            formattedString = formattedString.Replace("'", "'\\''");
            Console.WriteLine(formattedString);
            
            return 0;

        }
    }
}
