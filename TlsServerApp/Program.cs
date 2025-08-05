using System;
using System.Diagnostics;
using System.IO;

namespace TlsServerApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var certDirectory = Path.Combine(AppContext.BaseDirectory, "cert");
            var certFileName = "server.pfx";
            var certPath = Path.Combine(certDirectory, certFileName);

            if (!File.Exists(certPath))
            {
                var currentDirectory = Environment.CurrentDirectory;
                try
                {
                    Directory.CreateDirectory(certDirectory);
                    Environment.CurrentDirectory = certDirectory;
                    CreateCert(certFileName);
                }
                finally
                {
                    Environment.CurrentDirectory = currentDirectory;
                }
            }
            var server = new TlsEchoServer(certPath, 4433);
            server.StartAsync().Wait();
        }

        private static void CreateCert(string certFileName)
        {
            // 1. 秘密鍵を作成（2048bit RSA）
            Console.WriteLine(Execute("openssl genrsa -out secret.pem 2048"));

            // 2.自己署名証明書を作成（365日有効）
            Console.WriteLine(Execute("openssl req -new -x509 -key secret.pem -out server.pem -days 365 -subj \"/C=JP/O=SampleOrg/CN=localhost\""));

            // 3. .pfxファイルを作成
            Console.WriteLine(Execute($"openssl pkcs12 -export -out {certFileName} -inkey secret.pem -in server.pem -passout pass:"));
        }

        private static string Execute(string command)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            string output;
            using (var process = Process.Start(psi))
            {
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }
            return output;
        }
    }
}
