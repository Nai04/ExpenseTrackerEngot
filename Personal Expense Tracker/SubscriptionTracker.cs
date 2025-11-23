using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Personal_Expense_Tracker
{
    
    public partial class SubscriptionTracker : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int nLeft,
            int nTop,
            int nRigth,
            int nBottom,
            int nWidthEllipse,
            int nHeightEllipse
            );
        SqlConnection con = new SqlConnection(@"Server=IANPC;Database=ExpenseTracker;Integrated Security=True;");

        public SubscriptionTracker()
        {
            InitializeComponent();
        }

        private void SubscriptionTracker_Load(object sender, EventArgs e)
        {
            addButton.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, addButton.Width, addButton.Height, 30, 30));
            deleteButton.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, deleteButton.Width, deleteButton.Height, 30, 30));
            updateButton.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, updateButton.Width, updateButton.Height, 30, 30));
            exitButton.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, exitButton.Width, exitButton.Height, 30, 30));
            dataGridView1.ColumnCount = 2;
            dataGridView1.Columns[0].Name = "Subscription";
            dataGridView1.Columns[1].Name = "Amount";

            LoadSubscriptions();
            UpdateTotalAmount();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(textBox2.Text, out double amount))
                {
                    throw new Exception("Amount must be a valid number (decimal/double).");
                }

                // Save to database
                string query = "INSERT INTO Subscriptions (SubscriptionName, Amount) VALUES (@Name, @Amount)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Amount", amount);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                // Add to DataGridView
                dataGridView1.Rows.Add(textBox1.Text, amount.ToString("F2"));

                MessageBox.Show("Successfully Added to Database");
                ClearFields();
                UpdateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                try
                {
                    if (!double.TryParse(textBox2.Text, out double amount))
                    {
                        throw new Exception("Amount must be a valid number.");
                    }

                    // Get the selected row
                    DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                    string subName = selectedRow.Cells[0].Value.ToString(); // Keep subscription name unchanged

                    // Update only the Amount in the database
                    string query = "UPDATE Subscriptions SET Amount=@Amount WHERE SubscriptionName=@Name";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@Name", subName);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                    // Update only the Amount in the DataGridView
                    selectedRow.Cells[1].Value = amount.ToString("F2");

                    MessageBox.Show("Amount Successfully Updated in Database and Grid");
                    ClearFields();
                    UpdateTotalAmount();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    if (con.State == System.Data.ConnectionState.Open)
                        con.Close();
                }
            }
            else
            {
                MessageBox.Show("Please select a row to update.");
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string subName = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();

                // Delete from database
                string query = "DELETE FROM Subscriptions WHERE SubscriptionName=@Name";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", subName);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                // Remove from DataGridView
                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);

                MessageBox.Show("Successfully Deleted from Database");
                ClearFields();
                UpdateTotalAmount();
            }
            else
            {
                MessageBox.Show("Please select a row to delete.");
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
            Form expenseT = new ExpenseTracker();
            expenseT.Show();
        }

        private void ClearFields()
        {
            textBox1.Clear();
            textBox2.Clear();
            dataGridView1.ClearSelection();
        }

        private void UpdateTotalAmount()
        {
            double total = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[1].Value != null && double.TryParse(row.Cells[1].Value.ToString(), out double amount))
                {
                    total += amount;
                }
            }

            TotalAmount.Text = "₱" + total.ToString("F2");
        }

        private void LoadSubscriptions()
        {
            try
            {
                string query = "SELECT SubscriptionName, Amount FROM Subscriptions";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        dataGridView1.Rows.Add(reader["SubscriptionName"].ToString(),
                                               Convert.ToDecimal(reader["Amount"]).ToString("F2"));
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading subscriptions: " + ex.Message);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }
    }

    public class Subscribe
    {
        public string SubscribeName { get; set; }
        public double Amount { get; set; }

        public Subscribe(string subName, double amount)
        {
            SubscribeName = subName;
            Amount = amount;
        }
    }
}
