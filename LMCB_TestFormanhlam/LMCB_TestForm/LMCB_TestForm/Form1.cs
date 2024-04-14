using Newtonsoft.Json;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace LMCB_TestForm
{
    public partial class Form1 : Form
    {

        private TcpClient tcpClient;
        private SslStream sslStream;
        private Thread clientThread;
        private bool stopTcpClient = true;
        private System.Windows.Forms.TextBox textBox;
        private System.Windows.Forms.PictureBox pictureBox;
        public Form1()
        {
            InitializeComponent();
            InitializeDynamicHeightTextBox();
        }
        private void ClientRecv()
        {
            try
            {
                while (!stopTcpClient && tcpClient.Connected)
                {
                    Application.DoEvents();
                    StreamReader sr = new StreamReader(sslStream);
                    string jsonMessage = sr.ReadLine();
                    UpdateChatHistoryThreadSafe($"{jsonMessage}");
                }
            }
            catch (SocketException sockEx)
            {
                tcpClient.Close();
                sslStream.Close();
                return;
            }
        }

        private delegate void SafeCallDelegate(string text);

        private void UpdateChatHistoryThreadSafe(string text)
        {
            if (textBox.InvokeRequired)
            {
                var d = new SafeCallDelegate(UpdateChatHistoryThreadSafe);
                textBox.Invoke(d, new object[] { text });
            }
            else
            {
                InitializeDynamicHeightTextBox();
                var messagePost = JsonConvert.DeserializeObject<MessagePost>(text);
                string formattedMsg = $"{messagePost.From_Username}: {messagePost.Message}";
                if (messagePost.From_Username == textBox1.Text)
                {
                    textBox.TextAlign = HorizontalAlignment.Right;
                }
                else
                {
                    textBox.TextAlign = HorizontalAlignment.Left;
                }

                textBox.Text = formattedMsg;
                flowLayoutPanel1.Controls.Add(textBox);

                if (messagePost.Message.StartsWith("[Image]"))
                {
                    string base64Image = messagePost.Message.Substring(7);
                    byte[] imageBytes = Convert.FromBase64String(base64Image);
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        PictureBox pictureBox = new PictureBox
                        {
                            SizeMode = PictureBoxSizeMode.StretchImage,
                            Width = flowLayoutPanel1.Width - 8,
                            Height = 150,
                            Image = System.Drawing.Image.FromStream(ms)
                        };
                        flowLayoutPanel1.Controls.Add(pictureBox);
                    }
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var messagePost = new MessagePost
                {
                    From_Username = textBox1.Text,
                    To_Username = textBox3.Text,
                    Message = sendMsgTextBox.Text
                };

                string jsonMessage = JsonConvert.SerializeObject(messagePost);
                StreamWriter sw = new StreamWriter(sslStream);
                sw.WriteLine(jsonMessage);
                sw.Flush();

                sendMsgTextBox.Text = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Close(object sender, FormClosingEventArgs e)
        {
            if (tcpClient != null && tcpClient.Connected)
            {
                tcpClient.Close();
            }
            if (sslStream != null)
            {
                sslStream.Close();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                stopTcpClient = false;
                tcpClient = new TcpClient();
                tcpClient.Connect(new IPEndPoint(IPAddress.Parse(textBox2.Text), 8080));
                X509Certificate2 clientCertificate = new X509Certificate2(@"D:\SSL Certificate\MySslSocketCertificate.cer", "123");
                sslStream = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateCertificate), null);
                sslStream.AuthenticateAsClient("Local", new X509Certificate2Collection(new X509Certificate2[] { clientCertificate }), SslProtocols.Tls12, false);
                if (tcpClient.Connected)
                {
                    clientThread = new Thread(this.ClientRecv);
                    clientThread.Start();
                    MessageBox.Show("Connected");
                }
                else
                {
                    MessageBox.Show("Connection failed");
                }
            }
            catch (SocketException sockEx)
            {
                MessageBox.Show(sockEx.Message, "Network error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void InitializeDynamicHeightTextBox()
        {
            textBox = new System.Windows.Forms.TextBox
            {
                Multiline = true,
                Width = flowLayoutPanel1.Width - 5,
                Height = 30,
                WordWrap = true,
            };
            textBox.TextChanged += TextBox_TextChanged;
            this.Controls.Add(textBox);
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            var numberOfLines = textBox.Lines.Length;
            textBox.Height = numberOfLines * 17;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.gif)|*.jpg; *.jpeg; *.png; *.gif";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string imagePath = openFileDialog.FileName;
                    byte[] imageBytes = File.ReadAllBytes(imagePath);
                    string base64Image = Convert.ToBase64String(imageBytes);
                    string message = $"[Image]{base64Image}";
                    MessagePost messagePost = new MessagePost
                    {
                        From_Username = textBox1.Text,
                        To_Username = textBox3.Text,
                        Message = message
                    };
                    string jsonMessage = JsonConvert.SerializeObject(messagePost);
                    StreamWriter sw = new StreamWriter(sslStream);
                    sw.WriteLine(jsonMessage);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            Emoji emoji = new Emoji();
            if (emoji.ShowDialog() == DialogResult.OK)
            {
                string selectedEmoji = emoji.SelectedEmoji;
                sendMsgTextBox.Text += selectedEmoji;
            }
        }
        private static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; 
        }
    }
}
