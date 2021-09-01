using System;

namespace TaskTimesAzure.Common.Models
{
    public class Task
    {
        public int IdEmployee { get; set; }

        public DateTime DateHour { get; set; }

        public int Type { get; set; }

        public bool Consolidate { get; set; }

    }
}
