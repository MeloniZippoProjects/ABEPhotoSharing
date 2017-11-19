using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPServices;

namespace Tests
{
    class Test
    {
        static void Main(string[] args)
        {
            //String universe = "canide eta=18 cosi";

            //KPService.UniverseFilename = "universe2";
            //KPService.SuitePath = @"D:\Documenti\GitHub\ABEPhotoSharing\KPABESharingSystem\Tests\bin\Debug\kpabe\";
            KPService.SuitePath = @"D:\Users\Raff\Documents\GitHub\ABEPhotoSharing\bin\";

            List<UniverseAttribute> attributes = new List<UniverseAttribute>
            {
                new SimpleAttribute("canide"),
                new NumericalAttribute("eta", 18, 12),
                new SimpleAttribute("cosi")
            };

            KPService.Universe = new Universe(attributes);

            /*
            List<PolicyElement> policies = new List<PolicyElement>
            {
                new PolicyElement(attributes.ElementAt(0)),
                new PolicyElement(attributes.ElementAt(1),PolicyType.GreaterThanOrEqual)
            };


            foreach(PolicyElement policy in policies)
            {
                Console.WriteLine(policy);
            }
            */
            
            KPService.Setup();
            KPService.Keygen("'cosi or eta > 18 # 12'", "priv_key");
            KPService.Encrypt("prova", "'canide' 'eta = 24 # 12'", false, "prova.encrypted");
            KPService.Decrypt("prova.encrypted", "priv_key", false, "prova.decrypted");

            Console.ReadLine();

            //Console.WriteLine(KPService.Universe);

            //KPService.Keygen(policies);

            //Console.WriteLine();
            return;
            
        }
    }
}
