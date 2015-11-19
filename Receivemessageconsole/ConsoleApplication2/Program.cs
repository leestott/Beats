using System;
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

                JToken token = JObject.Parse(data);

                string measure = (string)token.SelectToken("measurename");
                int temp = 30;
                int heart = 80;


                if (measure == "SkinTemperature")
                {
                    temp = (int)token.SelectToken("value");
                    Console.WriteLine(measure + " " + temp);
                }
                else if(measure == "HeartRate")
                {
                    heart = (int)token.SelectToken("value");
                    Console.WriteLine(measure + " " + heart);
                }


                Console.WriteLine(measure + " " + temp);

                
                //create sentimentdata object
                var sentimentData = new SentimentData() { Heart = heart, Temp = temp, Steps = 5000, Name = "beats" };

                //post sentimentdata to api
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("http://beatsapi.azurewebsites.net");
                    var response = client.PostAsJsonAsync("/api/sentimentdata", sentimentData).Result;
                }



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
            string eventHubConnectionString = "Endpoint=sb://beats-ns.servicebus.windows.net/;SharedAccessKeyName=manage;SharedAccessKey=hyZPOugpflLQ6/nIlwzKh3uqwtQa1NwLOOL9yxWPR+s=";
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
