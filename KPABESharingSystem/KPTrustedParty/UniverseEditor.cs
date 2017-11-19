﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KPTrustedParty
{
    partial class TPServer
    {
        private static void UniverseEditor()
        {
            Console.WriteLine("------- UniverseEditor -------");
            UniverseEditorHelp();

            List<string> universe = LoadCurrentUniverse();

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
                            //TODO: continue implementation
                        }

                        break;
                    }

                    case "remove":
                    case "r":
                    case "-":
                    {
                        //todo: Try remove, check validity, then remove

                        PrintUniverse(universe);
                        break;
                    }

                    case "reload":
                    {
                        universe = LoadCurrentUniverse();
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
                        */
                        break;
                    }

                    case "print":
                    case "p":
                    {
                        PrintUniverse(universe);
                        break;
                    }

                    case "quit":
                    {
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
            Console.WriteLine(
            @"The UniverseEditor tool is intended to define the KP attribute universe in a safe way.
            It checks existing data and avoids conflicts before committing the changes. 
            Until commit, all changes are on a temporary universe and do not interfere with the existing universe.
            In the following text, curly brackets {} contain aliases for the command. 
            For commands that have arguments, each single argument must surrounded by single ' or double quotes "".

                {add, a, +} attribute1 attribute2 ...
                    Adds the listed attributes 
            "
            //todo: continue writing the help
                );
        }

        private static List<string> LoadCurrentUniverse()
        {
            //todo: load it someway
            return new List<string>();
        }

        private static void PrintUniverse(List<string> universe)
        {
            //todo: print the current universe
        }
    }
}
