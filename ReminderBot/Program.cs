namespace ReminderBot
{
    class Program
    {
        static void Main(string[] args)
        {
            new ReminderBot().MainAsync().GetAwaiter().GetResult();
        }

    }
}
