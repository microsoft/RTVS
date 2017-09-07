Param(
    [Parameter(Position=1)]
    [string]$binPath,
    [Parameter(Position=2)]
    [string]$vscPath
)

$bin = Resolve-Path -Path $binPath
$vsc = Resolve-Path -Path $vscPath

#&npm install -g vsce

&cd $vsc

&md -Force server/rtvs/man
&md -Force server/rtvs/R
&cd server

&copy $bin/Newtonsoft.Json.dll

&copy $bin/Microsoft.R.Host.exe
&copy $bin/Microsoft.R.Host.Broker.Windows.exe
&copy $bin/Microsoft.R.Platform.Windows.dll
&copy $bin/Microsoft.R.Containers.Windows.dll

&copy $bin/netcoreapp1.1/Microsoft.R.Host.Broker.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Host.Broker.Linux.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Host.Protocol.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Host.Client.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Common.Core.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Components.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Platform.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Platform.Linux.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Core.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Editor.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Containers.dll
&copy $bin/netcoreapp1.1/Microsoft.R.Containers.Linux.dll
&copy $bin/netcoreapp1.1/Microsoft.R.LanguageServer.dll

&copy $bin/rtvs/DESCRIPTION ./rtvs
&copy $bin/rtvs/NAMESPACE ./rtvs
&copy $bin/rtvs/man/fetch_file.Rd ./rtvs/man
&copy $bin/rtvs/R/breakpoints.R ./rtvs/R
&copy $bin/rtvs/R/completions.R ./rtvs/R
&copy $bin/rtvs/R/eval.R ./rtvs/R
&copy $bin/rtvs/R/graphics.R ./rtvs/R
&copy $bin/rtvs/R/grid.R ./rtvs/R
&copy $bin/rtvs/R/help.R ./rtvs/R
&copy $bin/rtvs/R/mirror.R ./rtvs/R
&copy $bin/rtvs/R/overrides.R ./rtvs/R
&copy $bin/rtvs/R/packages.R ./rtvs/R
&copy $bin/rtvs/R/repr.R ./rtvs/R
&copy $bin/rtvs/R/rtvs ./rtvs/R
&copy $bin/rtvs/R/traceback.R ./rtvs/R
&copy $bin/rtvs/R/util.R ./rtvs/R

&copy $bin/Microsoft.AspNetCore.Antiforgery.dll
&copy $bin/Microsoft.AspNetCore.Authorization.dll
&copy $bin/Microsoft.AspNetCore.Authentication.dll
&copy $bin/Microsoft.AspNetCore.Cors.dll
&copy $bin/Microsoft.AspNetCore.Cryptography.Internal.dll
&copy $bin/Microsoft.AspNetCore.DataProtection.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.DataProtection.dll
&copy $bin/Microsoft.AspNetCore.Diagnostics.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.Hosting.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.Hosting.dll
&copy $bin/Microsoft.AspNetCore.Hosting.Server.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.Html.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.Http.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.Http.dll
&copy $bin/Microsoft.AspNetCore.Http.Extensions.dll
&copy $bin/Microsoft.AspNetCore.Http.Features.dll
&copy $bin/Microsoft.AspNetCore.JsonPatch.dll
&copy $bin/Microsoft.AspNetCore.Localization.dll
&copy $bin/Microsoft.AspNetCore.Mvc.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.Mvc.ApiExplorer.dll
&copy $bin/Microsoft.AspNetCore.Mvc.Core.dll
&copy $bin/Microsoft.AspNetCore.Mvc.Cors.dll
&copy $bin/Microsoft.AspNetCore.Mvc.DataAnnotations.dll
&copy $bin/Microsoft.AspNetCore.Mvc.dll
&copy $bin/Microsoft.AspNetCore.Mvc.Formatters.Json.dll
&copy $bin/Microsoft.AspNetCore.Mvc.Localization.dll
&copy $bin/Microsoft.AspNetCore.Mvc.Razor.dll
&copy $bin/Microsoft.AspNetCore.Mvc.Razor.Host.dll
&copy $bin/Microsoft.AspNetCore.Mvc.TagHelpers.dll
&copy $bin/Microsoft.AspNetCore.Mvc.ViewFeatures.dll
&copy $bin/Microsoft.AspNetCore.Razor.dll
&copy $bin/Microsoft.AspNetCore.Razor.Runtime.dll
&copy $bin/Microsoft.AspNetCore.Routing.Abstractions.dll
&copy $bin/Microsoft.AspNetCore.Routing.dll
&copy $bin/Microsoft.AspNetCore.Server.WebListener.dll
&copy $bin/Microsoft.AspNetCore.Server.Kestrel.dll
&copy $bin/Microsoft.AspNetCore.Server.Kestrel.Https.dll
&copy $bin/Microsoft.AspNetCore.WebSockets.Protocol.dll
&copy $bin/Microsoft.AspNetCore.WebSockets.Server.dll
&copy $bin/Microsoft.AspNetCore.WebUtilities.dll
&copy $bin/Microsoft.CodeAnalysis.CSharp.dll
&copy $bin/Microsoft.CodeAnalysis.dll
&copy $bin/Microsoft.DotNet.PlatformAbstractions.dll
&copy $bin/Microsoft.Extensions.Caching.Abstractions.dll
&copy $bin/Microsoft.Extensions.Caching.Memory.dll
&copy $bin/Microsoft.Extensions.Configuration.Abstractions.dll
&copy $bin/Microsoft.Extensions.Configuration.Binder.dll
&copy $bin/Microsoft.Extensions.Configuration.CommandLine.dll
&copy $bin/Microsoft.Extensions.Configuration.dll
&copy $bin/Microsoft.Extensions.Configuration.EnvironmentVariables.dll
&copy $bin/Microsoft.Extensions.Configuration.FileExtensions.dll
&copy $bin/Microsoft.Extensions.Configuration.Json.dll
&copy $bin/Microsoft.Extensions.DependencyInjection.Abstractions.dll
&copy $bin/Microsoft.Extensions.DependencyInjection.dll
&copy $bin/Microsoft.Extensions.DependencyModel.dll
&copy $bin/Microsoft.Extensions.FileProviders.Abstractions.dll
&copy $bin/Microsoft.Extensions.FileProviders.Composite.dll
&copy $bin/Microsoft.Extensions.FileProviders.Physical.dll
&copy $bin/Microsoft.Extensions.FileSystemGlobbing.dll
&copy $bin/Microsoft.Extensions.Globalization.CultureInfoCache.dll
&copy $bin/Microsoft.Extensions.Localization.Abstractions.dll
&copy $bin/Microsoft.Extensions.Localization.dll
&copy $bin/Microsoft.Extensions.Logging.Abstractions.dll
&copy $bin/Microsoft.Extensions.Logging.Console.dll
&copy $bin/Microsoft.Extensions.Logging.Debug.dll
&copy $bin/Microsoft.Extensions.Logging.EventLog.dll
&copy $bin/Microsoft.Extensions.Logging.TraceSource.dll
&copy $bin/Microsoft.Extensions.Logging.dll
&copy $bin/Microsoft.Extensions.ObjectPool.dll
&copy $bin/Microsoft.Extensions.Options.ConfigurationExtensions.dll
&copy $bin/Microsoft.Extensions.Options.dll
&copy $bin/Microsoft.Extensions.PlatformAbstractions.dll
&copy $bin/Microsoft.Extensions.Primitives.dll
&copy $bin/Microsoft.Extensions.WebEncoders.dll
&copy $bin/Microsoft.Net.Http.Server.dll
&copy $bin/Microsoft.Net.Http.Headers.dll
&copy $bin/Microsoft.Win32.Primitives.dll
&copy $bin/System.AppContext.dll
&copy $bin/System.Buffers.dll
&copy $bin/System.Collections.Immutable.dll
&copy $bin/System.ComponentModel.Primitives.dll
&copy $bin/System.ComponentModel.TypeConverter.dll
&copy $bin/System.Composition.AttributedModel.dll
&copy $bin/System.Composition.Convention.dll
&copy $bin/System.Composition.Hosting.dll
&copy $bin/System.Composition.Runtime.dll
&copy $bin/System.Composition.TypedParts.dll
&copy $bin/System.Console.dll
&copy $bin/System.Diagnostics.DiagnosticSource.dll
&copy $bin/System.Diagnostics.FileVersionInfo.dll
&copy $bin/System.Diagnostics.StackTrace.dll
&copy $bin/System.IO.Compression.dll
&copy $bin/System.IO.Compression.ZipFile.dll
&copy $bin/System.IO.FileSystem.dll
&copy $bin/System.IO.FileSystem.Primitives.dll
&copy $bin/System.Net.Http.dll
&copy $bin/System.Net.Http.WinHttpHandler.dll
&copy $bin/System.Net.Sockets.dll
&copy $bin/System.Numerics.Vectors.dll
&copy $bin/System.Reflection.Metadata.dll
&copy $bin/System.Runtime.CompilerServices.Unsafe.dll
&copy $bin/System.Runtime.InteropServices.RuntimeInformation.dll
&copy $bin/System.Security.Cryptography.Algorithms.dll
&copy $bin/System.Security.Cryptography.Encoding.dll
&copy $bin/System.Security.Cryptography.Primitives.dll
&copy $bin/System.Security.Cryptography.X509Certificates.dll
&copy $bin/System.Text.Encoding.CodePages.dll
&copy $bin/System.Text.Encodings.Web.dll
&copy $bin/System.Threading.Tasks.Dataflow.dll
&copy $bin/System.Threading.Tasks.Extensions.dll
&copy $bin/System.Threading.Thread.dll
&copy $bin/System.ValueTuple.dll
&copy $bin/System.Xml.ReaderWriter.dll
&copy $bin/System.Xml.XmlDocument.dll
&copy $bin/System.Xml.XPath.dll
&copy $bin/System.Xml.XPath.XDocument.dll
&copy $bin/libuv.dll

&cd ..
&vsce package
