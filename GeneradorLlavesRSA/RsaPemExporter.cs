using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GeneradorLlavesRSA
{
    /// <summary>
    /// Exporta RSAParameters a formatos PEM estándar sin dependencias externas.
    ///
    /// Llave pública:
    ///   X.509 SubjectPublicKeyInfo / BEGIN PUBLIC KEY
    ///
    /// Llave privada:
    ///   PKCS#8 PrivateKeyInfo / BEGIN PRIVATE KEY
    ///
    /// Esto permite utilizar la llave pública desde Web Crypto API y mantener
    /// la llave privada únicamente en el backend.
    /// </summary>
    internal static class RsaPemExporter
    {
        private static readonly byte[] RsaEncryptionAlgorithmIdentifier =
        {
            0x30, 0x0D,
            0x06, 0x09,
            0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01,
            0x05, 0x00
        };

        public static string ExportPublicKey(RSAParameters parameters)
        {
            // RSAPublicKey ::= SEQUENCE {
            //     modulus           INTEGER,
            //     publicExponent    INTEGER
            // }
            byte[] rsaPublicKey = EncodeSequence(
                EncodeInteger(parameters.Modulus),
                EncodeInteger(parameters.Exponent));

            // SubjectPublicKeyInfo ::= SEQUENCE {
            //     algorithm         AlgorithmIdentifier,
            //     subjectPublicKey  BIT STRING
            // }
            byte[] subjectPublicKeyInfo = EncodeSequence(
                RsaEncryptionAlgorithmIdentifier,
                EncodeBitString(rsaPublicKey));

            return ToPem("PUBLIC KEY", subjectPublicKeyInfo);
        }

        public static string ExportPrivateKeyPkcs8(RSAParameters parameters)
        {
            // RSAPrivateKey ::= SEQUENCE {
            //     version           INTEGER,
            //     modulus           INTEGER,
            //     publicExponent    INTEGER,
            //     privateExponent   INTEGER,
            //     prime1            INTEGER,
            //     prime2            INTEGER,
            //     exponent1         INTEGER,
            //     exponent2         INTEGER,
            //     coefficient       INTEGER
            // }
            byte[] rsaPrivateKey = EncodeSequence(
                EncodeInteger(new byte[] { 0x00 }),
                EncodeInteger(parameters.Modulus),
                EncodeInteger(parameters.Exponent),
                EncodeInteger(parameters.D),
                EncodeInteger(parameters.P),
                EncodeInteger(parameters.Q),
                EncodeInteger(parameters.DP),
                EncodeInteger(parameters.DQ),
                EncodeInteger(parameters.InverseQ));

            // PrivateKeyInfo (PKCS#8) ::= SEQUENCE {
            //     version                   INTEGER,
            //     privateKeyAlgorithm       AlgorithmIdentifier,
            //     privateKey                OCTET STRING
            // }
            byte[] privateKeyInfo = EncodeSequence(
                EncodeInteger(new byte[] { 0x00 }),
                RsaEncryptionAlgorithmIdentifier,
                EncodeOctetString(rsaPrivateKey));

            return ToPem("PRIVATE KEY", privateKeyInfo);
        }

        private static byte[] EncodeInteger(byte[] value)
        {
            if (value == null || value.Length == 0)
                value = new byte[] { 0x00 };

            int start = 0;

            // Elimina ceros innecesarios al inicio sin dejar el entero vacío.
            while (start < value.Length - 1 && value[start] == 0x00)
                start++;

            int length = value.Length - start;
            bool needsLeadingZero = (value[start] & 0x80) != 0;

            byte[] content = new byte[length + (needsLeadingZero ? 1 : 0)];

            if (needsLeadingZero)
            {
                content[0] = 0x00;
                Buffer.BlockCopy(value, start, content, 1, length);
            }
            else
            {
                Buffer.BlockCopy(value, start, content, 0, length);
            }

            return EncodeDerValue(0x02, content);
        }

        private static byte[] EncodeSequence(params byte[][] values)
        {
            return EncodeDerValue(0x30, Combine(values));
        }

        private static byte[] EncodeOctetString(byte[] value)
        {
            return EncodeDerValue(0x04, value);
        }

        private static byte[] EncodeBitString(byte[] value)
        {
            // Primer byte = cantidad de bits no utilizados en el último byte.
            byte[] content = new byte[value.Length + 1];
            content[0] = 0x00;
            Buffer.BlockCopy(value, 0, content, 1, value.Length);
            return EncodeDerValue(0x03, content);
        }

        private static byte[] EncodeDerValue(byte tag, byte[] value)
        {
            byte[] length = EncodeLength(value.Length);
            byte[] result = new byte[1 + length.Length + value.Length];

            result[0] = tag;
            Buffer.BlockCopy(length, 0, result, 1, length.Length);
            Buffer.BlockCopy(value, 0, result, 1 + length.Length, value.Length);

            return result;
        }

        private static byte[] EncodeLength(int length)
        {
            if (length < 0x80)
                return new byte[] { (byte)length };

            var bytes = new List<byte>();
            int temp = length;

            while (temp > 0)
            {
                bytes.Insert(0, (byte)(temp & 0xFF));
                temp >>= 8;
            }

            byte[] result = new byte[bytes.Count + 1];
            result[0] = (byte)(0x80 | bytes.Count);

            for (int i = 0; i < bytes.Count; i++)
                result[i + 1] = bytes[i];

            return result;
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            int totalLength = 0;

            foreach (byte[] array in arrays)
                totalLength += array.Length;

            byte[] result = new byte[totalLength];
            int offset = 0;

            foreach (byte[] array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        private static string ToPem(string label, byte[] derData)
        {
            string base64 = Convert.ToBase64String(derData);
            var builder = new StringBuilder();

            builder.AppendLine("-----BEGIN " + label + "-----");

            // PEM normalmente utiliza líneas de 64 caracteres.
            for (int i = 0; i < base64.Length; i += 64)
            {
                int count = Math.Min(64, base64.Length - i);
                builder.AppendLine(base64.Substring(i, count));
            }

            builder.AppendLine("-----END " + label + "-----");
            return builder.ToString();
        }
    }
}
