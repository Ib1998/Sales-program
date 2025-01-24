using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using QRCoder;
using System.Drawing.Printing;
using System.Net.NetworkInformation;
using System.Data.SQLite;
using System.Data.Common;


namespace blueflowr
{
    public partial class AdminMainForm : Form
    {
        static string conn = ConfigurationManager.ConnectionStrings["blueflowersConnectionString"].ConnectionString;
        private string currentInvoiceNumber; // تعريف المتغير على مستوى الصف

        public AdminMainForm()
        {
            InitializeComponent();
        }

        public static class Session
        {
            public static string LoggedInUserName { get; set; } // اسم المستخدم
            public static int LoggedInUserID { get; set; }     // معرف المستخدم
        }
        // طباعة الفاتورة
        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            // إعداد الخطوط
            Font headerFont = new Font("Traditional Arabic", 14, FontStyle.Bold);
            Font regularFont = new Font("Traditional Arabic", 10);
            float pageWidth = e.PageBounds.Width;
            float yPos = 20f;
            StringFormat centerFormat = new StringFormat { Alignment = StringAlignment.Center };

            // رأس الفاتورة
            e.Graphics.DrawString("فاتورة ضريبية مبسطة", headerFont, Brushes.Black, pageWidth / 2, yPos, centerFormat);
            yPos += 30;
            e.Graphics.DrawString("BLUE FLOWERS", headerFont, Brushes.Black, pageWidth / 2, yPos, centerFormat);
            yPos += 30;
            e.Graphics.DrawString(currentInvoiceNumber, regularFont, Brushes.Black, pageWidth / 2, yPos, centerFormat);
            yPos += 20;
            e.Graphics.DrawString("سجل تجاري رقم 5800020762",regularFont,Brushes.Black,pageWidth / 2,yPos, centerFormat);
            yPos += 20;
            e.Graphics.DrawString("محل مالك الزهراني للورود", regularFont, Brushes.Black, pageWidth / 2, yPos, centerFormat);
            yPos += 20;
            e.Graphics.DrawString("جوال المحل /0553060833", regularFont, Brushes.Black,pageWidth /2,yPos,centerFormat);
            yPos += 20;
            e.Graphics.DrawString("الباحه/حي الشفا/طريق الملك فيصل", regularFont, Brushes.Black, pageWidth / 2, yPos, centerFormat); 
            yPos += 20;
            e.Graphics.DrawString("الرقم الضريبي: 310027306400003", regularFont, Brushes.Black, pageWidth / 2, yPos, centerFormat);
            yPos += 30;
            e.Graphics.DrawString("التوقيت: " + DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss tt"), regularFont, Brushes.Black, pageWidth / 2, yPos, centerFormat);
            yPos += 30;

            // تفاصيل الجدول
            e.Graphics.DrawString("اسم المنتج", regularFont, Brushes.Black, 20, yPos);
            e.Graphics.DrawString("سعر الحبة", regularFont, Brushes.Black, 85, yPos);
            e.Graphics.DrawString("الكمية", regularFont, Brushes.Black, 135, yPos);
            e.Graphics.DrawString("الإجمالي", regularFont, Brushes.Black, 185, yPos);
            yPos += 20;

            // طباعة تفاصيل المنتجات
            decimal subtotal = 0;
            decimal taxRate = 0.15m;
            decimal totalTax = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    string productName = row.Cells["ProductName"].Value.ToString();
                    decimal priceWithTax = Convert.ToDecimal(row.Cells["Price"].Value); // سعر المنتج مع الضريبة
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                    // حساب السعر بدون الضريبة
                    decimal priceWithoutTax = priceWithTax / (1 + taxRate);
                    decimal totalProductWithoutTax = priceWithoutTax * quantity; // إجمالي المنتج بدون الضريبة
                    decimal productTax = totalProductWithoutTax * taxRate; // الضريبة على المنتج

                    e.Graphics.DrawString(productName, regularFont, Brushes.Black, 20, yPos);
                    e.Graphics.DrawString(priceWithoutTax.ToString("F2"), regularFont, Brushes.Black, 85, yPos); // عرض سعر الحبة بدون الضريبة
                    e.Graphics.DrawString(quantity.ToString(), regularFont, Brushes.Black, 140, yPos);
                    e.Graphics.DrawString((totalProductWithoutTax + productTax).ToString("F2"), regularFont, Brushes.Black, 185, yPos); // الإجمالي مع الضريبة

                    subtotal += totalProductWithoutTax; // جمع أسعار المنتجات بدون الضريبة
                    totalTax += productTax; // جمع قيمة الضرائب

                    yPos += 20;
                }
            }

            // حساب الإجمالي النهائي
            decimal totalWithTax = subtotal + totalTax;

            yPos += 30;
            e.Graphics.DrawString("الإجمالي الفرعي (بدون الضريبة): " + subtotal.ToString("F2") + " ريال", regularFont, Brushes.Black, 50, yPos);
            yPos += 20;
            e.Graphics.DrawString("قيمة الضريبة المضافة (15%): " + totalTax.ToString("F2") + " ريال", regularFont, Brushes.Black, 50, yPos);
            yPos += 20;
            e.Graphics.DrawString("الإجمالي شامل الضريبة: " + totalWithTax.ToString("F2") + " ريال", headerFont, Brushes.Black, 50, yPos);
            yPos += 40;

            // رمز QR
            string qrData = $"فاتورة رقم: S20240819-13285\nالإجمالي شامل الضريبة: {totalWithTax:F2}\nالتاريخ: {DateTime.Now}";
            Bitmap qrCode = GenerateQRCode(qrData);
            e.Graphics.DrawImage(qrCode, pageWidth / 2 - 50, yPos, 100, 100);
            yPos += 120;

            // رسالة الشكر
            e.Graphics.DrawString("شكراً لتعاملكم معنا!", regularFont, Brushes.Black, pageWidth / 2, yPos, centerFormat);
        }









        private void LoadData()
        {
            string connString = "Data Source=your-database-path;Version=3;";
            using (SQLiteConnection conn = new SQLiteConnection(connString))
            {
                try
                {
                    conn.Open();

                    // استعلام SQL لجلب البيانات
                    string query = "SELECT ProductName, Price, Price * 1.15 AS PriceWithTax, Quantity FROM Products";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // تعيين البيانات إلى DataGridView
                        dataGridView1.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء تحميل البيانات: " + ex.Message);
                }
            }
        }




        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void butnewinvok_Click(object sender, EventArgs e)
        {
            AddInvoice();
            // مسح الأعمدة والصفوف السابقة
            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // إعداد الأعمدة يدويًا
            dataGridView1.Columns.Add("Name", "اسم المنتج");
            dataGridView1.Columns.Add("Price", "السعر");
            dataGridView1.Columns.Add("StockQuantity", "الكمية المتوفرة");
            dataGridView1.Columns.Add("Quantity", "الكمية المطلوبة");
            dataGridView1.Columns.Add("TotalPrice", "المجموع");

            // إضافة صف فارغ لبدء الفاتورة
            dataGridView1.Rows.Add();

            // حساب المجموع الكلي للفاتورة
            CalculateTotal();

            // إعداد DataGridView بعد التحديث
            SetupDataGridView();

            // إضافة الحدث الخاص بتغيير القيم في الخلايا
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;

        }



        private void LoadOrderDetailsWithoutProductID()
        {
            try
            {
                // استعلام بدون ProductID
                string query = @"
            SELECT 
                OrderDetailID,
                OrderID,
                Quantity,
                Price,
                Total,
                ProductName,
                VAT
            FROM 
                [BluFlower].[dbo].[OrderDetails];";

                // إنشاء DataTable لجلب البيانات
                DataTable dtOrderDetails = new DataTable();

                using (SqlConnection connection = new SqlConnection("Server=IB1998MU\\SQLEXPRESS;Database=BluFlower;Trusted_Connection=True;"))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    adapter.Fill(dtOrderDetails);
                }

                // ربط البيانات بـ DataGridView
                dataGridView1.DataSource = dtOrderDetails;

                // تحسين عرض الأعمدة
                CustomizeOrderDetailsGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء جلب البيانات: " + ex.Message);
            }
        }
        private void CustomizeOrderDetailsGrid()
        {
            // تغيير خاصية ReadOnly للأعمدة القابلة للتعديل
            if (dataGridView1.Columns.Contains("Quantity"))
                dataGridView1.Columns["Quantity"].ReadOnly = false; // السماح بتعديل الكمية

            if (dataGridView1.Columns.Contains("Price"))
                dataGridView1.Columns["Price"].ReadOnly = false; // السماح بتعديل السعر

            // بقية الأعمدة تبقى للعرض فقط
            if (dataGridView1.Columns.Contains("ProductName"))
                dataGridView1.Columns["ProductName"].ReadOnly = true; // عرض فقط

            // ضبط عرض الأعمدة تلقائيًا
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void LoadUsers()
        {
            using (SqlConnection connection = new SqlConnection("Server=IB1998MU\\SQLEXPRESS;Database=blueflowers;Trusted_Connection=True;"))
            {
                string query = "SELECT UserID, Username, Password, Role,CreatedDate FROM Users";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable usersTable = new DataTable();
                adapter.Fill(usersTable);

                dataGridView1.DataSource = usersTable;
            }
        }


        private void AdminMainForm_Load(object sender, EventArgs e)
        {


            // استدعاء دالة SetupDataGridView لتكوين الـ DataGridView عند تحميل النموذج
            SetupDataGridView();
            //رقم الفاتورة تم التوليد بنجاح
            GenerateAndDisplayInvoiceNumber();



            lblUserName.Text = $"مرحبًا، {Session.LoggedInUserName}";


        }
        private void CalculateTotal()
        {
            decimal total = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (!row.IsNewRow)
                {
                    decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                    int quantity = Convert.ToInt32(row.Cells["StockQuantity"].Value);

                    total += price * quantity;
                }
            }


        }


        private void SetupDataGridView()
        {
            // مسح الأعمدة الحالية (إذا لزم الأمر)
            dataGridView1.Columns.Clear();

            // إضافة الأعمدة
            //dataGridView1.Columns.Add("ProductID", "معرف المنتج");
            dataGridView1.Columns.Add("ProductName", "اسم المنتج");
            dataGridView1.Columns.Add("Price", "السعر");
            dataGridView1.Columns.Add("Quantity", "الكمية");
            dataGridView1.Columns.Add("VAT", "الضريبة"); // عمود الضريبة
            dataGridView1.Columns.Add("Total", "الإجمالي"); // عمود الإجمالي

            // التأكد أن المستخدم لا يستطيع تعديل عمود الضريبة يدويًا
            dataGridView1.Columns["VAT"].ReadOnly = true;
            dataGridView1.Columns["Total"].ReadOnly = true;
            dataGridView1.CellValueChanged += dataGridView1_CellValueChanged;

        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // التحقق من أن الصف موجود
            {
                var row = dataGridView1.Rows[e.RowIndex];

                if (row.Cells["Price"].Value != null && row.Cells["Quantity"].Value != null)
                {
                    decimal price = Convert.ToDecimal(row.Cells["Price"].Value);
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                    // نسبة الضريبة ثابتة
                    decimal vatRate = 0.15m;
                    decimal vat = price * vatRate;
                    decimal total = (price + vat) * quantity;

                    // تحديث الخلايا
                    row.Cells["VAT"].Value = vat;
                    row.Cells["Total"].Value = total;
                }
            }
        }

        private void butndalate_Click(object sender, EventArgs e)
        {
            // التحقق من وجود صف محدد في DataGridView
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // الحصول على الصف المحدد
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // التحقق من وجود قيمة في الخلايا
                if (selectedRow.Cells["ProductName"].Value != null && selectedRow.Cells["Quantity"].Value != null)
                {
                    // حذف السطر من DataGridView
                    dataGridView1.Rows.Remove(selectedRow);

                    MessageBox.Show("تم الحذف بنجاح!");
                }
                else
                {
                    MessageBox.Show("الرجاء التأكد من أن جميع الحقول تحتوي على قيم.");
                }
            }
            else
            {
                MessageBox.Show("يرجى تحديد منتج للحذف.");
            }

        }

        // دالة لحذف السطر من جدول InvoiceDetails في قاعدة البيانات
        private void DeleteInvoiceDetailFromDatabase(int productId, int quantity)
        {
            // دالة لحذف تفاصيل الفاتورة من جدول InvoiceDetails بناءً على ProductID و Quantity
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                string query = "DELETE FROM InvoiceDetails WHERE ProductID = @ProductID AND Quantity = @Quantity";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@ProductID", productId);
                command.Parameters.AddWithValue("@Quantity", quantity);

                connection.Open();
                command.ExecuteNonQuery();  // تنفيذ عملية الحذف
            }
        }


        private void butnupdate_Click(object sender, EventArgs e)
        {
            // التحقق من وجود صف محدد في DataGridView
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // الحصول على الصف المحدد
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                // التحقق من أن الخلايا تحتوي على بيانات
                if (selectedRow.Cells["ProductName"].Value != null && selectedRow.Cells["Quantity"].Value != null)
                {
                    // الحصول على اسم المنتج من الصف المحدد
                    string productName = selectedRow.Cells["ProductName"].Value.ToString();
                    int quantity = Convert.ToInt32(selectedRow.Cells["Quantity"].Value);

                    // تحديث الكمية في DataGridView
                    selectedRow.Cells["Quantity"].Value = quantity; // تحديث الكمية

                    // استرجاع ProductID باستخدام اسم المنتج
                    int productId = GetProductIdByName(productName);
                    if (productId == 0)
                    {
                        MessageBox.Show($"Product {productName} not found.");
                        return; // إذا لم يتم العثور على المنتج
                    }

                    // استرجاع سعر المنتج
                    decimal productPrice = GetProductPrice(productId);
                    decimal totalPrice = productPrice * quantity;

                    // تحديث إجمالي السعر في DataGridView (إذا كان لديك عمود "Total")
                    selectedRow.Cells["Total"].Value = totalPrice;

                    // رسالة لإعلام المستخدم بالنجاح
                    MessageBox.Show("تم التحديث بنجاح!");
                }
                else
                {
                    MessageBox.Show("الرجاء التأكد من أن جميع الحقول تحتوي على قيم.");
                }
            }
            else
            {
                MessageBox.Show("الرجاء تحديد منتج لتحديثه.");
            }

        }


        private int GetCustomerId(string name, string phone)
        {
            int customerId = 0;

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                string query = "SELECT CustomerID FROM Customers WHERE Name = @Name AND Phone = @Phone";

                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Phone", phone);

                connection.Open();

                var result = command.ExecuteScalar();

                if (result != null)
                {
                    customerId = Convert.ToInt32(result); // تعيين معرف العميل
                }

                connection.Close();
            }
        

            return customerId;
        }
        private void AddCustomerIfNotExist(string name, string phone)
        {
            // الاتصال بقاعدة بيانات SQLite
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                connection.Open();

                // تحقق إذا كان العميل موجودًا في قاعدة البيانات
                string checkQuery = "SELECT COUNT(*) FROM Customers WHERE Name = @Name AND Phone = @Phone";
                SQLiteCommand checkCommand = new SQLiteCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@Name", name);
                checkCommand.Parameters.AddWithValue("@Phone", phone);

                int customerCount = Convert.ToInt32(checkCommand.ExecuteScalar());

                // إذا لم يكن العميل موجودًا، أضفه
                if (customerCount == 0)
                {
                    string insertQuery = "INSERT INTO Customers (Name, Phone) VALUES (@Name, @Phone)";
                    SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@Name", name);
                    insertCommand.Parameters.AddWithValue("@Phone", phone);

                    insertCommand.ExecuteNonQuery();

                    MessageBox.Show("تم إضافة العميل بنجاح!");
                }
                else
                {
                    MessageBox.Show("العميل موجود بالفعل.");
                }

                connection.Close();
            }
        }
        private int GetProductIdByName(string productName)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                string query = "SELECT ProductID FROM Products WHERE Name = @ProductName";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@ProductName", productName);

                connection.Open();
                object result = command.ExecuteScalar();

                return result != null ? Convert.ToInt32(result) : 0; // إذا لم يتم العثور على المنتج، إرجاع 0
            }

        }

        private decimal GetProductPrice(int productId)
        {

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                string query = "SELECT Price FROM Products WHERE ProductID = @ProductID";
                SQLiteCommand command = new SQLiteCommand(query, connection);
                command.Parameters.AddWithValue("@ProductID", productId);

                connection.Open();
                object result = command.ExecuteScalar();

                return result != null ? Convert.ToDecimal(result) : 0;
            }
        }

        private void InsertInvoiceDetails(int invoiceId)
        {
            using (SqlConnection connection = new SqlConnection("Server=IB1998MU\\SQLEXPRESS;Database=blueflowers;Trusted_Connection=True;"))
            {
                connection.Open();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["ProductName"].Value != null && row.Cells["Quantity"].Value != null)
                    {
                        string productName = row.Cells["ProductName"].Value.ToString();
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                        // استرجاع ProductID باستخدام اسم المنتج
                        int productId = GetProductIdByName(productName);

                        if (productId == 0)
                        {
                            MessageBox.Show($"Product {productName} not found.");
                            continue; // الانتقال إلى الصف التالي إذا لم يتم العثور على المنتج
                        }

                        // استرجاع سعر المنتج
                        decimal productPrice = GetProductPrice(productId);

                        // حساب السعر الكلي
                        decimal totalPrice = productPrice * quantity;

                        // إدخال تفاصيل الفاتورة
                        string invoiceDetailQuery = "INSERT INTO InvoiceDetails (InvoiceID, ProductID, Quantity, UnitPrice, TotalPrice) " +
                                                    "VALUES (@InvoiceID, @ProductID, @Quantity, @UnitPrice, @TotalPrice)";

                        SqlCommand invoiceDetailCommand = new SqlCommand(invoiceDetailQuery, connection);
                        invoiceDetailCommand.Parameters.AddWithValue("@InvoiceID", invoiceId); // معرف الفاتورة
                        invoiceDetailCommand.Parameters.AddWithValue("@ProductID", productId); // معرف المنتج
                        invoiceDetailCommand.Parameters.AddWithValue("@Quantity", quantity); // الكمية
                        invoiceDetailCommand.Parameters.AddWithValue("@UnitPrice", productPrice); // سعر الوحدة
                        invoiceDetailCommand.Parameters.AddWithValue("@TotalPrice", totalPrice); // السعر الكلي

                        invoiceDetailCommand.ExecuteNonQuery();
                    }
                }

                connection.Close();
            }
        }

        private void SaveSaleDetails(int saleId)
        {
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                connection.Open();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["ProductName"].Value != null && row.Cells["Quantity"].Value != null)
                    {
                        string productName = row.Cells["ProductName"].Value.ToString();
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                        // استرجاع ProductID باستخدام اسم المنتج
                        int productId = GetProductIdByName(productName);

                        if (productId != 0)
                        {
                            // استرجاع سعر المنتج (UnitPrice)
                            decimal productPrice = GetProductPrice(productId);

                            // تحقق من أن السعر صالح وليس NULL
                            if (productPrice <= 0)
                            {
                                MessageBox.Show($"سعر المنتج {productName} غير صالح.");
                                continue; // الانتقال إلى الصف التالي إذا كان السعر غير صالح
                            }

                            decimal totalPrice = productPrice * quantity;

                            // إدخال تفاصيل المبيعات في جدول SaleDetails
                            string saleDetailsQuery = "INSERT INTO SaleDetails (SaleID, ProductID, Quantity, UnitPrice, TotalPrice) " +
                                                      "VALUES (@SaleID, @ProductID, @Quantity, @UnitPrice, @TotalPrice)";
                            SQLiteCommand saleDetailsCommand = new SQLiteCommand(saleDetailsQuery, connection);
                            saleDetailsCommand.Parameters.AddWithValue("@SaleID", saleId);
                            saleDetailsCommand.Parameters.AddWithValue("@ProductID", productId);
                            saleDetailsCommand.Parameters.AddWithValue("@Quantity", quantity);
                            saleDetailsCommand.Parameters.AddWithValue("@UnitPrice", productPrice);
                            saleDetailsCommand.Parameters.AddWithValue("@TotalPrice", totalPrice);

                            saleDetailsCommand.ExecuteNonQuery();
                        }
                        else
                        {
                            MessageBox.Show($"المنتج {productName} غير موجود في قاعدة البيانات.");
                        }
                    }
                }

                connection.Close();
            }
        }




        private void butnsave_Click(object sender, EventArgs e)
        {




            string customerName = txtCustomerName.Text; // اسم العميل من TextBox
            string customerPhone = txtCustomerPhone.Text; // رقم هاتف العميل من TextBox
            string invoiceNumber = txtInvoiceID.Text; // رقم الفاتورة من TextBox (يتم توليده تلقائيًا)

            // التحقق من أن البيانات الأساسية موجودة
            if (string.IsNullOrWhiteSpace(customerName) || string.IsNullOrWhiteSpace(customerPhone) || string.IsNullOrWhiteSpace(invoiceNumber))
            {
                MessageBox.Show("يرجى ملء جميع الحقول الأساسية.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // تأكد من أن العميل موجود أو إضافته إذا لم يكن موجودًا
            AddCustomerIfNotExist(customerName, customerPhone);

            // استخراج CustomerID بناءً على الاسم ورقم الهاتف
            int selectedCustomerId = GetCustomerId(customerName, customerPhone);

            if (selectedCustomerId == 0)
            {
                MessageBox.Show("لم يتم العثور على العميل.");
                return; // إذا لم يتم العثور على العميل بعد إضافته
            }

            using (SQLiteConnection connection = new SQLiteConnection("Data Source=blueflowers.db;Version=3;"))
            {
                connection.Open();

                // حساب المجموع
                decimal totalAmount = 0;
                decimal taxAmount = 0;
                decimal discountAmount = 0;

                // افترض أن لديك DataGridView لعرض تفاصيل الفاتورة مثل المنتجات
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["ProductName"].Value != null && row.Cells["Quantity"].Value != null)
                    {
                        string productName = row.Cells["ProductName"].Value.ToString();
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                        // استرجاع ProductID باستخدام اسم المنتج
                        int productId = GetProductIdByName(productName);

                        if (productId == 0)
                        {
                            MessageBox.Show($"المنتج {productName} غير موجود.");
                            continue; // الانتقال إلى الصف التالي إذا لم يتم العثور على المنتج
                        }

                        // استرجاع سعر المنتج
                        decimal productPrice = GetProductPrice(productId);

                        // حساب إجمالي السعر للمنتج
                        decimal totalPrice = productPrice * quantity;
                        totalAmount += totalPrice; // إضافة إجمالي المنتج إلى المجموع
                    }
                }

                // حساب الضريبة والخصم بعد إضافة كافة المنتجات
                taxAmount = totalAmount * 0.15m; // افتراض ضريبة بنسبة 15%
                discountAmount = totalAmount * 0.10m; // افتراض خصم بنسبة 10%

                // حساب الإجمالي النهائي بعد الضريبة والخصم
                decimal finalAmount = totalAmount + taxAmount - discountAmount;

                // إدخال الطلب أولاً في جدول Sales
                string salesQuery = "INSERT INTO Sales (InvoiceNumber, SaleDate, CustomerID, TotalAmount, Tax, Discount, FinalAmount, PaymentStatus) " +
                                    "VALUES (@InvoiceNumber, @SaleDate, @CustomerID, @TotalAmount, @Tax, @Discount, @FinalAmount, @PaymentStatus);" +
                                    "SELECT LAST_INSERT_ROWID();"; // للحصول على SaleID بعد الإدخال

                SQLiteCommand salesCommand = new SQLiteCommand(salesQuery, connection);
                salesCommand.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                salesCommand.Parameters.AddWithValue("@SaleDate", DateTime.Now); // تاريخ البيع
                salesCommand.Parameters.AddWithValue("@CustomerID", selectedCustomerId);
                salesCommand.Parameters.AddWithValue("@TotalAmount", totalAmount); // إجمالي المبلغ
                salesCommand.Parameters.AddWithValue("@Tax", taxAmount); // قيمة الضريبة
                salesCommand.Parameters.AddWithValue("@Discount", discountAmount); // قيمة الخصم
                salesCommand.Parameters.AddWithValue("@FinalAmount", finalAmount); // الإجمالي النهائي
                salesCommand.Parameters.AddWithValue("@PaymentStatus", "Unpaid"); // حالة الدفع (افتراض "غير مدفوع")

                int saleId = Convert.ToInt32(salesCommand.ExecuteScalar());

                // إدخال تفاصيل الفاتورة في جدول SaleDetails
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.Cells["ProductName"].Value != null && row.Cells["Quantity"].Value != null)
                    {
                        string productName = row.Cells["ProductName"].Value.ToString();
                        int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                        // استرجاع ProductID باستخدام اسم المنتج
                        int productId = GetProductIdByName(productName);

                        if (productId != 0)
                        {
                            decimal productPrice = GetProductPrice(productId);
                            decimal totalPrice = productPrice * quantity;

                            // إضافة UnitPrice إلى الاستعلام إذا كان مطلوبًا في جدول SaleDetails
                            string saleDetailsQuery = "INSERT INTO SaleDetails (SaleID, ProductID, Quantity, TotalPrice, UnitPrice) " +
                                                      "VALUES (@SaleID, @ProductID, @Quantity, @TotalPrice, @UnitPrice)";

                            SQLiteCommand saleDetailsCommand = new SQLiteCommand(saleDetailsQuery, connection);
                            saleDetailsCommand.Parameters.AddWithValue("@SaleID", saleId);
                            saleDetailsCommand.Parameters.AddWithValue("@ProductID", productId);
                            saleDetailsCommand.Parameters.AddWithValue("@Quantity", quantity);
                            saleDetailsCommand.Parameters.AddWithValue("@TotalPrice", totalPrice);
                            saleDetailsCommand.Parameters.AddWithValue("@UnitPrice", productPrice); // إضافة سعر الوحدة للمنتج

                            saleDetailsCommand.ExecuteNonQuery(); // تنفيذ الاستعلام لإدخال التفاصيل في SaleDetails
                        }
                    }
                }

                MessageBox.Show("تم حفظ الفاتورة بنجاح!", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                connection.Close();
            }
        }

         

        




        private void btnprint_Click(object sender, EventArgs e)
        {
            // إنشاء كائن PrintDocument
            PrintDocument printDocument = new PrintDocument();

            // ضبط حجم الورقة
            PaperSize paperSize = new PaperSize("Custom", 280, 600); // العرض 80 مم والطول 60 سم
            printDocument.DefaultPageSettings.PaperSize = paperSize;

            // ربط الحدث PrintPage
            printDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);

            // فتح نافذة معاينة الطباعة (اختياري)
            PrintPreviewDialog previewDialog = new PrintPreviewDialog();
            previewDialog.Document = printDocument;

            if (previewDialog.ShowDialog() == DialogResult.OK)
            {
                // تنفيذ الطباعة
                printDocument.Print();
            }
        }
        private Bitmap GenerateQRCode(string data)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            return qrCodeImage;
        }

        private void butnaddeduser_Click(object sender, EventArgs e)
        {
            //انشاء مستخدم جديد 
            Register regs = new Register();
            regs.Show();
            this.Hide();

        }

        private void butnaddproduct_Click(object sender, EventArgs e)
        {
            addproduct addproduct = new addproduct();
            addproduct.Show();
            this.Hide();
        }

        private void lblUserName_Click(object sender, EventArgs e)
        {

        }



        //استرداد الفاتورة بواسطة رقم الفاتورة
        private void button3_Click(object sender, EventArgs e)
        {
            retuninvok retuni = new retuninvok();

            retuni.ShowDialog();


            //// التحقق من أن المستخدم أدخل رقم الفاتورة
            //if (string.IsNullOrWhiteSpace(txtInvoiceID.Text))
            //{
            //    MessageBox.Show("يرجى إدخال رقم الفاتورة.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}

            //// استرجاع التفاصيل باستخدام رقم الفاتورة
            //string invoiceNumber = txtInvoiceID.Text;
            //string result = GetInvoice(invoiceNumber);

            //// عرض التفاصيل في مربع نص آخر أو رسالة
            //MessageBox.Show(result, "تفاصيل الفاتورة", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //// تفريغ مربع النص لإدخال رقم جديد
            //txtInvoiceID.Text = string.Empty;


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

      
        

       
           private void btnCancelInvoice_Click(object sender, EventArgs e)
        {
            // تأكيد إلغاء الفاتورة
            var confirmation = MessageBox.Show("هل تريد الغاء الفاتورة?", "الغاء", MessageBoxButtons.YesNo);

            if (confirmation == DialogResult.Yes)
            {
                try
                {
                    // 1. إرجاع المنتجات للمخزون: نقوم بتعديل الكميات في DataGridView
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["ProductName"].Value != null)
                        {
                            // إعادة الكميات إلى المخزون، إذا كان هناك تعديل على المخزون
                            int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                            // على سبيل المثال، يمكن إضافة الكمية مرة أخرى للمخزون
                            // (يمكن إضافة كود خاص بالـ "إرجاع" إلى المخزون هنا حسب متطلباتك)
                            row.Cells["Quantity"].Value = 0; // إعادة الكمية إلى صفر (إلغاء الفاتورة)
                        }
                    }

                    // 2. إلغاء الحسابات والإجمالي: إعادة تعيين الإجمالي في DataGridView
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (row.Cells["Total"].Value != null)
                        {
                            // تعيين الإجمالي إلى 0 لأن الفاتورة قد تم إلغاؤها
                            row.Cells["Total"].Value = 0;
                        }
                    }

                    // 3. إزالة الفاتورة من DataGridView أو إلغاء جميع البيانات
                    dataGridView1.Rows.Clear(); // مسح جميع البيانات في DataGridView

                    // 4. إظهار إشعار تأكيد
                    MessageBox.Show(".تم إلغاء الفاتورة بنجاح ");
                }
                catch (Exception ex)
                {
                    // في حال حدوث خطأ
                    MessageBox.Show("حدث خطأ أثناء إلغاء الفاتورة " + ex.Message);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string columnNames = "أسماء الأعمدة الموجودة في DataGridView:\n";
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                columnNames += column.Name + "\n";
            }
            MessageBox.Show(columnNames, "أسماء الأعمدة");

        }



        // InvoiceDetails في قاعدة البيانات
        private string connectionString = "Data Source=blueflowers.db;Version=3;";

        public string AddInvoice()
        {
            string invoiceNumber = GenerateInvoiceNumber();

            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string insertQuery = "INSERT INTO Invoices (InvoiceNumber, DateCreated) VALUES (@InvoiceNumber, @DateCreated)";
                SQLiteCommand cmd = new SQLiteCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
            }
            return invoiceNumber;
        }

        // استرجاع فاتورة باستخدام رقم الفاتورة
        public string GetInvoice(string invoiceNumber)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionString))
            {
                conn.Open();
                string selectQuery = "SELECT * FROM Invoices WHERE InvoiceNumber = @InvoiceNumber";
                SQLiteCommand cmd = new SQLiteCommand(selectQuery, conn);
                cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

                using (SQLiteDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return $"رقم الفاتورة: {reader["InvoiceNumber"]}, التاريخ: {reader["DateCreated"]}";
                    }
                    else
                    {
                        return "الفاتورة غير موجودة.";
                    }
                }
            }
        }

        // توليد رقم فاتورة فريد
        private string GenerateInvoiceNumber()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd"); // جزء التاريخ
            string randomPart = new Random().Next(1000, 9999).ToString(); // رقم عشوائي
            return $"S{datePart}-{randomPart}";
            
        }
        private void GenerateAndDisplayInvoiceNumber()
        {
            // توليد رقم الفاتورة
            string invoiceNumber = GenerateInvoiceNumber();
            // توليد رقم في الطباعةالفاتورة
            currentInvoiceNumber = GenerateInvoiceNumber();
            // عرض الرقم في مربع النص
            txtInvoiceID.Text = invoiceNumber;
        }

        private void txtInvoiceID_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
    









