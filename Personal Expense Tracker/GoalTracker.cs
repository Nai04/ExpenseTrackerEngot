using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Personal_Expense_Tracker
{
    public partial class GoalTracker : Form
    {
        SqlConnection con = new SqlConnection(@"Server=IANPC;Database=ExpenseTracker;Integrated Security=True;");

        public GoalTracker()
        {
            InitializeComponent();
        }

        private void GoalTracker_Load(object sender, EventArgs e)
        {
            dataGridView1.ColumnCount = 4;
            dataGridView1.Columns[0].Name = "Goal Name";
            dataGridView1.Columns[1].Name = "Target Amount";
            dataGridView1.Columns[2].Name = "Saved Money";
            dataGridView1.Columns[3].Name = "Goal Progress";

            LoadGoals();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            try
            {
                string goalName = textBox1.Text.Trim();
                if (!double.TryParse(textBox2.Text, out double targetAmount))
                    throw new Exception("Target amount must be a valid number.");

                string query = "INSERT INTO Goals (GoalName, TargetAmount, SavedMoney) VALUES (@Name, @Target, @Saved)";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", goalName);
                    cmd.Parameters.AddWithValue("@Target", targetAmount);
                    cmd.Parameters.AddWithValue("@Saved", 0.00);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                dataGridView1.Rows.Add(goalName, targetAmount.ToString("F2"), "0.00", "0%");
                MessageBox.Show("Goal added successfully.");
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                try
                {
                    string goalName = textBox1.Text.Trim();
                    if (!double.TryParse(textBox2.Text, out double newTarget))
                        throw new Exception("Target amount must be a valid number.");

                    DataGridViewRow row = dataGridView1.SelectedRows[0];
                    double saved = Convert.ToDouble(row.Cells[2].Value);

                    string query = "UPDATE Goals SET TargetAmount=@Target WHERE GoalName=@Name";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Target", newTarget);
                        cmd.Parameters.AddWithValue("@Name", goalName);

                        con.Open();
                        cmd.ExecuteNonQuery();
                        con.Close();
                    }

                    double progress = Math.Min((saved / newTarget) * 100, 100);
                    row.Cells[0].Value = goalName;
                    row.Cells[1].Value = newTarget.ToString("F2");
                    row.Cells[3].Value = $"{progress:F0}%";
                    
                    MessageBox.Show("Goal updated successfully.");
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Update error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please select a goal to update.");
            }
            
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string goalName = dataGridView1.SelectedRows[0].Cells[0].Value.ToString();

                string query = "DELETE FROM Goals WHERE GoalName=@Name";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Name", goalName);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                }

                dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[0].Index);
                MessageBox.Show("Goal deleted successfully.");
                ClearFields();
            }
            else
            {
                MessageBox.Show("Please select a goal to delete.");
            }
        }

        private void fundsButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a goal to add funds.");
                return;
            }

            if (!double.TryParse(FundsTextbox.Text, out double addAmount))
            {
                MessageBox.Show("Enter a valid amount.");
                return;
            }

            DataGridViewRow row = dataGridView1.SelectedRows[0];
            string goalName = row.Cells[0].Value.ToString();
            double target = Convert.ToDouble(row.Cells[1].Value);
            double saved = Convert.ToDouble(row.Cells[2].Value);

            double newSaved = Math.Min(saved + addAmount, target);

            string query = "UPDATE Goals SET SavedMoney=@Saved WHERE GoalName=@Name";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Saved", newSaved);
                cmd.Parameters.AddWithValue("@Name", goalName);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            MessageBox.Show("Added Succesfully");
            double progress = Math.Min((newSaved / target) * 100, 100);
            row.Cells[2].Value = newSaved.ToString("F2");
            row.Cells[3].Value = $"{progress:F0}%";
            FundsTextbox.Clear();
        }

        private void deductFunds_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a goal to deduct funds.");
                return;
            }

            if (!double.TryParse(FundsTextbox.Text, out double deductAmount))
            {
                MessageBox.Show("Enter a valid amount.");
                return;
            }
            MessageBox.Show("Deducted Succesfully");
            DataGridViewRow row = dataGridView1.SelectedRows[0];
            string goalName = row.Cells[0].Value.ToString();
            double target = Convert.ToDouble(row.Cells[1].Value);
            double saved = Convert.ToDouble(row.Cells[2].Value);

            double newSaved = Math.Max(saved - deductAmount, 0);

            string query = "UPDATE Goals SET SavedMoney=@Saved WHERE GoalName=@Name";
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Saved", newSaved);
                cmd.Parameters.AddWithValue("@Name", goalName);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }

            double progress = Math.Max((newSaved / target) * 100, 0);
            row.Cells[2].Value = newSaved.ToString("F2");
            row.Cells[3].Value = $"{progress:F0}%";
            FundsTextbox.Clear();
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                textBox1.Text = row.Cells[0].Value.ToString();
                textBox2.Text = row.Cells[1].Value.ToString();
            }
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            this.Close();
            new ExpenseTracker().Show();
        }

        private void ClearFields()
        {
            textBox1.Clear();
            textBox2.Clear();
            FundsTextbox.Clear();
            dataGridView1.ClearSelection();
        }

        private void LoadGoals()
        {
            try
            {
                string query = "SELECT GoalName, TargetAmount, SavedMoney FROM Goals";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string name = reader["GoalName"].ToString();
                        double target = Convert.ToDouble(reader["TargetAmount"]);
                        double saved = Convert.ToDouble(reader["SavedMoney"]);
                        double progress = Math.Min((saved / target) * 100, 100);

                        dataGridView1.Rows.Add(
                            name,
                            target.ToString("F2"),
                            saved.ToString("F2"),
                            $"{progress:F0}%"
                        );
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading goals: " + ex.Message);
            }
        }
    }
}
