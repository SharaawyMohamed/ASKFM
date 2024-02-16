using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ASK
{

    internal class UserManage
    {

        static Dictionary<int, User> AllUsers = new Dictionary<int, User>();
        static List<Question> Questions = new List<Question>();
        static Dictionary<int, Question> Parent = new Dictionary<int, Question>();
        static Dictionary<int, List<Question>> Threads = new Dictionary<int, List<Question>>();
        static UserManage()
        {
            #region All Users
            string sql = "Select * from [User]";
            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int qid = (int)reader["Id"];
                if (!AllUsers.ContainsKey(qid))
                {
                    AllUsers[qid] = new User(); // Initialize the list if key doesn't exist
                }
                AllUsers[(int)reader["Id"]] =
                    new User()
                    {
                        Id = (int)reader["ID"],
                        Name = (string)reader["Name"],
                        Anonymous = (int)reader["Anonymous"]
                    };
            }
            reader.Close();
            cmd.Parameters.Clear();
            #endregion
            #region AllQuestions And Thread

            sql = "Select * from Question where Thread is null";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Parent[(int)reader["Id"]] = new Question()
                {
                    QId = (int)reader["Id"],
                    Body = (string)reader["Body"],
                    Answer = reader["Answer"] == DBNull.Value ? "" : (string)reader["Answer"],
                    FromId = (int)reader["FromId"],
                    ToId = (int)reader["ToId"]
                };//System.InvalidCastException: 'Unable to cast object of type 'System.DBNull' to type 'System.String'.'
            }
            reader.Close();
            cmd.Parameters.Clear();
            sql = "Select * from Question where Thread is not null ";
            cmd = new SqlCommand(sql, con);
            //  cmd.Parameters.Add(new SqlParameter("@Id", user.Id));
            reader = cmd.ExecuteReader(); // wrong is here must be closed first
            while (reader.Read())
            {
                int threadId = (int)reader["Thread"];
                if (!Threads.ContainsKey(threadId))
                {
                    Threads[threadId] = new List<Question>(); // Initialize the list if key doesn't exist
                }
                Threads[threadId].Add(
                new Question()
                {
                    QId = (int)reader["Id"],
                    Body = (string)reader["Body"],
                    Answer = reader["Answer"] == DBNull.Value ? "" : (string)reader["Answer"],
                    FromId = (int)reader["FromId"],
                    ToId = (int)reader["ToId"]
                });
            }
            reader.Close();
            cmd.Parameters.Clear();
            #endregion
            #region alll Questions
            sql = "select * from Question";
            cmd = new SqlCommand(sql, con);
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                Questions.Add(
                   new Question
                   {
                       QId = (int)reader["Id"],
                       Body = (string)reader["Body"],
                       ToId = (int)reader["ToId"],
                       FromId = (int)reader["FromId"],
                       Answer = reader["Answer"] == DBNull.Value ? "" : (string)reader["Answer"],
                       Thread = reader["Thread"] == DBNull.Value ? -1 : (int)reader["Thread"]
                   }
                );
            }
            con.Close();
            #endregion

            foreach (Question i in Questions)
            {
                if (i.Thread == -1)
                {
                    Parent[i.QId] = i;
                }
                else
                {
                    if (!Threads.ContainsKey(i.Thread))
                    {
                        Threads[i.Thread] = new List<Question>();
                    }
                    Threads[i.Thread].Add(i);
                }
            }
        }

        public static void PrintToMe(int id)
        {
            var pr = Parent.Where(p => p.Value.ToId == id && p.Value.Thread == -1);// To Return all Questions To me
            bool ok = false;                                                                     // var thr = Threads.Where(t => pr.Any(p => p.Key == t.Key));
            List<Question> all = new List<Question>();
            foreach (var P in pr)
            {
                ok = true;
                QuestionManage.PrintParentQ(P.Value);
                all.Add(P.Value);
                if (!Threads.ContainsKey(P.Key))
                {
                    continue;
                }
                List<Question> THR = new List<Question>();
                THR = Threads[P.Key];
                foreach (Question i in THR)
                {
                    if (i.ToId == id)
                    {
                        QuestionManage.PrintThreadQ(i);
                        all.Add(i);
                    }
                }
            }
            var stay = Questions.Except(all).Where(p => p.ToId == id);
            foreach (var i in stay)
            {
                ok = true;
                QuestionManage.PrintParentQ(i);
            }
            if (!ok)
            {
                Console.WriteLine("You haven't been asked any question");
            }
        }// Done
        public static void PrintQFromMe(int id)
        {
            bool ok = false;
            var pr = Parent.Where(p => p.Value.FromId == id && p.Value.Thread == -1);// To Return all Questions To me
            List<Question> all = new List<Question>();
            foreach (var P in pr)
            {
                ok = true;
                QuestionManage.PrintParentQ(P.Value, Anonymo: AllUsers[P.Value.ToId].Anonymous, FromMe: true);

                all.Add(P.Value);
                if (!Threads.ContainsKey(P.Key))
                {
                    continue;
                }
                var THR = Threads[P.Key];
                foreach (var i in THR)
                {
                    if (i.FromId == id)
                    {
                        QuestionManage.PrintThreadQ(i, Anonymo: AllUsers[i.ToId].Anonymous, FromMe: true);
                        all.Add(i);
                    }
                }
            }
            var stay = Questions.Except(all).Where(p => p.FromId == id);
            foreach (var i in stay)
            {
                ok = true;
                QuestionManage.PrintParentQ(i, Anonymo: AllUsers[id].Anonymous, FromMe: true);
            }
            if (!ok)
            {
                Console.WriteLine("You didn't ask any questions.");
            }
        }// Done
        public static void AnswerQ(User user)
        {
            int id;
            Console.Write("Enter Question ID or -1 to Cancel : ");
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid ID Try Again: ");
            }
            if (id == -1) return;// to cancle

            bool found = Questions.Any(p => p.QId == id);//continue
            if (!found)
            {
                Console.WriteLine("Not Found Question ");
                return;
            }
            bool Answerd = Questions.Any(p => p.QId == id && !string.IsNullOrEmpty(p.Answer));
            if (Answerd)
            {
                Console.WriteLine("Note: this  question is Answerd, press -1 to cancle or any 'key' to update Answer");
                string contin = Console.ReadLine();
                if (contin == "-1") return;
            }
            QuestionManage.AnswerQ(id);
        }// Done
        public static void Delete(int userid)
        {
            Console.WriteLine("Enter Question ID or -1 to Cancel");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid ID Try Again");
            }

            bool found = Questions.Any(p => p.QId == id);
            bool can = Questions.Any(p => p.QId == id && p.FromId == userid);
            if (!found)
            {
                Console.WriteLine("This Question Not Found.");
                return;
            }
            if (!can)
            {
                Console.WriteLine("You Can't Delete a Question For Someone Else.");
                return;
            }
            QuestionManage.Delete(id);
        }// stay delete multilevel thread
        public static void AskQ(User user)
        {
            Console.WriteLine("Enter user Id or -1 to cancel:");
            int id;// user id
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid ID Try Again");
            }
            if (id == -1) return;

            if (!AllUsers.ContainsKey(id))
            {
                Console.WriteLine("Not Found User !!");
                return;
            }
            if (AllUsers[id].Anonymous == 0)
            {
                Console.WriteLine("Note:Anonymous Questions are not allowed for this user");
            }

            Console.WriteLine("For thread question enter question id or -1 To new question: ");

            int qid;// question id
            while (!int.TryParse(Console.ReadLine(), out qid))
            {
                Console.WriteLine("Invalid ID Try Again:");
            }

            Console.WriteLine("Enter Body Quetion:");
            string quest = Console.ReadLine();
            do
            {
                if (!string.IsNullOrEmpty(quest))
                {
                    break;
                }
                Console.WriteLine("Invalid Question Tray Again:");
                quest = Console.ReadLine();
            } while (true);

            if (qid == -1)
                QuestionManage.AskQ(quest, user.Id, id);
            else
                QuestionManage.AskQ(quest, user.Id, id, qid);

        }// not done
        public static void Users()
        {
            foreach (var i in AllUsers)
            {
                Console.WriteLine($"ID: {i.Value.Id} , Name: {i.Value.Name}");
            }
        }// Done
        public static void Feed()
        {
            foreach (var parent in Parent)
            {
                QuestionManage.PrintParentQ(parent.Value);// print parent question

                if (!Threads.ContainsKey(parent.Key))// if question Don't have any Threads
                {
                    continue;
                }
                List<Question> list = Threads[parent.Key];

                foreach (var Thrd in list)
                {
                    if (string.IsNullOrEmpty(Thrd.Answer))// if question have no Answer 
                    {
                        continue;
                    }

                    QuestionManage.PrintThreadQ(Thrd);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }// Done
        public static void Logout()
        {
            Environment.Exit(0);

        }// Done


    }
}
