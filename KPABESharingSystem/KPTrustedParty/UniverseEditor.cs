using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPTrustedParty
{
    partial class TPServer
    {
        private static void UniverseEditor()
        {
            UniverseEditorHelp();

            List<string> universe = LoadCurrentUniverse();

            while (true)
            {
                Console.Write(">> ");
                string commandLine = Console.ReadLine();
                if (commandLine == null)
                    continue;
                string[] commandWords = commandLine.Split(null);
                string command = commandWords[0];
                string[] args = commandWords.Skip(1).ToArray();

                switch (command)
                {
                    case "add":
                    case "a":
                    case "+":
                    {
                        universe.AddRange(
                            args.Where(arg => arg != "or" || arg != "and"));
                        PrintUniverse(universe);
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
            //todo: print help
        }

        private static List<String> LoadCurrentUniverse()
        {
            //todo: load it someway
        }

        private static void PrintUniverse(List<string> universe)
        {
            //todo: print the current universe
        }
    }
}
