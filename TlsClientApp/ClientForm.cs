using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
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
            catch (AuthenticationException exception)
            {
                MessageBox.Show(exception.Message, "Error");
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
            try
            {
                Directory.CreateDirectory("cert");
                foreach (var filePath in Directory.EnumerateFiles("cert", "*.crt"))
                {
                    try
                    {
                        var cert = X509Certificate.CreateFromCertFile(filePath);
                        if (cert.Equals(certificate)) return true;
                    }
                    catch { }
                }
            }
            catch { }

            bool authed = false;
            Invoke((MethodInvoker)(() =>
            {
                if (MessageBox.Show(
                    $"未認証の証明書です。認証しますか？\n{certificate.ToString()}",
                    "Information",
                    MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                try
                {
                    using (var file = File.OpenWrite($"cert/{certificate.GetSerialNumberString()}.crt"))
                    {
                        var rawData = certificate.GetRawCertData();
                        file.Write(rawData, 0, rawData.Length);
                    }
                    authed = true;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error");
                }
            }));

            return authed;
        }
    }
}
