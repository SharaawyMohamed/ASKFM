using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
public static class DB
{
    public const string Source = "Data Source=.;Initial Catalog=ASK;Integrated Security=True";
}
namespace ASK
{

    internal class System
    { 
        User user = new User();
        public System()
        {
           
            Console.WriteLine("1 for Sign in");
            Console.WriteLine("2 for sign up");

            int regist;
            while (!int.TryParse(Console.ReadLine(), out regist) || (regist > 2 || regist < 1))
            {
                Console.WriteLine("Ivalid Number please Try again");
            }
            //open connection

            if (regist == 1)
            {
                SqlConnection con = new SqlConnection(DB.Source);
                con.Open();
                string sql = "SELECT * FROM [User] WHERE UserName = @username and @pass=Password";
                SqlCommand cmd = new SqlCommand(sql, con);
                do
                {
                    Console.Write("Enter UserName: ");
                    string username = Console.ReadLine();
                    Console.Write("Enter Password: ");
                    string password = Console.ReadLine();

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@pass", password);
                    using SqlDataReader read = cmd.ExecuteReader();
                    // found = (int)cmd.ExecuteScalar();
                    if (read.Read())
                    {
                        user.UserName = (string)read["UserName"];
                        user.Id = (int)read["Id"];
                        user.Email = (string)read["Email"];
                        user.Name = (string)read["Name"];
                        user.Password = (string)read["Password"];
                        break;
                    }
                } while (true);

                con.Close();

            }
            else
            {
                Console.Write("Enter you Name: ");
                user.Name = Console.ReadLine();
                while (string.IsNullOrEmpty(user.Name))
                {
                    Console.Write("Ivalid value Try again :");
                    user.Name = Console.ReadLine();

                }

                Console.Write("Enter you Email: ");
                user.Email = Console.ReadLine();
                while (string.IsNullOrEmpty(user.Email))
                {
                    Console.Write("Ivalid value Try again :");
                    user.Email = Console.ReadLine();

                }

                Console.Write("Enter Your UserName:");
                user.UserName = Console.ReadLine();
                while (string.IsNullOrEmpty(user.UserName))
                {
                    Console.WriteLine("Ivalid Value Try again :");
                    user.UserName = Console.ReadLine();
                }

                Console.Write("Enter Your Password:");
                user.Password = Console.ReadLine();
                while (string.IsNullOrEmpty(user.Password))
                {
                    Console.WriteLine("Ivalid Value Try again :");
                    user.Password = Console.ReadLine();

                }
                Console.Write("Are You Accept Anonymous Question? (1 to Accept and 0 To deny):");
                int an;
                while (!int.TryParse(Console.ReadLine(), out an) || (an > 1 || an < 0))
                {
                    Console.WriteLine("Ivalid value Try Again:");
                }

                SqlConnection con = new SqlConnection(DB.Source);
                con.Open();
                string sql = "insert into [User] values (@Name, @Email, @UserName, @Password, @Anonymous)";
                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.Add(new SqlParameter("@Name", user.Name));
                cmd.Parameters.Add(new SqlParameter("@Email", user.Email));
                cmd.Parameters.Add(new SqlParameter("@UserName", user.UserName));
                cmd.Parameters.Add(new SqlParameter("@Password", user.Password));
                cmd.Parameters.Add(new SqlParameter("@Anonymous", user.Anonymous));
                cmd.ExecuteNonQuery();
                con.Close();
            }
            Console.WriteLine($"\nWelcome, {user.Name}\n");
        }
        public void StartAsk()
        {
          //  UserManage usermanage = new UserManage();
            #region System Begin
            Console.WriteLine("Enter Mumber From Minue:");
            Console.WriteLine("\n"+
            "1: Print Question To Me.\n" +              
            "2: Print Question From Me.\n" +            
            "3: Answer Question.\n" +                   
            "4: Delete Question.\n" +
            "5: Ask Question.\n" +
            "6: List System Users.\n" +
            "7: Feed.\n" +
            "8: Logout");
            #endregion

            int ON;
            while (!int.TryParse(Console.ReadLine(), out ON) || (ON > 8 || ON < 1))
            {
                Console.WriteLine("Invalid input pleas enter number from Menue");
            }
            if (ON == 1)
            {
                UserManage.PrintToMe(user.Id);
            }
            else if (ON == 2)
            {
                UserManage.PrintQFromMe(user.Id);
            }
            else if (ON == 3)
            {
                UserManage.AnswerQ(user);
            }
            else if (ON == 4)
            {
                UserManage.Delete(user.Id);
            }
            else if (ON == 5)
            {
                UserManage.AskQ(user);
            }
            else if (ON == 6)
            {
                UserManage.Users();
            }
            else if (ON == 7)
            {
                UserManage.Feed();
            }
            else
            {
                UserManage.Logout();
            }
        }
    }
}

