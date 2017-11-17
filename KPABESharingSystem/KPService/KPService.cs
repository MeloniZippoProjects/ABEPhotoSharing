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

        private static Universe _Universe = null;

        public static Universe Universe
        {
            get
            {
                try
                { 
                    return _Universe ?? LoadUniverseFromFile();
                }
                catch(IOException)
                {
                    throw new UniverseNotDefinedException(String.Format("Universe is not defined and it cannot be loaded from file {0}", UniverseFilename));
                }

            }
            set
            {
                _Universe = value;
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
            Console.WriteLine(kpabeSetupProcess.StartInfo.FileName);
            kpabeSetupProcess.StartInfo.CreateNoWindow = true;
            kpabeSetupProcess.StartInfo.UseShellExecute = false;
            kpabeSetupProcess.StartInfo.Arguments = Universe.ToString();

            kpabeSetupProcess.Start();
            kpabeSetupProcess.WaitForExit();
        }

        public static Universe LoadUniverseFromFile()
        {
            _Universe = Universe.ReadFromFile(UniverseFilename);
            return _Universe;
        }

        public static void SaveUniverseToFile()
        {
            _Universe.SaveToFile(UniverseFilename);
        }

        



    }
}
