﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.ServiceBus.Messaging;


namespace ConsoleApplication1
{
    class Program
    {
        static string eventHubName = "beats";
        static string connectionString = "Endpoint=sb://beats-ns.servicebus.windows.net/;SharedAccessKeyName=Sendrule;SharedAccessKey=TtYhAVSWvjyD0tGXcT5ZBoZxMsn8xztp7FKZPe10jXs=";

        static void SendingRandomMessages()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            while (true)
            {
                try
                {
                    var message = Guid.NewGuid().ToString();
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, message);
                    eventHubClient.Send(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                Thread.Sleep(200);
            }
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();
            SendingRandomMessages();
        }
    }
}
