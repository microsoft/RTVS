﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.R.Host.Client {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Microsoft.R.Host.Client.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Previous R session terminated unexpectedly, and its global workspace is currently being saved.
        ///Do you want to abort this operation and start current session immediately?.
        /// </summary>
        internal static string AbortRDataAutosave {
            get {
                return ResourceManager.GetString("AbortRDataAutosave", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The security certificate presented by the Remote R Services does not allow us to prove that you are indeed connecting to the machine {0}.
        ///
        ///This should never happen with a production Remote R Services, so please check with your server administrator.
        ///
        ///If you are using a test Remote R Server with a self-signed certificate and are certain about the remote machine security, click Yes, otherwise click NO to terminate the connection..
        /// </summary>
        internal static string CertificateSecurityWarning {
            get {
                return ResourceManager.GetString("CertificateSecurityWarning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0}  is missing from the installation directory.
        ///Please reinstall R Tools for Visual Studio 2015.
        /// </summary>
        internal static string Error_BinaryMissing14 {
            get {
                return ResourceManager.GetString("Error_BinaryMissing14", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} is missing from the installation directory.
        ///Please reinstall Data Science workload or repair the Visual Studio installation..
        /// </summary>
        internal static string Error_BinaryMissing15 {
            get {
                return ResourceManager.GetString("Error_BinaryMissing15", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Machine &apos;{0}&apos; appears to be online, but the Remote R Service is not running..
        /// </summary>
        internal static string Error_BrokerNotRunning {
            get {
                return ResourceManager.GetString("Error_BrokerNotRunning", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation failed with unknown Win32 error, please check broker logs..
        /// </summary>
        internal static string Error_BrokerUnknownWin32Error {
            get {
                return ResourceManager.GetString("Error_BrokerUnknownWin32Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Operation failed with Win32 error: {0}.
        /// </summary>
        internal static string Error_BrokerWin32Error {
            get {
                return ResourceManager.GetString("Error_BrokerWin32Error", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Http request to &apos;{0}&apos; has failed.
        ///The machine may be offline or unreachable or  the network has been disconnected.
        ///Error: {1}.
        /// </summary>
        internal static string Error_HostNotResponding {
            get {
                return ResourceManager.GetString("Error_HostNotResponding", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Http request to &apos;{0}&apos; has failed and machine did not respond to a ping.
        ///The machine may be offline or unreachable or the network has been disconnected.
        ///Error: {1}.
        /// </summary>
        internal static string Error_HostNotRespondingToPing {
            get {
                return ResourceManager.GetString("Error_HostNotRespondingToPing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Specified R interpreter [{0}] was not found.
        /// </summary>
        internal static string Error_InterpreterNotFound {
            get {
                return ResourceManager.GetString("Error_InterpreterNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The URL format it invalid: {0}.
        /// </summary>
        internal static string Error_InvalidUrl {
            get {
                return ResourceManager.GetString("Error_InvalidUrl", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remote machine &apos;{0}&apos; does not have certificate installed for the TLS with the Remote R Service..
        /// </summary>
        internal static string Error_NoBrokerCertificate {
            get {
                return ResourceManager.GetString("Error_NoBrokerCertificate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No R interpreters installed.
        /// </summary>
        internal static string Error_NoRInterpreters {
            get {
                return ResourceManager.GetString("Error_NoRInterpreters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Web request has timed out..
        /// </summary>
        internal static string Error_OperationTimedOut {
            get {
                return ResourceManager.GetString("Error_OperationTimedOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Ping to {0}:{1} timed out..
        /// </summary>
        internal static string Error_PingTimedOut {
            get {
                return ResourceManager.GetString("Error_PingTimedOut", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to This session already has an active client connection.
        /// </summary>
        internal static string Error_PipeAlreadyConnected {
            get {
                return ResourceManager.GetString("Error_PipeAlreadyConnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Remote R requested to open {0}, but only http:// URIs are supported for remote..
        /// </summary>
        internal static string Error_RemoteUriNotSupported {
            get {
                return ResourceManager.GetString("Error_RemoteUriNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Version of R Services on the remote machine ({0})
        ///is higher than the version of R Tools on the local computer ({1}). 
        ///Please upgrade local R Tools installation to match..
        /// </summary>
        internal static string Error_RemoteVersionHigher {
            get {
                return ResourceManager.GetString("Error_RemoteVersionHigher", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Version of R Services on the remote machine ({0})
        ///is lower than the version of R Tools on the local computer ({1}). 
        ///Please upgrade remote R Services installation to match..
        /// </summary>
        internal static string Error_RemoteVersionLower {
            get {
                return ResourceManager.GetString("Error_RemoteVersionLower", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to create HTTP server for remote {0}..
        /// </summary>
        internal static string Error_RemoteWebServerCreationFailed {
            get {
                return ResourceManager.GetString("Error_RemoteWebServerCreationFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP server for remote stopped with following exception:
        ///{0}.
        /// </summary>
        internal static string Error_RemoteWebServerException {
            get {
                return ResourceManager.GetString("Error_RemoteWebServerException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP server for remote {0} failed : {1}.
        /// </summary>
        internal static string Error_RemoteWebServerFailed {
            get {
                return ResourceManager.GetString("Error_RemoteWebServerFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to R session is stopped.
        /// </summary>
        internal static string Error_RHostIsStopped {
            get {
                return ResourceManager.GetString("Error_RHostIsStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The following exception occurred during initialization of R session, and the session has been terminated:
        ///
        ///{0}.
        /// </summary>
        internal static string Error_SessionInitialization {
            get {
                return ResourceManager.GetString("Error_SessionInitialization", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to enable workspace auto-saving: {0}.
        /// </summary>
        internal static string Error_SessionInitializationAutosave {
            get {
                return ResourceManager.GetString("Error_SessionInitializationAutosave", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to set R locale to codepage {0}: {1}.
        /// </summary>
        internal static string Error_SessionInitializationCodePage {
            get {
                return ResourceManager.GetString("Error_SessionInitializationCodePage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to set CRAN mirror to {0}: {1}.
        /// </summary>
        internal static string Error_SessionInitializationMirror {
            get {
                return ResourceManager.GetString("Error_SessionInitializationMirror", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to set options(): {0}.
        /// </summary>
        internal static string Error_SessionInitializationOptions {
            get {
                return ResourceManager.GetString("Error_SessionInitializationOptions", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to R Services broker process did not start on the machine &apos;{0}&apos;. Exception:  {1}.
        /// </summary>
        internal static string Error_UnableToStartBrokerException {
            get {
                return ResourceManager.GetString("Error_UnableToStartBrokerException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to R session process did not start on the machine &apos;{0}&apos;. Exception:  {1}.
        /// </summary>
        internal static string Error_UnableToStartHostException {
            get {
                return ResourceManager.GetString("Error_UnableToStartHostException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Unknown error.
        /// </summary>
        internal static string Error_UnknownError {
            get {
                return ResourceManager.GetString("Error_UnknownError", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP error while creating session on the machine &apos;{0}&apos;. Exception: {1}.
        /// </summary>
        internal static string HttpErrorCreatingSession {
            get {
                return ResourceManager.GetString("HttpErrorCreatingSession", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP server for remote transfers data from remote R machine.
        ///Server at {0} http://{1}:{2} connects to {3} {4}..
        /// </summary>
        internal static string Info_RemoteWebServerDetails {
            get {
                return ResourceManager.GetString("Info_RemoteWebServerDetails", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP server for remote {0} listening on http://{1}:{2}. .
        /// </summary>
        internal static string Info_RemoteWebServerStarted {
            get {
                return ResourceManager.GetString("Info_RemoteWebServerStarted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP server for remote {0} starting ....
        /// </summary>
        internal static string Info_RemoteWebServerStarting {
            get {
                return ResourceManager.GetString("Info_RemoteWebServerStarting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to HTTP server for remote {0} stopped..
        /// </summary>
        internal static string Info_RemoteWebServerStopped {
            get {
                return ResourceManager.GetString("Info_RemoteWebServerStopped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Interactive Window is disconnected from R session..
        /// </summary>
        internal static string RHostDisconnected {
            get {
                return ResourceManager.GetString("RHostDisconnected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connecting to R Workspace failed.
        ///Reason: {0}.
        /// </summary>
        internal static string RSessionProvider_ConnectionFailed {
            get {
                return ResourceManager.GetString("RSessionProvider_ConnectionFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The supplied TLS certificate is not trusted: {0}.
        /// </summary>
        internal static string Trace_UntrustedCertificate {
            get {
                return ResourceManager.GetString("Trace_UntrustedCertificate", resourceCulture);
            }
        }
    }
}
