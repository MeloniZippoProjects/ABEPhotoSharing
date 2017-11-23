using System;
using System.Linq;
using System.Text;

namespace KPTrustedParty
{
    partial class TPServer
    {
        private static void commandLineLoop()
        {
            while (true)
            {
                Console.Write("> ");
                string command = Console.ReadLine();
                if (command == null)
                    continue;

                //todo: proper quote handling like in UniverseEditor
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

                        KPDatabase.DetailUser(username);
                        break;
                    }

                    case "listUsers":
                    case "l":
                    {
                        KPDatabase.ListUsers();
                        break;
                    }

                    case "registerUser":
                    case "r":
                    {
                        if (!argumentCountCheck(args, 2))
                            break;
                        string username = args[0];
                        string password = args[1];

                        KPDatabase.RegisterUser(username, password);
                        
                        break;
                    }

                    case "setPolicy":
                    case "s":
                    {
                        if (!argumentCountCheck(args, 2))
                            break;
                        string username = args[0];
                        string policy = args[1];

                        //todo: SetPolicy: Define how to do this
                        //Set policy for the user and generate its keys


                        //todo: validate policy
                        KPDatabase.SetUserPolicy(username, policy);

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