using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = richTextBox1.Text;
            // Create an instance of Form1
            Form1 form1 = new Form1(username);

            // Show Form1 and close Form2
            this.Hide();
            form1.Show();   
            
        }

        private bool IsValidUsername(string username)
        {
            // Add your validation logic here
            // For example, check if the username meets certain criteria
            return !string.IsNullOrEmpty(username) && username.Length <= 10;
        }
    }
}
