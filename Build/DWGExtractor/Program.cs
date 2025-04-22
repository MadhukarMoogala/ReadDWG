using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using DWGLib;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DWGExtractor
{
    internal class Program
    {
        /// <summary>
        /// A minimal host to satisfy RealDWG requirements.
        /// </summary>
        class FakeHost : HostApplicationServices
        {
            public override string FindFile(string fileName, Database database, FindFileHint hint)
            {
                return string.Empty; // No-op for now
            }
        }

        static void PreLoad()
        {
            var realDwgPath = Environment.GetEnvironmentVariable("REALDWGSDK");
            if (string.IsNullOrWhiteSpace(realDwgPath))
            {
                Console.Error.WriteLine("REALDWGSDK environment variable not set.");
                Environment.Exit(1);
            }

            var path = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", $"{realDwgPath};{path}");

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                if (args.Name.StartsWith("Acdbmgd,", StringComparison.OrdinalIgnoreCase))
                {
                    return Assembly.LoadFrom(Path.Combine(realDwgPath, "acdbmgd.dll"));
                }
                return null;
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadAcDbMgd()
        {
            int enUS = 0x409;
            RuntimeSystem.Initialize(new FakeHost(), enUS);
        }

        static void SetUpRealDwgEnvironment()
        {
            PreLoad();
            LoadAcDbMgd();
        }

        static void Main(string[] args)
        {
            if (args.Length != 4 || args[0] != "-i" || args[2] != "-o")
            {
                Console.WriteLine("Usage: DWGExtactor.exe -i <input.dwg> -o <output.txt>");
                return;
            }

            string inputDwg = args[1];
            string outputTxt = args[3];

            if (!File.Exists(inputDwg))
            {
                Console.WriteLine($"Input DWG file not found: {inputDwg}");
                return;
            }

            SetUpRealDwgEnvironment();

            IProcessDrawing drawing = new ProcessDrawing
            {
                DrawingPath = inputDwg
            };

            List<string> names = drawing.GetAllLayersAndBlocks();
            if (names == null || names.Count == 0)
            {
                Console.WriteLine("No layers or blocks found, or failed to read DWG.");
                return;
            }

            File.WriteAllLines(outputTxt, names);
            Console.WriteLine($"Output written to: {outputTxt}");
        }
    }
}
