using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace DiscordImpossible
{
    internal class Program
    {
        static string token = Environment.GetEnvironmentVariable("DiscordImpossibleBotToken");
        static DiscordClient discordDSharpClient;
        static List<(Guid trackingId, DiscordMessage message, DiscordMessage botMessage, DateTime expiry)> inflightInbox = new();
        static ulong? announceChannel = null;
        static DataTable dataTable = new() { TableName = "Inflight Queue" };
        const bool clearChannelMessages = false;
        static void Main(string[] args)
        {
            try
            {
                MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch
            {
                // swallow any exceptions
            }
        }

        static async Task MainAsync(string[] args)
        {
            DSharpPlusHandler().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task DSharpPlusHandler()
        {
            discordDSharpClient = new DiscordClient(
               new DiscordConfiguration
               {
                   Token = token,
                   TokenType = DSharpPlus.TokenType.Bot
               });

            if (clearChannelMessages)
            {
                var channel = await discordDSharpClient.GetChannelAsync(announceChannel);
                await channel.DeleteMessagesAsync(await channel.GetMessagesAsync());
            }

            // Use this to announce when the bot is up and running
            await Announce(announceChannel);

            discordDSharpClient.MessageCreated += async (sender, e) =>
            {
                var incomingMessage = e.Message.Content.ToLower();

                switch (incomingMessage)
                {
                    case string s when s.StartsWith("!!!"):
                        await GetAndDisplayQueueStatus(e);
                        break;
                    case string s when s.StartsWith("!!"):
                        await AddMessageToDestructionQueue(e);
                        break;
                }
            };

            await discordDSharpClient.ConnectAsync();

            await Task.Delay(-1);
        }

        /// <summary>
        /// Outputs the queue information both to Discord and the console
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private static async Task GetAndDisplayQueueStatus(MessageCreateEventArgs e)
        {
            Console.Clear();
            string message;
            if (inflightInbox.Count == 0)
            {
                message = "There are no messages in the queue to be destroyed";
                await e.Message.RespondAsync(message);
                Console.WriteLine(message);
            }
            else
            {
                dataTable.Print("Channel", "Author", "Expiry Time", "Original Message ID", "Response Message ID", "Tracking GUID");
                message = $"There are {inflightInbox.Count} messages in the queue to be destroyed";
                await e.Message.RespondAsync(message);
                Console.WriteLine(message);

                StringBuilder data = new();
                foreach (var m in inflightInbox)
                {
                    data.AppendLine($"{m.trackingId}\t{m.message.Content}\t{m.expiry}");
                }
                await e.Message.RespondAsync(data.ToString());
                await e.Message.RespondAsync(dataTable.PrintAsDiscordTable());
            }
        }

        /// <summary>
        /// Starts up a thread and monitors/watches the inflight inbox. If the inbox becomes empty, the thread will be cleaned up.
        /// </summary>
        static void ProcessQueue()
        {
            new Thread(async () =>
            {
                Console.WriteLine("Thread started");
                Console.Write($"{DateTime.UtcNow}");
                do
                {
                    for (var i = 0; i < inflightInbox.Count; i++)
                    {
                        var message = inflightInbox[i];
                        var channel = await discordDSharpClient.GetChannelAsync(message.message.ChannelId);
                        if (DateTime.UtcNow >= message.expiry)
                        {
                            Console.Write(
                                $" :: Removing message {message.trackingId} :: Inflight Count {inflightInbox.Count()}");

                            await channel.DeleteMessageAsync(message.message);
                            await channel.DeleteMessageAsync(message.botMessage);
                            var index = Program.inflightInbox.IndexOf(message);
                            inflightInbox.RemoveAt(index);
                            Console.Write($" :: Successfully removed {message.trackingId} :: Inflight Count {inflightInbox.Count()}");
                            var row = dataTable.Rows.Find(message.trackingId);
                            dataTable.Rows.Remove(row);
                        }
                    }
                }
                while (inflightInbox.Count > 0);
            }).Start();
        }

        /// <summary>
        /// Used to announce when the bot is available
        /// </summary>
        /// <param name="channelId">The ID of the channel you which to announce the bot in</param>
        /// <returns></returns>
        private static async Task Announce(ulong channelId)
        {
            var channel = await discordDSharpClient.GetChannelAsync(channelId);
            await discordDSharpClient.SendMessageAsync(channel, "DiscordImpossible[DSharpPlus] is up and running");
        }

        private static async Task AddMessageToDestructionQueue(MessageCreateEventArgs e)
        {
            DiscordMessage responseMessage;
            var messageId = e.Message.Id;
            string suffix;
            int.TryParse(e.Message.Content.Substring(2), out int intervalInSeconds);
            if (intervalInSeconds == 0)
            {
                responseMessage = await e.Message
                    .RespondAsync(
                        $"This message will self destruct in 1 minute (couldn't decipher time or no time was specified");
            }
            else
            {
                if (intervalInSeconds <= 59)
                {
                    suffix = "seconds";
                }
                else if (intervalInSeconds >= 60)
                {
                    suffix = "a minute";
                }
                else if (intervalInSeconds > 3600)
                {
                    await e.Message.RespondAsync($"You can't schedule a message for deletion for any longer than an hour. We have remove the original message, but this is the content if you want to try again: > {e.Message.Content}");
                    await e.Message.DeleteAsync();
                    return;
                }
                else
                {
                    suffix = $"about {intervalInSeconds / 60} minutes";
                }
                responseMessage = await e.Message
                    .RespondAsync($"This message will self destruct in { intervalInSeconds } {suffix} (at {DateTime.UtcNow.AddSeconds(intervalInSeconds)} (UTC))");

            }
            AddToQueue(e, responseMessage, intervalInSeconds);
        }

        private static void AddToDataTable(MessageCreateEventArgs e, DiscordMessage r, int t, Guid guid)
        {
            if (dataTable.Columns.Count == 0)
            {
                dataTable.Columns.Add("Channel");
                dataTable.Columns.Add("Author");
                dataTable.Columns.Add("Expiry Time");
                dataTable.Columns.Add("Original Message ID");
                dataTable.Columns.Add("Response Message ID");
                dataTable.Columns.Add("Tracking GUID");
                dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns["Tracking GUID"] };
            }

            var row = dataTable.NewRow();
            row[0] = e.Channel.Id;
            row[1] = e.Message.Author.Username;
            row[2] = DateTime.UtcNow.AddSeconds(t);
            row[3] = e.Message.Id;
            row[4] = r.Id;
            row[5] = guid;
            dataTable.Rows.Add(row);
        }

        private static void AddToQueue(MessageCreateEventArgs e, DiscordMessage responseMessage, int intervalInSeconds)
        {
            // We may not have a thread if all messages in the inbox is empty, so do a check here, if true, we'll start up a new thread after we've added our message to the list
            var guid = Guid.NewGuid();
            var startNewThread = inflightInbox.Count > 0 ? false : true;
            inflightInbox.Add(
                new(
                    guid,
                    e.Message,
                    responseMessage,
                    DateTime.UtcNow.AddSeconds(intervalInSeconds)));
            AddToDataTable(e, responseMessage, intervalInSeconds, guid);


            if (startNewThread) { ProcessQueue(); }
        }
    }
}
