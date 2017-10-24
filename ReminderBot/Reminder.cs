using System;

namespace ReminderBot
{
    class Reminder
    {
        public int reminderId { get; set; }
        public DateTimeOffset when { get; set; }
        public string message { get; set; }
        public ulong userId { get; set; }
        public ulong channelId { get; set; }
        public int interval { get; set; }
        public int repeat { get; set; }    
        public bool hasMention { get; set; }

        public Reminder() { } //For serializing and deserializing; Probably not safe
        public Reminder(ReminderBuilder r)
        {
            if (default(DateTimeOffset) == r.when)
            {
                throw new ArgumentNullException("An reminder has not been set or has been set to an invalid time");
            }
            if (r.message == null)
            {
                message = "";
            }

            reminderId = r.reminderId;
            when = r.when;
            message = r.message;
            userId = r.userId;
            channelId = r.channelId;
            interval = r.interval;
            repeat = r.repeat;
            hasMention = r.hasMention;
        }
    }
}
