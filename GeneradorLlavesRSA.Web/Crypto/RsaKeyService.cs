using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace GeneradorLlavesRSA.Web.Crypto
{
    internal static class RsaKeyService
    {
        public const int ExpectedKeySizeBits = 3072;
        public const int CiphertextLengthBytes = ExpectedKeySizeBits / 8;
        public const int MaximumOaepSha256PayloadBytes = CiphertextLengthBytes - (2 * 32) - 2;

        public static string ReadPublicKeyDerBase64(string keyDirectory)
        {
            byte[] publicKeyDer = ReadPem(
                Path.Combine(keyDirectory, "publicKey.pem"),
                "PUBLIC KEY");

            try
            {
                return Convert.ToBase64String(publicKeyDer);
            }
            finally
            {
                Array.Clear(publicKeyDer, 0, publicKeyDer.Length);
            }
        }

        public static string GetPublicKeyFingerprintSha256(string keyDirectory)
        {
            byte[] publicKeyDer = ReadPem(
                Path.Combine(keyDirectory, "publicKey.pem"),
                "PUBLIC KEY");

            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hash = sha256.ComputeHash(publicKeyDer);
                    return BitConverter.ToString(hash).Replace("-", ":");
                }
            }
            finally
            {
                Array.Clear(publicKeyDer, 0, publicKeyDer.Length);
            }
        }

        public static string DecryptOaepSha256(string ciphertextBase64, string keyDirectory)
        {
            if (string.IsNullOrWhiteSpace(ciphertextBase64))
                throw new FormatException("No se recibió un criptograma.");

            byte[] ciphertext = Convert.FromBase64String(ciphertextBase64);
            byte[] privateKeyDer = null;
            byte[] plaintext = null;

            try
            {
                if (ciphertext.Length != CiphertextLengthBytes)
                    throw new CryptographicException("El criptograma no corresponde a RSA de 3072 bits.");

                privateKeyDer = ReadPem(
                    Path.Combine(keyDirectory, "privateKey.pem"),
                    "PRIVATE KEY");

                using (CngKey key = CngKey.Import(privateKeyDer, CngKeyBlobFormat.Pkcs8PrivateBlob))
                using (var rsa = new RSACng(key))
                {
                    if (rsa.KeySize != ExpectedKeySizeBits)
                        throw new CryptographicException("La llave privada no es RSA de 3072 bits.");

                    plaintext = rsa.Decrypt(ciphertext, RSAEncryptionPadding.OaepSHA256);
                }

                return new UTF8Encoding(false, true).GetString(plaintext);
            }
            finally
            {
                Array.Clear(ciphertext, 0, ciphertext.Length);

                if (privateKeyDer != null)
                    Array.Clear(privateKeyDer, 0, privateKeyDer.Length);

                if (plaintext != null)
                    Array.Clear(plaintext, 0, plaintext.Length);
            }
        }

        private static byte[] ReadPem(string path, string label)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException("No se encontró el archivo PEM configurado.", path);

            var fileInfo = new FileInfo(path);
            if (fileInfo.Length <= 0 || fileInfo.Length > 16384)
                throw new FormatException("El tamaño del archivo PEM no es válido.");

            string pem = File.ReadAllText(path, Encoding.ASCII);
            string beginMarker = "-----BEGIN " + label + "-----";
            string endMarker = "-----END " + label + "-----";
            int begin = pem.IndexOf(beginMarker, StringComparison.Ordinal);
            int end = pem.IndexOf(endMarker, StringComparison.Ordinal);

            if (begin < 0 || end <= begin)
                throw new FormatException("El archivo no contiene un bloque PEM " + label + " válido.");

            begin += beginMarker.Length;
            string base64 = pem.Substring(begin, end - begin)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace(" ", string.Empty)
                .Replace("\t", string.Empty);

            return Convert.FromBase64String(base64);
        }
    }
}
