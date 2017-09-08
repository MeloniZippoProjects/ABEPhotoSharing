using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPTrustedParty
{
    partial class TPServer
    {
        static void Main()
        {
            //Initialize server component: async tasks or threads?

            commandLineLoop();
        }

        private static void commandLineLoop()
        {
            while (true)
            {
                Console.Write("> ");
                string command = Console.ReadLine();
                if(command == null)
                    continue;
                string[] commandWords = command.Split(null);
                string[] args = commandWords.Skip(1).ToArray();
                switch (commandWords[0])
                {
                    case "universeEditor":
                    case "ue":
                    {
                        UniverseEditor();
                        break;
                    }

                    case "detailUser":
                    case "d":
                    {
                        if (!argumentCountCheck(args, 1))
                            break;
                        string username = args[0];

                        //List data and stats for the user

                        break;
                    }

                    case "listUsers":
                    case "l":
                    {
                        //List users from database
                        //Good idea?

                        break;
                    }

                    case "registerUser":
                    case "r":
                    {
                        if (!argumentCountCheck(args, 2))
                                break;
                        string username = args[0];
                        string password = args[1];
                        
                        //Register user in database (salted hash)

                        break;
                    }

                    case "setPolicy":
                    case "s":
                    {
                        if (!argumentCountCheck(args, 2))
                            break;
                        string username = args[0];
                        string policy = string.Join(" ", args.Skip(1));

                        //Set policy for the user and generate its keys

                        break;
                    }

                    case "help":
                    default:
                    {
                        displayHelp();
                        break;
                    }
                }
            }
        }

        private static bool argumentCountCheck(string[] args, int required)
        {
            if (args.Length < required)
            {
                Console.WriteLine("Not enough arguments");
                return false;
            }
            return true;
        }

        private static void displayHelp()
        {
            throw new NotImplementedException();
        }
    }
}
