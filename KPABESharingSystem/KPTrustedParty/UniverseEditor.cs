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
            UEHelp();

            List<string> universe = new List<string>(); //load it someway

            while (true)
            {
                Console.Write(">> ");
                string command = Console.ReadLine();
                if (command == null)
                    continue;
                string[] commandWords = command.Split(null);
                string[] args = commandWords.Skip(1).ToArray();
                switch (commandWords[0])
                {
                    case "add":
                    case "a":
                    case "+":
                        {
                            universe.AddRange(
                                args.Where(arg => arg != "or" || arg != "and"));
                            break;
                        }

                    case "remove":
                    case "r":
                    case "-":
                        {
                            //Try remove, check validity, then remove
                            break;
                        }

                    case "reload":
                        {
                            //Reload to the current universe, discarding changes
                            break;
                        }

                    case "commit":
                        {
                            /*
                             * Make the changes effective. Should be important to make it transanctional so to not break everything
                             * - Stop communications with clients, so stop giving old keys
                             * - Compute the new ABE keys
                             * - For each file, take its symmetric key, decrypt with old abe, encrypt with new abe
                             * - Invalid user policies when necessary
                             * - Resume communication like in a fresh start
                             */
                            break;
                        }

                    case "quit":
                        {
                            return;
                        }

                    case "help":
                    default:
                        {
                            UEHelp();
                            break;
                        }
                }
            }
        }

        private static void UEHelp()
        {
            //Print stuff
        }
    }
}
