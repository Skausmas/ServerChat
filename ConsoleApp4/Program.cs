using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using Fleck;
using Newtonsoft.Json;
using System.Data.SqlClient;


namespace ConsoleApp4
{
    internal class Program
    {
        static List<IWebSocketConnection> clients = new List<IWebSocketConnection>();
        static void Main(string[] args)
        {
            var server = new WebSocketServer("ws://127.0.0.1:8888");
            
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    Console.WriteLine("Соединение открыто");
                    
                    clients.Add(socket);


                };

                socket.OnClose = () =>
                {
                    Console.WriteLine("Соединение закрыто");
                };

                socket.OnMessage = message =>
                {
                    Console.WriteLine("Получено сообщение: " + message);


                    
                    UserData user1 = JsonConvert.DeserializeObject<UserData>(message);

                    if (user1.UserWant == "auth")
                    {

                        SqlConnection sqlConnection = new SqlConnection();
                        sqlConnection.ConnectionString = @"Data Source=PP;Initial Catalog=ChatProj;Integrated Security=True";

                        string sqlquery = $"select * from user_accounts where name = '{user1.Name}' and password ='{user1.Password}'";
                        SqlCommand command = new SqlCommand(sqlquery, sqlConnection);
                        sqlConnection.Open();

                        SqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            if (user1.Name == reader[1].ToString() && user1.Password == reader[2].ToString())
                            {


                                UserData newuser = new UserData();

                                newuser.Name = reader[1].ToString();
                                newuser.WSID = socket.ConnectionInfo.Id.ToString();
                                newuser.UserWant = "auth";
                                
                                Broadcast(JsonConvert.SerializeObject(newuser));

                            }
                        }
                    }


                    if (user1.UserWant == "mesAll")
                    {
                        Broadcast(JsonConvert.SerializeObject(user1));
                    }


                    if (user1.UserWant == "whisper")
                    {
                        foreach (var client in clients)
                        {
                            if (client.ConnectionInfo.Id.ToString() == user1.WSID.ToString())
                            {

                                client.Send(JsonConvert.SerializeObject(user1));

                            }
                        }

                        //var socket1 = clients.Find(client => client.ConnectionInfo.Id.ToString() == user1.WSID.ToString());
                        //socket1.Send(message);

                    }




                };
            });

            Console.WriteLine("Сервер запущен. Ожидание подключений...");
            Console.ReadLine();

            server.Dispose();


        }

        static void Broadcast(string message)
        {
            foreach (var client in clients)
            {
                
                client.Send(message);
            }
        }
        static void Auth()
        {
            Console.WriteLine("Avtorisatia");
        }

  


    }
    
}
