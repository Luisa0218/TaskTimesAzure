using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace TaskTimesAzure.Functions.Entities
{
     public class TaskEntity:TableEntity
    {
        public int IdEmployee { get; set; }

        public DateTime DateHour { get; set; }

        public int Type { get; set; }

        public bool Consolidate { get; set; }

    }
}
