using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KPServices;

namespace KPTrustedParty
{
    partial class TPServer
    {
        private static void UniverseEditor()
        {
            Console.WriteLine("------- UniverseEditor -------");
            UniverseEditorHelp();

            Universe editedUniverse = Universe?.Copy();

            while (true)
            {
                Console.Write(">> ");
                string commandLine = Console.ReadLine();
                if (commandLine == null)
                    continue;

                Regex argumentFormat = new Regex("['\"](?<attribute>.+?)['\"]");
                string command = commandLine.Split(null)[0];
                string[] arguments;
                try
                {
                     arguments = argumentFormat.Matches(commandLine).Cast<Match>()
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
                            Console.WriteLine("At least one attribute needed; attribute format = '<attr>'");
                            break;
                        }
                        foreach (string argument in arguments)
                        {
                            try
                            {
                                if (editedUniverse == null)
                                    editedUniverse = Universe.FromString($"'{argument}'", false);
                                else
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
                            Console.WriteLine("At least one attribute needed; attribute format = '<attr>'");
                            break;
                        }
                        foreach (string argument in arguments)
                        {
                            bool? result = editedUniverse?.RemoveAttribute(argument);
                            if(result ?? false)
                                Console.WriteLine($"Attribute {argument} removed.");
                            else
                                Console.WriteLine($"Attribute {argument} not present.");
                        }
                       
                        PrintEditedUniverse(editedUniverse);
                        break;
                    }

                    case "reload":
                    {
                        editedUniverse = Universe?.Copy();
                        break;
                    }

                    case "commit":
                    {
                        Universe = editedUniverse?.Copy();
                        kpService.Universe = Universe;
                        kpService.Setup();
                        KPDatabase.InsertUniverse(Universe.ToString(), kpService.Keys.MasterKey, kpService.Keys.PublicKey);
                        return;
                    }

                    case "print":
                    case "p":
                    {
                        PrintUniverse();
                        PrintEditedUniverse(editedUniverse);
                        break;
                    }

                    case "quit":
                    {
                        Console.WriteLine(" ------- Exiting from the UniverseEditor -------");
                        return;
                    }

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
            //todo: write more about commit command
            Console.WriteLine(
            @"
SYNOPSIS
    The UniverseEditor tool is intended to define the KP attribute Universe in a safe way.
    It checks existing data and avoids conflicts before committing the changes. 
    Until commit, all changes are on a temporary Universe and do not interfere with the existing Universe.
    In the following text, curly brackets {} contain aliases for the command. 
    For commands that have arguments, each single argument must surrounded by single ' or double quotes "".

ADD
    {add, a, +} attribute1 attribute2 ...
        Adds the listed attributes to the Universe. 
        In case of errors, they are reported and attributed is skipped.
        The attribute must comply with the KPABE syntax, which is
            - The attribute name can be any sequence of letters, digits and _ symbol 
                that starts with a letter and is not one of 'and', 'or', 'of'
            - In case of a numerical attribute, it has a = following the name and an 
                optional '# k', 0 < k <= 64 which specifies the bit resolution

REMOVE
    {remove, r, -} attributeName1 attribute
        Removes the listed attributes from the Universe. 
        In case of errors, they are reported and attributed is skipped.

COMMIT
    commit
        Make the changes definitive. # Lots of checks and stuff to be defined #
                    
RELOAD
    reload
        Resets the edited Universe to the current Universe, that is the last committed version.

PRINT
    {print, p}
        Displays the attributes contained in the current Universe and in the edited Universe.

QUIT
    quit
        Quits the UniverseEditor. Any un-committed change to the Universe is discarded.
HELP
    {help, anything that is not a valid command}
        Displays this guide.
        ");
        }

        private static void PrintUniverse()
        {
            if(Universe == null)
                Console.WriteLine("There is no current Universe defined.");
            else
                Console.WriteLine($"The current Universe is: {Universe}");
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
