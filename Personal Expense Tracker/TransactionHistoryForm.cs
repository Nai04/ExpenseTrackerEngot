using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System.Data.SqlClient;

namespace Personal_Expense_Tracker
{
    public partial class TransactionHistoryForm : Form
    {
        SqlConnection con = new SqlConnection(@"Server=IANPC;Database=ExpenseTracker;Integrated Security=True;");

        public TransactionHistoryForm()
        {
            InitializeComponent();
            cmbCategory.SelectedIndexChanged += cmbCategory_SelectedIndexChanged;
        }

        private void TransactionHistoryForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadTransactions();
        }

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("All");

            try
            {
                string query = "SELECT DISTINCT Category FROM Expenses ORDER BY Category";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        cmbCategory.Items.Add(reader["Category"].ToString());
                    }
                    con.Close();
                }

                cmbCategory.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading categories: " + ex.Message);
            }
        }

        private void LoadTransactions(string filter = "")
        {
            try
            {
                string query = "SELECT Amount, Date, Category, Account FROM Expenses WHERE Date BETWEEN @From AND @To";
                if (cmbCategory.SelectedItem != null && cmbCategory.SelectedItem.ToString() != "All")
                {
                    query += " AND Category = @Category";
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@From", dtpFrom.Value.Date);
                    cmd.Parameters.AddWithValue("@To", dtpTo.Value.Date);

                    if (cmbCategory.SelectedItem != null && cmbCategory.SelectedItem.ToString() != "All")
                    {
                        cmd.Parameters.AddWithValue("@Category", cmbCategory.SelectedItem.ToString());
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    // Optional: filter by amount range or exact value
                    string amountText = txtAmount.Text.Trim();
                    if (!string.IsNullOrEmpty(amountText))
                    {
                        if (amountText.Contains("-"))
                        {
                            var parts = amountText.Split('-');
                            if (parts.Length == 2 &&
                                decimal.TryParse(parts[0].Trim(), out decimal minAmount) &&
                                decimal.TryParse(parts[1].Trim(), out decimal maxAmount))
                            {
                                table = table.AsEnumerable()
                                    .Where(row =>
                                        decimal.TryParse(row["Amount"].ToString(), out decimal amt) &&
                                        amt >= minAmount && amt <= maxAmount)
                                    .CopyToDataTable();
                            }
                            else
                            {
                                MessageBox.Show("Invalid amount range format. Use min-max, e.g. 100-500.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            if (decimal.TryParse(amountText, out decimal exactAmount))
                            {
                                table = table.AsEnumerable()
                                    .Where(row =>
                                        decimal.TryParse(row["Amount"].ToString(), out decimal amt) &&
                                        amt == exactAmount)
                                    .CopyToDataTable();
                            }
                            else
                            {
                                MessageBox.Show("Invalid amount value.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }

                    dataGridView1.DataSource = table;
                    UpdateSummary(table);
                    UpdateChart(table);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading transactions: " + ex.Message);
            }
        }

        private void UpdateSummary(DataTable table)
        {
            decimal total = 0;
            var summary = new StringBuilder();

            var groups = table.AsEnumerable()
                .GroupBy(row => row.Field<string>("Category"));

            foreach (var group in groups)
            {
                decimal categoryTotal = group.Sum(row => decimal.TryParse(row.Field<string>("Amount"), out decimal amt) ? amt : 0);
                total += categoryTotal;
                summary.AppendLine($"{group.Key}: ₱{categoryTotal:N2}");
            }

            lblTotal.Text = $"Total Spending: ₱{total:N2}";
            lblCategorySummary.Text = summary.Length > 0 ? summary.ToString() : "No data to summarize.";
        }

        private void UpdateChart(DataTable table)
        {
            chartSummary.Series.Clear();
            chartSummary.ChartAreas.Clear();

            var chartArea = new ChartArea();
            chartSummary.ChartAreas.Add(chartArea);

            var series = new Series
            {
                Name = "SpendingByCategory",
                ChartType = SeriesChartType.Pie,
                IsValueShownAsLabel = true,
                LabelFormat = "₱{0:N2}"
            };

            chartSummary.Series.Add(series);

            var groups = table.AsEnumerable()
                .GroupBy(row => row.Field<string>("Category"));

            foreach (var group in groups)
            {
                decimal totalAmount = group.Sum(row => decimal.TryParse(row.Field<string>("Amount"), out decimal amt) ? amt : 0);
                series.Points.AddXY(group.Key, totalAmount);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
            Form expenseT = new ExpenseTracker();
            expenseT.Show();
        }

        private void cmbCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTransactions();
        }

        private void btnExportPDF_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                Title = "Save transaction report",
                FileName = "TransactionReport.pdf"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    Document doc = new Document(PageSize.A4, 10, 10, 10, 10);
                    PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    doc.Open();

                    Paragraph title = new Paragraph("Transaction History", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16));
                    title.Alignment = Element.ALIGN_CENTER;
                    doc.Add(title);
                    doc.Add(new Paragraph("\n"));

                    PdfPTable table = new PdfPTable(dataGridView1.Columns.Count);
                    table.WidthPercentage = 100;

                    foreach (DataGridViewColumn column in dataGridView1.Columns)
                    {
                        table.AddCell(new Phrase(column.HeaderText, FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                    }

                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                string cellText = cell.Value?.ToString() ?? "";
                                table.AddCell(new Phrase(cellText, FontFactory.GetFont(FontFactory.HELVETICA, 11)));
                            }
                        }
                    }

                    doc.Add(table);
                    doc.Close();

                    MessageBox.Show("PDF Exported Successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting PDF: " + ex.Message);
                }
            }
        }

        private void TransactionHistoryForm_Load_1(object sender, EventArgs e) { }
        private void lblCategorySummary_Click(object sender, EventArgs e) { }
    }
}

