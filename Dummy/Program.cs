using ReadDWG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClI
{
    internal class Program
    {
        static void Main()
        {
            IProcessDrawing drawing = new ProcessDrawing
            {
                DrawingPath = "blocks_and_tables_-_metric.dwg"
            };
            List<string> names = drawing.GetAllLayersAndBlocks();
        }
    }
}
