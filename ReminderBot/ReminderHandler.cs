using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task HandleCommand(SocketMessage message, string prefix)
        {
            //Ignore system and bot messages
            var msg = message as SocketUserMessage;
            if (msg == null || msg.Author.IsBot) return;       

            //Creates variable to keep track of where the prefix ends and commands start
            int pos = 0;


            //Checks if the message is meant for the bot
            if (msg.HasStringPrefix(prefix, ref pos))
            {
                // Create a Command Context.
                var context = new SocketCommandContext(_client, msg);

                char[] delimiters = { ' ' };
                var commands = context.Message.Content.Split(delimiters);

                //Check if no arguements are provided
                if (commands.Length < 2) return;

                //We only want the prefix to be .r and not .rX where X is any character                 
                if (commands[0].Equals(prefix))
                {
                    //Checks if the second arguement is a number of is a time format
                    if (Regex.IsMatch(commands[1], @"^\d+$"))
                    {
                        await context.Channel.SendMessageAsync($"Invisible alarm set to notify {context.Message.Author} in {commands[1]} minutes.");
                    }
                    else if (TimeSpan.TryParse(commands[1], out var dummyOutput))
                    {
                        await context.Channel.SendMessageAsync($"Sorry bud, but you're gonna miss that {commands[1]} alarm.");
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync($"The heck bro? You think {commands[1]} is a time? Seriously?");
                    }
                }
            }
        }
    }
}
