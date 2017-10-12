using System;

namespace ReminderBot
{
    class ReminderBuilder
    {
        public int reminderId { get; set; }
        public DateTime when { get; set; }
        public string message { get; set; }
        public ulong userId { get; set; }
        public ulong channelId { get; set; }
        public int interval { get; set; }
        public int repeat { get; set; }
        public bool hasMention { get; set; }

        public ReminderBuilder() { } //In case more constructors are added

        public ReminderBuilder ReminderId(int r)
        {
            this.reminderId = r;
            return this;
        }

        public ReminderBuilder When(DateTime w)
        {
            this.when = w;
            return this;
        }

        public ReminderBuilder Message(string m)
        {
            if (m == null)
            {
                m = "";
            }

            this.message = m;
            return this;
        }

        public ReminderBuilder UserId(ulong u)
        {
            this.userId = u;
            return this;
        }

        public ReminderBuilder ChannelId(ulong c)
        {
            this.channelId = c;
            return this;
        }

        public ReminderBuilder Interval(int i)
        {
            this.interval = i;
            return this;
        }

        public ReminderBuilder Repeat(int r)
        {
            this.repeat = r;
            return this;
        }

        public Reminder Build()
        {
            return new Reminder(this);
        }

        public ReminderBuilder HasMention(bool m)
        {
            this.hasMention = m;
            return this;
        }
    }
}
