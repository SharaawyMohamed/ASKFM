using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASK
{
    internal class User
    {
        internal int Id { get; set; }
        internal string Name { get; set; }
        internal string Password { get; set; }
        internal string UserName { get; set; }
        internal string Email { get; set; }
        internal int Anonymous { get; set; } = 0;
        
    }
}
