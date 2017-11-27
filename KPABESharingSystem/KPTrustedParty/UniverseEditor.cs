﻿using System;
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

            Universe editedUniverse = universe?.Copy();

            while (true)
            {
                Console.Write(">> ");
                string commandLine = Console.ReadLine();
                if (commandLine == null)
                    continue;

                Regex argumentFormat = new Regex("['\"](?<attribute>.+?)['\"]");
                string command = commandLine.Split(null)[0];
                string[] arguments = argumentFormat.Matches(commandLine).Cast<Match>()
                                     .Select(match => match.Groups["attribute"])
                                     .First().Captures.Cast<Capture>()
                                     .Select(capture => capture.Value)
                                     .ToArray();

                switch (command)
                {
                    case "add":
                    case "a":
                    case "+":
                    {
                        foreach (string argument in arguments)
                        {
                            try
                            {
                                if (editedUniverse == null)
                                    editedUniverse = Universe.FromString(argument, false);
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
                        editedUniverse = universe?.Copy();
                        break;
                    }

                    case "commit":
                    {
                        /* todo: make the changes effective
                        * Make the changes effective. Should be important to make it transanctional so to not break everything
                        * - Stop communications with clients, so stop giving old keys
                        * - Compute the new ABE keys
                        * - For each file, take its symmetric key, decrypt with old abe, encrypt with new abe
                        * - Invalid user policies when necessary
                        * - Resume communication like in a fresh start
                        * asd
                        */

                        //todo: stop communications
                        universe = editedUniverse?.Copy();
                        KPService.Universe = universe;
                        KPService.Setup();
                        foreach(var user in KPDatabase.GetUsersList())
                        {
                            try
                            {
                                var privKeyName = $"{user.Name}_privKey";
                                KPService.Keygen(user.Policy, privKeyName);
                                var privKeyFile = File.Open(privKeyName, FileMode.Open);
                                var memStream = new MemoryStream();
                                privKeyFile.CopyTo(memStream);
                                var privKeyBytes = memStream.ToArray();
                                KPDatabase.SetUserPrivateKey(user.Name, privKeyBytes); 
                                File.Delete(privKeyName);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                                throw;
                            }
                        }
                        
                        break;
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
                        Console.WriteLine("------- Exiting from the UniverseEditor -------");
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
            @"The UniverseEditor tool is intended to define the KP attribute universe in a safe way.
            It checks existing data and avoids conflicts before committing the changes. 
            Until commit, all changes are on a temporary universe and do not interfere with the existing universe.
            In the following text, curly brackets {} contain aliases for the command. 
            For commands that have arguments, each single argument must surrounded by single ' or double quotes "".

                {add, a, +} attribute1 attribute2 ...
                    Adds the listed attributes to the universe. In case of errors, they are reported and attributed is skipped.
                    The attribute must comply with the KPABE syntax, which is
                        - The attribute name can be any sequence of letters, digits and _ symbol that starts with a letter and is not one of 'and', 'or', 'of'
                        - In case of a numerical attribute, it has a = following the name and an optional '# k', 0 < k <= 64 which specifies the bit resolution

                {remove, r, -} attributeName1 attribute
                    Removes the listed attributes from the universe. In case of errors, they are reported and attributed is skipped.

                commit
                    Make the changes definitive. # Lots of checks and stuff to be defined #
                    

                reload
                    Resets the edited universe to the current universe, that is the last committed version.

                {print, p}
                    Displays the attributes contained in the current universe and in the edited universe.

                quit
                    Quits the UniverseEditor. Any un-committed change to the universe is discarded.

                {help, anything that is not a valid command}
                    Displays this guide.
            ");
        }

        private static void PrintUniverse()
        {
            if(universe == null)
                Console.WriteLine("There is no current universe defined.");
            else
                Console.WriteLine($"The current universe is: {universe}");
        }

        private static void PrintEditedUniverse(Universe editedUniverse)
        {
            if (editedUniverse == null || editedUniverse.Count == 0)
                Console.WriteLine("The edited universe is not defined");
            else
                Console.WriteLine($"The edited universe is: {editedUniverse}");
        }
    }
}
