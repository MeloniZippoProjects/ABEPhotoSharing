using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Grapevine.Server;
using KPServices;
using KPTrustedParty.Database;

namespace KPTrustedParty
{
    partial class TpServer
    {
        private static void CommandLineLoop()
        {
            while (true)
            {
                Console.Write("> ");
                string commandLine = Console.ReadLine();
                if (commandLine == null)
                    continue;

                Regex argumentFormat =
                    new Regex("(?<quote>['\"])?(?<argument>((\\w+(\\s(=|<|>|<=|>=))?)+(?(1)\\s)?)+)(?(quote)['\"]) ");

                string command = commandLine.Split(null)[0];
                string[] args = argumentFormat.Matches(commandLine + " ").Cast<Match>()
                    .Select(match => match.Groups["argument"])
                    .Select(capture => capture.Value)
                    .Skip(1).ToArray();

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
                            //bug: 'mario or cose = 2' is parsed wrongly, only 'mario' is registered
                            byte[] privateKey = KpService.Keygen(policy);
                            KpDatabase.SetUserPolicy(username, policy);

                            KpDatabase.SetUserPrivateKey(username, privateKey);
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
                            if (_server.IsListening)
                            {
                                _server.Stop();
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
                            if (!_server.IsListening)
                            {
                                _server = new RestServer();
                                SetupRestServer();
                                _server.Start();
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
                        if (_server.IsListening)
                        {
                            try
                            {
                                _server.Stop();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Cannot restart REST Server; cause: cannot stop server. Exception: {ex.Message}");
                            }
                        }
                        try
                        {
                            _server = new RestServer();
                            SetupRestServer();
                            _server.Start();
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

UNIVERSE EDITOR
    {ue, universeEditor}
        Opens the Universe Editor interface. Further help can be found inside the utility.

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

");
        }

        private static void DisplayServerStatus()
        {
            Console.WriteLine(_server.IsListening
                ? $"Server listening at: {_server.Host}:{_server.Port}"
                : "Server is not listening");
        }
    }
}