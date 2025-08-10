using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows.Forms;

namespace TlsClientApp
{
    public partial class ClientForm : Form
    {
        public ClientForm()
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
                    using (var sslStream = new SslStream(client.GetStream(), false, ValidateRemoteCertificate))
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

        private bool ValidateRemoteCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
