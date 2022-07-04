using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ReadDWG
{
    public interface IProcessDrawing
    {
        string DrawingPath { get; set; }
        List<string> GetAllLayersAndBlocks();

    }


    public class ProcessDrawing : IProcessDrawing
    {


        public string DrawingPath { get; set; }
        public List<string> GetAllLayersAndBlocks()
        {

            
            List<string> nReturn = new List<string>();

            try
            {
                string dwgName = DrawingPath;                         

                DWGHost host = new DWGHost();
                Autodesk.AutoCAD.Runtime.RuntimeSystem.Initialize(host, 1033);                
                using (Database db = new Database(false, true))
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        try
                        {
                            host.MainDwgLocation = Path.GetDirectoryName(dwgName);
                            if (Path.GetExtension(dwgName).Equals(".dxf", StringComparison.CurrentCultureIgnoreCase))
                            {
                                db.DxfIn(dwgName, null);

                            }
                            else
                            {
                                db.ReadDwgFile(dwgName, FileOpenMode.OpenTryForReadShare, true, null);
                            }
                        }
                        catch (System.Exception ex)
                        {
                            Debug.WriteLine("Exception: {0}", ex.ToString());
                            nReturn = null;
                        }

                        HostApplicationServices.WorkingDatabase = db;
                        dynamic layers = db.LayerTableId;
                        foreach (dynamic layer in layers)
                            nReturn.Add(layer.Name);
                        dynamic blocks = db.BlockTableId;
                        foreach (dynamic block in blocks)
                            nReturn.Add(block.Name);
                        tr.Commit();

                    }
                }
            }
            catch (System.Exception)
            {

                nReturn = null;
            }

            return nReturn;
        }
    }
}
