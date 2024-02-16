using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASK
{
    internal class Question
    {
        internal int QId { get; set; }
        internal int ToId { get; set; }
        internal int FromId { get; set; }
        internal string Body { get; set; }
        internal string? Answer { get; set; } = null;
        internal int Thread { get; set; }
    }
}
