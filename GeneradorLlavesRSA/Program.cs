using System;
using System.IO;
using System.Security.Cryptography;

namespace GeneradorLlavesRSA
{
    internal class Program
    {
        private const int KeySize = 3072;

        private static void Main(string[] args)
        {
            Console.Title = "Generador de llaves RSA";

            try
            {
                Console.WriteLine("===============================================");
                Console.WriteLine("      GENERADOR DE LLAVES RSA - Microfinanzas 2026");
                Console.WriteLine("===============================================");
                Console.WriteLine();
                Console.WriteLine("Tamaño de llave : {0} bits", KeySize);
                Console.WriteLine("Llave pública   : SubjectPublicKeyInfo (PEM)");
                Console.WriteLine("Llave privada   : PKCS#8 (PEM)");
                Console.WriteLine();

                // Se crea una carpeta distinta en cada ejecución para evitar
                // sobrescribir accidentalmente llaves existentes.
                string outputDirectory = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "RSA_Keys_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));

                Directory.CreateDirectory(outputDirectory);

                string publicKeyPath = Path.Combine(outputDirectory, "publicKey.pem");
                string privateKeyPath = Path.Combine(outputDirectory, "privateKey.pem");

                // RSACryptoServiceProvider está disponible de forma nativa en
                // .NET Framework 4.8, por lo que el proyecto no requiere NuGet.
                using (var rsa = new RSACryptoServiceProvider(KeySize))
                {
                    // Evita que Windows conserve la llave en el contenedor CSP.
                    rsa.PersistKeyInCsp = false;

                    RSAParameters publicParameters = rsa.ExportParameters(false);
                    RSAParameters privateParameters = rsa.ExportParameters(true);

                    string publicPem = RsaPemExporter.ExportPublicKey(publicParameters);
                    string privatePem = RsaPemExporter.ExportPrivateKeyPkcs8(privateParameters);

                    File.WriteAllText(publicKeyPath, publicPem);
                    File.WriteAllText(privateKeyPath, privatePem);
                }

                Console.WriteLine("Llaves generadas correctamente.");
                Console.WriteLine();
                Console.WriteLine("Carpeta:");
                Console.WriteLine(outputDirectory);
                Console.WriteLine();
                Console.WriteLine("Archivos:");
                Console.WriteLine("  - publicKey.pem");
                Console.WriteLine("  - privateKey.pem");
                Console.WriteLine();
                Console.WriteLine("IMPORTANTE:");
                Console.WriteLine("La llave privada NO debe publicarse ni enviarse al frontend.");
                Console.WriteLine("Únicamente la llave pública debe ser utilizada por JavaScript.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            Console.WriteLine();
            Console.WriteLine("Presione una tecla para cerrar...");
            Console.ReadKey();
        }
    }
}
