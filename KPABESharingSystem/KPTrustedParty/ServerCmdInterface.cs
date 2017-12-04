using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Grapevine.Server;
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

                Regex argumentFormat = new Regex("(?<quote>['\"])?(?<argument>((\\w+(\\s=)?)+(?(1)\\s)?)+)(?(quote)['\"]) ");
       
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

                        var usernameValid = new Regex(@"[\w\d]{3,21}");
                        if (usernameValid.Matches(username).Count != 1)
                        {
                            Console.WriteLine("Username must be a alphanumeric string of 3 to 21 characters.");
                            break;
                        }

                        var passwordValid = new Regex(@"[^\s'""]{8,}");
                        if (passwordValid.Matches(password).Count != 1)
                        {
                            Console.WriteLine("Password must be a string of at least 8 characters without ' or \".");
                            break;
                        }

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
                            //bug: 'mario or cose = 2' is parsed wrongly, only 'mario' is registered
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

                    case "q":
                    case "exit":
                    case "quit":
                        Environment.Exit(0);
                        break;

                    case "serverStop":
                        try
                        {
                            if (server.IsListening)
                            {
                                server.Stop();
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

                    case "serverStart":
                        try
                        {
                            if (!server.IsListening)
                            {
                                server = new RestServer();
                                SetupRestServer();
                                server.Start();
                                Console.WriteLine("REST Server succesfully started");
                                DisplayServerStatus();
                            }
                            else
                            {
                                Console.WriteLine("REST Server is already running");
                            }
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine($"Cannot start REST Server. Exception: {ex.Message}");
                        }
                        
                        break;

                    case "serverRestart":
                        if (server.IsListening)
                        {
                            try
                            {
                                server.Stop();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Cannot restart REST Server; cause: cannot stop server. Exception: {ex.Message}");
                            }
                        }
                        try
                        {
                            server = new RestServer();
                            SetupRestServer();
                            server.Start();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Cannot restart REST Server; cause: cannot start server. Exception: {ex.Message}");
                        }
                        Console.WriteLine("Server succesfully restarted");
                        DisplayServerStatus();
                        break;


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
            //Console.WriteLine("Help unavailable: check the source code.");

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
            if(server.IsListening)
                Console.WriteLine($"Server listening at: {server.Host}:{server.Port}");
            else
            {
                Console.WriteLine("Server is not listening");
            }
        }
    }
}