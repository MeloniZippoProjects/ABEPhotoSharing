using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Grapevine.Server;
using KPServices;
using KPTrustedParty.Database;
using KPTrustedParty.Properties;

namespace KPTrustedParty
{
    partial class TpServer
    {
        static readonly Regex CmdArgumentFormat =
            new Regex("(?<quote>['\"])?(?<argument>((\\w+(\\s(=|<|>|<=|>=))?)+(?(1)\\s)?)+)(?(quote)['\"]) ");
        
        private static void CommandLineLoop()
        {
            while (true)
            {
                Console.Write("> ");
                string commandLine = Console.ReadLine();
                if (commandLine == null)
                    continue;

                string command = commandLine.Split(null)[0];
                string[] args = CmdArgumentFormat.Matches(commandLine + " ").Cast<Match>()
                    .Select(match => match.Groups["argument"])
                    .Select(capture => capture.Value)
                    .Skip(1).ToArray();

                //todo: add commands to check and change the settings
                switch (command)
                {
                    case "universeInfo":
                    {
                        Console.WriteLine($"Universe is: {Universe}");
                        break;    
                    }

                    case "universeReset":
                    {
                        Console.Write(
                            @"This operation will clear any keys and policies related to this Universe." +
                            "Data encrypted using such keys could become unretrievable." +
                            "Are you sure? (Y/N): ");
                        string answer = Console.ReadLine();
                        if (answer != "Y")
                        {
                            Console.WriteLine("Reset not confirmed, aborted.");
                            break;
                        }

                        try {
                            restServer.Stop();
                        } catch {
                            //Suppressing errors 
                        }

                        KpDatabase.ResetUniverse();
                        Console.WriteLine("Universe reset completed");
                        Environment.Exit(0);
                        break;
                    }

                    case "detailUser":
                    case "d":
                    {
                        if (!ArgumentCountCheck(args, 1))
                            break;
                        string username = args[0];

                        KpDatabase.DetailUser(username);
                        break;
                    }

                    case "listUsers":
                    case "l":
                    {
                        KpDatabase.ListUsers();
                        break;
                    }

                    case "registerUser":
                    case "r":
                    {
                        if (!ArgumentCountCheck(args, 2))
                            break;
                        string username = args[0];
                        string password = args[1];

                        Regex usernameValid = new Regex(@"[\w\d]{3,21}");
                        if (usernameValid.Matches(username).Count != 1)
                        {
                            Console.WriteLine("Username must be a alphanumeric string of 3 to 21 characters.");
                            break;
                        }

                        Regex passwordValid = new Regex(@"[^\s'""]{8,}");
                        if (passwordValid.Matches(password).Count != 1)
                        {
                            Console.WriteLine("Password must be a string of at least 8 characters without ' or \".");
                            break;
                        }

#if DEBUG
                        Console.WriteLine($"Captured username is: [{username}]");
                        Console.WriteLine($"Captured password is: [{password}]");
#endif

                        KpDatabase.RegisterUser(username, password);

                        break;
                    }

                    case "setPolicy":
                    case "s":
                    {
                        if (!ArgumentCountCheck(args, 2))
                            break;

                        string username = args[0];
                        string policy = args[1];
                        try
                        {
                            //todo: we could do some parsing just to make the policy syntax more flexible
                            using (TemporaryBytes privateKey = KpService.Keygen(policy))
                            {
                                KpDatabase.SetUserPolicy(username, policy);
                                KpDatabase.SetUserPrivateKey(username, privateKey);
                            }
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

                    case "q":
                    case "exit":
                    case "quit":
                    {
                        Environment.Exit(0);
                        break;
                    }

                    case "serverStop":
                    {
                        try
                        {
                            if (restServer.IsListening)
                            {
                                restServer.Stop();
                                Console.WriteLine("REST Server succesfully stopped");
                            }
                            else
                            {
                                Console.WriteLine("REST Server is not running.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot stop REST Server. Exception: {ex.Message}");
                        }
                        break;
                    }

                    case "serverStart":
                    {
                        try
                        {
                            if (!restServer.IsListening)
                            {
                                restServer = new RestServer();
                                SetupRestServer();
                                restServer.Start();
                                Console.WriteLine("REST Server succesfully started");
                                DisplayServerStatus();
                            }
                            else
                            {
                                Console.WriteLine("REST Server is already running");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot start REST Server. Exception: {ex.Message}");
                        }

                        break;
                    }

                    case "serverRestart":
                    {
                        if (restServer.IsListening)
                        {
                            try
                            {
                                restServer.Stop();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Cannot restart REST Server; cause: cannot stop server. Exception: {ex.Message}");
                            }
                        }
                        try
                        {
                            restServer = new RestServer();
                            SetupRestServer();
                            restServer.Start();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"Cannot restart REST Server; cause: cannot start server. Exception: {ex.Message}");
                        }
                        Console.WriteLine("Server succesfully restarted");
                        DisplayServerStatus();
                        break;
                    }

                    case "config":
                        Settings settings = Settings.Default;
                        switch (args[0])
                        {
                            case "server":
                                if (args[1].Equals("port"))
                                {
                                    try
                                    {
                                        Int16 port = Int16.Parse(args[2]);
                                        settings.ServerPort = port;
                                    }
                                    catch (FormatException)
                                    {
                                        Console.WriteLine("The specified port cannot be parsed");
                                    }
                                    catch (OverflowException)
                                    {
                                        Console.WriteLine("The port must be an integer between 0 and 65535");
                                    }
                                }
                                else if (args[1].Equals("host"))
                                {
                                    settings.ServerHost = args[2];
                                }
                                else
                                {
                                    Console.WriteLine($"Option {args[2]} not recognized");
                                }

                                break;
                            case "kpabe":
                                if (args[1].Equals("suite"))
                                {
                                    if (Path.IsPathRooted(args[2]))
                                        settings.KPSuitePath = args[2];
                                    else
                                    {
                                        var newPath = Path.Combine(Directory.GetCurrentDirectory(), args[2]);
                                        settings.KPSuitePath = newPath.ToString();
                                        KpService.SuitePath = newPath;
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"Option {args[2]} not recognized");
                                }

                               
                                break;
                            default:
                                Console.WriteLine($"Option {args[1]} not recognized");
                                break;
                        }

                        break;
                    // ReSharper disable once RedundantCaseLabel
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
            Console.WriteLine(@"
SYNOPSIS
This is the command line interface for the KPABE server. 
It allows the administrator to create users, list users, set their policies 
and start or stop the REST Server which is used by the clients to 
obtain the keys. 

UNIVERSE INFO
    {universeInfo}
        Displays the attributes in the universe.

UNIVERSE RESET
    {universeReset}
        Removes any information about the universe, that is its definition, the related keys
        and the users' policies. Users' login informations are not removed or changed.
        After this operation the server shuts down and must be restarted to configure the new universe.

DETAIL USER
    {d, detailUser} username
        Shows information about the User with the specified username.

LIST USERS
    {l, listUsers}
        Shows a list of every registered Users and their policies.

REGISTER USER
    {r, registerUser} username, password
        Creates a new User without any policy, with the specified username and password. 

SET POLICY
    {s, setPolicy} username, policy
        Set a policy for the User with the given username.

SERVER START 
    {serverStart} 
        Starts the REST Server, if it is not listening.

SERVER STOP
    {serverStop}
        Stops the REST Server, if it is listening.

SERVER RESTART
    {serverRestart}
        Restarts the REST Server.

CONFIG
    {config}
        Used to configure settings of the server.
        
        config server port 1064 -> sets the server port to 1064 
            (you need to restart the server for this to have effect

        config server host example.org -> sets the server host to example.org
            (you need to restart the server for this to have effect)

        config kpabe suite C:\kpabe\ -> sets the suite path to C:\kpabe\
            (there must be a valid installation of kpabe for the application to keep working)
");
        }

        private static void DisplayServerStatus()
        {
            Console.WriteLine(restServer.IsListening
                ? $"Server listening at: {restServer.Host}:{restServer.Port}"
                : "Server is not listening");
        }
    }
}