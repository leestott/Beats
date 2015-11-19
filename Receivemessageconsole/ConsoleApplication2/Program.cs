﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using System.Diagnostics;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace ConsoleApplication2
{
    class SentimentData
    {
        public int Heart { get; set; }
        public int Temp { get; set; }
        public int Steps { get; set; }
        public string Name { get; set; }
    }


    class SimpleEventProcessor : IEventProcessor
    {
        Stopwatch checkpointStopWatch;

        async Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("SimpleEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
            this.checkpointStopWatch = new Stopwatch();
            this.checkpointStopWatch.Start();
            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                string data = Encoding.UTF8.GetString(eventData.GetBytes());

                Console.WriteLine(string.Format("Message received.  Partition: '{0}', Data: '{1}'",
                    context.Lease.PartitionId, data));

               // int heartRate = Int32.Parse(data);

                ////create sentimentdata object
                //var sentimentData = new SentimentData() { Heart = heartRate, Temp = heartRate, Steps = heartRate, Name = "beats" };

                ////post sentimentdata to api
                //using (var client = new HttpClient())
                //{
                //    client.BaseAddress = new Uri("http://beatsapi.azurewebsites.net");
                //    var response = client.PostAsJsonAsync("/api/sentimentdata", sentimentData).Result;
                //}



            }

            //Call checkpoint every 5 minutes, so that worker can resume processing from the 5 minutes back if it restarts.
            if (this.checkpointStopWatch.Elapsed > TimeSpan.FromMinutes(5))
            {
                await context.CheckpointAsync();
                this.checkpointStopWatch.Restart();
            }
        }
    }


    class Program

    {

        static void Main(string[] args)
        {
            string eventHubConnectionString = "Endpoint=sb://beats-ns.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=AqPi7c7I3JkVZQ9OGUjt3aEtW1GJ4voOgB+wagPZydY=";
            string eventHubName = "beats";
            string storageAccountName = "beast";
            string storageAccountKey = "qIol5cl06GF1OO8anp2r5Kl1yCR7WcBQcTUSSZ3Ta48Knrrl6vpTgAWhyrRTR1C/54BAvKMbuXOjSuak7UpMBw==";
            string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", storageAccountName, storageAccountKey);

            string eventProcessorHostName = Guid.NewGuid().ToString();
            EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<SimpleEventProcessor>().Wait();

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
