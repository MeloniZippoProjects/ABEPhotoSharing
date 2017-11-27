using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine.Server;
using KPServices;

namespace KPTrustedParty
{
    partial class TPServer
    {
        private static Universe universe;
        public static string Host;

        static void Main()
        {
            InitializeKPABE();
            Console.InputEncoding = Encoding.UTF8;
            //Initialize server component: async tasks or threads?
            using (var server = new RestServer())
            {
                server.LogToConsole();
                server.Start();
                Host = server.Host;
                CommandLineLoop();
                server.Stop();
            }
                
        }

        private static void InitializeKPABE()
        {
            KPService.SuitePath = @"./kpabe/bin";
            //KPService.UniversePath = @"./kpabe/universe";
            KPDatabase.GetLatestUniverse();

            //todo: remove this skipping after db development
            bool universeCheck = false;
            //bool universeCheck = true;

            while (!universeCheck)
            {
                try
                {
                    universe = KPService.Universe;
                    universeCheck = true;
                }
                catch(UniverseNotDefinedException ex)
                {
                    Console.WriteLine($"WARNING: {ex.Message}");
                    Console.WriteLine("If this is the first execution of the server, continue with the UniverseEditor to define the universe");

                    UniverseEditor();
                }
            }

            //TODO: check for master and public key existance, generate them otherwise. Use toy encryption/decription for the test
        }
    }
}
