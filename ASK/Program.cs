using System.Data.Common;
using System.Data.SqlClient;

namespace ASK
{
    internal class Program
    {
        static void Main(string[] args)
        {

            System AskFm = new System();
            while(true)
            {
                AskFm.StartAsk();
            }
            
        }
    }
}
