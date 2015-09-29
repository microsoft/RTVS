using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;

namespace Microsoft.Languages.Editor.Test.Composition
{
    [ExcludeFromCodeCoverage]
    public static class TestCompositionCatalog
    {
        private static CompositionContainer _container;
        private static object _containerLock = new object();

        private static string _idePath;
        private static string _editorPath;
        private static string _privatePath;

        private static string _partsData;
        private static string _exportsData;

        private static string[] _editorAssemblies = new string[]
        {
            "Microsoft.VisualStudio.CoreUtility.dll",
            "Microsoft.VisualStudio.Editor.dll",
            "Microsoft.VisualStudio.Language.Intellisense.dll",
            "Microsoft.VisualStudio.Platform.VSEditor.dll",
            "Microsoft.VisualStudio.Text.Data.dll",
            "Microsoft.VisualStudio.Text.Logic.dll",
            "Microsoft.VisualStudio.Text.UI.dll",
            "Microsoft.VisualStudio.Text.UI.Wpf.dll",
        };

        private static string[] _rtvsMefAssemblies = new string[]
        {
            "Microsoft.Languages.Editor.dll",
            "Microsoft.Languages.Editor.Test.dll",
            "Microsoft.R.Editor.dll",
            "Microsoft.R.Editor.Test.dll",
            "Microsoft.R.Support.dll",
            "Microsoft.R.Support.Test.dll",
            "Microsoft.Markdown.Editor.dll",
        };

        private static IEnumerable<string> _additionalMefAssemblies = new string[]
        {
            "Microsoft.Languages.Editor.Application.dll"
        };

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string name = args.Name.Substring(0, args.Name.IndexOf(',')) + ".dll";
            Assembly asm = null;

            if (!string.IsNullOrEmpty(_privatePath))
            {
                string path = Path.Combine(_privatePath, name);

                if (File.Exists(path))
                {
                    asm = Assembly.LoadFrom(path);
                }
            }

            if (asm == null && !string.IsNullOrEmpty(_idePath))
            {
                string path = Path.Combine(_idePath, name);

                if (File.Exists(path))
                {
                    asm = Assembly.LoadFrom(path);
                }
            }

            return asm;
        }

        private static string GetHostVersion()
        {
            string version = Environment.GetEnvironmentVariable("ExtensionsVSVersion");

            foreach (string checkVersion in new string[]
            {
                "14.0",
            })
            {
                if (string.IsNullOrEmpty(version))
                {
                    using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\" + checkVersion))
                    {
                        if (key != null)
                        {
                            version = checkVersion;
                        }
                    }
                }
            }

            return version;
        }

        private static string GetHostExePath()
        {
            string path = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\" + GetHostVersion(), "InstallDir", string.Empty) as string;
            Assert.IsTrue(!string.IsNullOrEmpty(path) && Directory.Exists(path));
            return path;
        }

        private static CompositionContainer CreateContainer()
        {
            string thisAssembly = Assembly.GetExecutingAssembly().Location;
            string assemblyLoc = Path.GetDirectoryName(thisAssembly);

            _idePath = GetHostExePath();
            _editorPath = Path.Combine(_idePath, @"CommonExtensions\Microsoft\Editor");
            _privatePath = Path.Combine(_idePath, @"PrivateAssemblies\");

            AggregateCatalog aggregateCatalog = new AggregateCatalog();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            foreach (string asmName in _editorAssemblies)
            {
                string asmPath = Path.Combine(_editorPath, asmName);
                Assembly editorAssebmly = Assembly.LoadFrom(asmPath);

                AssemblyCatalog editorCatalog = new AssemblyCatalog(editorAssebmly);
                aggregateCatalog.Catalogs.Add(editorCatalog);
            }

            foreach (string assemblyName in _rtvsMefAssemblies)
            {
                AddAssemblyToCatalog(assemblyLoc, assemblyName, aggregateCatalog);
            }

            if (_additionalMefAssemblies != null)
            {
                foreach (string assemblyName in _additionalMefAssemblies)
                {
                    AddAssemblyToCatalog(assemblyLoc, assemblyName, aggregateCatalog);
                }
            }

            AssemblyCatalog thisAssemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly());
            aggregateCatalog.Catalogs.Add(thisAssemblyCatalog);

            return BuildCatalog(aggregateCatalog);
        }

        private static void AddAssemblyToCatalog(string assemblyLoc, string assemblyName, AggregateCatalog aggregateCatalog)
        {
            string[] paths = new string[]
            {
                Path.Combine(assemblyLoc, assemblyName),
            };

            try
            {
                Assembly assembly = null;

                foreach (string path in paths)
                {
                    if (File.Exists(path))
                    {
                        assembly = Assembly.LoadFrom(path);
                        break;
                    }
                }

                if (assembly == null)
                {
                    throw new FileNotFoundException(assemblyName);
                }

                AssemblyCatalog editorCatalog = new AssemblyCatalog(assembly);
                aggregateCatalog.Catalogs.Add(editorCatalog);
            }
            catch (Exception)
            {
                Assert.Fail("Can't find editor assembly: " + assemblyName);
            }
        }

        private static CompositionContainer BuildCatalog(AggregateCatalog aggregateCatalog)
        {
            CompositionContainer container = new CompositionContainer(aggregateCatalog, isThreadSafe: true);

            StringBuilder parts = new StringBuilder();
            StringBuilder exports = new StringBuilder();

            foreach (ComposablePartDefinition part in container.Catalog.Parts)
            {
                parts.AppendLine("===============================================================");
                parts.AppendLine(part.ToString());

                exports.AppendLine("===============================================================");
                exports.AppendLine(part.ToString());

                bool first = true;

                if (part.ExportDefinitions.FirstOrDefault() != null)
                {
                    parts.AppendLine("\t --- EXPORTS --");
                    exports.AppendLine("\t --- EXPORTS --");

                    foreach (ExportDefinition exportDefinition in part.ExportDefinitions)
                    {
                        parts.AppendLine("\t" + exportDefinition.ContractName);
                        exports.AppendLine("\t" + exportDefinition.ContractName);

                        foreach (KeyValuePair<string, object> kvp in exportDefinition.Metadata)
                        {
                            string valueString = kvp.Value != null ? kvp.Value.ToString() : string.Empty;

                            parts.AppendLine("\t" + kvp.Key + " : " + valueString);
                            exports.AppendLine("\t" + kvp.Key + " : " + valueString);
                        }

                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            parts.AppendLine("------------------------------------------------------");
                            exports.AppendLine("------------------------------------------------------");
                        }
                    }
                }

                if (part.ImportDefinitions.FirstOrDefault() != null)
                {
                    parts.AppendLine("\t --- IMPORTS ---");

                    foreach (ImportDefinition importDefinition in part.ImportDefinitions)
                    {
                        parts.AppendLine("\t" + importDefinition.ContractName);
                        parts.AppendLine("\t" + importDefinition.Constraint.ToString());
                        parts.AppendLine("\t" + importDefinition.Cardinality.ToString());

                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            parts.AppendLine("------------------------------------------------------");
                        }
                    }
                }
            }

            _partsData = parts.ToString();
            _exportsData = exports.ToString();

            return container;
        }

        public static ICompositionService CompositionService
        {
            get
            {
                lock (_containerLock)
                {
                    if (_container == null)
                    {
                        _container = CreateContainer();
                    }

                    return _container;
                }
            }
        }

        public static ExportProvider ExportProvider
        {
            get
            {
                return CompositionService as ExportProvider;
            }
        }
    }
}
