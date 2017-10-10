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
        private static SortedList<DateTime, int> _alarmIds;
        private static Dictionary<int, Alarm> _alarms;
        private EventWaitHandle _ewh;
        private readonly Object _alarmLock = new Object();
        private readonly Object _jsonLock;

        public AlarmHandler(DiscordSocketClient c, Object jsonLock)
        {
            _client = c;
            _alarmIds = new SortedList<DateTime, int>();
            _alarms = new Dictionary<int, Alarm>();
            _jsonLock = jsonLock;
            AddAlarmsFromJson();            
            _ewh = new EventWaitHandle(false, EventResetMode.ManualReset);            
        }

        public AlarmHandler(DiscordSocketClient c, string db)
        {
            throw new NotImplementedException("Alarm Handler (Db constructor)");
        }

        /**<summary>Gets prexisting alarms from the default json file and saves them.</summary>*/
        private void AddAlarmsFromJson()
        {
            lock (_jsonLock)
            {
                string fileLocation = Path.Combine(Environment.CurrentDirectory, "alarms.json");               
                if (File.Exists(fileLocation))
                {
                    StreamReader s = new StreamReader(fileLocation);
                    string json = s.ReadToEnd();
                    Dictionary<int, Alarm> jsonAlarms = JsonConvert.DeserializeObject<Dictionary<int, Alarm>>(json);
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
            
        }
        
        /** <summary>Cycle that indefinitely runs. Updates alarms when signaled.
         * Sends messages when timers goes off</summary>*/
        public void MainCycle()
        {                        
            while (true)
            {
                bool signaled;                
                if (_alarmIds.Count == 0)
                {                    
                    signaled = _ewh.WaitOne(-1); //No alarms, so we wait until we get one                   
                }
                else
                {
                    TimeSpan difference;
                    lock (_alarmLock)
                    { 
                         difference = _alarmIds.First().Key - DateTime.UtcNow;
                    }                    
                    if (difference > TimeSpan.Zero)
                    {                        
                        signaled = _ewh.WaitOne(difference);   //Wait until it's time to signal the alarm                                  
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

        /** <summary>Removes earliest alarm if not meant to repeat. Otherwise updates when it is to go off</summary>*/
        private void UpdateAlarms()
        {
            if (_alarmIds.Count <= 0)
            {
                return;
            }

            lock (_alarmLock)
            { 
                int id = _alarmIds.First().Value;
                _alarmIds.RemoveAt(0);

                if (!_alarms.ContainsKey(id))
                {
                    throw new ArgumentException("Alarm dictonary and id list has gotten out of sync somehow. Report this to the developer.");
                }

                Alarm a = _alarms[id];                

                //If the alarm is to repeat, add the interval of when it's to repeat and adds/updates the entries
                if (a.repeat > 0 || a.repeat == -1)
                {
                    a.when = a.when.AddMinutes(a.interval);
                    if (a.repeat > 0)
                    {
                        a.repeat--;
                    }

                    AddAlarm(a);
                }
                else
                {
                    _alarms.Remove(id);
                }                
            }
        }

        /** <summary>Updates the json file with the alarm entries</summary>*/
        private void UpdateJson()
        {
            lock (_jsonLock)
            {
                string fileLocation = Path.Combine(Environment.CurrentDirectory, "alarms.json");

                File.WriteAllText(fileLocation,
                    JsonConvert.SerializeObject(_alarms, Formatting.Indented));
            }
        }

        /** <summary>Updates the database with the alarm entries</summary>*/
        private void UpdateDatabase()
        {
            throw new NotImplementedException("Database Alarm Update");
        }

        /** <summary>Sends/triggers the earliest alarm</summary>*/
        private async void SendAlarm()
        {
            if(_alarmIds.Count == 0)
            {
                return;
            }

            int id = _alarmIds.First().Value;
            if (!_alarms.ContainsKey(id))
            {
                throw new ArgumentException("Alarm dictonary and id list has gotten out of sync. Report this to the developer.");
            }

            Alarm a = _alarms[id];
            
            if (a.when > DateTime.UtcNow)
            {
                return;
            }

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

            if(a.userId != 0 && !a.hasMention)
            {
                SocketUser user = _client.GetUser(a.userId);
                if(user != null)
                {
                    msg = user.Mention + " " + msg;
                }          
            }

            await chn.SendMessageAsync(msg);
        }

        /** <summary>Adds alarm to to the dictionary and sorted list</summary>
         * <param name="a">Alarm to be added</param>
         */
        public void AddAlarm(Alarm a)
        {            
            lock (_alarmLock)
            {
                if (_alarms.ContainsKey(a.alarmId))
                {
                    _alarms[a.alarmId] = a;
                }
                else
                {
                    _alarms.Add(a.alarmId, a);
                }
                                
                _alarmIds.Add(a.when, a.alarmId);

                //Tell the thread that there's a new alarm
                if (_ewh != default(EventWaitHandle))
                {                    
                    _ewh.Set();
                }
            }            
        }        
    }
}
