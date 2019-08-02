using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;



namespace server
{
    class server
    {
        static Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        static User[] guests = new User[10];
        static int k = 0;
        static Thread thread1;
        static bool active = true;
        static bool read = false;


        static void AcceptCallBack(IAsyncResult result)
        {
            #region Приём подключений

            try
            {
                #region Попытка подключения

                byte[] tmpBuffer;
                int bytesTransferred;

                Socket listener_tmp = result.AsyncState as Socket;
                Socket clientSocket_tmp = serverSocket.EndAccept(out tmpBuffer, out bytesTransferred, result);
                string clientName_tmp = Encoding.ASCII.GetString(tmpBuffer, 0, bytesTransferred);
                Console.WriteLine(clientName_tmp + " was connected.");

                guests[k] = new User(clientSocket_tmp, clientName_tmp);
                k++;

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка подключения клиента.");
                Console.WriteLine(ex.Message);
            };

            // Следующая попытка
            try { serverSocket.BeginAccept(1024, AcceptCallBack, null); }
            catch { }


            #endregion
        }


        static void Recive_Reply()
        {
            # region Приём и отправка сообщений

            while (true) if (active)
                    for (int i = 0; i < k; i++) try {

                        
                            if ((guests[i] != null) && (guests[i].guestSocket.Available != 0))

                                if (guests[i].guestSocket.Available != 0)
                                {
                                    byte[] buffer = new byte[1024];
                                    guests[i].guestSocket.Receive(buffer);
                                    string message = guests[i].guestName + " : " + Encoding.UTF8.GetString(buffer);
                                    if (read) Console.WriteLine(message);
                                    for (int j = 0; j < k; j++) if (guests[j] != null)
                                            guests[j].guestSocket.Send(Encoding.UTF8.GetBytes(message));
                                }
                            
                    }
                    catch (Exception ex) { Console.WriteLine("error[RR]: " + ex.Message); }



           
            #endregion
        }


        static void Main(string[] args)
        {
            # region Инициализация сервера

            Console.WriteLine("Server.     Port:");
            int port = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Started.\n\n\n");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any,port));
            serverSocket.Listen(10);

            #endregion

            #region Асинхронные вызовы

            serverSocket.BeginAccept(1024, AcceptCallBack, null);
            thread1 = new Thread(new ThreadStart(() => Recive_Reply()));
            thread1.Start();

            #endregion

            while (true)
            {
                string say = Console.ReadLine();

                switch (say)
                {
                    case "send":
                    {
                        Console.WriteLine("Client name : ");
                        string name = Console.ReadLine();
                        Console.WriteLine("Message : ");
                        string message = Console.ReadLine();
                        message = "Server : " + message;
                        bool flag = false;
                        int c = 0;
                        for (int i = 0; i < k; i++)
                        {
                            if ((guests[i] != null) && (guests[i].guestName == name) && (guests[i].guestSocket.Connected))
                            {
                                guests[i].guestSocket.Send(Encoding.UTF8.GetBytes(message));
                                c++;
                                flag = true;
                            }
                        }
                        if (flag) Console.WriteLine($"Sent! ({c})\n");
                        else Console.WriteLine($"Client {name} was not found!\n");
                        break;
                    }

                    case "say":
                    {
                        Console.WriteLine("Message : ");
                        string message = Console.ReadLine();
                        message = "Server : " + message;
                        int c = 0;
                        for (int i = 0; i < k; i++)
                        {
                            if (guests[i] != null)
                            {
                                guests[i].guestSocket.Send(Encoding.UTF8.GetBytes(message));
                                c++;
                            }

                        }
                        Console.WriteLine($"Sent! ({c})");
                        break;
                    }

                    case "kick":
                    {
                        Console.WriteLine("Client name : ");
                        string name = Console.ReadLine();
                        Console.WriteLine("Message : ");
                        string message = Console.ReadLine();
                        bool flag = false;
                        int c = 0;
                        active = false;
                        Thread.Sleep(2000);
                        for (int i = 0; i < k; i++)
                        {
                            if ((guests[i] != null) && (guests[i].guestName == name))
                            {
                                guests[i].guestSocket.Send(Encoding.UTF8.GetBytes("You was kciked by server! Message : \n" + message));
                                guests[i].guestSocket.Shutdown(SocketShutdown.Both);
                                guests[i].guestSocket.Close();
                                guests[i] = null;
                                for (int j = 0; j < k; j++) if (guests[j]!=null)
                                    guests[j].guestSocket.Send(Encoding.UTF8.GetBytes(name + " was kicked by server! \n( " + message + " )"));
                                
                                c++;
                                flag = true;
                                
                            }
                        }
                        active = true;
                        if (flag) Console.WriteLine($"kicked! ({c})");
                        else Console.WriteLine($"Client {name} was not found!");
                        Console.WriteLine();
                        break;
                    }

                    case "ls":
                    {
                        foreach(User i in guests)
                        {
                            if (i != null)
                                Console.WriteLine(i.guestName + " " + i.guestSocket.RemoteEndPoint);
                        }
                        break;
                    }

                    case "stop":
                    {
                        try
                        {
                            for (int j = 0; j < k; j++) if (guests[j] != null)
                            {
                                 guests[j].guestSocket.Send(Encoding.UTF8.GetBytes("DISCONNECTED"));
                                 guests[j].guestSocket.Close();
                            }
                            serverSocket.Close();

                            Environment.Exit(0);
                        }
                        catch { Environment.Exit(0); }


                        break;
                    }

                    case "read":
                    {
                        read = !read;
                        break;
                    }
                }
                


            }


        }
    }

    class User
    {
        public Socket guestSocket { get; set; }
        public string guestName { get; set; }

        public User(Socket socket, string name)
        { guestName = name; guestSocket = socket; }

    }
}
