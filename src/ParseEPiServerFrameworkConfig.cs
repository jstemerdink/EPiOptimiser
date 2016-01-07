using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

using EPiServer.Framework.Configuration;

namespace EPiOptimiser
{
    public class ParseEPiServerFrameworkConfig
    {
        public IList<string> GetExcludedAssemblyList(string configFilePath)
        {
            List<string> excludedList = new List<string>();

            //Use the .net config manager as it will automatically pick up config sections that are added with the configSource attribute
            ConfigurationFileMap fileMap = new ConfigurationFileMap(configFilePath);

            Configuration configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
            EPiServerFrameworkSection section = ((EPiServerFrameworkSection)configuration.Sections.Get("episerver.framework"));

            if (section == null)
            {
                return excludedList;
            }

            AssemblyElementCollection scanAssemblySection = section.ScanAssembly;

            //Bit of reflection needed to access the private property "Items"
            ArrayList assemblies =
                (ArrayList)
                ((typeof(ConfigurationElementCollection).GetProperty(
                    "Items",
                    BindingFlags.Instance | BindingFlags.NonPublic)).GetValue(scanAssemblySection, null));

            foreach (object assem in assemblies)
            {
                string assemName =
                    ((assem.GetType()).GetField("_key", BindingFlags.Instance | BindingFlags.NonPublic)).GetValue(
                        assem).ToString();
                string entryType =
                    ((assem.GetType()).GetField("_entryType", BindingFlags.Instance | BindingFlags.NonPublic))
                        .GetValue(assem).ToString();

                if (entryType == "Removed")
                {
                    excludedList.Add(assemName);
                }
            }

            return excludedList;
        }

        private class Entry
        {
            // Fields
            internal EntryType EntryType;

            internal readonly object Key;

            internal ConfigurationElement Value;

            // Methods
            internal Entry(EntryType type, object key, ConfigurationElement value)
            {
                this.EntryType = type;
                this.Key = key;
                this.Value = value;
            }

            internal object GetKey(ConfigurationElementCollection ThisCollection)
            {
                return this.Key;
                //if (this._value != null)
                //{
                //    return ThisCollection.GetElementKeyInternal(this._value);
                //}
                //return this._key;
            }
        }

        private enum EntryType
        {
            Inherited,

            Replaced,

            Removed,

            Added
        }
    }
}