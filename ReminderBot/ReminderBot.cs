using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.UDPClient;
using Discord.Net.Providers.WS4Net;


using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ReminderBot
{
    class ReminderBot
    {
        private readonly DiscordSocketClient _client;        
        private ReminderHandler _reminders;

        //Values from json file
        private string _token;
        private string _prefix;

        public ReminderBot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                //Setup bot to work with .NET Standard 1.1
                WebSocketProvider = WS4NetProvider.Instance,
                UdpSocketProvider = UDPClientProvider.Instance,

                //Logging information to console
                LogLevel = LogSeverity.Info,

                //Message cache used to check reactions, content of edited/deleted messages, etc.
                MessageCacheSize = 50
            });
        }

        /* 
         * Main Async function. Adds command handler, console logging and connects to discord
         * Keeps running until the program closes
         */
        public async Task MainAsync()
        {
            //Add logging handler
            _client.Log += new Logger().Log;

            //Add command handler(s)
            _reminders = new ReminderHandler(_client);
            _client.MessageReceived += HandleCommandAsync;

            await InitializeVariables();
            await ConnectToDiscord();
           
            //Blocks until program is closed
            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            await _reminders.HandleCommand(message, _prefix);
        }
        
        private Task InitializeVariables()
        {
            JObject credentials;

            //If the credentials file doesn't exist, create it
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "credentials.json")))
            {
                credentials = new JObject(
                new JProperty("ClientID", null),
                new JProperty("BotID", null),
                new JProperty("Token", null),
                new JProperty("Owners", null),
                new JProperty("Database", null),
                new JProperty("Prefix", ".r"));

                File.WriteAllText(@"credentials.json", credentials.ToString());
            }

            //Get credential json file
            credentials = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "credentials.json")));

            //Check if bot's token is set so that it can connect to discord            
            if (credentials["Token"].Type == JTokenType.Null)
            {
                throw new System.ArgumentNullException("Missing Bot's Token in credentials.json");
            }
            _token = credentials["Token"].ToString();
            
            if (credentials["Prefix"].Type == JTokenType.Null)
            {
                throw new System.ArgumentNullException("Missing prefix for commands in credentials.json");
            }
            _prefix = credentials["Prefix"].ToString();

            /* TODO: Implement the following credentials
             * Owner; Can be null
             * Database; Can be null
             * BotID; TODO: Investigate uses for BotID
             * ClientID; TODO: Investigate uses for ClientID
             */

            return Task.CompletedTask;
        }

        private async Task ConnectToDiscord()
        {            
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }
    }
}
