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

        public (bool, string) PrintDocument(string printerName, List<byte[]> files)
        {
            IntPtr hPrinter = IntPtr.Zero; // En vez de nint

            try
            {
                // Verificar si la impresora se puede abrir
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
                    //string test = "Hola impresora\r\n\r\n\f"; // \f = Form Feed (forzar impresión)
                    //byte[] bytes = Encoding.ASCII.GetBytes(test);
                    //bool success = RawPrinterHelper.SendBytesToPrinter("EPSON4381B7 (L4150 Series)", bytes);

                    //bool success = RawPrinterHelper.SendBytesToPrinter(printerName, fileBytes);
                    bool success = RawPrinterHelper.PrintPdfFromBytes(printerName, fileBytes);

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
            finally
            {
                if (hPrinter != IntPtr.Zero)
                    RawPrinterHelper.ClosePrinter(hPrinter);
            }
        }
    }
}
