using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static blueflowr.AdminMainForm;


namespace blueflowr
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        static string conn = "Data Source=blueflowers.db;Version=3;";  // الاتصال بقاعدة بيانات SQLite
        SQLiteConnection connect = new SQLiteConnection(conn);




        //static string conn = ConfigurationManager.ConnectionStrings["blueflowersConnectionString"].ConnectionString;
        //SqlConnection connect = new SqlConnection(conn);

        private void CreateTable()
        {
            try
            {
                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        UserID INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserName TEXT NOT NULL,
                        Password TEXT NOT NULL,
                        Role TEXT NOT NULL
                    );";

                SQLiteCommand command = new SQLiteCommand(createTableQuery, connect);
                connect.Open();
                command.ExecuteNonQuery();
                connect.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل إنشاء الجدول: " + ex.Message, "رسالة خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






        private void Login_Load(object sender, EventArgs e)
        {
            // تعيين مسار قاعدة البيانات باستخدام |DataDirectory|
            AppDomain.CurrentDomain.SetData("DataDirectory", @"C:\Users\ib199\source\repos\blueflowr\blueflowr\blueflowers.db");
            CreateTable();  // التأكد من أن الجدول موجود

            try
            {
                CreateTable();  // التأكد من أن الجدول موجود
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل في الاتصال بقاعدة البيانات: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }



        }
        public bool emptyFields()
        {
            return string.IsNullOrEmpty(login_username.Text) || string.IsNullOrEmpty(login_password.Text);
        }
        private void login_btn_Click(object sender, EventArgs e)
        {
            if (emptyFields())
            {
                MessageBox.Show("الرجاء إكمال الحقول", "رسالة خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                connect.Open();

                // التحقق من وجود اسم المستخدم وكلمة المرور في قاعدة البيانات
                string query = "SELECT UserID FROM Users WHERE UserName = @UserName AND Password = @Password";
                SQLiteCommand command = new SQLiteCommand(query, connect);
                command.Parameters.AddWithValue("@UserName", login_username.Text.Trim());
                command.Parameters.AddWithValue("@Password", login_password.Text.Trim());

                // تنفيذ الاستعلام
                object result = command.ExecuteScalar();

                //// رسالة لتوضيح نتيجة الاستعلام
                //MessageBox.Show($"النتيجة: {result}", "رسالة معلوماتية", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // التحقق إذا كانت النتيجة ليست فارغة أو null
                if (result != null && result != DBNull.Value)
                {
                    // استخدام Convert.ToInt32 لتحويل النتيجة إلى int
                    int userID = Convert.ToInt32(result);

                    if (userID > 0) // التأكد أن UserID هو رقم صالح
                    {
                        Session.LoggedInUserID = userID;  // تخزين UserID في الجلسة
                        MessageBox.Show("تم تسجيل الدخول بنجاح!");

                        // فتح شاشة الإدارة
                        AdminMainForm adminForm = new AdminMainForm();
                        adminForm.Show();
                        this.Hide();
                    }
                    else
                    {
                        MessageBox.Show("الرجاء التأكد من البيانات المدخلة.", "رسالة خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة.", "رسالة خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("فشل الاتصال: " + ex.Message, "رسالة خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                connect.Close();
            }
        }   



        //using (SqlConnection connection = new SqlConnection("Server=(LocalDB)\\MSSQLLocalDB\r\n;Database=blueflowers;Trusted_Connection=True;"))
        //{
        //string query = "SELECT UserID FROM Users WHERE UserName = @UserName AND Password = @Password";
        // SqlCommand command = new SqlCommand(query, connection);
        //command.Parameters.AddWithValue("@UserName", login_username.Text);
        //command.Parameters.AddWithValue("@Password", login_password.Text);

            //connection.Open();
            //object result = command.ExecuteScalar();
            //connection.Close();

            //if (result != null)
            //{
            //  Session.LoggedInUserID = (int)result; // تخزين UserID
            // MessageBox.Show("تم تسجيل الدخول بنجاح!");
            // }
            //else
            //{
            //  MessageBox.Show("اسم المستخدم أو كلمة المرور غير صحيحة.");
            //}

            //if (emptyFields())
            //{
            //   MessageBox.Show(".الرجاء اكمال الحقول ", "رسالة خطا", MessageBoxButtons.OK);
            // }
            //else
            //{
            //     if (connect.State == ConnectionState.Closed)
            //   {
            //     try
            //   {
            //     connect.Open();

            //   string selectAccount = "SELECT COUNT(*) FROM users WHERE username = @user AND   Password = @pass";

            // using (SqlCommand cmd = new SqlCommand(selectAccount, connect))
            //{
            //  cmd.Parameters.AddWithValue("@user", login_username.Text.Trim());
            //cmd.Parameters.AddWithValue("@pass", login_password.Text.Trim());


            //int rowCount = (int)cmd.ExecuteScalar();

            //  if (rowCount > 0)
            // {
            //   string selectRole = "SELECT role FROM users WHERE username = @user AND Password = @pass";

            // using (SqlCommand getRole = new SqlCommand(selectRole, connect))
            //{
            //  getRole.Parameters.AddWithValue("@user", login_username.Text.Trim());
            //getRole.Parameters.AddWithValue("@pass", login_password.Text.Trim());

            //}
            //}                                else
            //{
            //  MessageBox.Show(".الرجاء التحقق من كلمة السر و اسم المستخدم", "رسالة خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // }
            //}

            //}
            //catch (Exception ex)
            // {
            //   MessageBox.Show(":فشل الاتصال " + ex, "رسالة خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
            // }
            //finally
            //{
            //  connect.Close();
            // }





        private void login_showPass_CheckedChanged(object sender, EventArgs e)
        {
            login_password.PasswordChar = login_showPass.Checked ? '\0' : '*';

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

    }
}
