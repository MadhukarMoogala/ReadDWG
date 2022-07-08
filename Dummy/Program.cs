using Autodesk.AutoCAD.DatabaseServices;
using ReadDWG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dummy
{
    internal class Program
    {
        /// <summary>
        ///  This a fake host to make RealDWG things happen,
        ///  The actual Drawing Host residing in ReadDWG library will replace anyway.
        /// </summary>
        class FakeHost : HostApplicationServices
        {
            public override string FindFile(string fileName, Database database, FindFileHint hint)
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// AcDbMgd path resolution logic.
        /// Make sure reference AcDbMgd `CopyLocal` is set to False, we don't want our webapp struggling to find it.
        /// </summary>
        static void PreLoad()
        {

            // this needs to be outside so I can find acdbxx.dll separately from AcDbMgd
            var strRealDwgPath = Environment.GetEnvironmentVariable("REALDWGSDK");

            //make sure we find our unmanaged dependencies (add folder to the %PATH%)
            var path = Environment.GetEnvironmentVariable("PATH");
            Environment.SetEnvironmentVariable("PATH", $"{strRealDwgPath};{path}");

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                // we get here if assembly resolution fails, let's see if it was acdbmgd
                if (args.Name.StartsWith("Acdbmgd,"))
                {
                    //load acdbmgd from the requested folder
                    return Assembly.LoadFrom(Path.Combine(strRealDwgPath, "acdbmgd.dll"));
                }
                return null;
            };
        }
        /// <summary>
        /// Initialize a fake AcDbHostApplication service.
        /// </summary>

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadAcDbMgd()
        {
            //Ref: https://docs.microsoft.com/en-us/windows/win32/msi/localizing-the-error-and-actiontext-tables

            int enUS = 0X409;
            Autodesk.AutoCAD.Runtime.RuntimeSystem.Initialize(new FakeHost(), enUS);
        }
        static void SetUpRealDwgEnviroment()
        {
            PreLoad();
            LoadAcDbMgd();
        }
        static void Main()
        {
            SetUpRealDwgEnviroment();
            using (var client = new WebClient())
            {
                client.DownloadFile("https://download.autodesk.com/us/samplefiles/acad/blocks_and_tables_-_metric.dwg", "sample.dwg");
            }
            IProcessDrawing drawing = new ProcessDrawing
            {
              DrawingPath = @"sample.dwg"
            };
            List<string> names = drawing.GetAllLayersAndBlocks();
            names.ForEach((name) => Console.WriteLine($"{name}\n"));
        }
    }
}
