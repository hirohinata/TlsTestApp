using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


namespace TlsServerApp
{
    class TlsEchoServer
    {
        private readonly X509Certificate2 _certificate;
        private readonly TcpListener _listener;

        public TlsEchoServer(string certPath, int port)
        {
            _certificate = new X509Certificate2(certPath);
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("TLS Echo Server started...");

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            using (var sslStream = new SslStream(client.GetStream(), false))
            {
                try
                {
                    await sslStream.AuthenticateAsServerAsync(_certificate, false, System.Security.Authentication.SslProtocols.Tls13, false);
                    Console.WriteLine("TLS handshake completed.");

                    var buffer = new byte[1024];
                    int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);
                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine($"Received: {received}");

                    await sslStream.WriteAsync(Encoding.UTF8.GetBytes(received), 0, bytesRead); // Echo back
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                finally
                {
                    client.Close();
                }
            }
        }
    }
}