using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

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
        public static String UniversePath = "universe";
        
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
                    throw new UniverseNotDefinedException($"Universe is not defined and it cannot be loaded from file {UniversePath}");
                }
            }
            set => universe = value;
        }

        public static Universe LoadUniverseFromFile()
        {
            universe = Universe.ReadFromFile(UniversePath);
            return universe;
        }

        public static void SaveUniverseToFile()
        {
            universe.SaveToFile(UniversePath);
        }

        private static void PrepareStartInfo(ProcessStartInfo startInfo)
        {
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
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
            kpabeSetupProcess.StartInfo.FileName = kpabeSetupPath;
            PrepareStartInfo(kpabeSetupProcess.StartInfo);

            try
            {
                kpabeSetupProcess.Start();
            }
            catch(System.ComponentModel.Win32Exception)
            {
                throw new SuiteExeNotFound($"Cannot find {SuitePath}{SetupExe}");
            }

            String stderr = kpabeSetupProcess.StandardOutput.ReadToEnd();
            kpabeSetupProcess.WaitForExit();


            if (!stderr.Equals("") || kpabeSetupProcess.ExitCode != 1)
                throw new SuiteErrorException("Error during KPABE Setup");
            
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
            PrepareStartInfo(kpabeKeygenProcess.StartInfo);

            //create argument string and specify output if outputFile is not empty
            String argumentsString = String.IsNullOrEmpty(outputFile) ? "" : $" --output {outputFile}";
            argumentsString += $" {PublicKey} {MasterKey} {policy}";
            //Console.WriteLine(argumentsString);
            kpabeKeygenProcess.StartInfo.Arguments = argumentsString;

            try
            {
                kpabeKeygenProcess.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                throw new SuiteExeNotFound($"Cannot find {SuitePath}{SetupExe}");
            }

            String stderr = kpabeKeygenProcess.StandardError.ReadToEnd();
            kpabeKeygenProcess.WaitForExit();

            if (new Regex("unsatisfiable integer comparison").IsMatch(stderr))
                throw new UnsatisfiablePolicyException(stderr);

            if (new Regex("trivially satisfied integer comparison").IsMatch(stderr))
                throw new TrivialPolicyException(stderr);

            if (new Regex("Check your attribute universe").IsMatch(stderr))
                throw new AttributeNotFoundException(stderr);

            if (!stderr.Equals("") || kpabeKeygenProcess.ExitCode != 0)
                throw new SuiteErrorException("Error during KPABE Setup");

            Console.WriteLine(stderr);
            //todo: any checks for the result?? Errors?
        }

        public static void Encrypt(String sourceFilePath, String attributes, bool deleteSourceFile = false, String outputFile = "")
        {
            String kpabeEncryptPath = SuitePath + EncryptExe;

            Process kpabeEncryptProcess = new Process();
            kpabeEncryptProcess.StartInfo.FileName = kpabeEncryptPath;
            //kpabeEncryptProcess.StartInfo.CreateNoWindow = true;
            kpabeEncryptProcess.StartInfo.UseShellExecute = false;
            kpabeEncryptProcess.StartInfo.RedirectStandardOutput = true;

            String argumentString = ( deleteSourceFile ? "" : " --keep-input-file" ) +  (String.IsNullOrEmpty(outputFile) ? "" : $" --output '{outputFile}'" );
            argumentString += $" '{PublicKey}' '{sourceFilePath}' {attributes}";
            Console.WriteLine(argumentString);
            kpabeEncryptProcess.StartInfo.Arguments = argumentString;

            kpabeEncryptProcess.Start();
            String stdout = kpabeEncryptProcess.StandardOutput.ReadToEnd();
            kpabeEncryptProcess.WaitForExit();

            Console.WriteLine(stdout);

            //todo: any checks for the result?? Errors?
        }

        public static void Decrypt(String sourceFilePath, String privateKeyFilePath, bool deleteSourceFile = false, String outputFile = "")
        {
            String kpabeDecryptPath = SuitePath + DecryptExe;

            Process kpabeDecryptProcess = new Process();
            kpabeDecryptProcess.StartInfo.FileName = kpabeDecryptPath;
            //kpabeDecryptProcess.StartInfo.CreateNoWindow = true;
            kpabeDecryptProcess.StartInfo.UseShellExecute = false;
            kpabeDecryptProcess.StartInfo.RedirectStandardOutput = true;

            String argumentString = (deleteSourceFile ? "" : " --keep-input-file") + (String.IsNullOrEmpty(outputFile) ? "" : $" --output {outputFile}");
            argumentString += $" '{PublicKey}' '{privateKeyFilePath}' '{sourceFilePath}'";
            Console.WriteLine(argumentString);
            kpabeDecryptProcess.StartInfo.Arguments = argumentString;

            kpabeDecryptProcess.Start();
            String stdout = kpabeDecryptProcess.StandardOutput.ReadToEnd();
            kpabeDecryptProcess.WaitForExit();

            Console.WriteLine(stdout);

            //todo: any checks for the result?? Errors?
        }
    }
}
