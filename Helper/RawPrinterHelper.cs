using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Management;
using System.Drawing.Printing;
using System.Text;
using System.Drawing;
using System.IO;

namespace WebSocketC_.Helper
{
    public class RawPrinterHelper
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA",
            SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool OpenPrinter(string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter",
            SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA",
            SetLastError = true, CharSet = CharSet.Ansi,
            ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter",
            SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter",
            SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter",
            SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter",
            SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

        public static bool PrintPdfFromBytes(string printerName, byte[] pdfBytes)
        {
            try
            {
                using (var stream = new MemoryStream(pdfBytes))
                using (var pdfDocument = PdfiumViewer.PdfDocument.Load(stream)) // Load en lugar de constructor
                {
                    using (var printDoc = pdfDocument.CreatePrintDocument())
                    {
                        printDoc.PrinterSettings.PrinterName = printerName;

                        if (!printDoc.PrinterSettings.IsValid)
                            return false;

                        printDoc.Print();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al imprimir PDF: " + ex.Message);
                return false;
            }
        }

        public static bool SendBytesToPrinter(string printerName, byte[] bytes)
        {
            try
            {
                IntPtr pBytes = Marshal.AllocCoTaskMem(bytes.Length);
                Marshal.Copy(bytes, 0, pBytes, bytes.Length);

                bool success = SendBytesToPrinter(printerName, pBytes, bytes.Length);

                Marshal.FreeCoTaskMem(pBytes);

                return success;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al enviar bytes RAW: " + ex.Message);
                return false;
            }
        }

        private static bool SendBytesToPrinter(string printerName, IntPtr pBytes, int dwCount)
        {
            IntPtr hPrinter = IntPtr.Zero;
            int dwWritten = 0;
            DOCINFOA di = new DOCINFOA();
            bool success = false;

            di.pDocName = "Documento Térmico Directo";
            di.pDataType = "RAW";

            if (OpenPrinter(printerName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                if (StartDocPrinter(hPrinter, 1, di))
                {
                    if (StartPagePrinter(hPrinter))
                    {
                        // Escribimos los bytes puros en la impresora
                        success = WritePrinter(hPrinter, pBytes, dwCount, out dwWritten);
                        EndPagePrinter(hPrinter);
                    }
                    EndDocPrinter(hPrinter);
                }
                ClosePrinter(hPrinter);
            }
            return success;
        }

        public static bool PrinterOnline(string printerName)
        {
            string query = $"SELECT * FROM Win32_Printer WHERE Name = '{printerName}'";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject printer in searcher.Get())
                {
                    bool workOffline = (bool)printer["WorkOffline"];
                    return !workOffline;
                }
            }
            return false;
        }

        public static Tuple<bool, string> StatePrinter(string printerName)
        {
            try
            {
                string query = $"SELECT * FROM Win32_Printer WHERE Name = '{printerName}'";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                {
                    foreach (ManagementObject printer in searcher.Get())
                    {
                        bool offline = printer["WorkOffline"] != null && (bool)printer["WorkOffline"];
                        int printerStatus = printer["PrinterStatus"] != null ? Convert.ToInt32(printer["PrinterStatus"]) : 0;
                        int extStatus = printer["ExtendedPrinterStatus"] != null ? Convert.ToInt32(printer["ExtendedPrinterStatus"]) : 0;
                        string statusText = printer["Status"]?.ToString() ?? "Desconocido";

                        if (offline)
                            return Tuple.Create(false, $"La impresora '{printerName}' está desconectada.");

                        // Falta de papel -> PrinterStatus = 11 o ExtendedPrinterStatus = 11
                        if (printerStatus == 11 || extStatus == 11)
                            return Tuple.Create(false, $"La impresora '{printerName}' no tiene papel.");

                        // Error detectado
                        if (printerStatus == 4 || printerStatus == 7 || extStatus == 9) // Ej: Printing=4, Offline=7, PaperJam=9
                            return Tuple.Create(false, $"La impresora '{printerName}' tiene un error o no está lista. Estado: {printerStatus}/{extStatus}");

                        // Si no está en Idle
                        if (printerStatus != 3)
                            return Tuple.Create(false, $"La impresora '{printerName}' no está lista. Estado: {printerStatus} ({statusText})");
                    }
                }

                return Tuple.Create(true, $"La impresora '{printerName}' está lista.");
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, $"No se pudo obtener el estado de la impresora: {ex.Message}");
            }
        }
    }
}
