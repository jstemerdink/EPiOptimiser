namespace EPiOptimiser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;
    using System.Reflection;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.Configuration;
    using EPiServer.Framework.Configuration;
    using Microsoft.Build.Utilities;
    using System.Windows.Forms;

    class Program
    {
        [STAThreadAttribute]
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No project web.config path specified.");
                Console.ReadKey();
                return;
            }

            string sourcePath = args[0];

            if (!File.Exists(sourcePath))
            {
                Console.WriteLine("Could not open file \"" + sourcePath + "\".");
                try { Console.ReadKey(); }
                catch { }
                return;
            }

            var assemblyParser = new ParseAssembliesForPlugIns();

            StringBuilder sb = new StringBuilder();
            foreach (string assemblyName in assemblyParser.GetSafeToIgnoreAssemblies(sourcePath))
            {
                string remove = "<remove assembly=\"" + assemblyName + "\" />";
                Console.WriteLine(remove);
                sb.Append(remove + System.Environment.NewLine);
            }

            Clipboard.SetText(sb.ToString());

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("Remove assembly list copied to clipboard");
            Console.WriteLine("========================================");

            try { Console.ReadKey(); }
            catch { }
        }
    }
}