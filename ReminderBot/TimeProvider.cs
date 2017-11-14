using System;

namespace ReminderBot
{
    public abstract class TimeProvider
    {
        private static TimeProvider current = DefaultTimeProvider.Instance;

        public static TimeProvider Current
        {
            get { return TimeProvider.current; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Cannot be null");
                }
                TimeProvider.current = value;
            }
        }

        public abstract DateTimeOffset UtcNow { get; }

        public static void ResetToDefault()
        {
            TimeProvider.current = DefaultTimeProvider.Instance;
        }
    }

    public class DefaultTimeProvider : TimeProvider
    {
        private readonly static DefaultTimeProvider instance = new DefaultTimeProvider();

        private DefaultTimeProvider() { }

        public override DateTimeOffset UtcNow
        {
            get { return DateTimeOffset.UtcNow; }
        }

        public static DefaultTimeProvider Instance
        {
            get { return DefaultTimeProvider.instance; }
        }
    }
}
