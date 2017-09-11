using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ReminderBot
{
    class ReminderHandler
    {
        private readonly DiscordSocketClient _client;        

        public ReminderHandler(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task HandleCommand(SocketMessage msg, string prefix)
        {
            //Ignore system and bot messages
            var userMsg = msg as SocketUserMessage;
            if (userMsg == null || userMsg.Author.IsBot) return;       

            //Creates variable to keep track of where the prefix ends and commands start
            int pos = 0;

            //Checks if the message is meant for the bot
            if (userMsg.HasStringPrefix(prefix, ref pos))
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, userMsg);               

                //Split up user message
                char[] delimiters = { ' ' };
                var args = context.Message.Content.Split(delimiters);

                //Check if no arguments are provided
                if (args.Length < 2)
                {
                    await context.Channel.SendMessageAsync("Please provide a time for when the alarm should go off.");
                    return;
                }

                int repeat = ParseRepeat(args[0], prefix);
                if(repeat < -1)
                {
                    await context.Channel.SendMessageAsync(args[0] + " is an invalid command prefix. Was this meant for us?"); //Might be a request to another bot on the channel
                    return;
                }

                int index = ParseWhen(args, out DateTime when, out int interval);
                if (index <= 0)
                {
                    if (index == 0)
                    {
                        await context.Channel.SendMessageAsync("Alarm cannot be set to a time in the past ("
                            + when.ToString() + ").");
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("Beep. Boop. Invalid time/time format given or time missing.");
                    }
                    return;
                }                

                string alarmMessage = ParseAlarmMessage(context.Message.Content, args, index, delimiters);

                Alarm alarm = new AlarmBuilder()
                    .ChannelId(context.Message.Channel.Id)
                    .Interval(interval)
                    .Message(alarmMessage)
                    .Repeat(repeat)
                    .UserId(context.Message.Author.Id)
                    .When(when)
                    .Build();
                
                //TODO
                //Notify user
                //Start Alarm                
            }
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
;                   repeat = -2;
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
         * <para> 0: Time for alarm is in the past</para>
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
            for(int i = 1; i<args.Length; i++)
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
                    when = DateTime.Now.AddMinutes(interval);
                    whenEndpoint = 1;
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

        private void AddAlarmEntry(Alarm a)
        {   
            //TODO
            //IF DB EXISTS
                //RETURN ADD TO DB
            //ELSE
                //RETURN ADD TO JSON
        }
        
        private bool AddAlarmEntryToJson() //Possibly return int to help with identifying errors instead of just success or failure
        {
            //TODO
            //Get list of alarms
            //Get get empty alarm id
            //Set alarm id for alarm
            //Add alarm to list
            //Serialize list

            return true;            
        }

        private bool AddAlarmEntryToDB()
        {
            //TODO
            return true;
        }        

        //Temporary function for manual testing (Class structures subject to change). Will be removed once I implement unit tests
        public async void PrintAlarm(Alarm a)
        {
            var chn = _client.GetChannel(a.channelId) as ISocketMessageChannel;
            string rpt;
            rpt = a.repeat == -1 ? "Forever" : a.repeat.ToString() + " times";
            
            await chn.SendMessageAsync("Hello, " + _client.GetUser(a.userId).Username + " sent me here with the following alarm: ```" +
                "Id: " + a.alarmId + "\n" +
                "When: " + a.when + "\n" +
                "Message: " + a.message + "\n" +
                "Repeats: " + rpt + " every " + a.interval + " minutes\n" +
                "Alarm Started: " + a.started + "\n" +                
                "```");
        }
    }
}
