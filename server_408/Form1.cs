using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace client_408
{
    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        private NetworkStream clientStream;
        private Thread receiveThread;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void ConnectToServer(string serverIp, int serverPort, string username)
        {
            try
            {
                tcpClient = new TcpClient(serverIp, serverPort);
                clientStream = tcpClient.GetStream();

                // Send the username to the server
                //SendMessage($"CONNECT|{username}");

                // Start a thread to listen for messages from the server
                receiveThread = new Thread(new ThreadStart(ReceiveMessages));
                receiveThread.Start();

                richTextBox8.AppendText($"Connected to server {serverIp}:{serverPort} as {username}\n");
            }
            catch (Exception ex)
            {
                richTextBox8.AppendText($"Error connecting to server: {ex.Message}\n");
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    byte[] message = new byte[4096];
                    int bytesRead = clientStream.Read(message, 0, 4096);

                    if (bytesRead == 0)
                        break;

                    string data = Encoding.ASCII.GetString(message, 0, bytesRead);
                    ProcessServerMessage(data);
                }
            }
            catch (Exception ex)
            {
                richTextBox8.AppendText($"Error receiving message from server: {ex.Message}\n");
            }
        }

        private void ProcessServerMessage(string message)
        {
            // Implement your logic to handle different types of server messages
            // For example, you can split the message into parts and switch based on the action
            string[] parts = message.Split('|');

            if (parts.Length >= 2)
            {
                string action = parts[0].ToUpper();
                string channel = parts[1].ToUpper();

                switch (action)
                {
                    case "MESSAGE":
                        if (parts.Length >= 4)
                        {
                            string sender = parts[2];
                            string data = parts[3];
                            DisplayMessage(channel, sender, data);
                        }
                        break;
                        // Add cases for other server actions if needed
                }
            }
            else
            {
                richTextBox8.AppendText($"Invalid message format from server: {message}\n");
            }
        }

        private void DisplayMessage(string channel, string sender, string data)
        {
            // Implement how you want to display the message in the client GUI
            richTextBox1.AppendText($"[{channel}] {sender}: {data}\n");
        }

        private void SendMessage(string message)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                //richTextBox8.AppendText($"Error sending message to server");
                clientStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                richTextBox8.AppendText($"Error sending message to server: {ex.Message}\n");
            }
        }


        private void button7_Click(object sender, EventArgs e)
        {
            string message = richTextBox4.Text;
            SendMessage($"SEND|IF100|{message}");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string message = richTextBox5.Text;
            SendMessage($"SEND|SPS101|{message}");
        }
   

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up resources when the form is closing
            if (tcpClient != null)
            {
                tcpClient.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            {
                string serverIp = richTextBox1.Text;
                int serverPort;

                if (int.TryParse(richTextBox2.Text, out serverPort))
                {
                    string username = richTextBox3.Text;
                    ConnectToServer(serverIp, serverPort, username);
                }
                else
                {
                    richTextBox6.AppendText("Invalid server port number.\n");
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Implement how to subscribe to channels
            // You may need to modify this based on your specific design
            SendMessage("SUBSCRIBE|IF100");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Implement how to subscribe to channels
            // You may need to modify this based on your specific design
            SendMessage("SUBSCRIBE|SPS101");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // Implement how to subscribe to channels
            // You may need to modify this based on your specific design
            SendMessage("UNSUBSCRIBE|IF1000");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // Implement how to subscribe to channels
            // You may need to modify this based on your specific design
            SendMessage("UNSUBSCRIBE|SPS101");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Implement how to subscribe to channels
            // You may need to modify this based on your specific design
            SendMessage("DISCONNECT");
        }

    }
}

