using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;


namespace ReminderBot
{
    class AlarmHandler
    {
        private readonly DiscordSocketClient _client;
        private static SortedList<DateTime, Alarm> alarms;
        private EventWaitHandle ewh;
        private Object alarmLock = new Object();

        public AlarmHandler(DiscordSocketClient c)
        {
            _client = c;
            alarms = new SortedList<DateTime, Alarm>();
            AddAlarmsFromJson();
            ewh = new EventWaitHandle(false, EventResetMode.ManualReset);
            Console.WriteLine("Let's begin");
        }

        //TODO alarmhandler contructor for database

        private void AddAlarmsFromJson()
        {
            string fileLocation = Path.Combine(Environment.CurrentDirectory, "alarms.json");
            Dictionary<int, Alarm> jsonAlarms;
            if (File.Exists(fileLocation))
            {
                StreamReader s = new StreamReader(fileLocation);
                string json = s.ReadToEnd();
                jsonAlarms = JsonConvert.DeserializeObject<Dictionary<int, Alarm>>(json);
                s.Close();

                foreach(KeyValuePair<int, Alarm> k in jsonAlarms)
                {
                    AddAlarm(k.Value);
                }
            }
        }

        public void MainCycle()
        {                        
            while (true)
            {
                bool signaled;
                bool oldAlarm = false;
                
                if (alarms.Count != 0)
                {
                    signaled = ewh.WaitOne(-1);                    
                }
                else
                {
                    TimeSpan difference = alarms.First().Key - DateTime.UtcNow;
                    if (difference > TimeSpan.Zero)
                    {
                        signaled = ewh.WaitOne(difference);                        
                    }
                    else
                    {
                        signaled = false;
                        if (difference.Hours > 1) //Program may not have been running when it was supposed to go off
                        {
                            oldAlarm = true;
                        }                        
                    }
                }

                if (!signaled)
                {
                    if (!oldAlarm)
                    {                        
                        SendAlarm();
                    }                    
                    UpdateAlarms();
                }                
            }
        }

        private void UpdateAlarms()
        {
            if (alarms.Count <= 0)
            {
                return;
            }

            Alarm a = alarms.First().Value;
            alarms.RemoveAt(0);

            if (a.repeat > 0 || a.repeat == -1)
            {
                a.when = a.when.AddMinutes(a.interval);
                if(a.repeat > 0)
                {
                    a.repeat--;
                }

                AddAlarm(a);
            }

            
            //update json || database
        }

        private async void SendAlarm()
        {
            Alarm a = alarms.First().Value;

            ISocketMessageChannel chn = _client.GetChannel(a.channelId) as ISocketMessageChannel;                        

            string msg = a.message;
            if(a.userId != 0)
            {
                msg = _client.GetUser(a.userId).Mention + msg;
            }

            await chn.SendMessageAsync(msg);
        }

        public void AddAlarm(Alarm a)
        {
            Console.WriteLine("added alarm #" + a.alarmId);
            lock (alarmLock)
            {
                alarms.Add(a.when, a);

                if (ewh != default(EventWaitHandle))
                {
                    Console.WriteLine("???");
                    ewh.Set();
                }
            }
        }        
    }
}
