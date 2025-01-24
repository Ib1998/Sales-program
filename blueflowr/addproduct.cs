using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace blueflowr
{
    public partial class addproduct : Form
    {

        private void LoadData()
        {
            using (SqlConnection connection = new SqlConnection("Server=IB1998MU\\SQLEXPRESS;Database=blueflowers;Trusted_Connection=True;"))

                try
                {
                    string query = "SELECT ProductID, Name, Description, Price, Stock FROM Products";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // عرض البيانات في DataGridView
                    dataGridViewProducts.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء تحميل البيانات: " + ex.Message);
                }
        }

        private void ProductsForm_Load(object sender, EventArgs e)
        {
            LoadProducts();
        }
        private void AddProduct(string Name, string description, decimal price, string categoryName, int stockQuantity)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                try
                {
                    connection.Open();

                    // إضافة المنتج إلى جدول Products
                    string queryProduct = "INSERT INTO Products (Name, Description, Price, Category, Stock) " +
                                          "VALUES (@Name, @Description, @Price, @Category, @Stock)";
                    using (SQLiteCommand productCommand = new SQLiteCommand(queryProduct, connection))
                    {
                        productCommand.Parameters.AddWithValue("@Name", Name);
                        productCommand.Parameters.AddWithValue("@Description", description);
                        productCommand.Parameters.AddWithValue("@Price", price);
                        productCommand.Parameters.AddWithValue("@Category", categoryName); // هنا نمرر اسم الفئة مباشرة
                        productCommand.Parameters.AddWithValue("@Stock", stockQuantity);

                        productCommand.ExecuteNonQuery();
                        MessageBox.Show("تم إضافة المنتج بنجاح!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ: " + ex.Message);
                }
            }
        }
        private int selectedProductId; // تعريف المتغير

        public addproduct()
        {
            InitializeComponent();
        }

        private void addproduct_Load(object sender, EventArgs e)
        {
            // الاتصال بقاعدة البيانات وجلب البيانات
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                string query = "SELECT ProductID, Name, Description, Price, Stock, Category FROM Products";

                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                DataTable productsTable = new DataTable();
                adapter.Fill(productsTable);

                // تعيين مصدر البيانات لـ DataGridView
                dataGridViewProducts.DataSource = productsTable;


                // تغيير أسماء الأعمدة إلى اللغة العربية
                dataGridViewProducts.Columns["ProductID"].HeaderText = "معرف المنتج";
                dataGridViewProducts.Columns["Name"].HeaderText = "اسم المنتج";
                dataGridViewProducts.Columns["Description"].HeaderText = "وصف المنتج";
                dataGridViewProducts.Columns["Price"].HeaderText = "السعر";
                dataGridViewProducts.Columns["Stock"].HeaderText = "المخزون";
                dataGridViewProducts.Columns["Category"].HeaderText = "الفئة";
            }
        }
        
        private void LoadProducts()
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                connection.Open();

                // استعلام لجلب المنتجات مع الفئة من نفس الجدول
                string query = "SELECT ProductID, Name, Description, Price, Stock, Category FROM Products";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                DataTable productsTable = new DataTable();
                adapter.Fill(productsTable);

                // تعيين البيانات إلى DataGridView
                dataGridViewProducts.DataSource = productsTable;
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            // بيانات المنتج التي سيتم إدخالها
            string name = txtName.Text;
            string description = txtDescription.Text;
            decimal price;
            int stock;
            string category = textcat.Text;

            // التحقق من أن السعر والمخزون أرقام صحيحة
            if (decimal.TryParse(txtPrice.Text, out price) && int.TryParse(txtStockQuantity.Text, out stock))
            {
                // إضافة المنتج إلى قاعدة البيانات
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
                {
                    string query = "INSERT INTO Products (Name, Description, Price, Stock, Category) " +
                                   "VALUES (@Name, @Description, @Price, @Stock, @Category)";

                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@Stock", stock);
                    command.Parameters.AddWithValue("@Category", category);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    // تحقق من ما إذا كانت عملية الإدخال ناجحة
                    if (result > 0)
                    {
                        MessageBox.Show("تم إضافة المنتج بنجاح!");
                        btnRefresh.PerformClick(); // تنفيذ ضغط الزر التحديث تلقائيًا بعد الإضافة
                    }
                    else
                    {
                        MessageBox.Show("فشل إضافة المنتج.");
                    }
                }
            }
            else
            {
                MessageBox.Show("الرجاء إدخال قيم صحيحة للسعر والمخزون.");
            }

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label8_Click(object sender, EventArgs e)
        {

        }
        //الرجوع للقائمة الرئيسية
        private void button1_Click(object sender, EventArgs e)
        {
            AdminMainForm admina = new AdminMainForm();
            admina.Show();
            this.Hide();
        }

        

        private void btnEditProduct_Click(object sender, EventArgs e)
        {
            // التحقق من وجود صف محدد في DataGridView
            if (dataGridViewProducts.SelectedRows.Count > 0)
            {
                // الحصول على معرف المنتج المحدد
                int productId = Convert.ToInt32(dataGridViewProducts.SelectedRows[0].Cells["ProductID"].Value);

                // جلب بيانات المنتج المحدد إلى مربعات النص
                txtName.Text = dataGridViewProducts.SelectedRows[0].Cells["Name"].Value.ToString();
                txtDescription.Text = dataGridViewProducts.SelectedRows[0].Cells["Description"].Value.ToString();
                txtPrice.Text = dataGridViewProducts.SelectedRows[0].Cells["Price"].Value.ToString();
                txtStockQuantity.Text = dataGridViewProducts.SelectedRows[0].Cells["Stock"].Value.ToString();
                textcat.Text = dataGridViewProducts.SelectedRows[0].Cells["Category"].Value.ToString();

                // تغيير النص في الزر إلى "Save Changes"
                btnEditProduct.Text = "حفظ التعديلات";

                // تعديل الحدث للزر ليكون حفظ التعديلات بدلاً من التعديل
                btnEditProduct.Click -= btnEditProduct_Click;  // إزالة الحدث الحالي
                btnEditProduct.Click += new EventHandler(btnSaveProduct_Click);  // إضافة الحدث لحفظ التعديلات
            }
            else
            {
                MessageBox.Show("يرجى تحديد منتج لتعديله.");
            }
        }

        private void btnSaveProduct_Click(object sender, EventArgs e)
        {

            // الحصول على البيانات المعدلة من المربعات النصية
            string name = txtName.Text;
            string description = txtDescription.Text;
            decimal price;
            int stock;
            string category = textcat.Text;

            // التحقق من أن السعر والمخزون أرقام صحيحة
            if (decimal.TryParse(txtPrice.Text, out price) && int.TryParse(txtStockQuantity.Text, out stock))
            {
                // الحصول على معرف المنتج المحدد
                int productId = Convert.ToInt32(dataGridViewProducts.SelectedRows[0].Cells["ProductID"].Value);

                // الاتصال بقاعدة البيانات لحفظ التعديلات
                using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
                {
                    // استعلام تحديث بيانات المنتج
                    string query = "UPDATE Products SET Name = @Name, Description = @Description, Price = @Price, Stock = @Stock, Category = @Category WHERE ProductID = @ProductID";

                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@Stock", stock);
                    command.Parameters.AddWithValue("@Category", category);
                    command.Parameters.AddWithValue("@ProductID", productId); // تحديث المنتج بناءً على معرفه

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    // إذا كانت عملية التحديث ناجحة
                    if (result > 0)
                    {
                        MessageBox.Show("تم تحديث المنتج بنجاح!");
                        btnRefresh.PerformClick(); // تحديث البيانات في DataGridView بعد التعديل
                        btnEditProduct.Text = "تعديل المنتج"; // إعادة النص في الزر إلى "Edit Product"
                        btnEditProduct.Click -= btnSaveProduct_Click; // إزالة الحدث الحالي
                        btnEditProduct.Click += new EventHandler(btnEditProduct_Click); // إعادة الحدث السابق
                    }
                    else
                    {
                        MessageBox.Show("فشل في تحديث المنتج.");
                    }
                }
            }
            else
            {
                MessageBox.Show("الرجاء إدخال السعر والمخزون بشكل صحيح.");
            }
        }

        private void btnDeleteProduct_Click(object sender, EventArgs e)
        {
            // التحقق من وجود صف محدد في DataGridView
            if (dataGridViewProducts.SelectedRows.Count > 0)
            {
                // الحصول على معرف المنتج المحدد
                int productId = Convert.ToInt32(dataGridViewProducts.SelectedRows[0].Cells["ProductID"].Value);

                // تأكيد الحذف
                DialogResult result = MessageBox.Show("هل أنت متأكد من الحذف؟", "حذف المنتج", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // الاتصال بقاعدة البيانات لحذف المنتج
                    using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
                    {
                        string query = "DELETE FROM Products WHERE ProductID = @ProductID";
                        SQLiteCommand command = new SQLiteCommand(query, connection);
                        command.Parameters.AddWithValue("@ProductID", productId);

                        connection.Open();
                        int resultDelete = command.ExecuteNonQuery();

                        if (resultDelete > 0)
                        {
                            MessageBox.Show("تم الحذف بنجاح");
                            btnRefresh.PerformClick(); // تحديث البيانات في DataGridView بعد الحذف
                        }
                        else
                        {
                            MessageBox.Show("فشل الحذف");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("يرجى تحديد منتج للحذف.");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            AdminMainForm admina = new AdminMainForm();
            admina.Show();
            this.Hide();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // الاتصال بقاعدة البيانات وإعادة تحميل البيانات
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                string query = "SELECT ProductID, Name, Description, Price, Stock, Category FROM Products";

                // استخدام SQLiteDataAdapter لتحميل البيانات
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                DataTable productsTable = new DataTable();
                adapter.Fill(productsTable);

                // تعيين البيانات المحملة إلى DataGridView
                dataGridViewProducts.DataSource = productsTable;
            
             }
        }
        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
