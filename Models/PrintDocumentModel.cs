using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketC_.Models
{
    public class PrintDocumentModel
    {
        public string NamePrinter { get; set; }
        public List<byte[]> Documents { get; set; }

        public PrintDocumentModel()
        {
            NamePrinter = string.Empty;
            Documents = new List<byte[]>();
        }
    }
}
