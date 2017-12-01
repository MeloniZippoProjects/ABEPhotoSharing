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

                Regex argumentFormat = new Regex("(?<quote>['\"])?(?<argument>(\\w+(?(1)\\s)?)+)(?(quote)['\"]) ");
                string command = commandLine.Split(null)[0];
                string[] args = argumentFormat.Matches(commandLine + " ").Cast<Match>()
                    .Select(match => match.Groups["argument"])
                    .Select(capture => capture.Value)
                    .Skip(1).ToArray();

                //todo: add commands to pilot the underlaying REST server
                //todo: add commands to check and change the settings
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

#if DEBUG
                        Console.WriteLine($"Captured username is: [{username}]");
                        Console.WriteLine($"Captured password is: [{password}]");
#endif

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
                            //todo: we could do some parsing just to make the policy syntax more flexible
                            byte[] privateKey = kpService.Keygen(policy);
                            KPDatabase.SetUserPolicy(username, policy);
                            
                            KPDatabase.SetUserPrivateKey(username, privateKey);
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

        private static void DisplayHelp()
        {
            //todo: write this help page
            Console.WriteLine("Help unavailable: check the source code.");
        }
    }
}