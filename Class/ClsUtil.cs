using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Text;
using WebSocketC_.Helper;

namespace WebSocketC_.Class
{
    public class ClsUtil
    {
        public List<string> GetPrinters()
        {
            var list = new List<string>();
            foreach (string printer in PrinterSettings.InstalledPrinters)
            {
                list.Add(printer);
            }

            return list;
        }

        public (bool, string) PrintDocument(string printerName, string typePrinter, List<byte[]> files)
        {
            IntPtr hPrinter = IntPtr.Zero;

            try
            {
                if (!RawPrinterHelper.PrinterOnline(printerName))
                {
                    return (false, $"La impresora '{printerName}' está instalada pero sin conexión.");
                }

                var estado = RawPrinterHelper.StatePrinter(printerName);

                if (!estado.Item1)
                {
                    return (false, estado.Item2);
                }

                foreach (var fileBytes in files)
                {
                    bool success = false;

                    if (string.IsNullOrEmpty(typePrinter) || typePrinter.ToUpper() == "NORMAL")
                    {
                        // Usar Pdfium para PDFs o impresoras normales
                        success = RawPrinterHelper.PrintPdfFromBytes(printerName, fileBytes);
                    }
                    else
                    {
                        // Usar vía directa RAW para ESCPOS, ZPL, TSPL
                        success = RawPrinterHelper.SendBytesToPrinter(printerName, fileBytes);
                    }

                    if (!success)
                    {
                        return (false, $"Error al imprimir un documento en la impresora {printerName}");
                    }
                }

                return (true, "Documento(s) impreso(s) correctamente");
            }
            catch (Exception ex)
            {
                return (false, $"Ocurrió un error al imprimir: {ex.Message}");
            }
        }
    }
}
