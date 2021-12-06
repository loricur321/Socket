using System;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sono un Client");
            Console.WriteLine("Press Enter to Connect");
            Console.ReadLine();

            //localEP - Client
            var localPoint = new IPEndPoint(IPAddress.Loopback, 8000);
            //remoteEP - Server
            var endpoint = new IPEndPoint(IPAddress.Loopback, 9000);

            //Bind e connect
            var socket = new Socket(localPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localPoint);
            //Effettuo la connessione al server
            socket.Connect(endpoint);

            do
            {
                do
                {
                    //invio dati
                    Console.WriteLine("Inserire il messaggio da inviare al server: ");
                    var msg = Console.ReadLine().ToLower();
                    //formato del messaggio
                    var buffer = System.Text.Encoding.UTF8.GetBytes(msg);
                    socket.Send(buffer);

                    //Ricevo la risposta dal sevrer
                    var bufferReceive = new byte[1024];
                    int lenght = socket.Receive(bufferReceive);
                    var msgServer = System.Text.Encoding.UTF8.GetString(bufferReceive[0..lenght]).ToLower();

                    //In caso il server invii il messaggio "s1|stop|ack|" chiudo il programma Client
                    if (msgServer == "s1|stop|ack|")
                    {
                        socket.Close();
                        Environment.Exit(0);
                    }

                    Console.WriteLine($"Messaggio di ritorno del server: {msgServer}");

                    Console.WriteLine("Si vuole inviare un altro messaggio al server? (si, no)");

                    if (Console.ReadLine() != "si")
                        break;
                } while (true);

                Console.WriteLine("Si vuole stabile una nuova connessione con il server? (si, no)");

                if (Console.ReadLine() != "si")
                {
                    socket.Close();//Chiudo la socket lato Client dal server
                    Environment.Exit(0);
                }
                    
            } while (true);
        }
    }
}
