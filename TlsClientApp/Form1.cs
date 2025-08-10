using System;
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

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                commPanel.Visible = true;
                using (var client = new TcpClient())
                {
                    await client.ConnectAsync("localhost", 4433);
                    using (var sslStream = new SslStream(client.GetStream(), false, (_1, _2, _3, _4) => true))
                    {
                        await sslStream.AuthenticateAsClientAsync("localhost");

                        string message = sendTextBox.Text;
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        await sslStream.WriteAsync(data, 0, data.Length);

                        var buffer = new byte[1024];
                        int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
                        recvTextBox.Text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    }
                }
            }
            finally
            {
                commPanel.Visible = false;
            }
        }
    }
}
