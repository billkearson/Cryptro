namespace Cryptro
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string info = Crypto.Encrypt(textBox1.Text, textBox3.Text);            
            listBox1.Items.Add($"Encrypted: {info}");
            textBox2.Text = info;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string info = Crypto.Decrypt(textBox2.Text, textBox3.Text);
            listBox1.Items.Add($"Decrypted: {info}");
        }
    }
}
