using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderBot
{
    class Program
    {
        /*
         * Entry point of program
         */
        static void Main(string[] args)
        {            
           new ReminderBot().MainAsync().GetAwaiter().GetResult();
        }

    } 
}
