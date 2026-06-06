using System;
using System.Collections.Generic;
using WebSocketC_.Models;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json; 

namespace WebSocketC_.Class
{
    public class ClsWebSocket : WebSocketBehavior
    {
        public string _port = null;
        public string _route = null;

        private ClsUtil util = new ClsUtil();

        protected override void OnMessage(MessageEventArgs e)
        {
            object value = null;

            try
            {
                var data = JsonConvert.DeserializeObject<ResultModel<object>>(e.Data);

                if (data == null)
                {
                    value = new ResultModel<object>
                    {
                        Action = "Serializer",
                        StatusCode = 501,
                        Message = "No se pudo deserializar el objeto",
                        ResponseModel = null
                    };
                }

                Console.WriteLine($"Mensaje recibido: {e.Data}");

                switch (data?.Action)
                {
                    case "GetPrinters":
                        var printers = util.GetPrinters();

                        value = new ResultModel<object>
                        {
                            Action = "GetPrinters",
                            StatusCode = 200,
                            Message = "Lista de impresoras obtenida correctamente",
                            ResponseModel = printers
                        };
                        break;

                    case "PrintDocument":
                        var printDataJson = data.ResponseModel != null ? data.ResponseModel.ToString() : "{}";
                        var printData = JsonConvert.DeserializeObject<PrintDocumentModel>(printDataJson);

                        var print = util.PrintDocument(printData.NamePrinter, printData.Documents);

                        value = new ResultModel<object>
                        {
                            Action = "PrintDocument",
                            StatusCode = print.Item1 ? 200 : 501,
                            Message = print.Item1 ? "Documentos Impresos correctamente" : print.Item2,
                            ResponseModel = null
                        };
                        break;

                    default:
                        value = new ResultModel<object>
                        {
                            Action = "Default",
                            StatusCode = 400,
                            Message = "Opcion No Disponible",
                            ResponseModel = null
                        };
                        break;
                }
            }
            catch (Exception ex)
            {
                value = new ResultModel<object>
                {
                    Action = "Catch",
                    StatusCode = 501,
                    Message = $"Excepcion: {ex.Message}",
                    ResponseModel = null
                };
            }

            Send(JsonConvert.SerializeObject(value));
        }

        protected override void OnOpen()
        {
            Console.WriteLine($"Cliente Conectado: {ID}");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine($"Cliente Desconectado: {ID}");
        }
    }
}
