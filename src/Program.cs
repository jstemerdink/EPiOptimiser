using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EPiOptimiser
{
    internal class Program
    {
        [STAThread]
        private static void Main(string[] args)
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
                try
                {
                    Console.ReadKey();
                }
                catch
                {
                }
                return;
            }

            ParseAssembliesForPlugIns assemblyParser = new ParseAssembliesForPlugIns();

            StringBuilder sb = new StringBuilder();

            foreach (string assemblyName in assemblyParser.GetSafeToIgnoreAssemblies(sourcePath))
            {
                string remove = "<remove assembly=\"" + assemblyName + "\" />";
                Console.WriteLine(remove);
                sb.Append(remove + Environment.NewLine);
            }

            Clipboard.SetText(sb.ToString());

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("Remove assembly list copied to clipboard");
            Console.WriteLine("========================================");

            try
            {
                Console.ReadKey();
            }
            catch
            {
            }
        }
    }
}
