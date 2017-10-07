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
                if (jsonAlarms != null)
                {
                    foreach (KeyValuePair<int, Alarm> k in jsonAlarms)
                    {
                        AddAlarm(k.Value);
                    }
                }
            }
        }

        public void MainCycle()
        {                        
            while (true)
            {
                bool signaled;                
                if (alarms.Count == 0)
                {                    
                    signaled = ewh.WaitOne(-1); //No alarms, so we wait until we get one                   
                }
                else
                {                    
                    TimeSpan difference = alarms.First().Key - DateTime.UtcNow;                    
                    if (difference > TimeSpan.Zero)
                    {                        
                        signaled = ewh.WaitOne(difference);   //Wait until it's time to signal the alarm                                  
                    }
                    else
                    {
                        //TODO?: ADD LEEWAY TO CONFIG INSTEAD OF HARDCODING
                        if (difference.TotalMinutes >= -1) //Allow some leeway 'cause calucations aren't instantenous
                        {                                                     
                            signaled = false;                            
                        }
                        else
                        {
                            //Alarm is way past threshold, so we ignore it and remove it//update it
                            UpdateAlarms();
                            signaled = true;
                        }                        
                    }
                }

                if (!signaled)
                {
                    if (_client.ConnectionState == Discord.ConnectionState.Connected)
                    {
                        SendAlarm();
                        UpdateAlarms();
                    }
                }
            }
        }

        private void UpdateAlarms()
        {
            if (alarms.Count <= 0)
            {
                return;
            }

            lock (alarmLock)
            { 
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
            }

            //update json || database
        }

        private async void SendAlarm()
        {
            if(alarms.Count == 0)
            {
                return;
            }

            Alarm a = alarms.First().Value;

            ISocketMessageChannel chn = _client.GetChannel(a.channelId) as ISocketMessageChannel;   
            if(chn == null)
            {
                return;
            }

            string msg = a.message;
            if(msg == null)
            {
                msg = "";
            }

            if(a.userId != 0)
            {
                SocketUser user = _client.GetUser(a.userId);
                if(user != null)
                {
                    msg = user.Mention + " " + msg;
                }          
            }

            await chn.SendMessageAsync(msg);
        }

        public void AddAlarm(Alarm a)
        {            
            lock (alarmLock)
            {
                alarms.Add(a.when, a);

                if (ewh != default(EventWaitHandle))
                {                    
                    ewh.Set();
                }
            }
        }        
    }
}
