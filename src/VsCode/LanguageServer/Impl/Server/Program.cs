// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// Based on https://github.com/CXuesong/LanguageServer.NET

#define WAIT_FOR_DEBUGGER

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using JsonRpc.Standard.Client;
using JsonRpc.Standard.Contracts;
using JsonRpc.Standard.Server;
using JsonRpc.Streams;
using LanguageServer.VsCode;
using Microsoft.Common.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Console = System.Console;

namespace Microsoft.R.LanguageServer.Server {
    internal static class Program {
        private static void Main(string[] args) {
            var debugMode = CheckDebugMode(args);
            StartSessions(debugMode);
        }

        private static void StartSessions(bool debugMode) {
            var logWriter = CreateLogWriter(debugMode);

            using (logWriter)
            using (var cin = Console.OpenStandardInput())
            using (var bcin = new BufferedStream(cin))
            using (var cout = Console.OpenStandardOutput())
            using (var reader = new PartwiseStreamMessageReader(bcin))
            using (var writer = new PartwiseStreamMessageWriter(cout)) {
                var contractResolver = new JsonRpcContractResolver {
                    NamingStrategy = new CamelCaseJsonRpcNamingStrategy(),
                    ParameterValueConverter = new CamelCaseJsonValueConverter(),
                };
                var clientHandler = new StreamRpcClientHandler();
                var client = new JsonRpcClient(clientHandler);
                if (debugMode) {
                    // We want to capture log all the LSP server-to-client calls as well
                    clientHandler.MessageSending += (_, e) => {
                        lock (logWriter) logWriter.WriteLine("<C{0}", e.Message);
                    };
                    clientHandler.MessageReceiving += (_, e) => {
                        lock (logWriter) logWriter.WriteLine(">C{0}", e.Message);
                    };
                }
                // Configure & build service host
                var session = new LanguageServerSession(client, contractResolver);
                var host = BuildServiceHost(logWriter, contractResolver, debugMode);
                var serverHandler = new StreamRpcServerHandler(host,
                    StreamRpcServerHandlerOptions.ConsistentResponseSequence |
                    StreamRpcServerHandlerOptions.SupportsRequestCancellation);
                serverHandler.DefaultFeatures.Set(session);

                // If we want server to stop, just stop the "source"
                using (serverHandler.Attach(reader, writer))
                using (clientHandler.Attach(reader, writer))
                using (var rConnection = new RConnection()) {
                    rConnection.ConnectAsync().DoNotWait();
                    // Wait for the "stop" request.
                    session.CancellationToken.WaitHandle.WaitOne();
                }
            }
            logWriter?.WriteLine("Exited");
        }

        private static IJsonRpcServiceHost BuildServiceHost(TextWriter logWriter,
            IJsonRpcContractResolver contractResolver, bool debugMode) {
            var loggerFactory = new LoggerFactory();
            loggerFactory.AddProvider(new DebugLoggerProvider(null));

            var builder = new JsonRpcServiceHostBuilder {
                ContractResolver = contractResolver,
                LoggerFactory = loggerFactory
            };

            builder.UseCancellationHandling();
            builder.Register(typeof(Program).GetTypeInfo().Assembly);

            if (debugMode) {
                // Log all the client-to-server calls.
                builder.Intercept(async (context, next) => {
                    lock (logWriter) {
                        logWriter.WriteLine("> {0}", context.Request);
                    }

                    await next();

                    lock (logWriter) {
                        logWriter.WriteLine("< {0}", context.Response);
                    }
                });
            }
            return builder.Build();
        }

        private static StreamWriter CreateLogWriter(bool debugMode) {
            StreamWriter logWriter = null;
            if (debugMode) {
                var tempPath = Path.GetTempPath();
                var fileName = "messages-" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
                logWriter = File.CreateText(Path.Combine(tempPath, fileName));
                logWriter.AutoFlush = true;
            }
            return logWriter;
        }

        private static bool CheckDebugMode(string[] args) {
            var debugMode = args.Any(a => a.EqualsOrdinal("--debug"));
            if (debugMode) {
#if WAIT_FOR_DEBUGGER
                while (!Debugger.IsAttached) {
                    Thread.Sleep(1000);
                }
#endif
            }
            return debugMode;
        }
    }
}