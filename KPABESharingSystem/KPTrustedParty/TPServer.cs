using System;
using System.Collections.Generic;
using System.IO;
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

        public static byte[] KpPublicKey => kpService.Keys.PublicKey;
        private static readonly KPService kpService = new KPService();

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
            KPService.SuitePath = Path.Combine(Directory.GetCurrentDirectory(), "kpabe");
            //KPService.UniversePath = @"./kpabe/universe";
            var dbLatestUniverse = KPDatabase.GetLatestUniverse();

            //todo: remove this skipping after db development
            bool universeCreated;
            //bool universeCheck = true;

            if (dbLatestUniverse != null)
            {
                kpService.Universe = Universe.FromString(dbLatestUniverse.UniverseString);
                universe = kpService.Universe.Copy();
                kpService.Keys.MasterKey = dbLatestUniverse.MasterKey;
                kpService.Keys.PublicKey = dbLatestUniverse.PublicKey;
                universeCreated = false;
            }
            else
            {
                Console.WriteLine("WARNING: Universe not defined");
                Console.WriteLine("If this is the first execution of the server, continue with the UniverseEditor to define the universe");
                UniverseEditor();
                kpService.Universe = universe;
                kpService.Setup();
                universeCreated = true;
            }
            
            if(universeCreated)
                KPDatabase.InsertUniverse(universe.ToString(), kpService.Keys.MasterKey, kpService.Keys.PublicKey);
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
    }

}
