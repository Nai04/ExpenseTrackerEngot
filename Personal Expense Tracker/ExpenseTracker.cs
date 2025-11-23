using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Personal_Expense_Tracker
{
    public partial class ExpenseTracker: Form
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
        public ExpenseTracker()
        {
            InitializeComponent();
        }

        private void ExpenseTracker_Load(object sender, EventArgs e)
        {
            button1.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button1.Width, button1.Height, 30, 30));
            button3.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button3.Width, button3.Height, 30, 30));
            btnViewTransactions.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, btnViewTransactions.Width, btnViewTransactions.Height, 30, 30));
            button4.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button4.Width, button4.Height, 30, 30));
            button5.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button5.Width, button5.Height, 30, 30));
            button6.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, button6.Width, button6.Height, 30, 30));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form Tracker = new Tracker();
            Tracker.Show();
            this.Close();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            Form goalTracker = new GoalTracker();
            goalTracker.Show();
            this.Close();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Form subtracker = new SubscriptionTracker();
            subtracker.Show();
            this.Close();
        }

        private void btnViewTransactions_Click(object sender, EventArgs e)
        {
            TransactionHistoryForm historyForm = new TransactionHistoryForm();
            historyForm.Show();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form healthscore = new HealthScore();
            healthscore.Show();
            this.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Logout Successfully");          
            this.Close();
            Form form1 = new Form1();
            form1.Show();

;        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
