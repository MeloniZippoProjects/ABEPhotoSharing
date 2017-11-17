using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace KPServices
{
    /// <summary>
    /// Provides utilities for the KPABE suite
    /// </summary>
    public class KPService
    {
        /// <summary>
        /// Configurable name for universe file
        /// </summary>
        public static String UniverseFilename = "universe";
        
        /// <summary>
        /// Configurable path for the KPABE suite
        /// </summary>
        public static String SuitePath = @"kpabe\";

        /// <summary>
        /// Configurable filename for KPABE Setup
        /// </summary>
        public static String SetupExe = "kpabe-setup.exe";

        /// <summary>
        /// Configurable filename for KPABE Encrypt
        /// </summary>
        public static String EncryptExe = "kpabe-enc.exe";

        /// <summary>
        /// Configurable filename for KPABE Decrypt
        /// </summary>
        public static String DecryptExe = "kpabe-dec.exe";

        /// <summary>
        /// Configurable filename for KPABE Keygen
        /// </summary>
        public static String KeygenExe = "kpabe-keygen.exe";

        public static String MasterKey = "master_key";

        public static String PublicKey = "pub_key";

        private static Universe universe = null;

        public static Universe Universe
        {
            get
            {
                try
                { 
                    return universe ?? LoadUniverseFromFile();
                }
                catch(IOException)
                {
                    throw new UniverseNotDefinedException(String.Format("Universe is not defined and it cannot be loaded from file {0}", UniverseFilename));
                }

            }
            set
            {
                universe = value;
            }
        }


        /// <summary>
        /// Setups the KPABE encryption suite by creating the 
        /// master key and the public key.
        /// 
        /// It executes the file kpabe-setup.exe with 
        /// the current universe as argument.
        /// </summary>
        public static void Setup()
        {

            String filename = SuitePath + SetupExe;

            Process kpabeSetupProcess = new Process();
            String pwd = System.IO.Directory.GetCurrentDirectory().ToString();
            kpabeSetupProcess.StartInfo.FileName =filename;
            kpabeSetupProcess.StartInfo.CreateNoWindow = true;
            kpabeSetupProcess.StartInfo.UseShellExecute = false;
            kpabeSetupProcess.StartInfo.Arguments = Universe.ToString();

            kpabeSetupProcess.Start();
            kpabeSetupProcess.WaitForExit();
        }

        public static void Keygen(IEnumerable<PolicyElement> policies, String outputFile = "")
        {
            String policyString = "";
            foreach(PolicyElement policy in policies)
            {
                policyString += policy + " ";
            }

            policyString = policyString.Substring(0, policyString.Length - 1);
        }

        public static void Keygen(String policyString, String outputFile)
        {
            String filename = SuitePath + KeygenExe;

            Process kpabeKeygenProcess = new Process();
            String pwd = System.IO.Directory.GetCurrentDirectory().ToString();
            kpabeKeygenProcess.StartInfo.FileName = filename;
            kpabeKeygenProcess.StartInfo.CreateNoWindow = true;
            kpabeKeygenProcess.StartInfo.UseShellExecute = false;

            //create argument string and specify output if outputFile is not empty
            String argumentsString = ((outputFile.Equals("")) ? "" : ("-o " + outputFile));
            argumentsString += " " + PublicKey + " " + MasterKey + " \"" + policyString + "\"";
            Console.WriteLine(argumentsString);
            kpabeKeygenProcess.StartInfo.Arguments = argumentsString;

            kpabeKeygenProcess.Start();
            kpabeKeygenProcess.WaitForExit();
        }

        public static Universe LoadUniverseFromFile()
        {
            universe = Universe.ReadFromFile(UniverseFilename);
            return universe;
        }

        public static void SaveUniverseToFile()
        {
            universe.SaveToFile(UniverseFilename);
        }
    }
}
