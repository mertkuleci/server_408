using System;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace client_408
{

    public partial class Form1 : Form
    {
        private TcpClient tcpClient;
        private NetworkStream clientStream;
        private Thread receiveThread;
        bool isConnected = false;
        bool threadTerminating = false;
        public Form1()
        {
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
        }



        //Connecting to the server
        private void ConnectToServer(string serverIp, int serverPort, string username)
        {
            try
            {
                tcpClient = new TcpClient(serverIp, serverPort);
                clientStream = tcpClient.GetStream();

                SendMessage($"CONNECT|dummy|{username}");
                isConnected = true;

                receiveThread = new Thread(new ThreadStart(ReceiveMessages));
                receiveThread.Start();
            }
            catch 
            {
                UpdateRichTextBox("Error connecting to server.\n");
            }
        }


        //Updating the action RichTextBox
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


        //Receive the message from server
        private void ReceiveMessages()
        {
            try
            {
                while (isConnected)
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
                if (!threadTerminating)
                {
                    UpdateRichTextBox($"Error receiving message from server: {ex.Message}\n");
                }
            }
        }

        //Processing the received message from server
        private void ProcessServerMessage(string message)
        {
            string[] parts = message.Split('|');
            int firstIndex = message.IndexOf('|');

            string channel = parts[0]; // It is either "IF100" or "SPS101" or "CONNECT" or "DISCONNECT" or "NOTUNIQUE"
            string sent_message = message.Substring(firstIndex + 1);

            //Processing each message type for each kind of request that came from the server
            //Also updating the colors of the UI

            //If the client subcribed to IF100
            if (sent_message == "SubscribedtoIF100")
            {
                this.button4.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button5.BackColor = System.Drawing.Color.Crimson;
                this.button7.BackColor = System.Drawing.Color.Green;

                UpdateRichTextBox("You subscribed to the channel IF100! \n");
            }

            //If the client subcribed to SPS101
            else if (sent_message == "SubscribedtoSPS101")
            {
                this.button2.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button6.BackColor = System.Drawing.Color.Crimson;
                this.button8.BackColor = System.Drawing.Color.Green;

                UpdateRichTextBox("You subscribed to the channel SPS101! \n");
            }

            //If the client unsubcribed from IF100
            else if (sent_message == "UnsubscribedfromIF100")
            {

                this.button4.BackColor = System.Drawing.Color.LawnGreen;
                this.button5.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button7.BackColor = System.Drawing.SystemColors.ButtonShadow;

                UpdateRichTextBox("You unsubscribed from the channel IF101! \n");
            }

            //If the client unsubcribed from SPS101
            else if (sent_message == "UnsubscribedfromSPS101")
            {
                this.button2.BackColor = System.Drawing.Color.LawnGreen;
                this.button6.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button8.BackColor = System.Drawing.SystemColors.ButtonShadow;

                UpdateRichTextBox("You unsubscribed from the channel SPS101!\n");
            }

            //If messages received from the channels
            else if (channel == "IF100" || channel == "SPS101")
            {
                DisplayMessage(channel, sent_message);
            }

            //If client tries to send message or subscribe if it is not connected or subcribed
            else if(channel == "IF100unsub" || channel == "SPS101unsub" || channel == "IF100uncon" || channel == "SPS101uncon"){
                UpdateRichTextBox(sent_message);
                UpdateRichTextBox("\n");
            }

            //If there is an user that already exist with the entered username.
            // therefore, connection is terminated with a warning message.
            else if (channel == "NOTUNIQUE")
            {
                UpdateRichTextBox(sent_message);
            }

            //If the connect button is clicked
            else if (channel == "CONNECTED")
            {
                this.button1.BackColor = System.Drawing.SystemColors.ButtonShadow;
                this.button3.BackColor = System.Drawing.Color.IndianRed;
                button1.Invoke(new Action(() => button1.Enabled = false));
                button3.Invoke(new Action(() => button3.Enabled = true));
                UpdateRichTextBox("You are connected to the server!\n");
                isConnected = true;
            }

            //If the disconnect button is clicked
            else if (channel == "DISCONNECT")
            {
                this.button1.BackColor = System.Drawing.Color.MediumSeaGreen;
                this.button3.BackColor = System.Drawing.SystemColors.ButtonShadow;
                button1.Invoke(new Action(() => button1.Enabled = true));
                button3.Invoke(new Action(() => button3.Enabled = false));
                this.button2.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button6.BackColor = System.Drawing.Color.Crimson;
                this.button8.BackColor = System.Drawing.SystemColors.ButtonShadow;
                this.button4.BackColor = System.Drawing.SystemColors.ActiveBorder;
                this.button5.BackColor = System.Drawing.Color.Crimson;
                this.button7.BackColor = System.Drawing.SystemColors.ButtonShadow;
                UpdateRichTextBox("You are disconnected!\n");
                isConnected = false;
            }
        }

        //Displaying message of IF100 or SPS101 channels
        private void DisplayMessage(string channel, string message)
        {
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



        //To update IF100 and SPS101 channels message boxes
        private void AppendTextToRichTextBox(RichTextBox richTextBox, string text)
        {
            DateTime currentTime = DateTime.Now; //To write the current time for each message sent
            string time = currentTime.ToString("hh:mm:ss tt");
            text = time + ": " + text;

            if (richTextBox.InvokeRequired)
            {
                richTextBox.Invoke(new Action(() => richTextBox.AppendText(text)));
            }
            else
            {
                richTextBox.AppendText(text);
            }
        }



        //Sending message to the server
        private void SendMessage(string message)
        {

            string username = richTextBox3.Text;
            string updatedusername = username.Replace("|", "?");

            message = message + "|" + updatedusername; //To send the username to the server

            if (isConnected || message.IndexOf("|") == 7)
            {
                try
                {
                    byte[] buffer = Encoding.ASCII.GetBytes(message);
                    clientStream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    UpdateRichTextBox($"Error sending message to server: {ex.Message}\n");
                }
            }
            else
            {
                UpdateRichTextBox("You are not connected!\n");
            }

           
        }

        // To clean up resources when the form is closing
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (tcpClient != null)
            {
                SendMessage("DISCONNECT|dummy|dummy");
                threadTerminating = true;
                isConnected = false;
                tcpClient.Close();
            }

        }



        //Here are the buttons of the client:

        //Button to connect to the server
        private void button1_Click(object sender, EventArgs e)
        {
            {
                string serverIp = richTextBox1.Text;
                int serverPort;

                if (int.TryParse(richTextBox2.Text, out serverPort))
                {
                    string username = richTextBox3.Text;
                    string updatedusername = username.Replace("|", "?"); // To avoid usernames that contains '|'

                    ConnectToServer(serverIp, serverPort, updatedusername);
                }
                else
                {
                 
                     UpdateRichTextBox("Invalid server port number.\n");
                  
                }
            }
        }

        //Button for subscribing to the IF100 channel
        private void button4_Click(object sender, EventArgs e)
        {
            SendMessage("SUBSCRIBE|IF100|dummy");
        }

        //Button for subscribing to the SPS101 channel
        private void button2_Click(object sender, EventArgs e)
        {
            SendMessage("SUBSCRIBE|SPS101|dummy");
        }

        //Button for unsubscribing from the IF100 channel
        private void button5_Click(object sender, EventArgs e)
        {
            SendMessage("UNSUBSCRIBE|IF100|dummy");
        }

        //Button for unsubscribing from the SPS101 channel
        private void button6_Click(object sender, EventArgs e)
        {
            SendMessage("UNSUBSCRIBE|SPS101|dummy");
        }


        //Button that sends message to the IF100 channel
        private void button7_Click(object sender, EventArgs e)
        {
            string message = richTextBox4.Text;
            richTextBox4.Clear();
            SendMessage($"SEND|IF100|{message}");

        }

        //Button that sends message to the SPS101 channel
        private void button8_Click(object sender, EventArgs e)
        {
            string message = richTextBox5.Text;
            richTextBox5.Clear();
            SendMessage($"SEND|SPS101|{message}");
        }

        //Button for the disconnection
        private void button3_Click(object sender, EventArgs e)
        {
            SendMessage("DISCONNECT|dummy|dummy");

        }

    }
}

