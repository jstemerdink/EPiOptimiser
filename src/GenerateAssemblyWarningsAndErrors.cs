using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using EPiOptimiser;
using System.IO;
using System;
using System.Reflection;

public class GenerateAssemblyWarningsAndErrors : Task
{
    // http://www.developerfusion.com/article/84411/customising-your-build-process-with-msbuild/

    private ITaskItem _configfile;

    // Name and path of configuration file to parse
    [Required]
    public ITaskItem ConfigFile
    {
        get { return _configfile; }
        set { _configfile = value; }
    }

    public override bool Execute()
    {
        ParseAssembliesForPlugIns assemblyParser = new ParseAssembliesForPlugIns();
        var safeToIgnoreAssemblies = assemblyParser.GetSafeToIgnoreAssemblies(ConfigFile.GetMetadata("FullPath"));

        ParseEPiServerFrameworkConfig configParser = new ParseEPiServerFrameworkConfig();
        var excludedAssemblies = configParser.GetExcludedAssemblyList(ConfigFile.GetMetadata("FullPath"));

        //Generate warnings where an assembly isn't excluded but could be excluded
        foreach (string assembly in safeToIgnoreAssemblies)
        {
            if (!excludedAssemblies.Contains(assembly))
            {
                Log.LogWarning("Assembly \"" + assembly + "\" can be safely removed from being scanned by the EPiServer intialisation system. Consider adding a <remove assembly=\"" + assembly + "\" /> to the <episerver.framework><scanAssembly> section in order to improve start up times.");
            }
        }

        //Generate errors where an assembly is excluded but contains init modules or plug ins
        foreach (string assembly in excludedAssemblies)
        {
            if (!safeToIgnoreAssemblies.Contains(assembly))
            {
                Log.LogError("Assembly \"" + assembly + "\" is excluded from scanning in the <episerver.framework><scanAssembly> section but contains an EPiServer plug in or initialisation module. Remove this exclusion to ensure all plug ins are loaded correctly.");
            }
        }

        return true;
    }
}