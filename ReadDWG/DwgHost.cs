using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReadDWG
{
    /// <summary>
    /// Host class to create conversation with RealDwg 
    /// </summary>
    class DWGHost : HostApplicationServices
    {
        public DWGHost()
        {
            RuntimeSystem.Initialize(this, 1033);
        }


        // 2022 change public override string RegistryProductRootKey
        public override string UserRegistryProductRootKey
        {
            get
            {
                return @"Software\ADNWorks\ReadDWG\1.0";
            }
        }

        private string dwgLocation;
        public string MainDwgLocation
        {
            get { return dwgLocation; }
            set { dwgLocation = value; }

        }

        /// <summary>
        /// Search for the file.. Code pulled from Autodesk sample code.  This search 
        /// is using the Autocad rules, maybe should be expanded 
        /// to use the Plot Station rules
        /// </summary>
        /// <param name="fileName">file we are searching for</param>
        /// <returns>full path of found file </returns>
        private string SearchPath(string fileName)
        {
            string testName;

            // check if the file is already with full path
            if (System.IO.File.Exists(fileName))
                return fileName;

            if (Path.IsPathRooted(fileName))
            {
                testName = Path.GetFileName(fileName);
            }
            else
            {
                // its a relative path, so use the full path
                testName = fileName;
            }

            string validatedPath = Path.GetFullPath(MainDwgLocation + "\\" + testName);
            if (File.Exists(validatedPath))
            {
                return validatedPath;
            }

            // check application folder
            string applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (File.Exists(applicationPath + "\\" + testName))
                return applicationPath + "\\" + testName;

            // we can also check the Acad path
            string acad = Environment.GetEnvironmentVariable("ACAD");
            if (File.Exists(acad + "\\" + testName))
                return acad + "\\" + testName;

            // search folders in %PATH%
            string[] paths = Environment.GetEnvironmentVariable("Path").Split(new char[] { ';' });
            foreach (string path in paths)
            {
                // some folders end with \\, some don't
                validatedPath = Path.GetFullPath(path + "\\" + testName);
                if (File.Exists(validatedPath))
                    return validatedPath;
            }

            // check the Fonts folders
            string systemFonts = Environment.GetEnvironmentVariable("SystemRoot") + "\\Fonts\\";
            if (File.Exists(systemFonts + testName))
                return systemFonts + testName;

            // if we installed fonts in our own folder  
            string rdwgFonts = applicationPath + "\\Fonts\\";
            if (File.Exists(rdwgFonts + testName))
                return rdwgFonts + testName;

            if (Directory.Exists(rdwgFonts))
            {
                if (File.Exists(rdwgFonts + testName))
                    return rdwgFonts + testName;
            }

            // we did not find the file :-(
            Console.WriteLine("The file '" + testName + "' could not be found and therefore could cause RealDWG problems in its ability to correctly read this drawing");

            return "";
        }

        /// <summary>
        /// Autodesk says you must override this..  Possibly, we need to incorporate some of the 
        /// Plot Station rules for finding files.
        /// </summary>
        /// <param name="fileName">File we are looking for</param>
        /// <param name="database">Autocad database</param>
        /// <param name="hint">hint for file type. Used when file has no extension</param>
        /// <returns></returns>
        public override string FindFile(
                 string fileName,
                 Database database,
                 FindFileHint hint)
        {

            // add extension if not already part of the file name
            if (!fileName.Contains("."))
            {
                string extension;
                switch (hint)
                {
                    case FindFileHint.CompiledShapeFile:
                        extension = ".shx";
                        break;
                    case FindFileHint.TrueTypeFontFile:
                        extension = ".ttf";
                        break;
                    case FindFileHint.PatternFile:
                        extension = ".pat";
                        break;
                    case FindFileHint.ArxApplication:
                        extension = ".dbx";
                        break;
                    case FindFileHint.FontMapFile:
                        extension = ".fmp";
                        break;
                    case FindFileHint.XRefDrawing:
                        extension = ".dwg";
                        break;
                    // Fall through. These could have
                    // various extensions
                    case FindFileHint.FontFile:
                    case FindFileHint.EmbeddedImageFile:
                    default:
                        extension = "";
                        break;
                }

                fileName += extension;
            }

            // add it to the unresolved items if it could not be resolved

            string ret = SearchPath(fileName);

            return (ret);
        }
        /// <summary>
        /// Autodesk says you must add this
        /// </summary>
        public override string AlternateFontName
        {
            get
            {
                return "txt.shx";
            }
        }
    }
}
