using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
                string commandLine = Console.ReadLine();
                if (commandLine == null)
                    continue;

                Regex argumentFormat = new Regex("(?<quote>['\"])?(?<argument>(\w+(?(1)\s)?)+)(?(quote)['\"]) ");
                string command = commandLine.Split(null)[0];
                string[] args = argumentFormat.Matches(commandLine + " ").Cast<Match>()
                    .Select(match => match.Groups["attribute"])
                    .First().Captures.Cast<Capture>()
                    .Select(capture => capture.Value)
                    .ToArray();
                switch (command)
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