using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System.Reflection;


namespace DWGLib
{
    /// <summary>
    /// Host class for RealDWG integration.
    /// </summary>
    public class DWGHost : HostApplicationServices
    {
        public DWGHost()
        {
            RuntimeSystem.Initialize(this, 1033); // LCID for English - United States
        }

        public override string UserRegistryProductRootKey =>
            @"Software\ADNWorks\ReadDWG\1.0";

        public string? MainDwgLocation { get; set; }

        public override string AlternateFontName => "txt.shx";

        public override string FindFile(string fileName, Database database, FindFileHint hint)
        {
            if (!Path.HasExtension(fileName))
            {
                fileName += hint switch
                {
                    FindFileHint.CompiledShapeFile => ".shx",
                    FindFileHint.TrueTypeFontFile => ".ttf",
                    FindFileHint.PatternFile => ".pat",
                    FindFileHint.ArxApplication => ".dbx",
                    FindFileHint.FontMapFile => ".fmp",
                    FindFileHint.XRefDrawing => ".dwg",
                    _ => ""
                };
            }

            return SearchPath(fileName);
        }

        private string SearchPath(string fileName)
        {
            if (File.Exists(fileName))
                return fileName;

            string? assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? 
                throw new InvalidOperationException("Assembly location not found.");
            if (!Path.Exists(assemblyLocation))
                throw new InvalidOperationException($"Assembly location '{assemblyLocation}' does not exist.");


            string testName = Path.GetFileName(fileName);
            string[] searchPaths =
            [
                Path.Combine(MainDwgLocation ?? "", testName),
                Path.Combine(assemblyLocation, testName),
                Path.Combine(Environment.GetEnvironmentVariable("ACAD") ?? "", testName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), testName),
                Path.Combine(assemblyLocation, "Fonts", testName)
            ];

            foreach (var path in searchPaths)
            {
                if (File.Exists(path))
                    return path;
            }

            foreach (var path in (Environment.GetEnvironmentVariable("Path") ?? "").Split(';'))
            {
                string combined = Path.Combine(path.Trim(), testName);
                if (File.Exists(combined))
                    return combined;
            }

            Console.WriteLine($"File '{testName}' not found. RealDWG may not resolve it correctly.");
            return "";
        }
    }
}
