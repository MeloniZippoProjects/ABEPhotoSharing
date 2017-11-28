using System;
using System.IO;
using System.Linq;
using System.Text;
using KPServices;

namespace KPTrustedParty
{
    partial class TPServer
    {
        private static void CommandLineLoop()
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
                        if (!ArgumentCountCheck(args, 1))
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
                        if (!ArgumentCountCheck(args, 2))
                            break;
                        string username = args[0];
                        string password = args[1];

                        KPDatabase.RegisterUser(username, password);
                        
                        break;
                    }

                    case "setPolicy":
                    case "s":
                    {
                        if (!ArgumentCountCheck(args, 2))
                            break;

                        var username = args[0];
                        var policy = args[1];
                        try
                        {
                            KPService.Keygen(policy, $"{username}_privKey");
                            KPDatabase.SetUserPolicy(username, policy);
                            using (var privKeyStream = new MemoryStream())
                            {
                                using (var fileStream = File.Open($"{username}_privKey", FileMode.Open))
                                {
                                    fileStream.CopyTo(privKeyStream);
                                }

                                KPDatabase.SetUserPrivateKey(username, privKeyStream.ToArray());
                            }
                            File.Delete($"{username}_privKey");
                        }
                        catch (AttributeNotFound)
                        {
                            Console.WriteLine("The policy contains some invalid attributes");
                        }
                        catch (UnsatisfiablePolicy ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (TrivialPolicy ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        catch (FileNotFoundException)
                        {
                            Console.WriteLine("FATAL: can't find file created right now");
                        }
                        
                        break;
                    }

                    case "help":
                    default:
                    {
                        DisplayHelp();
                        break;
                    }
                }
            }
        }

        private static bool ArgumentCountCheck(string[] args, int required)
        {
            if (args.Length < required)
            {
                Console.WriteLine("Not enough arguments");
                return false;
            }
            return true;
        }

        private static void DisplayHelp()
        {
            throw new NotImplementedException();
        }
    }
}