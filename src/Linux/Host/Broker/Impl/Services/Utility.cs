using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Startup;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Microsoft.R.Host.Broker.Services {
    public class Utility {
        public static bool AuthenticateUser(ILogger<IAuthenticationService> logger, IProcessServices ps,  string username, string password, out string profileDir) {
            bool retval = false;
            Process proc = null;
            string userDir = string.Empty;
            try {
                proc = CreateRLaunchProcess(ps, true);
                using (BinaryWriter writer = new BinaryWriter(proc.StandardInput.BaseStream))
                using (BinaryReader reader = new BinaryReader(proc.StandardOutput.BaseStream)) {
                    byte[] userBytes = Encoding.UTF8.GetBytes(username);
                    byte[] passBytes = Encoding.UTF8.GetBytes(password);

                    writer.Write(userBytes.Length);
                    writer.Write(userBytes);
                    writer.Write(passBytes.Length);
                    writer.Write(passBytes);

                    // wait for the process to exit;
                    while (!proc.HasExited) {
                        Thread.Sleep(100);
                    }

                    if (proc.ExitCode == 0) {
                        retval = true;
                        int size = (int)reader.ReadUInt32();
                        byte[] bytes = new byte[size];
                        if (reader.Read(bytes, 0, size) == size) {
                            userDir = new string(Encoding.UTF8.GetChars(bytes));
                        } else {
                            logger.LogCritical(Resources.Error_NoProfileDir);
                        }
                    } else {
                        logger.LogCritical(Resources.Error_AuthFailed, GetRLaunchExitCodeMessage(proc.ExitCode));
                        retval = false;
                    }
                }
            } catch (Exception ex) {
                logger.LogCritical(Resources.Error_AuthFailed, ex.Message);
            } finally {
                if(proc != null && !proc.HasExited) {
                    proc.Kill();
                }
            }

            profileDir = userDir;
            return retval;
        }

        private static Process CreateRLaunchProcess(IProcessServices ps, bool authenticateOnly) {
            // usage:
            // R.Launch.out <-a|-r>
            //    -a : authenticate only
            //    -r : authenticate and run command
            const string rLaunchBinary = "R.Launch.out";

            string brokerDir = Path.GetDirectoryName(typeof(Program).GetTypeInfo().Assembly.Location);
            string rLaunchPath = Path.Combine(brokerDir, rLaunchBinary);

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = rLaunchPath;
            psi.Arguments = authenticateOnly ? "-a" : "-r";
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;

            return ps.Start(psi);
        }

        private static string GetRLaunchExitCodeMessage(int exitcode) {
            switch (exitcode) {
                case 200:
                    return Resources.Error_AuthInitFailed;
                case 201:
                    return Resources.Error_AuthBadInput;
                case 202:
                    return Resources.Error_AuthNoInput;
                default:
                    return exitcode.ToString();
            }
        }
    }
}
