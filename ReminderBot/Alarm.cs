using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderBot
{
    class Alarm
    {
        public ulong alarmId { get; set; }
        public string when { get; set; }
        public string message { get; set; }
        public ulong userId { get; set; }
        public ulong channelId { get; set; }
        public int interval { get; set; }
        public int repeat { get; set; }
        public bool started { get; set; }

        public Alarm(AlarmBuilder a)
        {            
            if (String.IsNullOrEmpty(a.when))
            {
                throw new ArgumentNullException("Alarm cannot be set to a null/empty time. Check the construction of the alarm.");
            }
            if (a.message == null)
            {
                message = "";
            }

            alarmId = a.alarmId;
            when = a.when;
            message = a.message;
            userId = a.userId;
            channelId = a.channelId;
            interval = a.interval;
            repeat = a.repeat;
            started = false;            
        }
    }
}
