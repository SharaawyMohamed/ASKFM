using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ASK
{
    internal static class QuestionManage
    {
        public static void PrintParentQ(Question quest, int Anonymo = 0, bool FromMe = false)
        {
            string Anon = Anonymo == 1 ? "" : " !AQ";
            string Fromme = !FromMe ? $"From User ID({quest.FromId})" : Anon;
            string Ans = string.IsNullOrEmpty(quest.Answer) ? "No Answer For this Question" : quest.Answer;
            Console.WriteLine($"Question ID({quest.QId}){Fromme} To User ID({quest.ToId}) Question: {quest.Body} "); // from & to
            Console.WriteLine($"Answer: {Ans}");
        }
        public static void PrintThreadQ(Question quest, int Anonymo = 0, bool FromMe = false)
        {
            string Anon = Anonymo == 1 ? "" : " !AQ";
            string Fromme = !FromMe ? $"From User ID({quest.FromId})" : Anon;
            string Ans = string.IsNullOrEmpty(quest.Answer) ? "No Answer For this Question" : quest.Answer;

            Console.WriteLine($"Thread :   Question ID({quest.QId}) {Fromme} To User ID({quest.ToId}) {quest.Body}");
            Console.WriteLine($"Thread :   Answer:   {Ans}");
        }
        public static void Delete(int id)
        {
            try
            {
                string query = "Delete FROM Question WHERE Thread = @ID";
                SqlConnection connection = new SqlConnection(DB.Source);
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteScalar();

                query = "Delete FROM Question WHERE Id = @ID";
                command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteScalar();
                connection.Close();
                Console.WriteLine("Done!");
            }
            catch (Exception)
            {
                Console.WriteLine("You Can't delete this question because have multi thread");
            }


        }// user recursion to Delete Multi Thread
        public static void AnswerQ(int id)
        {
            Console.Write("Enter Question Answer: ");
            string Ans = Console.ReadLine();
            do
            {
                if (!string.IsNullOrEmpty(Ans)) break;
                Console.WriteLine("Invalid Answer");
                Ans = Console.ReadLine();
            } while (true);

            string sql = "Update [Question] set Answer=@ans where Id=@id ";
            SqlConnection con = new SqlConnection(DB.Source);
            SqlCommand cmd = new SqlCommand(sql, con);
            con.Open();
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@ans", Ans);
            cmd.ExecuteScalar();//System.InvalidOperationException: 'ExecuteNonQuery requires an open and available Connection. The connection's current state is 
            con.Close();
            Console.WriteLine("Done!\n");
        }
        public static void AskQ(string quest, int FromId, int ToId, int thread)
        {
            string sql = "INSERT INTO Question(Body, [FromId], [ToId],[Thread]) VALUES (@body, @from, @to,@thread)";
            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@body", quest));
            cmd.Parameters.Add(new SqlParameter("@from", FromId));
            cmd.Parameters.Add(new SqlParameter("@to", ToId));
            cmd.Parameters.Add(new SqlParameter("@thread", thread));
            cmd.ExecuteNonQuery();
            con.Close();
            Console.WriteLine("Done!\n");
        }
        public static void AskQ(string quest, int FromId, int ToId)
        {
            string sql = "INSERT INTO Question(Body, [FromId], [ToId]) VALUES (@body, @from, @to)";
            SqlConnection con = new SqlConnection(DB.Source);
            con.Open();
            SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add(new SqlParameter("@body", quest));
            cmd.Parameters.Add(new SqlParameter("@from", FromId));
            cmd.Parameters.Add(new SqlParameter("@to", ToId));
            cmd.ExecuteNonQuery();
            con.Close();

            Console.WriteLine("Done!\n");
        }

    }

}
