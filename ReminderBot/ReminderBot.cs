using Discord;
using Discord.Net.Providers.WS4Net;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ReminderBot
{
    class ReminderBot
    {
        private readonly DiscordSocketClient _client;
        private CommandHandler _reminders;
        private AlarmHandler _alarm;
        private Thread _alarmThread;

        //Values from json file
        private string _token;
        private string _prefix;

        public ReminderBot()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                WebSocketProvider = WS4NetProvider.Instance,
                //UdpSocketProvider = UDPClientProvider.Instance,

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
            _reminders = new CommandHandler(_client);                 
            _client.MessageReceived += HandleCommandAsync;
            _client.Ready += StartAlarmHandler;
            
            await InitializeVariablesFromJson();
            await ConnectToDiscord();
           
            _alarm = new AlarmHandler(_client);            
            _alarmThread = new Thread(new ThreadStart(_alarm.MainCycle));            
            
            //Blocks until program is closed
            await Task.Delay(-1);
        }

        private Task StartAlarmHandler()
        {            
            _alarmThread.Start();
            return Task.CompletedTask;
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {            
            Alarm a = await _reminders.HandleCommand(message, _prefix);
            if(a != null)
            {
                _alarm.AddAlarm(a);
            }

        }

        /*
         * Parses variables from json file
         */
        private Task InitializeVariablesFromJson()
        {
            //If the credentials file doesn't exist, create it
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "credentials.json")))
            {
                CreateDefaultCredentialsFile();
            }

            //Get credential.json
            JObject credentials = JObject.Parse(File.ReadAllText(Path.Combine(Environment.CurrentDirectory, "credentials.json")));

            //Check if bot's token is set so that it can connect to discord            
            if (credentials["Token"].Type == JTokenType.Null)
            {
                throw new System.ArgumentNullException("Missing Bot's Token in credentials.json");
            }
            _token = credentials["Token"].ToString();

            //Check if the prefix for setting comands has been set
            if (credentials["Prefix"].Type == JTokenType.Null || credentials["Prefix"].ToString().Trim(' ').Equals(""))
            {
                throw new System.ArgumentNullException("Missing prefix for commands in credentials.json");
            }
            _prefix = credentials["Prefix"].ToString();

            /* TODO: Implement the following credentials
             * Owner; Can be null
             * Database; Can be null (Saves to file)
             * BotID; TODO: Investigate uses for BotID
             * ClientID; TODO: Investigate uses for ClientID
             */

            return Task.CompletedTask;
        }

        /* 
         * Creates the default credentials.json file where information is stored
         */
        private void CreateDefaultCredentialsFile()
        {
            JObject credentials = new JObject(
                new JProperty("ClientID", null),
                new JProperty("BotID", null),
                new JProperty("Token", null),
                new JProperty("Owners", null),
                new JProperty("Database", null),
                new JProperty("Prefix", ".r"));

            File.WriteAllText(@"credentials.json", credentials.ToString());
        }

        private async Task ConnectToDiscord()
        {
            if (_token == null)
            {
                throw new System.ArgumentNullException("Bot's token is null. Either the value hasn't been set in " +
                    "credentials.json or the value hasn't been parsed yet.");
            }

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }
    }
}

