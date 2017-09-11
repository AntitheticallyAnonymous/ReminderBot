using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderBot
{
    class AlarmBuilder
    {
        public ulong alarmId { get; set; }
        public string when { get; set; }
        public string message { get; set; }
        public ulong userId { get; set; }
        public ulong channelId { get; set; }
        public int interval { get; set; }
        public int repeat { get; set; }
        
        public AlarmBuilder(){} //In case more constructors are added

        public AlarmBuilder AlarmId(ulong a)
        {
            this.alarmId = a;
            return this;
        }

        public AlarmBuilder When(DateTime w)
        {
            this.when = w.ToString();
            return this;
        }

        private AlarmBuilder When(string w)
        {
            if(String.IsNullOrEmpty(w))
            {
                throw new ArgumentNullException("Alarm cannot be set at an empty/null time. This shouldn't happen"); //Only called indirectly by the above polymorphed function
            }
            
            this.when = w;
            return this;
        }

        public AlarmBuilder Message(string m)
        {
            if(m == null)
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
    }
}
