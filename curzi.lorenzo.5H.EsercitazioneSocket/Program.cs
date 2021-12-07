﻿using curzi.lorenzo._5H.EsercitazioneSocket.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace curzi.lorenzo._5H.EsercitazioneSocket
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Sono un Server");

            const int PORT = 9000; //specifico la porta che userà il server

            const int BACKLOG = 128; //numeri di host in coda a cui il server può rispondere

            const string FILEPATH = @"..\..\..\..\fileServer.txt";

            const string JSONPATH = @"..\..\..\..\testJson.json";

            var endPoint = new IPEndPoint(IPAddress.Loopback, PORT); //creo l'enpoint specificando la porta del server

            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp); //creo la socket che mi permetterà la comunicazione

            socket.Bind(endPoint); //collego la socket all'end point

            socket.Listen(BACKLOG); //metto in ascolto la socket per un massimo di 128 client

            do
            {
                Console.WriteLine("Sono in ascolto");
				
                Socket clientSocket = socket.Accept(); //attendo la connessione di un client

                var endPointClient = (IPEndPoint)clientSocket.RemoteEndPoint; //Client remoto

                //una volta connesso posso stabilire una comunicazione
                Console.WriteLine("Client connesso");

                Console.WriteLine($"L'indirizzo IP del client è: {endPointClient.Address}");
                Console.WriteLine($"La porta del client è: {endPointClient.Port}");

                Console.WriteLine("Ricezione messaggi dal client:");
                do
                {
                    var buffer = new byte[1024];
                    int result = 0;
                    try
                    {
                        result = clientSocket.Receive(buffer); 
                    }
                    catch
                    {
                        Console.WriteLine("Connessione interrotta dal Client.");
                        socket.Close();
                        clientSocket.Close();
                        break;
                    }

                    //In caso la lunghezza del messaggio recevuta dal client sia 0 termino la connessione in quanto segnala la disconnessione di esso
                    if (result == 0)
                    {
                        clientSocket.Close();
                        socket.Close();
                        break;
                    }

                    string msgClient = System.Text.Encoding.UTF8.GetString(buffer[0..result]);

                    //Se il client invia il messaggio "c1|stop|" il server invia a sua volta "s1|stop|ack|" e termino il programma
                    if (msgClient == "c1|stop|")
                    {
                        byte[] bufferSend = System.Text.Encoding.UTF8.GetBytes("s1|stop|ack|");
                        clientSocket.Send(bufferSend);
                        clientSocket.Close();
                        socket.Close();
                        Console.WriteLine("Comunicazione terminata con s1|stop|ack|");
                        Environment.Exit(0);
                    }

                    //Se il client invia il messaggio "c1|file|" il server il file fileServer.txt
                    if (msgClient == "c1|file|")
                    {
                        clientSocket.SendFile(FILEPATH);
                        continue;
                    }

                    //Se il client invia il messaggio "c1|json|" il server il file crea il file testJson.json e lo invia al client
                    if (msgClient == "c1|json|")
                    {
                        CreateJson(JSONPATH);
                        clientSocket.SendFile(JSONPATH);

                        continue;
                    }

                    Console.WriteLine($"Il client ha scritto: {msgClient}");
                    clientSocket.Send(buffer);
                } while (true);

                Console.WriteLine("Si desidera stabilire una nuova connessione col client? Premere ESC per terminare la connessione");

                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    clientSocket.Close(); //chiudo la comunicazione col client
                    socket.Close();
                    break;
                }
            } while (true);
        }

        static void CreateJson(string JSONPATH)
        {
            List<Employee> employee = new List<Employee>();

            employee.Add(
                new Employee("AAAA", "AAAALast", "407-111-1111")
                );

            employee.Add(
                new Employee("BBBB", "BBBBLast", "407-222-2222")
                );

            Customer customer = new Customer("10", "company one", employee); //Creo l'istanza cliente da serializzare nel file Json

            string jsonText = JsonConvert.SerializeObject(customer, Formatting.Indented);

            if (!File.Exists(JSONPATH))
                File.WriteAllText(JSONPATH, jsonText);
            else
            {
                File.Delete(JSONPATH);
                File.WriteAllText(JSONPATH, jsonText);
            }

        }
    }
}
