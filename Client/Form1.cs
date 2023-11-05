using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private Thread receiveThread;
        private string username;
        public Form1(string username)
        {
            this.username = username;
            InitializeComponent();
            client = new TcpClient();
            client.Connect(IPAddress.Parse("127.0.0.1"), 8080);
            stream = client.GetStream();

            // Start a thread to continuously receive messages
            receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string encryptedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        string decryptedMessage = customDecrypt(encryptedMessage);

                        DisplayMessage(decryptedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as disconnection from the server
                Console.WriteLine(ex.Message);
            }
        }
        private void DisplayMessage(string message)
        {
            // Update the TextBox to display the received message
            Invoke(new Action(() =>
            {
                richTextBox2.Text += message + "\n";
                if (message.Contains("@" + username.ToLower()))
                {
                    // Display a Windows notification
                    ShowNotification("New Mention", message);
                }
            }));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            richTextBox1.Multiline = true;
            richTextBox1.WordWrap = true;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string message = "[" + username + "] " + richTextBox1.Text;
            if (!string.IsNullOrWhiteSpace(message))
            {
                // Send the message to the server
                SendMessageToServer(message);

                // Add the sent message to the chat display
                // DisplayMessage($"{username}" + message);

                // Clear the RichTextBox
                richTextBox1.Clear();
            }
        }

        private void SendMessageToServer(string message)
        {
            try
            {
                string encryptedMessage = customEncrypt(message);

                // Convert the encrypted message to bytes
                byte[] messageBytes = Encoding.UTF8.GetBytes(encryptedMessage);

                // Send the message to the server using the NetworkStream
                DisplayMessage(customDecrypt(encryptedMessage));
                stream.Write(messageBytes, 0, messageBytes.Length);
            }
            catch (Exception ex)
            {
                // Handle exceptions, such as disconnection from the server
                Console.WriteLine(ex.Message);
            }
        }
        private string customEncrypt(string plaintext)
        {
            char[] encryptedText = plaintext.ToCharArray();
            for (int i = 0; i < encryptedText.Length; i++)
            {
                if (Char.IsLower(encryptedText[i]))
                {
                    encryptedText[i] = (char)('z' - (encryptedText[i] - 'a'));
                }
                else if (Char.IsUpper(encryptedText[i]))
                {
                    encryptedText[i] = (char)('Z' - (encryptedText[i] - 'A'));
                }
            }
            return new string(encryptedText);
        }
        private string customDecrypt(string encryptedText)
        {
            char[] decryptedText = encryptedText.ToCharArray();
            for (int i = 0; i < decryptedText.Length; i++)
            {
                if (Char.IsLower(decryptedText[i]))
                {
                    decryptedText[i] = (char)('a' + ('z' - decryptedText[i]));
                }
                else if (Char.IsUpper(decryptedText[i]))
                {
                    decryptedText[i] = (char)('A' + ('Z' - decryptedText[i]));
                }
            }
            return new string(decryptedText);
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {

        }
        private void ShowNotification(string title, string message)
        {
            // Create a notification object
            NotifyIcon notifyIcon = new NotifyIcon();

            // Set the notification icon (you can change this to your preferred icon)
            notifyIcon.Icon = SystemIcons.Information;

            // Display the notification
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.Visible = true;

            // Show the notification for a specific duration (e.g., 3 seconds)
            notifyIcon.ShowBalloonTip(3000);

            // Dispose of the notification object after use
            notifyIcon.Dispose();
        }
    }
}
