using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketC_.Class;
using WebSocketSharp.Server;

namespace WebSocketImpresion_NET4._8
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string port = "10000";
            string route = "print";

            var wssv = new WebSocketServer($"ws://0.0.0.0:{port}");

            wssv.AddWebSocketService<ClsWebSocket>($"/{route}", () =>
                new ClsWebSocket
                {
                    _port = port,
                    _route = route,
                }
            );

            wssv.Start();

            Console.WriteLine($"Servidor WebSocket Inicializado en ws://0.0.0.0:{port}/{route}");
            Console.WriteLine("Presiona Enter para detener...");

            Console.ReadLine();
            Console.WriteLine("Servidor detenido");

            wssv.Stop();

        }
    }
}
