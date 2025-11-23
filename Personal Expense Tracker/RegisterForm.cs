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
    public partial class RegisterForm : Form
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

        public RegisterForm()
        {
            InitializeComponent();
        }

        public bool checkConnection()
        {
            return (con.State == ConnectionState.Closed ? true : false);
        }

        // Hash password using SHA256 - SAME METHOD AS LOGIN
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void RegisterForm_Load(object sender, EventArgs e) {
            SignUp.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, SignUp.Width, SignUp.Height, 30, 30));
            button1.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button1.Width, button1.Height, 30, 30));
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            
            PassReg1.PasswordChar = checkBox1.Checked ? '\0' : '*';
            PassReg2.PasswordChar = checkBox1.Checked ? '\0' : '*';
        }

        // SIGN UP BUTTON - FIXED DATABASE INTEGRATION
        private void SignUp_Click(object sender, EventArgs e)
        {
            if (RegText1.Text == "" || PassReg1.Text == "" || PassReg2.Text == "")
            {
                MessageBox.Show("Please fill the blank fields", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string email = RegText1.Text.Trim();
            string pass1 = PassReg1.Text.Trim();
            string pass2 = PassReg2.Text.Trim();

            if (pass1.Length < 8)
            {
                MessageBox.Show("Invalid Password. Please enter at least 8 characters", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (pass1 != pass2)
            {
                MessageBox.Show("Password does not match.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (checkConnection())
            {
                try
                {
                    con.Open();

                    // ===== CHECK IF EMAIL EXISTS =====
                    string checkEmailQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";

                    using (SqlCommand checkCmd = new SqlCommand(checkEmailQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", email);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("This email is already registered.", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // ===== INSERT NEW USER WITH HASHED PASSWORD =====
                    string insertQuery = "INSERT INTO Users (Email, Password, CreatedDate) VALUES (@Email, @Pass, @Date)";

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                    {
                        insertCmd.Parameters.AddWithValue("@Email", email);
                        insertCmd.Parameters.AddWithValue("@Pass", HashPassword(pass1));  // HASHED PASSWORD
                        insertCmd.Parameters.AddWithValue("@Date", DateTime.Now);

                        insertCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Registered Successfully", "Information Message", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Form1 loginForm = new Form1();
                    loginForm.Show();
                    this.Hide();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed Connection: " + ex.Message, "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    con.Close();
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Form1 F1 = new Form1();
            F1.Show();
            this.Hide();
        }

        private void panel5_Paint(object sender, PaintEventArgs e) { }
        private void label5_Click(object sender, EventArgs e) { }
        private void panel4_Paint(object sender, PaintEventArgs e) { }
        private void Header_Click(object sender, EventArgs e) { }
        private void header2_Click(object sender, EventArgs e) { }
        private void header3_Click(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
    }
}