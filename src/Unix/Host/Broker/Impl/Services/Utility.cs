// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Sessions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Microsoft.R.Host.Broker.Services {
    public class Utility {
        private const string PamInfo = "pam-info";
        private const string PamError = "pam-error";
        private const string SysError = "unix-error";
        private const string JsonError = "json-error";
        private const string RtvsResult = "rtvs-result";
        private const string RtvsError = "rtvs-error";

        public static IProcess RunAsCurrentUser(ILogger<Session> logger, IProcessServices ps, string arguments, string rHomePath, string loadLibPath) {
            var psi = new ProcessStartInfo {
                FileName = PathConstants.RunHostBinPath,
                Arguments = arguments,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                WorkingDirectory = Environment.GetEnvironmentVariable("PWD")
            };

            // All other should be same as the broker environment. Only these are set based on interpreters. 
            // R_HOME is explictly set on the R-Host.
            psi.Environment.Add("R_HOME", rHomePath);
            psi.Environment.Add("LD_LIBRARY_PATH", loadLibPath);

            return ps.Start(psi);
        }

        public static IProcess AuthenticateAndRunAsUser(ILogger<Session> logger, IProcessServices ps, string username, string password, string profileDir, IEnumerable<string> arguments, IDictionary<string, string> environment) {
            var proc = CreateRunAsUserProcess(ps, true);
            using (var writer = new BinaryWriter(proc.StandardInput.BaseStream, Encoding.UTF8, true)) {
                var message = new AuthenticateAndRunMessage() {
                    Username = GetUnixUserName(username),
                    Password = password,
                    Arguments = arguments,
                    Environment = environment.Select(e => $"{e.Key}={e.Value}"),
                    WorkingDirectory = profileDir
                };
                var json = JsonConvert.SerializeObject(message, GetJsonSettings());
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                writer.Write(jsonBytes.Length);
                writer.Write(jsonBytes);
                writer.Flush();
            }

            return proc;
        }

        public static bool AuthenticateUser(ILogger<IPlatformAuthenticationService> logger, IProcessServices ps,  string username, string password, string allowedGroup, out string profileDir) {
            var retval = false;
            IProcess proc = null;
            var userDir = string.Empty;
            try {
                proc = CreateRunAsUserProcess(ps, false);
                using (var writer = new BinaryWriter(proc.StandardInput.BaseStream, Encoding.UTF8, true))
                using (var reader = new BinaryReader(proc.StandardOutput.BaseStream, Encoding.UTF8, true)) {
                    var message = new AuthenticationOnlyMessage() { Username = GetUnixUserName(username), Password = password, AllowedGroup = allowedGroup };
                    var json = JsonConvert.SerializeObject(message, GetJsonSettings());
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    writer.Write(jsonBytes.Length);
                    writer.Write(jsonBytes);
                    writer.Flush();

                    proc.WaitForExit(3000);

                    if (proc.HasExited && proc.ExitCode == 0) {
                        var size = reader.ReadInt32();
                        var bytes = reader.ReadBytes(size);
                        var arr = JsonConvert.DeserializeObject<JArray>(Encoding.UTF8.GetString(bytes));
                        if(arr.Count > 1) {
                            var respType = arr[0].Value<string>();
                            switch (respType) {
                                case PamInfo:
                                case PamError:
                                    var pam = arr[1].Value<string>();
                                    logger.LogCritical(Resources.Error_PAMAuthenticationError.FormatInvariant(pam));
                                    break;
                                case JsonError:
                                    var jerror = arr[1].Value<string>();
                                    logger.LogCritical(Resources.Error_RunAsUserJsonError.FormatInvariant(jerror));
                                    break;
                                case RtvsResult:
                                    userDir = arr[1].Value<string>();
                                    retval = true;
                                    if (userDir.Length == 0) {
                                        logger.LogError(Resources.Error_NoProfileDir);
                                    }
                                    break;
                                case RtvsError:
                                    var resource = arr[1].Value<string>();
                                    logger.LogCritical(Resources.Error_RunAsUserFailed.FormatInvariant(Resources.ResourceManager.GetString(resource)));
                                    break;
                            }
                        } else {
                            logger.LogCritical(Resources.Error_InvalidRunAsUserResponse);
                        }
                            
                    } else {
                        logger.LogCritical(Resources.Error_AuthFailed, GetRLaunchExitCodeMessage(proc.ExitCode));
                    }

                }
            } catch (Exception ex) {
                logger.LogCritical(Resources.Error_AuthFailed, ex.Message);
            } finally {
                if (proc != null && !proc.HasExited) {
                    try {
                        proc.Kill();
                    } catch (Exception ex) when (!ex.IsCriticalException()) {
                    }
                }
            }

            profileDir = userDir;
            return retval;
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

        public static string GetUnixUserName(string source) {
            // This is needed because the windows credential UI uses domain\username format.
            // This will not be required if we can show generic credential UI for Linux remote.
            // <<unix>>\<username>, #unix\<username> or #local\<username> format should be used 
            // to for local accounts only.
            const string unixPrefix = "<<unix>>\\";
            const string unixPrefix2 = "#unix\\";
            const string localPrefix = "#local\\";
            if (source.StartsWithIgnoreCase(unixPrefix)) {
                return source.Substring(unixPrefix.Length);
            } else if (source.StartsWithIgnoreCase(unixPrefix2)) {
                return source.Substring(unixPrefix2.Length);
            } else if (source.StartsWithIgnoreCase(localPrefix)) {
                return source.Substring(localPrefix.Length);
            }

            return source;
        }

        private static JsonSerializerSettings GetJsonSettings() {
            return new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static IProcess CreateRunAsUserProcess(IProcessServices ps, bool quietMode) {
            var psi = new ProcessStartInfo {
                FileName = PathConstants.RunAsUserBinPath,
                Arguments = quietMode ? "-q" : "",
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            return ps.Start(psi);
        }
    }
}
