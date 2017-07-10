// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Common.Core;
using Microsoft.Common.Core.OS;
using Microsoft.Extensions.Logging;
using Microsoft.R.Host.Broker.Sessions;
using Microsoft.R.Host.Broker.Startup;
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

        public static Process AuthenticateAndRunAsUser(ILogger<Session> logger, IProcessServices ps, string username, string password, string profileDir, IEnumerable<string> arguments, IDictionary<string, string> environment) {
            Process proc = CreateRunAsUserProcess(ps, true);
            using (BinaryWriter writer = new BinaryWriter(proc.StandardInput.BaseStream, Encoding.UTF8, true)) {
                var message = new AuthenticateAndRunMessage() {
                    Username = GetUnixUserName(username),
                    Password = password,
                    Arguments = arguments,
                    Environment = environment.Select(e => $"{e.Key}={e.Value}"),
                    WorkingDirectory = profileDir
                };
                string json = JsonConvert.SerializeObject(message, GetJsonSettings());
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                writer.Write(jsonBytes.Length);
                writer.Write(jsonBytes);
                writer.Flush();
            }

            return proc;
        }

        public static bool AuthenticateUser(ILogger<IAuthenticationService> logger, IProcessServices ps,  string username, string password, string allowedGroup, out string profileDir) {
            bool retval = false;
            Process proc = null;
            string userDir = string.Empty;
            try {
                proc = CreateRunAsUserProcess(ps, false);
                using (BinaryWriter writer = new BinaryWriter(proc.StandardInput.BaseStream, Encoding.UTF8, true))
                using (BinaryReader reader = new BinaryReader(proc.StandardOutput.BaseStream, Encoding.UTF8, true)) {
                    var message = new AuthenticationOnlyMessage() { Username = GetUnixUserName(username), Password = password, AllowedGroup = allowedGroup };
                    string json = JsonConvert.SerializeObject(message, GetJsonSettings());
                    var jsonBytes = Encoding.UTF8.GetBytes(json);
                    writer.Write(jsonBytes.Length);
                    writer.Write(jsonBytes);
                    writer.Flush();

                    proc.WaitForExit(3000);

                    if (proc.HasExited && proc.ExitCode == 0) {
                        int size = reader.ReadInt32();
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
            // <<unix>>\<username> format should be used to for local accounts only.
            const string unixPrefix = "<<unix>>\\";
            if (source.StartsWithIgnoreCase(unixPrefix)) {
                return source.Substring(unixPrefix.Length);
            }
            return source;
        }

        private static JsonSerializerSettings GetJsonSettings() {
            return new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static Process CreateRunAsUserProcess(IProcessServices ps, bool quietMode) {
        ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = PathConstants.RunAsUserBinPath;
            psi.Arguments = quietMode ? "-q" : "";
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;

            return ps.Start(psi);
        }
    }
}
