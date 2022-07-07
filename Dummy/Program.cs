using Autodesk.AutoCAD.DatabaseServices;
using ReadDWG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Dummy
{
    internal class Program
    {
        class FakeHost : HostApplicationServices
        {
            public override string FindFile(string fileName, Database database, FindFileHint hint)
            {
                throw new NotImplementedException();
            }
        }
        static void PreLoad(string strRealDwgPath)
        {
            //
            // this needs to be outside so I can find acdb23.dll separately from AcDbMgd

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadAcDbMgd()
        {
            
            Autodesk.AutoCAD.Runtime.RuntimeSystem.Initialize(new FakeHost(), 0x409);
        }
        static void LoadRealDwgFrom(string folder)
        {
            PreLoad(folder);          
            LoadAcDbMgd();            
        }
        static void Main()
        {
            LoadRealDwgFrom(@"D:\Rd23\RealDWG 2023\");
            IProcessDrawing drawing = new ProcessDrawing
            {
                //ref:https://download.autodesk.com/us/samplefiles/acad/blocks_and_tables_-_metric.dwg
                DrawingPath = @"sample.dwg"
            };
            List<string> names = drawing.GetAllLayersAndBlocks();
            names.ForEach((name) => Console.WriteLine($"{name}\n"));
        }
    }
}
