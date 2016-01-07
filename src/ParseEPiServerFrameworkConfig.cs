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
            var excludedList = new List<string>();

            //Use the .net config manager as it will automatically pick up config sections that are added with the configSource attribute
            var fileMap = new ConfigurationFileMap(configFilePath);

            var configuration = ConfigurationManager.OpenMappedMachineConfiguration(fileMap);
            var section = ((EPiServerFrameworkSection)configuration.Sections.Get("episerver.framework"));

            if (section != null)
            {
                var scanAssemblySection = section.ScanAssembly;

                //Bit of refelection needed to access the private property "Items"
                var assemblies =
                    (ArrayList)
                    ((typeof(ConfigurationElementCollection).GetProperty(
                        "Items",
                        BindingFlags.Instance | BindingFlags.NonPublic)).GetValue(scanAssemblySection, null));

                foreach (var assem in assemblies)
                {
                    var assemName =
                        ((assem.GetType()).GetField("_key", BindingFlags.Instance | BindingFlags.NonPublic)).GetValue(
                            assem).ToString();
                    var entryType =
                        ((assem.GetType()).GetField("_entryType", BindingFlags.Instance | BindingFlags.NonPublic))
                            .GetValue(assem).ToString();

                    if (entryType == "Removed")
                    {
                        excludedList.Add(assemName);
                    }
                }
            }

            return excludedList;
        }

        private class Entry
        {
            // Fields
            internal EntryType _entryType;

            internal readonly object _key;

            internal ConfigurationElement _value;

            // Methods
            internal Entry(EntryType type, object key, ConfigurationElement value)
            {
                this._entryType = type;
                this._key = key;
                this._value = value;
            }

            internal object GetKey(ConfigurationElementCollection ThisCollection)
            {
                return this._key;
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