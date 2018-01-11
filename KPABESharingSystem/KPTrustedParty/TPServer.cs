using System;
using System.IO;
using System.Text;
using Grapevine.Server;
using KPServices;
using KPTrustedParty.Database;
using KPTrustedParty.Properties;

namespace KPTrustedParty
{
    partial class TpServer
    {
        public static Universe Universe => KpService.Universe;

        public static void InitializeUniverse(Universe newUniverse)
        {
            KpService.Universe = newUniverse;
            KpService.Setup();
            using (TemporaryBytes masterKey = KpService.Keys.MasterKey,
                publicKey = KpService.Keys.PublicKey)
            {
                KpDatabase.InsertUniverse(
                    Universe.ToString(),
                    masterKey,
                    publicKey);
            }
        }
        
        public static string Host;

        public static SecureBytes KpPublicKey => KpService.Keys.PublicKey;
        private static readonly KpService KpService = new KpService();
        private static RestServer restServer = new RestServer();

        static void Main()
        {
            InitializeKpabe();
            Console.InputEncoding = Encoding.UTF8;
            Settings settings = Settings.Default;
            Host = settings.ServerHost;

            //server.LogToConsole();
            try
            {
                SetupRestServer();
                restServer.Start();
                DisplayServerStatus();
                CommandLineLoop();
            }
            finally
            {
                restServer.Stop();
            }
        }

        private static void SetupRestServer()
        {
            Settings settings = Settings.Default;
            restServer.Port = settings.ServerPort.ToString();
            restServer.UseHttps = true;
            restServer.Host = settings.ServerHost;
        }

        private static void InitializeKpabe()
        {
            CheckAndPopulateDefaultSettings();

            Settings settings = Settings.Default;

            KpService.SuitePath = settings.KPSuitePath;
            KpDatabase.Universe dbLatestUniverse = KpDatabase.GetLatestUniverse();

            if (dbLatestUniverse != null)
            {
                KpService.Universe = Universe.FromString(dbLatestUniverse.UniverseString);
                KpService.Keys.MasterKey = dbLatestUniverse.MasterKey;
                KpService.Keys.PublicKey = dbLatestUniverse.PublicKey;
            }
            else
            {
                Console.WriteLine("WARNING: Universe not defined");
                Console.WriteLine(
                    "If this is the first execution of the server, continue with the UniverseEditor to define the Universe");
                UniverseEditor();
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
            Settings settings = Settings.Default;

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