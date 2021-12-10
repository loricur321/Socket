using curzi.lorenzo._5H.EsercitazioneSocket.Models;
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
        const string JSONPATH = @"..\..\..\..\testJson.json"; //variabile globale
        static void Main(string[] args)
        {
            Console.WriteLine("Sono un Server");

            const int PORT = 9000; //specifico la porta che userà il server

            const int BACKLOG = 128; //numeri di host in coda a cui il server può rispondere

            const string FILEPATH = @"..\..\..\..\fileServer.txt";

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
                        CreateJson();
                        clientSocket.SendFile(JSONPATH);

                        continue;
                    }

                    //Nel caso l'aggiunta di un employee riscontri un errore il server si comporterà da echo server per evitare interruzioni
                    try
                    {
                        //con c1|employee| do al client la possibilità di aggiungre un employee al file json
                        //c1|customer|nome|cognome|phone
                        //0     1       2     3      4

                        var splittedMSG = msgClient.Split("|");
                        if (splittedMSG[1] == "employee")
                        {
                            AddEmployee(splittedMSG[2], splittedMSG[3], splittedMSG[4]);
                            clientSocket.SendFile(JSONPATH);
                            continue;
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Il client ha scritto: {msgClient}");
                        clientSocket.Send(buffer);
                    }
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

        /// <summary>
        /// Creazione del file Json contenente customer e employees
        /// </summary>
        /// <param name="employee">employee aggiunto dal client</param>
        static void CreateJson(Employee employee = null)
        {
            List<Employee> employees = new List<Employee>();

            Customer c = ReadJson(); //Ricavo le informazioni precendentemente salvate nel file deserializzando il file stesso

            //aggiungo gli employee presenti nel file più quello aggiunto dal client
            employees.AddRange(c.Employees);
            if(employee != null) employees.Add(employee);

            Customer customer = new Customer(c.Customer_id, c.Customer_name, employees); //Creo la nuova istanza customer da serializzare nel file

            //Serializzo customer e salvo il file json
            string jsonText = JsonConvert.SerializeObject(customer, Formatting.Indented);

            if (!File.Exists(JSONPATH))
                File.WriteAllText(JSONPATH, jsonText);
            else
            {
                File.Delete(JSONPATH);
                File.WriteAllText(JSONPATH, jsonText);
            }

        }

        /// <summary>
        /// Metodo per deserializzare il contenuto del file Json
        /// </summary>
        /// <returns>Istanza Customer letta dal file</returns>
        static Customer ReadJson() => JsonConvert.DeserializeObject<Customer>(File.ReadAllText(JSONPATH));

        /// <summary>
        /// Aggiunta di un employee al file Json
        /// </summary>
        /// <param name="firstName">Nome </param>
        /// <param name="lastName">Cognome</param>
        /// <param name="phone">Telefono</param>
        static void AddEmployee(string firstName, string lastName, string phone) => CreateJson(new Employee(firstName, lastName, phone));
    }
}
