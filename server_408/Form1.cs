using System;
using System.Drawing;
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
                SendMessage($"CONNECT|{username}");

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

        private void UpdateRichTextBox(string message)
        {
            if (richTextBox8.InvokeRequired && !richTextBox8.IsDisposed)
            {
                richTextBox8.Invoke(new Action<string>(UpdateRichTextBox), message);
            }
            else if (!richTextBox8.IsDisposed)
            {
                richTextBox8.AppendText(message);
            }

        }

        private void UpdateRichTextBoxIF(string message)
        {
            if (richTextBox6.InvokeRequired && !richTextBox6.IsDisposed)
            {
                richTextBox6.Invoke(new Action<string>(UpdateRichTextBox), message);
            }
            else if (!richTextBox6.IsDisposed)
            {
                richTextBox6.AppendText(message);
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
              
                    UpdateRichTextBox($"Error receiving message from server: {ex.Message}\n");
         
            }
        }

        private void ProcessServerMessage(string message)
        {
            // Implement your logic to handle different types of server messages
            // For example, you can split the message into parts and switch based on the action
            string[] parts = message.Split('|');
            
            string channel = parts[0];
            string sent_message = parts[1];

            if (sent_message == "SubscribedtoIF100")
            {
                this.button4.BackColor = System.Drawing.Color.LawnGreen;
                this.button5.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button7.BackColor = System.Drawing.Color.Green;


                UpdateRichTextBox("You subscribed to the channel IF100! \n");

            }
            else if (sent_message == "SubscribedtoSPS101")
            {
                this.button2.BackColor = System.Drawing.Color.LawnGreen;
                this.button6.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button8.BackColor = System.Drawing.Color.Green;


                UpdateRichTextBox("You subscribed to the channel SPS101! \n");

            }
            else if (sent_message == "UnsubscribedfromIF100")
            {

                this.button4.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button5.BackColor = System.Drawing.Color.Crimson;
                this.button7.BackColor = System.Drawing.SystemColors.ButtonShadow;


                UpdateRichTextBox("You unsubscribed from the channel IF101! \n");


            }
            else if (sent_message == "UnsubscribedfromSPS101")
            {

                this.button2.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button6.BackColor = System.Drawing.Color.Crimson;
                this.button8.BackColor = System.Drawing.SystemColors.ButtonShadow;


                UpdateRichTextBox("You unsubscribed from the channel SPS101!\n");


            }
            else if (channel == "IF100" || channel == "SPS101")
            {
                DisplayMessage(channel, sent_message);
            }
            else if(channel == "NOTUNIQUE")
            {
                // There is an user that already exist with the entered username.
                // therefore, connection is terminated with a warning message.
                UpdateRichTextBox(sent_message);
            }


            // Add cases for other server actions if needed



        }

        private void DisplayMessage(string channel, string message)
        {
            // Implement how you want to display the message in the client GUI
            switch (channel)
            {
                case "IF100":
                    AppendTextToRichTextBox(richTextBox6, message);
                    break;
                case "SPS101":
                    AppendTextToRichTextBox(richTextBox7, message);
                    break;
            }
        }

        private void AppendTextToRichTextBox(RichTextBox richTextBox, string text)
        {
            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.AppendText(text)));
            }
            else
            {
                richTextBox.AppendText(text);
            }
        }


        private void SendMessage(string message)
        {

            string username = richTextBox3.Text;

            message = message + "|" + username;
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(message);
                //richTextBox8.AppendText($"Error sending message to server");
                clientStream.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
             
                    UpdateRichTextBox($"Error sending message to server: {ex.Message}\n");
              
            }
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
                    Invoke(new Action(() =>
                    {
                        richTextBox6.AppendText("Invalid server port number.\n");
                    }));
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
            SendMessage("UNSUBSCRIBE|IF100");
             //HFY
             //MERT
             //SMT
             //OPE

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



        private void button7_Click(object sender, EventArgs e)
        {
            string message = richTextBox4.Text;
            //UpdateRichTextBox("if100_message");
            SendMessage($"SEND|IF100|{message}");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string message = richTextBox5.Text;
            //UpdateRichTextBox("sps101_message");
            SendMessage($"SEND|SPS101|{message}");
        }
    }
}

