using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Data.SQLite;  // استخدام مكتبة SQLite

namespace blueflowr
{
    public partial class Register : Form
    {





        private int userID;
        private string username;
        private string Password;
        private int Role;
        private bool status;

        public Register(int userID, string username, string Password, int Role, bool status)
        {
            InitializeComponent();

          
            
        }
          

        public Register()
        {
            InitializeComponent();
        }

        //static string conn = ConfigurationManager.ConnectionStrings["blueflowersConnectionString"].ConnectionString;

        static string conn = "Data Source=blueflowers.db;Version=3;";  // الاتصال بقاعدة بيانات SQLite
        SQLiteConnection connect = new SQLiteConnection(conn);
       

        public bool emptyFields()
        {
            if (register_username.Text == "" || register_password.Text == "" || register_cPassword.Text == "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }




        private void RegisterUser()
        {
            try
            {
                string connString = "Data Source=blueflowers.db;Version=3;"; // تأكد من المسار الصحيح
                using (SQLiteConnection conn = new SQLiteConnection(connString))
                {
                    conn.Open();

                    string query = "INSERT INTO Users (UserName, Password, Role) VALUES (@UserName, @Password, @Role)";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        // إضافة المعاملات (Parameters)
                        cmd.Parameters.AddWithValue("@UserName", register_username.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", register_password.Text.Trim()); // تأكد من تشفير كلمة المرور إذا لزم الأمر
                        cmd.Parameters.AddWithValue("@Role", "User"); // أو "Admin" إذا كنت تستخدمه

                        // تنفيذ الاستعلام
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"تم إضافة {rowsAffected} مستخدم إلى قاعدة البيانات.", "معلومات", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("فشل في إضافة المستخدم.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الاتصال أو الاستعلام: " + ex.Message, "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void register_btn_Click(object sender, EventArgs e)
        {

            RegisterUser();  // استدعاء دالة تسجيل المستخدم
            MessageBox.Show("تأكد من إضافة المستخدم.", "معلومات", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }


        private void LoadUsers()
        {
            string connString = "Data Source=blueflowers.db;Version=3;";
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    conn.Open();

                    // استعلام لعرض المستخدمين
                    string query = "SELECT UserID, UserName, Password, Role FROM Users";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        SQLiteDataAdapter dataAdapter = new SQLiteDataAdapter(cmd);
                        System.Data.DataTable dataTable = new System.Data.DataTable();
                        dataAdapter.Fill(dataTable);

                        // تنظيف الأعمدة القديمة
                        dataGridView1.Columns.Clear();

                        // إضافة عمود UserID
                        DataGridViewTextBoxColumn userIDColumn = new DataGridViewTextBoxColumn
                        {
                            Name = "UserID",
                            HeaderText = "User ID",
                            DataPropertyName = "UserID"
                        };
                        dataGridView1.Columns.Add(userIDColumn);

                        // إضافة عمود UserName
                        DataGridViewTextBoxColumn userNameColumn = new DataGridViewTextBoxColumn
                        {
                            Name = "UserName",
                            HeaderText = "User Name",
                            DataPropertyName = "UserName"
                        };
                        dataGridView1.Columns.Add(userNameColumn);

                        // إضافة عمود Password
                        DataGridViewTextBoxColumn passwordColumn = new DataGridViewTextBoxColumn
                        {
                            Name = "Password",
                            HeaderText = "Password",
                            DataPropertyName = "Password"
                        };
                        dataGridView1.Columns.Add(passwordColumn);

                        // إضافة عمود Role
                        DataGridViewTextBoxColumn roleColumn = new DataGridViewTextBoxColumn
                        {
                            Name = "Role",
                            HeaderText = "Role",
                            DataPropertyName = "Role"
                        };
                        dataGridView1.Columns.Add(roleColumn);

                        // تعيين البيانات إلى DataGridView
                        dataGridView1.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ في الاتصال: " + ex.Message, "رسالة خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }


        //using (SqlConnection connection = new SqlConnection("Server=IB1998MU\\SQLEXPRESS;Database=blueflowers;Trusted_Connection=True;"))
        //{
        //    string query = "SELECT UserID, Username, Password, Role,CreatedDate FROM Users";
        //    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
        //    DataTable usersTable = new DataTable();
        //    adapter.Fill(usersTable);

        //    dataGridView1.DataSource = usersTable;
        //}



        private void Register_Load(object sender, EventArgs e)
        {
            // تعيين المسار الكامل لقاعدة البيانات
            AppDomain.CurrentDomain.SetData("DataDirectory", @"C:\Users\ib199\source\repos\blueflowr\blueflowr");
            LoadUsers();

        }

        private void register_showPass_CheckedChanged(object sender, EventArgs e)
        {
            register_password.PasswordChar = register_showPass.Checked ? '\0' : '*';
            register_cPassword.PasswordChar = register_showPass.Checked ? '\0' : '*';
        }

        private void chosebox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnrefrsh_Click(object sender, EventArgs e)
        {
            LoadUsers();
            MessageBox.Show("تم تحديث البيانات بنجاح!");
        }

        private void butndelate_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedRows.Count > 0)
            {
                int userID = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["UserID"].Value);

                string conn = "Data Source=blueflowers.db;Version=3;";
                using (SQLiteConnection connection = new SQLiteConnection(conn))
                {
                    string query = "DELETE FROM Users WHERE UserID = @UserID";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@UserID", userID);

                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();  // تأكد من إغلاق الاتصال بعد التنفيذ
                }


                LoadUsers();
                MessageBox.Show("تم حذف المستخدم بنجاح!");
            
                    }
            else
            {
                MessageBox.Show("يرجى تحديد مستخدم للحذف.");
            }
        }

        private void butnedite_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];

                // التحقق من وجود الأعمدة المطلوبة في DataGridView
                if (dataGridView1.Columns.Contains("UserID") &&
                    dataGridView1.Columns.Contains("Username") &&
                    dataGridView1.Columns.Contains("Password") &&
                    dataGridView1.Columns.Contains("Role") &&
                    dataGridView1.Columns.Contains("Status"))
                {
                    int userID = Convert.ToInt32(selectedRow.Cells["UserID"].Value);
                    string username = selectedRow.Cells["Username"].Value.ToString();
                    string password = selectedRow.Cells["Password"].Value?.ToString() ?? string.Empty; // تأكد من القيمة
                    int role = Convert.ToInt32(selectedRow.Cells["Role"].Value);
                    bool status = Convert.ToBoolean(selectedRow.Cells["Status"].Value);

                    // تمرير البيانات إلى النموذج الجديد
                    Register editForm = new Register(userID, username, password, role, status);
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        LoadUsers(); // تحديث البيانات بعد التعديل
                    }
                }
               
            }
            else
            {
                MessageBox.Show("يرجى تحديد مستخدم للتعديل.");
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            AdminMainForm adminMainForm = new AdminMainForm();
            adminMainForm.Show();
            this.Hide();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
    }

