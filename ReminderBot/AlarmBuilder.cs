﻿using System;

namespace ReminderBot
{
    class AlarmBuilder
    {
        public int alarmId { get; set; }
        public DateTime when { get; set; }
        public string message { get; set; }
        public ulong userId { get; set; }
        public ulong channelId { get; set; }
        public int interval { get; set; }
        public int repeat { get; set; }
        public bool hasMention { get; set; }

        public AlarmBuilder() { } //In case more constructors are added

        public AlarmBuilder AlarmId(int a)
        {
            this.alarmId = a;
            return this;
        }

        public AlarmBuilder When(DateTime w)
        {
            this.when = w;
            return this;
        }

        public AlarmBuilder Message(string m)
        {
            if (m == null)
            {
                m = "";
            }

            this.message = m;
            return this;
        }

        public AlarmBuilder UserId(ulong u)
        {
            this.userId = u;
            return this;
        }

        public AlarmBuilder ChannelId(ulong c)
        {
            this.channelId = c;
            return this;
        }

        public AlarmBuilder Interval(int i)
        {
            this.interval = i;
            return this;
        }

        public AlarmBuilder Repeat(int r)
        {
            this.repeat = r;
            return this;
        }

        public Alarm Build()
        {
            return new Alarm(this);
        }

        public AlarmBuilder HasMention(bool m)
        {
            this.hasMention = m;
            return this;
        }
    }
}
