using System;
using System.Linq;
using System.Text.RegularExpressions;
using KPServices;
using KPTrustedParty.Database;

namespace KPTrustedParty
{
    partial class TpServer
    {
        private static readonly Regex UeArgumentFormat = new Regex("['\"](?<attribute>.+?)['\"]");

        private static void UniverseEditor()
        {
            Console.WriteLine("------- UniverseEditor -------");
            UniverseEditorHelp();

            Universe editedUniverse = new Universe();

            while (true)
            {
                Console.Write(">> ");
                string commandLine = Console.ReadLine();
                if (commandLine == null)
                    continue;
                
                string command = commandLine.Split(null)[0];
                string[] arguments;
                try
                {
                    arguments = UeArgumentFormat.Matches(commandLine).Cast<Match>()
                        .Select(match => match.Groups["attribute"])
                        .Select(capture => capture.Value)
                        .ToArray();
                }
                catch (Exception)
                {
                    arguments = new string[0];
                }

                switch (command)
                {
                    case "add":
                    case "a":
                    case "+":
                    {
                        if (!ArgumentCountCheck(arguments, 1))
                        {
                            Console.WriteLine("At least one attribute needed");
                            break;
                        }
                        foreach (string argument in arguments)
                        {
                            try
                            {
                                editedUniverse.AddAttribute(argument);
                            }
                            catch (ArgumentException e)
                            {
                                Console.WriteLine($"{argument} invalid: {e.Message}");
                            }
                        }

                        PrintEditedUniverse(editedUniverse);
                        break;
                    }

                    case "remove":
                    case "r":
                    case "-":
                    {
                        if (!ArgumentCountCheck(arguments, 1))
                        {
                            Console.WriteLine("At least one attribute needed");
                            break;
                        }
                        foreach (string argument in arguments)
                        {
                            bool result = editedUniverse.RemoveAttribute(argument);
                            Console.WriteLine(result
                                ? $"Attribute {argument} removed."
                                : $"Attribute {argument} not present.");
                        }

                        PrintEditedUniverse(editedUniverse);
                        break;
                    }

                    case "reset":
                    {
                        editedUniverse = new Universe();
                        break;
                    }

                    case "commit":
                    {
                        if (Universe.Count < 1)
                        {
                            Console.WriteLine($"Error: the current Universe has no attributes. Commit aborted");
                            break;
                        }

                        Universe = editedUniverse;

                        return;
                    }

                    case "print":
                    case "p":
                    {
                        PrintEditedUniverse(editedUniverse);
                        break;
                    }

                    case "quit":
                    {
                        Console.WriteLine(" ------- Exiting from the UniverseEditor -------");
                        return;
                    }

                    // ReSharper disable once RedundantCaseLabel
                    case "help":
                    default:
                    {
                        UniverseEditorHelp();
                        break;
                    }
                }
            }
        }

        private static void UniverseEditorHelp()
        {
            Console.WriteLine(
                @"
SYNOPSIS
    The UniverseEditor tool is intended to define the KP attribute Universe in a safe way.
    Until commit, all changes are on a temporary Universe and have no effect on keys or database.
    In the following text, curly brackets {} contain aliases for the command. 
    For commands that have arguments, each single argument must surrounded by single ' or double quotes "".

ADD
    {add, a, +} attribute1 attribute2 ...
        Adds the listed attributes to the Universe. 
        In case of errors, they are reported and attribute is skipped.
        The attribute must comply with the KPABE syntax, which is:
            - The attribute name can be any sequence of letters, digits and _ symbol 
                that starts with a letter and is not one of 'and', 'or', 'of'
            - In case of a numerical attribute, it has a = following the name and an 
                optional '# k', 0 < k <= 64 which specifies the bit resolution

REMOVE
    {remove, r, -} attributeName1 attributeName2 ...
        Removes the listed attributes from the Universe. 
        In case of errors, they are reported and attribute is skipped.

COMMIT
    commit
        Makes the changes definitive, genereting a new set of master and public keys.
                    
RESET
    reset
        Resets the edited Universe to a new blank Universe.

PRINT
    {print, p}
        Displays the attributes contained in the edited Universe.

QUIT
    quit
        Quits the UniverseEditor. Any un-committed change to the Universe is discarded.
HELP
    {help, anything that is not a valid command}
        Displays this guide.
        ");
        }

        private static void PrintEditedUniverse(Universe editedUniverse)
        {
            if (editedUniverse == null || editedUniverse.Count == 0)
                Console.WriteLine("The edited Universe is not defined");
            else
                Console.WriteLine($"The edited Universe is: {editedUniverse}");
        }
    }
}