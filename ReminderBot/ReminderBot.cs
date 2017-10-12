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
        private readonly Object _jsonLock;
        private CommandHandler _reminders;
        private ReminderHandler _reminder;
        private Thread _reminderThread;

        //Values from json file
        private string _token;
        private string _prefix;
        private string _db;

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
            InitializeVariablesFromJson();
            //if db isn't in json, create the lock
            _jsonLock = new Object();
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
            _reminders = new CommandHandler(_client, _jsonLock);                 
            _client.MessageReceived += HandleCommandAsync;
            _client.Ready += OnReady;            
                                 
            await ConnectToDiscord();
           
            _reminder = new ReminderHandler(_client, _jsonLock);            
            _reminderThread = new Thread(new ThreadStart(_reminder.MainCycle));            
            
            //Blocks until program is closed
            await Task.Delay(-1);
        }

        /**<summary>Code that executes when the bot's status is 'Ready'</summary>*/
        private Task OnReady()
        {
            StartReminderHandler();
            return Task.CompletedTask;
        }

        /**<summary>Starts the thread for handling reminders</summary>*/
        private Task StartReminderHandler()
        {
            _reminderThread.Start();
            return Task.CompletedTask;
        }

        /**<summary>Processes the message sent in by discord</summary>
         * <param name="message">The message sent in by discord</param>
         */
        private async Task HandleCommandAsync(SocketMessage message)
        {
            await HandleReminderCommand(message);
            return;
        }

        /**<summary>Processes the message to see if it's an reminder request. Adds the reminder if it is and starts the timer</summary>
         * <param name="message">The message sent in by discord</param>
         * <returns>
         * <para>True: Message was a valid reminder request and was successfully added and started.</para>
         * <para>False: Message was not a valid reminder request.</para>
         * </returns>
         */
        private async Task<bool> HandleReminderCommand(SocketMessage message)
        {
            Reminder r = await _reminders.HandleCommand(message, _prefix);
            if (r != null)
            {
                _reminder.AddReminder(r);
                return true;
            }

            return false;
        }

        /**<summary> Parses values from json file and puts them into the variables</summary>*/
        private void InitializeVariablesFromJson()
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
             * Owner; Can be null; Allows for bot config (Extra feature)
             * Database; Can be null (Saves to file) (Semi-core feature)
             * BotID; TODO: Investigate uses for BotID
             * ClientID; TODO: Investigate uses for ClientID
             */
            return;
        }

        /** <summary> Creates the default credentials.json file where information is stored </summary> */
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

        /** <summary> Creates the default credentials.json file where information is stored </summary> */
        private async Task ConnectToDiscord()
        {
            if (_token == null)
            {
                throw new System.ArgumentNullException("Bot's token is null. Either the value hasn't been set in " +
                    "credentials.json");
            }

            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }
    }
}

