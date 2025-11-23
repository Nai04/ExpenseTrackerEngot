using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Personal_Expense_Tracker
{
    public partial class Tracker : Form
    {
        SqlConnection con = new SqlConnection(@"Server=IANPC;Database=ExpenseTracker;Integrated Security=True;");

        public Tracker()
        {
            InitializeComponent();
        }

        private void Tracker_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(new string[] {
                "Cash", "Online Payment", "Card", "Others (Please type it in the box)"
            });

            comboBox2.Items.AddRange(new string[] {
                "Food", "Social Life", "Pets", "Transport",
                "Beauty", "Health", "Gift", "Education", "Others (Please type it in the box)"
            });

            dataGridView1.ColumnCount = 5;
            dataGridView1.Columns[0].Name = "Date";
            dataGridView1.Columns[1].Name = "Account";
            dataGridView1.Columns[2].Name = "Category";
            dataGridView1.Columns[3].Name = "Amount";
            dataGridView1.Columns[4].Name = "Note";

            LoadExpenses();
            UpdateTotalAmount();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!double.TryParse(textBox2.Text, out double amount))
                {
                    throw new Exception("Amount must be a valid number.");
                }

                string query = "INSERT INTO Expenses (Date, Account, Category, Amount, Note) VALUES (@Date, @Account, @Category, @Amount, @Note)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Date", dateTimePicker1.Value);
                    cmd.Parameters.AddWithValue("@Account", comboBox1.Text);
                    cmd.Parameters.AddWithValue("@Category", comboBox2.Text);
                    cmd.Parameters.AddWithValue("@Amount", amount);
                    cmd.Parameters.AddWithValue("@Note", textBox3.Text);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                dataGridView1.Rows.Add(
                    dateTimePicker1.Value.ToShortDateString(),
                    comboBox1.Text,
                    comboBox2.Text,
                    amount.ToString("F2"),
                    textBox3.Text
                );

                MessageBox.Show("Successfully Added to Database");
                ClearFields();
                UpdateTotalAmount();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }

        private void LoadExpenses()
        {
            try
            {
                string query = "SELECT Date, Account, Category, Amount, Note FROM Expenses";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        dataGridView1.Rows.Add(
                            Convert.ToDateTime(reader["Date"]).ToShortDateString(),
                            reader["Account"].ToString(),
                            reader["Category"].ToString(),
                            Convert.ToDecimal(reader["Amount"]).ToString("F2"),
                            reader["Note"].ToString()
                        );
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading expenses: " + ex.Message);
            }
            finally
            {
                if (con.State == System.Data.ConnectionState.Open)
                    con.Close();
            }
        }

        private void ClearFields()
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            textBox2.Clear();
            textBox3.Clear();
            dataGridView1.ClearSelection();
        }

        private void UpdateTotalAmount()
        {
            double total = 0;

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells[3].Value != null &&
                    double.TryParse(row.Cells[3].Value.ToString(), out double amount))
                {
                    total += amount;
                }
            }

            TotalAmount.Text = "₱" + total.ToString("F2");
        }

        private void button2_Click(object sender, EventArgs e) // Edit button
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                selectedRow.Cells[0].Value = dateTimePicker1.Value.ToShortDateString();
                selectedRow.Cells[1].Value = comboBox1.Text;
                selectedRow.Cells[2].Value = comboBox2.Text;
                selectedRow.Cells[3].Value = textBox2.Text;
                selectedRow.Cells[4].Value = textBox3.Text;

                MessageBox.Show("Successfully Updated");
                ClearFields();
                UpdateTotalAmount();
            }
            else
            {
                MessageBox.Show("Please select a row to edit.");
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int index = dataGridView1.SelectedRows[0].Index;
                dataGridView1.Rows.RemoveAt(index);
                MessageBox.Show("Successfully Deleted");
                ClearFields();
                UpdateTotalAmount();
            }
            else
            {
                MessageBox.Show("Please select a row to delete.");
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dataGridView1.Rows[e.RowIndex].Cells[0].Value != null)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                dateTimePicker1.Value = DateTime.Parse(row.Cells[0].Value.ToString());
                comboBox1.Text = row.Cells[1].Value.ToString();
                comboBox2.Text = row.Cells[2].Value.ToString();
                textBox2.Text = row.Cells[3].Value.ToString();
                textBox3.Text = row.Cells[4].Value.ToString();
            }
        }

        private void exitButtom_Click(object sender, EventArgs e)
        {
            this.Close();
            Form expenseT = new ExpenseTracker();
            expenseT.Show();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optional: logic when account type is changed
        }

        private void TotalAmount_Click(object sender, EventArgs e)
        {
            // Optional: could show breakdown or refresh
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Optional: click logic for label
        }
    }

    public class Expense
    {
        public DateTime Date { get; set; }
        public string Account { get; set; }
        public string Category { get; set; }
        public string Amount { get; set; }
        public string Note { get; set; }

        public Expense(DateTime date, string account, string category, string amount, string note)
        {
            Date = date;
            Account = account;
            Category = category;
            Amount = amount;
            Note = note;
        }
    }
}
