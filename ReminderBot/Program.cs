using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.UDPClient;
using Discord.Net.Providers.WS4Net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderBot
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            //Setup bot to work with .NET Standard 1.1
            var client = new DiscordSocketClient(new DiscordSocketConfig
            {
                WebSocketProvider = WS4NetProvider.Instance,
                UdpSocketProvider = UDPClientProvider.Instance,
            });
            client.Log += Log;

            //Connect to Discord
            string token = "asdf..."; //Place bot's token here
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            
            //Blocks until program is closed
            await Task.Delay(-1);
        }

        /* Logs information to console */
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
