using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASK
{
    internal class QuestionManage
    {
       // Operations
        internal QuestionManage()
        {

        }
        internal static void PrintQ(Question question)
        {
            Console.WriteLine($"From user ({question.FromId}) To user ({question.ToId}) Question ID :({question.QId})  {question.Body} ");
           

        }
        internal static void PrintTHread(List<Question> ThridList)
        {
            foreach (Question i in ThridList)
            {
                Console.Write("   Thread:");
                //   PrintQ(i);
            }
        }
        internal void printAns(string Answer)
        {
            Console.WriteLine(Answer);
        }
    }
}
