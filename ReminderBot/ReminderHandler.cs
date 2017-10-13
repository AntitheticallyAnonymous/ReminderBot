using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;


namespace ReminderBot
{
    class ReminderHandler
    {
        private readonly DiscordSocketClient _client;
        private static SortedList<DateTime, int> _reminderIds;
        private static Dictionary<int, Reminder> _reminders;
        private EventWaitHandle _ewh;
        private readonly Object _reminderLock = new Object();
        private readonly Object _jsonLock;

        public ReminderHandler(DiscordSocketClient c, Object jsonLock)
        {
            _client = c;
            _reminderIds = new SortedList<DateTime, int>();
            _reminders = new Dictionary<int, Reminder>();
            _jsonLock = jsonLock;
            AddRemindersFromJson();            
            _ewh = new EventWaitHandle(false, EventResetMode.ManualReset);            
        }

        public ReminderHandler(DiscordSocketClient c, string db)
        {
            throw new NotImplementedException("Reminder Handler (Db constructor)");
        }

        /**<summary>Gets prexisting reminders from the default json file and saves them.</summary>*/
        private void AddRemindersFromJson()
        {
            lock (_jsonLock)
            {
                string fileLocation = Path.Combine(Environment.CurrentDirectory, "reminders.json");               
                if (File.Exists(fileLocation))
                {
                    StreamReader s = new StreamReader(fileLocation);
                    string json = s.ReadToEnd();
                    Dictionary<int, Reminder> jsonReminders = JsonConvert.DeserializeObject<Dictionary<int, Reminder>>(json);
                    s.Close();
                    if (jsonReminders != null)
                    {
                        foreach (KeyValuePair<int, Reminder> k in jsonReminders)
                        {
                            AddReminder(k.Value);
                        }
                    }
                }
            }
            
        }
        
        /** <summary>Cycle that indefinitely runs. Updates reminders when signaled.
         * Sends messages when timers goes off</summary>*/
        public void MainCycle()
        {                        
            while (true)
            {
                bool signaled;
                
                if (_reminderIds.Count == 0)
                {                    
                    signaled = _ewh.WaitOne(-1); //No reminders, so we wait until we get one                   
                }
                else
                {
                    TimeSpan difference;
                    lock (_reminderLock)
                    { 
                         difference = _reminderIds.First().Key - DateTime.UtcNow;
                    }                    
                    if (difference > TimeSpan.Zero)
                    {                        
                        signaled = _ewh.WaitOne(difference);   //Wait until it's time to signal the reminder                                  
                    }
                    else
                    {
                        //TODO(?): Allow option to set leeway in config
                        if (difference.TotalMinutes >= -1) //Allow some leeway 'cause calucations aren't instantenous
                        {                                                     
                            signaled = false;                            
                        }
                        else
                        {
                            //Reminder is way past threshold, so we ignore it and remove it//update it
                            UpdateReminders();
                            signaled = true;
                        }                        
                    }
                }

                if (!signaled)
                {
                    if (_client.ConnectionState == Discord.ConnectionState.Connected)
                    {
                        SendReminder();
                        UpdateReminders();                        
                    }
                }
                else
                {
                    _ewh.Reset(); //Lets waitone block again
                }
            }
        }

        /** <summary>Removes earliest reminder if not meant to repeat. Otherwise updates when it is to go off</summary>*/
        private void UpdateReminders()
        {
            if (_reminderIds.Count <= 0)
            {
                return;
            }

            lock (_reminderLock)
            { 
                int id = _reminderIds.First().Value;
                _reminderIds.RemoveAt(0);

                if (!_reminders.ContainsKey(id))
                {
                    throw new ArgumentException("Reminder dictonary and id list has gotten out of sync somehow. Report this to the developer.");
                }

                Reminder r = _reminders[id];                

                //If the reminder is to repeat, add the interval of when it's to repeat and adds/updates the entries
                if (r.repeat > 0 || r.repeat == -1)
                {
                    r.when = r.when.AddMinutes(r.interval);
                    if (r.repeat > 0)
                    {
                        r.repeat--;
                    }

                    AddReminder(r);
                }
                else
                {
                    _reminders.Remove(id);
                }

                //If db != exist
                UpdateJson();
            }            
        }

        /** <summary>Updates the json file with the reminder entries</summary>*/
        private void UpdateJson()
        {
            lock (_jsonLock)
            {
                string fileLocation = Path.Combine(Environment.CurrentDirectory, "reminders.json");

                File.WriteAllText(fileLocation,
                    JsonConvert.SerializeObject(_reminders, Formatting.Indented));
            }
        }

        /** <summary>Updates the database with the reminder entries</summary>*/
        private void UpdateDatabase()
        {
            throw new NotImplementedException("Database Reminder Update");
        }

        /** <summary>Sends/triggers the earliest reminder</summary>*/
        private async void SendReminder()
        {
            if(_reminderIds.Count == 0)
            {
                return;
            }

            int id = _reminderIds.First().Value;
            if (!_reminders.ContainsKey(id))
            {
                throw new ArgumentException("Reminder dictonary and id list has gotten out of sync. Report this to the developer.");
            }

            Reminder r = _reminders[id];
            
            if (r.when > DateTime.UtcNow)
            {
                return;
            }

            ISocketMessageChannel chn = _client.GetChannel(r.channelId) as ISocketMessageChannel;   
            if(chn == null)
            {
                return;
            }
            
            string msg = r.message;
            if(msg == null)
            {
                msg = "";
            }            

            if(r.userId != 0 && !r.hasMention)
            {
                SocketUser user = _client.GetUser(r.userId);
                if(user != null)
                {
                    msg = user.Mention + " " + msg;
                }          
            }

            await chn.SendMessageAsync(msg);
        }

        /** <summary>Adds reminder to to the dictionary and sorted list</summary>
         * <param name="r">Reminder to be added</param>
         */
        public void AddReminder(Reminder r)
        {            
            lock (_reminderLock)
            {
                if (_reminders.ContainsKey(r.reminderId))
                {
                    _reminders[r.reminderId] = r;
                }
                else
                {
                    _reminders.Add(r.reminderId, r);
                }
                                
                _reminderIds.Add(r.when, r.reminderId);

                //Tell the thread that there's a new reminder
                if (_ewh != default(EventWaitHandle))
                {                    
                    _ewh.Set();
                }
            }            
        }        
    }
}
