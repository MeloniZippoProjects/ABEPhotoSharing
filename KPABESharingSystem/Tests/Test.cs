using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KPServices;
using Newtonsoft.Json;

namespace Tests
{
    class Test
    {
        static void Main(string[] args)
        {
            var jsonResponse = new Dictionary<string, string>();
            jsonResponse["error"] = "bad_format";
            jsonResponse["errorDescription"] =
                "The request format must be \"username=<username>;password=<password>\"";

            Console.WriteLine(JsonConvert.SerializeObject(jsonResponse));


            //string universe = "canide eta=18 cosi";

            //KPService.UniversePath = "universe2";
            //KPService.SuitePath = @"D:\Documenti\GitHub\ABEPhotoSharing\KPABESharingSystem\Tests\bin\Debug\kpabe\";

            //NumericalAttributeParsingTest();

            //Console.ReadLine();

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



            //Console.WriteLine(KPService.Universe);

            //KPService.Keygen(policies);

            //Console.WriteLine();
            return;
            
        }

        private static void NumericalAttributeParsingTest()
        {
            string[] numericalAttributes = new[]
            {
                //OK
                "eta = # 12",
                "eta=",
                "     eta          =          #            25         ",

                //NOT OK
                "and =",
                "eta",
                "eta = #",
                "eta = 24 # 12",
                "eta = # 120"
            };

            foreach (string numericalAttribute in numericalAttributes)
            {
                try
                {
                    var na = new NumericalAttribute(numericalAttribute);
                    Console.WriteLine(na);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private static void toolTest()
        {
            /*
            KPService.SuitePath = @"D:\Users\Raff\Documents\GitHub\ABEPhotoSharing\bin\";

            List<UniverseAttribute> attributes = new List<UniverseAttribute>
            {
                new SimpleAttribute("canide"),
                new NumericalAttribute("eta = # 12"),
                new SimpleAttribute("cosi")
            };

            KPService.Universe = new Universe(attributes);

            KPService.Setup();
            KPService.Keygen("'cosi or eta > 18 # 12'", "priv_key");
            KPService.Encrypt("prova", "'canide' 'eta = 24 # 12'", false, "prova.encrypted");
            KPService.Decrypt("prova.encrypted", "priv_key", false, "prova.decrypted");

            Console.ReadLine();
            */
        }
    }
}
