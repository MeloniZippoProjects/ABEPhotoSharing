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
        public static Universe Universe;
        public static string Host;

        public static byte[] KpPublicKey => kpService.Keys.PublicKey;
        private static readonly KPService kpService = new KPService();

        static void Main()
        {
            InitializeKPABE();
            Console.InputEncoding = Encoding.UTF8;
            var settings = KPTrustedParty.Properties.Settings.Default;

            //todo: server doesn't stop in case of exceptions
            using (var server = new RestServer())
            {
                server.LogToConsole();
                server.Port = settings.ServerPort.ToString();
                server.UseHttps = true;
                server.Host = settings.ServerHost;
                server.Start();
                CommandLineLoop();
                server.Stop();
            }
        }

        private static void InitializeKPABE()
        {
            CheckAndPopulateDefaultSettings();

            var settings = KPTrustedParty.Properties.Settings.Default;

            KPService.SuitePath = settings.KPSuitePath;
            var dbLatestUniverse = KPDatabase.GetLatestUniverse();

            if (dbLatestUniverse != null)
            {
                kpService.Universe = Universe.FromString(dbLatestUniverse.UniverseString);
                Universe = kpService.Universe.Copy();
                kpService.Keys.MasterKey = dbLatestUniverse.MasterKey;
                kpService.Keys.PublicKey = dbLatestUniverse.PublicKey;
            }
            else
            {
                Console.WriteLine("WARNING: Universe not defined");
                Console.WriteLine("If this is the first execution of the server, continue with the UniverseEditor to define the Universe");
                UniverseEditor();
                kpService.Universe = Universe;
                kpService.Setup();
                KPDatabase.InsertUniverse(Universe.ToString(), kpService.Keys.MasterKey, kpService.Keys.PublicKey);
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

        private static void CheckAndPopulateDefaultSettings()
        {
            var settings = KPTrustedParty.Properties.Settings.Default;

            if (String.IsNullOrEmpty(settings.KPSuitePath))
                settings.KPSuitePath = Path.Combine(Directory.GetCurrentDirectory(), "kpabe");

            if (String.IsNullOrEmpty(settings.ServerHost))
                settings.ServerHost = "192.168.1.115";

            if (settings.ServerPort == 0)
                settings.ServerPort = 443;

            settings.Save();
        }
    }
}
