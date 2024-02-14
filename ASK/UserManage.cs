using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASK
{

    internal static class UserManage
    {
        static Dictionary<int, User> AllUsers = new Dictionary<int, User>();
        static UserManage()
        {
            #region All Users
            string Query = "Select * from [User]";
            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            SqlCommand cmd = new SqlCommand(Query, con);
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
            con.Close();
            #endregion

        }

        public static void PrintToMe(User user)
        {
            Dictionary<int, Question> ParentQuestions = new Dictionary<int, Question>();
            Dictionary<int, List<Question>> ThreadsQuestions = new Dictionary<int, List<Question>>();

            using (SqlConnection con = new SqlConnection(DB.Source))
            {
                con.Open();
                string sql = "Select * FROM Question WHERE Thread IS NULL AND ToId = @Id";//select all parent questions which asked to me
                using (SqlCommand cmd = new SqlCommand(sql, con))// read all questions which asked to me
                {
                    cmd.Parameters.Add(new SqlParameter("@Id", user.Id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        ParentQuestions[(int)reader["Id"]] = new Question()// add parent question which asked to me  in parent dictionary
                        {
                            QId = (int)reader["Id"],
                            Body = (string)reader["Body"],
                            Answer = reader["Answer"] == DBNull.Value ? "" : (string)reader["Answer"],//can be null
                            FromId = (int)reader["FromId"],
                            ToId = (int)reader["ToId"]
                        };
                    }
                    reader.Close();
                }

                sql = "SELECT * FROM Question WHERE Thread IS NOT NULL";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        int ThrID = (int)reader["Thread"];
                        if (!ThreadsQuestions.ContainsKey(ThrID))
                        {
                            ThreadsQuestions[ThrID] = new List<Question>();
                        }

                        ThreadsQuestions[ThrID].Add(new Question()
                        {
                            QId = (int)reader["Id"],
                            Body = (string)reader["Body"],
                            Answer = reader["Answer"] == DBNull.Value ? "" : (string)reader["Answer"],
                            FromId = (int)reader["FromId"],
                            ToId = (int)reader["ToId"]
                        });
                    }
                }
            }
            if (ParentQuestions.Count() == 0)
            {
                Console.WriteLine("You Don't Asked Qustion Until Now\n");
                return;
            }

            foreach (var P in ParentQuestions)
            {
                Console.WriteLine($"Question ID:({P.Value.QId}) From User({P.Value.FromId})   Question: {P.Value.Body}");
                string answer = !string.IsNullOrEmpty(P.Value.Answer) ? $"Answer: {P.Value.Answer}" : "Answer: Not Answerd Question";
                Console.WriteLine(answer);

                if (!ThreadsQuestions.ContainsKey(P.Key))// if no threads on this question
                {
                    continue;
                }
                List<Question> Threads = ThreadsQuestions[P.Key];
                foreach (var i in Threads)
                {
                    // if (i.) continue;// if i aske question on may parent question        
                    Console.WriteLine($"Thread: Question ID({i.QId}) from User({i.FromId})    Question: {i.Body}");
                    string threadAnswer = !string.IsNullOrEmpty(i.Answer) ? $"Thread: Answer: {i.Answer}" : "Answer: Nont Answered Question";
                    Console.WriteLine(threadAnswer);
                }

            }

        }
        public static void PrintQFromMe(User user)
        {
            List<Question> QQ = new List<Question>();
            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            string sql = "select * from Question where FromId = @id";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@Id", user.Id));
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                QQ.Add(new Question()
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
            if (QQ.Count() == 0)
            {
                Console.WriteLine("You Don't Ask Any Question Until Now\n");
                return;
            }
            foreach (var i in QQ)
            {

                string Ans = string.IsNullOrEmpty(i.Answer) ? "Answer: Not Answered Question" : $"Answer: {i.Answer}";
                string Anon = AllUsers[i.ToId].Anonymous == 1 ? "" : " !AQ";
                Console.WriteLine($"Question ID({i.QId}){Anon} To User ID({i.ToId})      Question : {i.Body}");
                Console.WriteLine(Ans);
            }
            Console.WriteLine();
        }
        public static void AnswerQ(User user)
        {
            Console.Write("Enter Question ID or -1 to Cancel : ");

            ///////////////////////// 
            int count = 0;
            int id;
            string query = "SELECT COUNT(*) FROM Question WHERE ID = @ID";
            do
            {

                while (!int.TryParse(Console.ReadLine(), out id) || id < -1)
                {
                    Console.WriteLine();
                    Console.Write("Invalid value Tray Again : ");
                }
                if (id == -1)
                {
                    return;
                }
                SqlConnection connection = new SqlConnection(DB.Source);
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.Parameters.AddWithValue("@ID", id);
                count = (int)command.ExecuteScalar();
                if (count == 0)
                {
                    Console.WriteLine($"Not Found Question ID Try Again :{id}");
                    Console.Write("Enter Question ID Again or -1 to cancle: ");
                    if (id == -1)
                    {

                        Console.WriteLine();
                        return;
                    }
                }
                connection.Close();
            } while (count == 0);

            //////////////////////////////////

            string Q = "Select count(*) from Question Where ToID=@UserID and Id = @QuestID";
            SqlConnection conn = new SqlConnection(DB.Source);
            SqlCommand comm = new SqlCommand(Q, conn);
            conn.Open();
            comm.Parameters.AddWithValue("@UserID", user.Id);// user id
            comm.Parameters.AddWithValue("@QuestID", id);//question id
            count = (int)comm.ExecuteScalar();
            conn.Close();
            if (count == 0)
            {
                Console.WriteLine("You Can't Answer This Question \n");
                return;
            }
            /////////////////////////
            Console.WriteLine("Enter Answere : ");
            string Ans;
            do
            {
                Ans = Console.ReadLine();
                if (string.IsNullOrEmpty(Ans))
                {
                    Console.WriteLine("Ivalid Answer Try Again: ");
                }
            } while (string.IsNullOrEmpty(Ans));

            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            string sql = "Update [Question] set Answer=@ans where Id=@id ";
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@id", id));
            cmd.Parameters.Add(new SqlParameter("@ans", Ans));
            cmd.ExecuteNonQuery();
            con.Close();
            //cmd.Parameters.Clear()
            Console.WriteLine();
        }
        public static void Delete()
        {
            Console.WriteLine("Enter Question ID or -1 to Cancel");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid ID Try Again");
            }
            if (id == -1) return;
            string sql = "Delete from Question where @id=ID";
            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@id", id));
            cmd.ExecuteNonQuery();
            con.Close();
        }
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

            if (qid != -1)
            {
                string Q = "Select count(*) from Question Where  Id = @QuestID";
                SqlConnection conn = new SqlConnection(DB.Source);
                SqlCommand comm = new SqlCommand(Q, conn);
                conn.Open();
                comm.Parameters.AddWithValue("@QuestID", qid);//question id
                int count = (int)comm.ExecuteScalar();
                conn.Close();
                if (count == 0)
                {
                    Console.WriteLine("You Can't Thread On Not Found Question \n");
                    return;
                }
            }
            Console.WriteLine("Enter Body Quetion:");
            string quest = Console.ReadLine();
            if (qid == -1)
            {
                string sql = "INSERT INTO Question([Body], [FromId], [ToId]) VALUES (@body, @from, @to)";
                SqlConnection con = new SqlConnection(DB.Source);
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add(new SqlParameter("@body", quest));
                cmd.Parameters.Add(new SqlParameter("@from", user.Id));
                cmd.Parameters.Add(new SqlParameter("@to", id));
                cmd.ExecuteNonQuery();
                con.Close();

            }
            else
            {
                string sql = "INSERT INTO Question(Body, [FromId], [ToId],[Thread]) VALUES (@body, @from, @to,@thread)";
                SqlConnection con = new SqlConnection(DB.Source);
                con.Open();
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add(new SqlParameter("@body", quest));
                cmd.Parameters.Add(new SqlParameter("@from", user.Id));
                cmd.Parameters.Add(new SqlParameter("@to", id));
                cmd.Parameters.Add(new SqlParameter("@thread", qid));
                cmd.ExecuteNonQuery();
                con.Close();
            }

        }
        public static void Users()
        {
            foreach (var i in AllUsers)
            {
                Console.WriteLine($"ID: {i.Value.Id} , Name: {i.Value.Name}");
            }
        }
        public static void Feed()
        {
            Dictionary<int, Question> Parent = new Dictionary<int, Question>();
            Dictionary<int, List<Question>> Threads = new Dictionary<int, List<Question>>();

            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            string sql = "Select * from Question where Thread is null";
            SqlCommand cmd = new SqlCommand(sql, con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int qId = (int)reader["Id"];
                string body = (string)reader["Body"];
                string answer = reader["Answer"] == DBNull.Value ? "" : (string)reader["Answer"];
                int fromId = (int)reader["FromId"];
                int toId = (int)reader["ToId"];
                Parent[qId] = new Question()
                {
                    QId = qId,
                    Body = body,
                    Answer = answer,
                    FromId = fromId,
                    ToId = toId
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
            con.Close();
            foreach (var parent in Parent)
            {
                Console.WriteLine($"Question ID({parent.Key}):  From User ID({parent.Value.FromId})     Question: {parent.Value.Body} "); // from & to
                Console.WriteLine($"ID ({parent.Key}) A. {parent.Value.Answer ?? "N/A"} ?");

                // Check if the key exists in the Threads dictionary
                if (!Threads.ContainsKey(parent.Key))
                {
                    Console.WriteLine("No threads found for this question.");
                    return;
                }
                List<Question> list = Threads[parent.Key];
                foreach (var Thrd in list)
                {
                    string Ans = string.IsNullOrEmpty(Thrd.Answer) ? "No Answer For this Question" : Thrd.Answer;
                    Console.WriteLine($"Thread :   Question:  {Thrd.Body}");
                    Console.WriteLine($"Thread :   Answer:   {Ans}");
                }

            }

            Console.WriteLine();
        }
        public static void Logout()
        {
            Environment.Exit(0);

        }


    }
}
