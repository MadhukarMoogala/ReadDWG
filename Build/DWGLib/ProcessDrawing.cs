using Autodesk.AutoCAD.DatabaseServices;
using System.Diagnostics;
using Autodesk.AutoCAD.Runtime;
using Transaction = Autodesk.AutoCAD.DatabaseServices.Transaction;


namespace DWGLib
{
    public interface IProcessDrawing
    {
        string DrawingPath { get; set; }

        /// <summary>
        /// Retrieves all layer and block names from the drawing.
        /// </summary>
        List<string> GetAllLayersAndBlocks();

    }

    public class ProcessDrawing : IProcessDrawing
    {
        public string DrawingPath { get; set; } = string.Empty;

        public List<string> GetAllLayersAndBlocks()
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(DrawingPath) || !File.Exists(DrawingPath))
                throw new FileNotFoundException("Invalid drawing path.", DrawingPath);
            if(DrawingPath is null)
                throw new ArgumentNullException(nameof(DrawingPath), "Drawing path cannot be null.");

            var dir = Path.GetDirectoryName(DrawingPath)
            ?? throw new InvalidOperationException("DrawingPath must contain a directory.");

            DWGHost host = new DWGHost
            {
                MainDwgLocation = dir
            };

            RuntimeSystem.Initialize(host, 1033);
            HostApplicationServices.Current = host;

            using Database db = new Database(false, true);
            try
            {
                if (Path.GetExtension(DrawingPath).Equals(".dxf", StringComparison.OrdinalIgnoreCase))
                    db.DxfIn(DrawingPath, null);
                else
                    db.ReadDwgFile(DrawingPath, FileOpenMode.OpenTryForReadShare, true, null);

                HostApplicationServices.WorkingDatabase = db;

                using Transaction tr = db.TransactionManager.StartTransaction();
                // Layers
                var layerTable = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
                foreach (ObjectId id in layerTable)
                {
                    var layer = (LayerTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    result.Add("Layer: " + layer.Name);
                }

                // Blocks
                var blockTable = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId id in blockTable)
                {
                    var block = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                    result.Add("Block: " + block.Name);
                }

                tr.Commit();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("Failed to read drawing: " + ex);
                throw;
            }

            return result;
        }
    }
}
