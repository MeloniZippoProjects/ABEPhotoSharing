using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace KPServices
{
    public class ExeNames
    {
        public string Setup;
        public string Keygen;
        public string Encrypt;
        public string Decrypt;
    }

    public class Keys
    {
        public byte[] MasterKey;
        public byte[] PublicKey;
        public byte[] PrivateKey;
    }

    /// <summary>
    /// Provides utilities for the KPABE suite
    /// </summary>
    public class KpService
    {
        /// <summary>
        /// Configurable path for the KPABE suite
        /// </summary>
        public static string SuitePath = null;

        /// <summary>
        /// Configurable filenames for KPABE tools
        /// </summary>
        public static ExeNames ExeNames = new ExeNames()
        {
            Setup = "kpabe-setup.exe",
            Keygen = "kpabe-keygen.exe",
            Encrypt = "kpabe-enc.exe",
            Decrypt = "kpabe-dec.exe"
        };

        private static string GetTool(string toolName)
        {
            string path = Path.Combine(SuitePath ?? Directory.GetCurrentDirectory(), toolName);
            return path;
        }

        private static void PrepareProcessStart(ProcessStartInfo startInfo)
        {
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
        }

        /// <summary>
        /// Configurable filenames for KPABE keys
        /// </summary>
        public Keys Keys = new Keys();

        public Universe Universe { get; set; }

        public static bool ValidClientSuite =>
            File.Exists(GetTool(ExeNames.Encrypt)) && File.Exists(GetTool(ExeNames.Decrypt));

        /// <summary>
        /// Setups the KPABE encryption suite by creating the 
        /// master key and the public key.
        /// 
        /// It executes the file kpabe-setup.exe with 
        /// the current universe as argument.
        /// </summary>
        public void Setup()
        {
            string setupPath = GetTool(ExeNames.Setup);

            string publicKey = Path.GetTempFileName();
            string masterKey = Path.GetTempFileName();

            try
            {
                Process kpabeSetupProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = setupPath,
                        Arguments = $"-p \"{publicKey}\" -m \"{masterKey}\" {Universe}"
                    }
                };
                PrepareProcessStart(kpabeSetupProcess.StartInfo);

                try
                {
                    kpabeSetupProcess.Start();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    throw new ToolNotFound($"Cannot find {setupPath}");
                }

                string stderr = kpabeSetupProcess.StandardError.ReadToEnd();
                kpabeSetupProcess.WaitForExit();

#if DEBUG
            Console.WriteLine($"Command: {setupPath}");
            Console.WriteLine($"Arguments: {kpabeSetupProcess.StartInfo.Arguments}");
            Console.WriteLine($"Stderr:\n{stderr}\n/stderr");
#endif

                if (!stderr.Equals("") || kpabeSetupProcess.ExitCode != 0)
                    throw new SetupException("Error during KPABE Setup");

                Keys.PublicKey = File.ReadAllBytes(publicKey);
                Keys.MasterKey = File.ReadAllBytes(masterKey);
            }
            finally
            {
                File.Delete(publicKey);
                File.Delete(masterKey);
            }
        }

        public byte[] Keygen(string policy)
        {
            string keygenPath = GetTool(ExeNames.Keygen);

            string privateKey = Path.GetTempFileName();
            string publicKey = Path.GetTempFileName();
            string masterKey = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(publicKey, Keys.PublicKey);
                File.WriteAllBytes(masterKey, Keys.MasterKey);

                Process kpabeKeygenProcess = new Process {StartInfo = {FileName = keygenPath}};
                PrepareProcessStart(kpabeKeygenProcess.StartInfo);

                string argumentsString = $" --output {privateKey} \"{publicKey}\" \"{masterKey}\" \"{policy}\" ";
                kpabeKeygenProcess.StartInfo.Arguments = argumentsString;

                try
                {
                    kpabeKeygenProcess.Start();
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    throw new ToolNotFound($"Cannot find {keygenPath}");
                }

                string stderr = kpabeKeygenProcess.StandardError.ReadToEnd();
                kpabeKeygenProcess.WaitForExit();

#if DEBUG
            Console.WriteLine($"Stderr:\n{stderr}\n/stderr");
#endif

                if (new Regex("unsatisfiable integer comparison").IsMatch(stderr))
                    throw new UnsatisfiablePolicy(stderr);

                if (new Regex("trivially satisfied integer comparison").IsMatch(stderr))
                    throw new TrivialPolicy(stderr);

                if (new Regex("Check your attribute universe").IsMatch(stderr))
                    throw new AttributeNotFound(stderr);

                if (!stderr.Equals("") || kpabeKeygenProcess.ExitCode != 0)
                    throw new KeygenException("Error during KPABE Keygen");

                Keys.PrivateKey = File.ReadAllBytes(privateKey);
                return Keys.PrivateKey;
            }
            finally
            {
                File.Delete(publicKey);
                File.Delete(privateKey);
                File.Delete(masterKey);
            }
        }

        public void Encrypt(
            string sourceFilePath,
            string destFilePath,
            string attributes,
            bool deleteSourceFile = false)
        {
            string encryptPath = GetTool(ExeNames.Encrypt);

            string publicKey = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(publicKey, Keys.PublicKey);

                Process encryptProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = encryptPath,
                        Arguments =
                            $"{(deleteSourceFile ? "" : "--keep-input-file")} --output \"{destFilePath}\" \"{publicKey}\" \"{sourceFilePath}\" {attributes}"
                    }
                };
                PrepareProcessStart(encryptProcess.StartInfo);

#if DEBUG
            Console.WriteLine($"Encrypt path: {encryptPath}");
            Console.WriteLine($"Encrypt arguments: {encryptProcess.StartInfo.Arguments}");
#endif

                encryptProcess.Start();

                string stderr = encryptProcess.StandardError.ReadToEnd();
                encryptProcess.WaitForExit();

                if (!stderr.Equals("") || encryptProcess.ExitCode != 0)
                {
                    if (new Regex("Certain attribute not include").IsMatch(stderr))
                    {
                        //kpabe suite doesn't allow recognizing if attribute is missing
                        //or if numerical attribute has different number of bits
                        throw new AttributeNotFound(stderr);
                    }
                    if (new Regex("error parsing attribute").IsMatch(stderr))
                    {
                        throw new AttributeBitResolutionException(stderr);
                    }
                }
            }
            finally
            {
                File.Delete(publicKey);
            }
        }

        public void Decrypt(
            string sourceFilePath,
            string destFilePath,
            
            bool deleteSourceFile = false)
        {
            string decryptPath = GetTool(ExeNames.Decrypt);

            string publicKey = Path.GetTempFileName();
            string privateKey = Path.GetTempFileName();

            try
            {
                File.WriteAllBytes(publicKey, Keys.PublicKey);
                File.WriteAllBytes(privateKey, Keys.PrivateKey);

                Process decryptProcess = new Process
                {
                    StartInfo =
                    {
                        FileName = decryptPath,
                        Arguments =
                            $"{(deleteSourceFile ? "" : " --keep-input-file")} --output \"{destFilePath}\" \"{publicKey}\" \"{privateKey}\" \"{sourceFilePath}\""
                    }
                };
                PrepareProcessStart(decryptProcess.StartInfo);

#if DEBUG
            Console.WriteLine($"Decrypt path: {decryptPath}");
            Console.WriteLine($"Decrypt arguments: {decryptProcess.StartInfo.Arguments}");
#endif

                decryptProcess.Start();

                string stderr = decryptProcess.StandardError.ReadToEnd();
                decryptProcess.WaitForExit();

                if (stderr.Contains("cannot decrypt, attributes in ciphertext do not satisfy policy"))
                    throw new PolicyUnsatisfied("Attributes in ciphertext do not satisfy policy");

                if (!stderr.Equals("") || decryptProcess.ExitCode != 0)
                    throw new DecryptException("Error during KPABE Decrypt");
            }
            finally
            {
                File.Delete(publicKey);
                File.Delete(privateKey);
            }
        }
    }
}