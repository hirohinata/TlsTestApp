using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TlsClientApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var client = new TcpClient("localhost", 4433))
            {
                using (var sslStream = new SslStream(client.GetStream(), false, (_, cert, chain, errors) => true))
                {
                    sslStream.AuthenticateAsClientAsync("localhost").Wait();

                    string message = sendTextBox.Text;
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    sslStream.WriteAsync(data, 0, data.Length).Wait();

                    var buffer = new byte[1024];
                    int bytesRead = sslStream.ReadAsync(buffer, 0, buffer.Length).Result;
                    recvTextBox.Text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                }
            }
        }
    }
}
