using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

        /// <summary>
        /// Configurable filename for KPABE master key
        /// </summary>
        public static String MasterKey = "master_key";

        /// <summary>
        /// Configurable filename for KPABE public key
        /// </summary>
        public static String PublicKey = "pub_key";

        private static Universe universe;

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
                    throw new UniverseNotDefinedException($"Universe is not defined and it cannot be loaded from file {UniverseFilename}");
                }

            }
            set => universe = value;
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

        /// <summary>
        /// Setups the KPABE encryption suite by creating the 
        /// master key and the public key.
        /// 
        /// It executes the file kpabe-setup.exe with 
        /// the current universe as argument.
        /// </summary>
        public static void Setup()
        {
            String kpabeSetupPath = SuitePath + SetupExe;

            Process kpabeSetupProcess = new Process();
            //String pwd = Directory.GetCurrentDirectory();
            kpabeSetupProcess.StartInfo.FileName = kpabeSetupPath;
            kpabeSetupProcess.StartInfo.CreateNoWindow = true;
            kpabeSetupProcess.StartInfo.UseShellExecute = false;
            kpabeSetupProcess.StartInfo.Arguments = Universe.ToString();

            kpabeSetupProcess.Start();
            kpabeSetupProcess.WaitForExit();

            //todo: any checks for the result?? Errors?
        }

        /*
        public static void Keygen(IEnumerable<PolicyElement> policies, String outputFile = "")
        {
            String policyString = "";
            foreach(PolicyElement policy in policies)
            {
                policyString += policy + " ";
            }

            policyString = policyString.Substring(0, policyString.Length - 1);
        }
        */

        public static void Keygen(String policy, String outputFile = "")
        {
            String kpabeKeygenPath = SuitePath + KeygenExe;

            Process kpabeKeygenProcess = new Process();
            //String pwd = Directory.GetCurrentDirectory();
            kpabeKeygenProcess.StartInfo.FileName = kpabeKeygenPath;
            kpabeKeygenProcess.StartInfo.CreateNoWindow = true;
            kpabeKeygenProcess.StartInfo.UseShellExecute = false;

            //create argument string and specify output if outputFile is not empty
            String argumentsString = String.IsNullOrEmpty(outputFile) ? "" : $" --output {outputFile}";
            argumentsString += $" {PublicKey} {MasterKey} {policy}";
            Console.WriteLine(argumentsString);
            kpabeKeygenProcess.StartInfo.Arguments = argumentsString;

            kpabeKeygenProcess.Start();
            kpabeKeygenProcess.WaitForExit();

            //todo: any checks for the result?? Errors?
        }

        public static void Encrypt(String sourceFilePath, String attributes, bool deleteSourceFile = false, String outputFile = "")
        {
            String kpabeEncryptPath = SuitePath + KeygenExe;

            Process kpabeEncryptProcess = new Process();
            kpabeEncryptProcess.StartInfo.FileName = kpabeEncryptPath;
            kpabeEncryptProcess.StartInfo.CreateNoWindow = true;
            kpabeEncryptProcess.StartInfo.UseShellExecute = false;

            String argumentString = ( deleteSourceFile ? "" : "--keep-input-file" ) +  (String.IsNullOrEmpty(outputFile) ? "" : $"--output {outputFile}" );
            argumentString += $" {PublicKey} {sourceFilePath} {attributes}";
            Console.WriteLine(argumentString);
            kpabeEncryptProcess.StartInfo.Arguments = argumentString;

            kpabeEncryptProcess.Start();
            kpabeEncryptProcess.WaitForExit();

            //todo: any checks for the result?? Errors?
        }
    }
}
