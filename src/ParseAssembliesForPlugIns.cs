using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;

namespace EPiOptimiser
{
    /// <summary>
    ///     Parse assemblies in a folder that implement any kind of EPiServer plug in mechanism
    /// </summary>
    public class ParseAssembliesForPlugIns
    {
        private string targetPath;

        /// <summary>
        ///     Parse a target folder for assemblies that implement any kind of EPiServer plug in
        ///     mechanism and return a list of assemblies that are safe to ignore
        /// </summary>
        /// <param name="path">The path to scan, e.g. C:\projects\MyProject\wwwroot\bin</param>
        /// <returns>A list of assemblies that should be safe to ignore</returns>
        public IList<string> GetSafeToIgnoreAssemblies(string path)
        {
            List<string> safeRemoveAssemblies = new List<string>();

            //Ensure we always parse out the target path
            this.targetPath = path.Substring(0, path.Length - Path.GetFileName(path).Length);
            this.targetPath += @"\bin";

            //Add a custom assembly resolver to ensure we are always probing in the target path
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomainAssemblyResolve;

            foreach (FileInfo info in new DirectoryInfo(this.targetPath).GetFiles("*.dll"))
            {
                try
                {
                    //Check if there are any MEF references, to check for the EPiServer initialisation system
                    Assembly assembly = Assembly.LoadFile(info.FullName);
                    if (new AssemblyCatalog(assembly).Parts.Any())
                    {
                        continue;
                    }

                    bool isOKToRemove = true;

                    //Check if something has a plug in attribute before proceeding
                    Type[] types = assembly.GetTypes();
                    foreach (Type t in types)
                    {
                        foreach (object attrib in t.GetCustomAttributes(false))
                        {
                            if (attrib.GetType().Name.Contains("PlugInAttribute") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("ContentType") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("CatalogContentType") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("InitializableModule") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("UIDescriptorRegistration") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("EditorDescriptorRegistration") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("ServiceConfiguration") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("VisitorGroupCriterion") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("EPiServerDataStore") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                            if (attrib.GetType().Name.Contains("Gadget") && !t.IsAbstract)
                            {
                                isOKToRemove = false;
                                break;
                            }
                        }
                        if (!isOKToRemove)
                        {
                            break;
                        }
                    }

                    if (isOKToRemove)
                    {
                        safeRemoveAssemblies.Add(assembly.FullName.Split(',')[0]);
                    }
                }
                catch
                {
                    //Really don't like doing this, but the assembly couldn't be loaded so we can't be sure therefore it must be ignored
                }
            }

            return safeRemoveAssemblies;
        }

        private Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyPath = this.targetPath + @"\" + args.Name.Split(',')[0] + @".dll";
            return Assembly.LoadFile(assemblyPath);
        }
    }
}