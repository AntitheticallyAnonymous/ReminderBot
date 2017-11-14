using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReminderBot
{
    class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly Object _jsonLock;

        public CommandHandler(DiscordSocketClient client, Object jsonLock)
        {
            _client = client;
            _jsonLock = jsonLock;
        }

        /**<summary>Checks the command and, if meant for this, processes the request</summary>
        * <param name="msg">The message sent from discord</param>        
        * <param name="prefix">The prefix that is to be considered for commands</param>
        * <returns>Reminder that was parsed and added (if successful), otherwise null</returns>
        */
        public async Task<Reminder> HandleCommand(SocketMessage msg, string prefix)
        {            
            //Ignores empty and bot messages
            if (msg == null || msg.Author.IsBot)
            {
                return null;
            }

            int id = ParseCommand(msg, prefix, out Reminder reminder);            
            
            if (id <= 0)
            {
                await ReportErrorToSender(msg.Channel, id);
            }
            else
            {                
                reminder.reminderId = AddEntry(reminder);                
                await NotifySender(reminder);                
            }

            return reminder;
        }

        /**<summary>Parses the message send in by discord and builds it into an <c>Reminder</c></summary>
        * <param name="msg">The message sent from discord</param>        
        * <param name="prefix">The prefix that is to be considered for commands</param>
        * <param name="reminder">The reminder that was built</param>
        * <returns>
        * <para>1: Success</para>
        * <para>0: Command ignored (not meant for this class)</para>
        * <para>Negative: Errors</para>
        * <para>-1: Only prefix provided</para>
        * <para>-2: Invalid Prefix</para>
        * <para>-3: Invalid Time</para>
        * <para>-4: Missing when reminder should go off</para>
        * </returns>
        */
        private int ParseCommand(SocketMessage msg, string prefix, out Reminder reminder)
        {
            reminder = default(Reminder);            

            //Checks if the message is meant for the bot
            if (msg.Content.StartsWith(prefix))
            {
                //Split up user message
                char[] delimiters = { ' ' };
                var args = msg.Content.Split(delimiters);
                
                if (args.Length < 2) //Only prefix provided
                    return -1;

                int repeat = ParseRepeat(args[0], prefix);
                if (repeat < -1) //Invalid prefix
                    return -2;

                int index = ParseWhen(args, msg.Timestamp, out DateTimeOffset when, out int interval);

                if (index <= 0)
                {
                    if (index == 0)     //Invalid Time                      
                        return -3; 
                    else                //Missing when arguement
                        return -4;
                }

                string reminderMessage = ParseReminderMessage(msg.Content, args, index, delimiters);

                reminder = new ReminderBuilder()
                    .ChannelId(msg.Channel.Id)
                    .Interval(interval)
                    .Message(reminderMessage)
                    .Repeat(repeat)
                    .UserId(msg.Author.Id)
                    .HasMention(msg.MentionedUsers.Count > 0 || msg.MentionedRoles.Count > 0)
                    .When(when)
                    .Build();
                
                return 1;              
            }

            return 0;
        }

        /** <summary>Determines how many times the reminder should go off</summary> 
         * <param name="command">Trimmed string from the user that contains the prefix and potentially other characters</param>
         * <param name="prefix">String that the command should be starting with</param>
         * <returns>
         * <para> >=0: Repeat X number of times</para>
         * <para> -1: Repeat until removed</para>
         * <para> -2: Invalid command</para>
         * </returns>
         */
        private int ParseRepeat(string command, string prefix)
        {
            int repeat = 0;            

            if(prefix == null || prefix.Trim() == "")
            {
                throw new ArgumentNullException("prefix", "Check credentials file.");
            }
            if(command == null || command.Trim() == "")
            {
                throw new ArgumentNullException("command");
            }

            if (prefix == null || prefix == "")
            {
                throw new ArgumentNullException("Prefix cannot be null or empty.");
            }

            if (Regex.IsMatch(command, @"^" + prefix + @"[0-9]\d*$")) //repeat x times
            {
                if (!int.TryParse(command.Remove(0, prefix.Length), out repeat))
                {
                    repeat = -2;
                }
            }
            else if (command.Equals(prefix + 'r')) //repeat forever
            {
                repeat = -1;
            }
            else if (!prefix.Equals(command))
            {
                repeat = -2;
            }

            return repeat;
        }

        /**<summary>Determines when the reminder should go off</summary>
         * <param name="args"> User's message that has been split up by spaces.</param>
         * <param name="timestamp"> The timestamp on the message from the sender.</param>
         * <param name="when"> The time when the reminder should go off.</param>
         * <param name="interval">How frequently the reminder is to repeat in minutes.</param>
         * <returns> 
         * <para> Positive value: Position in <c>args</c> for last valid DateTimeOffset.</para>
         * <para> 0: Time for reminder is in the past or right now.</para>
         * <para> -1: Invalid format for time.</para>
         *  </returns>
         */
        private int ParseWhen(string[] args, DateTimeOffset timestamp, out DateTimeOffset when, out int interval)
        {

            interval = 1440; //Minutes in a day; Only changes if when was given in minutes instead of a DateTimeOffset
            when = default(DateTimeOffset);

            if (args.Length <= 1)
            {
                return -1;
            }

            int whenEndpoint = -1; //Keeps track of last valid position for DateTimeOffset in arg

            //Checks to see if time was given as a DateTimeOffset and tries to parse it.            
            string temp = "";
            for (int i = 1; i < args.Length; i++)
            {
                temp += " " + args[i];
                if (DateTimeOffset.TryParse(temp, null as IFormatProvider, DateTimeStyles.AssumeUniversal, out var output))
                {
                    when = output;
                    whenEndpoint = i;
                }
            }

            //If time wasn't given in a DateTimeOffset format, we check to see if it was given in minutes
            if (whenEndpoint == -1)
            {
                if (int.TryParse(args[1], out interval))
                {
                    when = timestamp.AddMinutes(interval);
                    if (interval == 0)
                    {
                        whenEndpoint = 0;
                    }
                    else
                    {
                        whenEndpoint = 1;
                    }
                }
                else
                {
                    return -1;
                }

                if (timestamp.Equals(default(DateTimeOffset)))
                {
                    return -1;
                }
            }

            

            //If time is in the past or is right now. Also determines if time wasn't given            
            if (when.CompareTo(TimeProvider.Current.UtcNow) <= 0)
            {
                whenEndpoint = 0;
            }            

            return whenEndpoint;
        }

        /**<summary>Parses the message that should be displayed when the reminder goes off</summary>
         * <param name="message">The original message sent by the user</param>
         * <param name="splitMessage">The original message split up into an array without spacing preserved</param>
         * <param name="whenEndpoint">Position in <c>splitMessage</c> for last valid DateTimeOffset</param>
         * <returns>Message to be sent when the reminder goes off</returns>
         */
        private string ParseReminderMessage(string message, string[] splitMessage, int whenEndpoint, char[] delimiters)
        {
            string reminderMessage = "";
            whenEndpoint++; //If there is an reminder message, it appears after the whenEndpoint
            if (whenEndpoint < splitMessage.Length)
            {
                string trimmedMessage = reminderMessage;
                string targetTrimmedMessage = string.Join(" ", splitMessage, whenEndpoint, splitMessage.Length - whenEndpoint); //The message without spaces preserved

                //Getting the message with spaces preserved
                int i = 0;
                while (!trimmedMessage.Equals(targetTrimmedMessage) && i != -1)
                {
                    i = message.IndexOf(splitMessage[whenEndpoint], i);
                    reminderMessage = message.Substring(i);
                    trimmedMessage = string.Join(" ", reminderMessage.Split(delimiters)); //Removes spaces in the middle                    
                }
            }
            return reminderMessage;
        }
     
        /**<summary>Adds reminder to the database (if applicable). Otherwises adds it to a json file</summary>
         * <param name="r">The reminder to be added</param>
         * <returns>The unique id given to the reminder</returns>
         */
        private int AddEntry(Reminder r)
        {
            //TODO
            //IF DB EXISTS
            //RETURN ADD TO DB
            //ELSE
            return AddEntryToJson(r);
        }

        /**<summary>Adds reminder to a json file</summary>
         * <param name="r">The reminder to be added</param>
         * <returns>The unique id given to the reminder</returns>
         */
        private int AddEntryToJson(Reminder r)
        {
            lock (_jsonLock)
            { 
                //Read json file
                string fileLocation = Path.Combine(Environment.CurrentDirectory, "reminders.json");
                Dictionary<int, Reminder> reminders = null;
                if (File.Exists(fileLocation))
                { 
                    StreamReader s = new StreamReader(fileLocation);            
                    string json = s.ReadToEnd();
                    reminders = JsonConvert.DeserializeObject<Dictionary<int, Reminder>>(json);
                    s.Close();
                }
                if(reminders == null)
                {
                    reminders = new Dictionary<int, Reminder>();
                }

                //Find open id and add it to the list
                r.reminderId = Enumerable.Range(0, int.MaxValue)
                        .Except(reminders.Keys)
                        .FirstOrDefault();
                reminders.Add(r.reminderId, r);
                        
                //Update json file
                File.WriteAllText(fileLocation, 
                    JsonConvert.SerializeObject(reminders, Formatting.Indented));
            }

            return r.reminderId;
        }

        /** <summary>Adds reminder to to the database</summary>
         * <param name="r">The reminder to be added</param>
         * <returns>The unique id given to the reminder</returns>
         */
        private int AddEntryToDb(Reminder r)
        {
            throw new NotImplementedException("AddReminderEntryToDB");
        }

        /** <summary>Sends message back to the message sender indicating the error that occurred when parsing</summary>
         * <param name="channel">The channel the sender sent the message in.</param>
         * <param name="error"> The error code</param>
         */
        private async Task ReportErrorToSender(ISocketMessageChannel channel, int error)
        {
            //0 is ignored since that means the message wasn't meant for this bot
            if (error == -1)
            {
                await channel.SendMessageAsync("Please provide a time for when the reminder should go off.");
            }
            else if (error == -2)
            {
                await channel.SendMessageAsync("Invalid command prefix. Was this meant for us?"); //Might be a request to another bot on the channel
            }
            else if(error == -3)
            {
                await channel.SendMessageAsync("Reminder cannot be set to a time in the past or the current time.");
            }
            else if(error == -4)
            {
                await channel.SendMessageAsync("Beep. Boop. Invalid time/time format given or time missing.");
            }
        }

        /** <summary>Notifies the message sender that their reminder was added.</summary>
         * <param name="r">The reminder that was added</param>
         */
        private async Task NotifySender(Reminder r)
        {
            ISocketMessageChannel chn = _client.GetChannel(r.channelId) as ISocketMessageChannel;

            await chn.SendMessageAsync(_client.GetUser(r.userId).Username +
                ", your reminder set to go off " + ((r.interval == 1440) ? ("at " + r.when) :
                    ("in " + r.interval + " minute(s)"))
                + " has been added" +
                ((r.repeat == 0) ? "." :
                    " and will repeat " + ((r.repeat == -1) ? "" : r.repeat + " time(s) ") + "every " + r.interval + " minute(s).")
                + " [" + r.reminderId + "]");

        }
    }
}

