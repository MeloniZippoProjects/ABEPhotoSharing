﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KPServices;

namespace KPTrustedParty
{
    partial class TPServer
    {
       
        static void Main()
        {
            InitializeKPABE();
            //Initialize server component: async tasks or threads?

            commandLineLoop();
        }

        private static void InitializeKPABE()
        {
            KPService.SuitePath = @"./kpabe/bin";
            KPService.UniversePath = @"./kpabe/universe";

            bool universeCheck = false;

            while(!universeCheck)
            {
                try
                {
                    Universe universe = KPService.Universe;
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
