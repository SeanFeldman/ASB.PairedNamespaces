using System;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace PairedNamespaces
{
    static class Program
    {
        static void Main()
        {
            MainAsync().GetAwaiter().GetResult();

            Console.WriteLine("press any key to exit");
            Console.ReadLine();
        }

        static async Task MainAsync()
        { 
            var connectionString1 = "Endpoint=sb://primary.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=";
            var namespaceManager1 = NamespaceManager.CreateFromConnectionString(connectionString1);
            var messagingFactory1 = MessagingFactory.CreateFromConnectionString(connectionString1);

            var connectionString2 = "Endpoint=sb://secondary.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=";
            var namespaceManager2 = NamespaceManager.CreateFromConnectionString(connectionString2);
            var messagingFactory2 = MessagingFactory.CreateFromConnectionString(connectionString2);

            if (!await namespaceManager1.QueueExistsAsync("testing").ConfigureAwait(false))
            {
                await namespaceManager1.CreateQueueAsync("testing").ConfigureAwait(false);
            }

            var sendAvailabilityPairedNamespaceOptions = new SendAvailabilityPairedNamespaceOptions(
                secondaryNamespaceManager:namespaceManager2,
                messagingFactory: messagingFactory2,
                backlogQueueCount: 3,
                enableSyphon: true,
                failoverInterval: TimeSpan.FromSeconds(3));

            await messagingFactory1.PairNamespaceAsync(sendAvailabilityPairedNamespaceOptions).ConfigureAwait(false);

            var sender1 = await messagingFactory1.CreateMessageSenderAsync("testing").ConfigureAwait(false);
            var message = new BrokeredMessage("testing");
            // Set a breakpoint here and modify hosts file to contain "0.0.0.0 primary-pairednamespace.servicebus.windows.net"
            await sender1.SendAsync(message).ConfigureAwait(false);
        }
    }
}