﻿using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace Receiver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var connectionString = args[0];
            var queueName = "queue1";
            var queueClient = new QueueClient(connectionString, queueName);
            var messageHandlerOptions = new MessageHandlerOptions(OnException);
            messageHandlerOptions.MaxConcurrentCalls = 4;
            Console.WriteLine($"{messageHandlerOptions.AutoComplete},{messageHandlerOptions.MaxAutoRenewDuration},{messageHandlerOptions.MaxConcurrentCalls}");
            queueClient.RegisterMessageHandler(OnMessage, messageHandlerOptions);
            Console.WriteLine("Listening, press any key");
            Console.ReadKey();
            await queueClient.CloseAsync();
        }

        static async Task OnMessage(Message m, CancellationToken ct)
        {
            
            var messageText = Encoding.UTF8.GetString(m.Body);
            Console.WriteLine("Got a message:");
            Console.WriteLine(messageText);
            
            Console.WriteLine($"Enqueued at {m.SystemProperties.EnqueuedTimeUtc}");
            Console.WriteLine($"CorrelationId: {m.CorrelationId}"); // not filled in for you
            Console.WriteLine($"ContentType: {m.ContentType}"); // not filled in for you
            Console.WriteLine($"Label: {m.Label}"); // not filled in for you
            Console.WriteLine($"MessageId: {m.MessageId}"); // used for deduplication - is provided for you
            foreach(var prop in m.UserProperties)
            {
                Console.WriteLine($"{prop.Key}={prop.Value}");
            }

            if (messageText.ToLower().Contains("sleep"))
            {
                await Task.Delay(5000);
            }
            
            if (messageText.ToLower().Contains("error"))
            {
                throw new InvalidOperationException("something went wrong handling this message");
            }

            Console.WriteLine($"Finished processing: {messageText}");
            
        }

        static Task OnException(ExceptionReceivedEventArgs args)
        {
            Console.WriteLine("Got an exception:");
            Console.WriteLine(args.Exception.Message);
            Console.WriteLine(args.ExceptionReceivedContext.Action);
            Console.WriteLine(args.ExceptionReceivedContext.ClientId);
            Console.WriteLine(args.ExceptionReceivedContext.Endpoint);
            Console.WriteLine(args.ExceptionReceivedContext.EntityPath);
            return Task.CompletedTask;
        }
    }
}
