using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        public CommandHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task<Alarm> HandleCommand(SocketMessage msg, string prefix)
        {
            //Ignores empty and bot messages
            if (msg == null || msg.Author.IsBot)
            {
                return null;
            }

            int id = ParseCommand(msg, prefix, out Alarm alarm);
            
            if (id <= 0)
            {
                await ReportErrorToUser(msg.Channel, id);
            }
            else
            {                
                alarm.alarmId = AddAlarmEntry(alarm);
                NotifyUser(_client.GetChannel(alarm.channelId) as ISocketMessageChannel);                
            }

            return alarm;
        }

        private int ParseCommand(SocketMessage msg, string prefix, out Alarm alarm)
        {
            alarm = default(Alarm);            

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

                int index = ParseWhen(args, out DateTime when, out int interval);

                if (index <= 0)
                {
                    if (index == 0)     //Invalid Time                      
                        return -3; 
                    else                //Missing when arguement
                        return -4;
                }

                string alarmMessage = ParseAlarmMessage(msg.Content, args, index, delimiters);

                alarm = new AlarmBuilder()
                    .ChannelId(msg.Channel.Id)
                    .Interval(interval)
                    .Message(alarmMessage)
                    .Repeat(repeat)
                    .UserId(msg.Author.Id)
                    .HasMention(msg.MentionedUsers.Count > 0 || msg.MentionedRoles.Count > 0)
                    .When(when)
                    .Build();
                
                return 1;              
            }

            return 0;
        }

        /** <summary>Determines how many times the alarm should go off</summary> 
         * <param name="command">Trimmed string from the user that contains the prefix and potentially other characters</param>
         * <param name="prefix">String that the command should be starting with</param>
         * <returns>
         * <para> >=0: Repeat X number of times
         * <para> -1: Repeat until removed
         * <para> -2: Invalid command
         * </returns>
         */
        private int ParseRepeat(string command, string prefix)
        {
            int repeat = 0;

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

        /**<summary>Determines when the alarm should go off</summary>
         * <param name="args"> User's message that has been split up by spaces</param>
         * <param name="when"> The time when the alarm should go off</param>
         * <param name="interval">How frequently the alarm is to repeat in minutes (assuming it repeats)</param>
         * <returns> 
         * <para> Positive value: Position in <c>args</c> for last valid DateTime</para>
         * <para> 0: Time for alarm is in the past or right now</para>
         * <para> -1: Invalid format for time</para>
         *  </returns>
         */
        private int ParseWhen(string[] args, out DateTime when, out int interval)
        {
            interval = 1440; //Minutes in a day; Only changes if when was given in minutes instead of a DateTime
            when = default(DateTime);

            int whenEndpoint = -1; //Keeps track of last valid position for DateTime in arg

            //Checks to see if time was given as a datetime and tries to parse it.            
            string temp = "";
            for (int i = 1; i < args.Length; i++)
            {
                temp += " " + args[i];
                if (DateTime.TryParse(temp, out var output))
                {
                    when = output;
                    whenEndpoint = i;
                }
            }

            //If time wasn't given in a datetime format, we check to see if it was given in minutes
            if (whenEndpoint == -1)
            {
                if (int.TryParse(args[1], out interval))
                {
                    when = DateTime.UtcNow.AddMinutes(interval);
                    if (interval == 0)
                    {
                        whenEndpoint = 0;
                    }
                    else
                    {
                        whenEndpoint = 1;
                    }                        
                }
            }

            //If time is in the past or is right now. Also determines if time wasn't given
            if (when.CompareTo(DateTime.Now) <= 0)
            {
                whenEndpoint = 0;
            }

            //TODO: Handle time zones and daylight savings time

            return whenEndpoint;
        }

        /**<summary>Parses the message that should be displayed when the alarm goes off</summary>
         * <param name="message">The original message sent by the user</param>
         * <param name="splitMessage">The original message split up into an array without spacing preserved</param>
         * <param name="whenEndpoint">Position in <c>splitMessage</c> for last valid DateTime</param>
         * <returns>Message to be sent when the alarm goes off</returns>
         */
        private string ParseAlarmMessage(string message, string[] splitMessage, int whenEndpoint, char[] delimiters)
        {
            string alarmMessage = "";
            whenEndpoint++; //If there is an alarm message, it appears after the whenEndpoint
            if (whenEndpoint < splitMessage.Length)
            {
                string trimmedMessage = alarmMessage;
                string targetTrimmedMessage = string.Join(" ", splitMessage, whenEndpoint, splitMessage.Length - whenEndpoint); //The message without spaces preserved

                //Getting the message with spaces preserved
                int i = 0;
                while (!trimmedMessage.Equals(targetTrimmedMessage) && i != -1)
                {
                    i = message.IndexOf(splitMessage[whenEndpoint], i);
                    alarmMessage = message.Substring(i);
                    trimmedMessage = string.Join(" ", alarmMessage.Split(delimiters)); //Removes spaces in the middle                    
                }
            }
            return alarmMessage;
        }

        private int AddAlarmEntry(Alarm a)
        {
            //TODO
            //IF DB EXISTS
            //RETURN ADD TO DB
            //ELSE
            return AddAlarmEntryToJson(a);
        }

        private int AddAlarmEntryToJson(Alarm a)
        {
            //TODO
            //get lock

            //Read json file
            string fileLocation = Path.Combine(Environment.CurrentDirectory, "alarms.json");
            Dictionary<int, Alarm> alarms = null;
            if (File.Exists(fileLocation))
            { 
                StreamReader s = new StreamReader(fileLocation);            
                string json = s.ReadToEnd();
                alarms = JsonConvert.DeserializeObject<Dictionary<int, Alarm>>(json);
                s.Close();
            }
            if(alarms == null)
            {
                alarms = new Dictionary<int, Alarm>();
            }

            //Find open id and add it to the list
            a.alarmId = Enumerable.Range(0, int.MaxValue)
                    .Except(alarms.Keys)
                    .FirstOrDefault();
            alarms.Add(a.alarmId, a);
                        
            //Update json file
            File.WriteAllText(fileLocation, 
                JsonConvert.SerializeObject(alarms, Formatting.Indented));

            return a.alarmId;
        }

        private bool AddAlarmEntryToDB(Alarm a)
        {
            //TODO
            return true;
        }

        private async Task ReportErrorToUser(ISocketMessageChannel channel, int error)
        {
            //0 is ignored since that means the message wasn't meant for this bot
            if (error == -1)
            {
                await channel.SendMessageAsync("Please provide a time for when the alarm should go off.");
            }
            else if (error == -2)
            {
                await channel.SendMessageAsync("Invalid command prefix. Was this meant for us?"); //Might be a request to another bot on the channel
            }
            else if(error == -3)
            {
                await channel.SendMessageAsync("Alarm cannot be set to a time in the past or the current time.");
            }
            else if(error == -4)
            {
                await channel.SendMessageAsync("Beep. Boop. Invalid time/time format given or time missing.");
            }
        }

        //Temporary function for manual testing (Class structures subject to change). Will be removed once I implement unit tests
        public async void NotifyUser(ISocketMessageChannel chn)
        {            
            await chn.SendMessageAsync("Your alarm has been added.");
        }
    }
}

