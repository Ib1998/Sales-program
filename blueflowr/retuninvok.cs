using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace blueflowr
{
    public partial class retuninvok : Form
    {

        private string connectionString = "Data Source=blueflowers.db;Version=3;";

        public retuninvok()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void retuninvok_Load(object sender, EventArgs e)
        {

        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // التحقق من إدخال المستخدم
            if (string.IsNullOrWhiteSpace(txtInvoiceNumber.Text))
            {
                MessageBox.Show("يرجى إدخال رقم الفاتورة.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // جلب التفاصيل باستخدام الدالة
            string invoiceNumber = txtInvoiceNumber.Text;
            FetchSalesDetails(invoiceNumber, dataGridViewInvoiceDetails);

            // تفريغ الحقل النصي
            txtInvoiceNumber.Clear();





            //// التحقق من إدخال رقم الفاتورة
            //if (string.IsNullOrWhiteSpace(txtInvoiceNumber.Text))
            //{
            //    MessageBox.Show("يرجى إدخال رقم الفاتورة.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    return;
            //}
            //    // جلب التفاصيل من قاعدة البيانات
            //    string invoiceNumber = txtInvoiceNumber.Text;
            //    DataTable invoiceDetails = GetInvoiceDetails(invoiceNumber);

            //    // التحقق من وجود بيانات
            //    if (invoiceDetails.Rows.Count > 0)
            //    {
            //        dataGridViewInvoiceDetails.DataSource = invoiceDetails; // عرض التفاصيل
            //    }
            //    else
            //    {
            //        MessageBox.Show("الفاتورة غير موجودة أو لا تحتوي على تفاصيل.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
        }
        public void FetchSalesDetails(string invoiceNumber, DataGridView dataGridView)
        {
            string connectionString = "Data Source=blueflowers.db;Version=3;";

            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT 
                    SaleID, 
                    SaleDate, 
                    TotalAmount, 
                    Tax, 
                    Discount, 
                    PaymentStatus, 
                    CustomerID, 
                    FinalAmount
                FROM 
                    Sales
                WHERE 
                    InvoiceNumber = @InvoiceNumber;
            ";

                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);

                        DataTable dataTable = new DataTable();
                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }

                        if (dataTable.Rows.Count > 0)
                        {
                            // عرض البيانات في DataGridView
                            dataGridView.DataSource = dataTable;
                        }
                        else
                        {
                            MessageBox.Show("لم يتم العثور على بيانات لهذه الفاتورة.", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء جلب البيانات: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
            




       
            

        









        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnRefundInvoice_Click(object sender, EventArgs e)
        {
            // التحقق من إدخال رقم الفاتورة
            if (string.IsNullOrWhiteSpace(txtInvoiceNumber.Text))
            {
                MessageBox.Show("يرجى إدخال رقم الفاتورة.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // رقم الفاتورة المدخل
            string invoiceNumber = txtInvoiceNumber.Text;

            // تأكيد رغبة المستخدم في استرداد الفاتورة
            DialogResult dialogResult = MessageBox.Show("هل تريد استرداد الفاتورة؟ سيتم حذفها وإرجاع الأموال للعميل.",
                                                        "تأكيد الاسترداد",
                                                        MessageBoxButtons.YesNo,
                                                        MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    string connectionString = "Data Source=blueflowers.db;Version=3;";
                    using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                    {
                        conn.Open();

                        // جلب مبلغ الفاتورة لإرجاع الأموال
                        string getInvoiceQuery = @"
                    SELECT FinalAmount, CustomerID 
                    FROM Sales 
                    WHERE InvoiceNumber = @InvoiceNumber;
                ";

                        decimal finalAmount = 0;
                        int customerId = 0;

                        using (SQLiteCommand cmd = new SQLiteCommand(getInvoiceQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                            using (SQLiteDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    finalAmount = Convert.ToDecimal(reader["FinalAmount"]);
                                    customerId = Convert.ToInt32(reader["CustomerID"]);
                                }
                                else
                                {
                                    MessageBox.Show("لم يتم العثور على الفاتورة.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return;
                                }
                            }
                        }

                        // تسجيل عملية الإرجاع في جدول آخر (اختياري)
                        string insertRefundQuery = @"
                    INSERT INTO Refunds (InvoiceNumber, CustomerID, RefundAmount, RefundDate) 
                    VALUES (@InvoiceNumber, @CustomerID, @RefundAmount, @RefundDate);
                ";

                        using (SQLiteCommand cmd = new SQLiteCommand(insertRefundQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                            cmd.Parameters.AddWithValue("@CustomerID", customerId);
                            cmd.Parameters.AddWithValue("@RefundAmount", finalAmount);
                            cmd.Parameters.AddWithValue("@RefundDate", DateTime.Now);
                            cmd.ExecuteNonQuery();
                        }

                        // حذف الفاتورة من جدول المبيعات
                        string deleteInvoiceQuery = @"
                    DELETE FROM Sales 
                    WHERE InvoiceNumber = @InvoiceNumber;
                ";

                        using (SQLiteCommand cmd = new SQLiteCommand(deleteInvoiceQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@InvoiceNumber", invoiceNumber);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show($"تم استرداد مبلغ {finalAmount} للعميل وحذف الفاتورة بنجاح.",
                                        "نجاح",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"حدث خطأ أثناء استرداد الفاتورة: {ex.Message}", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("تم إلغاء عملية الاسترداد.", "إلغاء", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // تفريغ مربع النص بعد العملية
            txtInvoiceNumber.Clear();
        }
    }
}
