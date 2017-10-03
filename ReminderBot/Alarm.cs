using System;

namespace ReminderBot
{
    class Alarm
    {
        public int alarmId { get; set; }
        public DateTime when { get; set; }
        public string message { get; set; }
        public ulong userId { get; set; }
        public ulong channelId { get; set; }
        public int interval { get; set; }
        public int repeat { get; set; }
        public bool started { get; set; }

        public Alarm() { } //For serializing and deserializing; Probably not safe
        public Alarm(AlarmBuilder a)
        {
            if (default(DateTime) == a.when)
            {
                throw new ArgumentNullException("An alarm has not been set or has been set to an invalid time");
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
