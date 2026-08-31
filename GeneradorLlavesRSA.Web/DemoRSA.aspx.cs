using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Web.Hosting;
using System.Web.UI;
using GeneradorLlavesRSA.Web.Crypto;

namespace GeneradorLlavesRSA.Web
{
    public partial class DemoRSA : Page
    {
        private bool requestAllowed;
        private bool keysReady;

        protected void Page_Load(object sender, EventArgs e)
        {
            requestAllowed = IsRequestAllowed();

            if (!requestAllowed)
            {
                Response.StatusCode = 403;
                Response.TrySkipIisCustomErrors = true;
                AccessDeniedPanel.Visible = true;
                RsaPanel.Visible = false;
                return;
            }

            try
            {
                string keyDirectory = GetKeyDirectory();
                EnsurePrivateKeyIsOutsideWebRoot(keyDirectory);

                PublicKeyDerField.Value = RsaKeyService.ReadPublicKeyDerBase64(keyDirectory);
                FingerprintLiteral.Text = RsaKeyService.GetPublicKeyFingerprintSha256(keyDirectory);
                keysReady = true;
            }
            catch (Exception ex) when (
                ex is ConfigurationErrorsException ||
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is FormatException ||
                ex is CryptographicException)
            {
                Trace.Warn("RSA", "No se pudo cargar el par de llaves: " + ex.GetType().Name);
                ShowServerError("No se pudo cargar el par de llaves. Revisa RsaKeyDirectory y los permisos de los archivos PEM.");
                SubmitCipherButton.Enabled = false;
            }
        }

        protected void SubmitCipherButton_Click(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;

            if (!requestAllowed || !keysReady)
                return;

            try
            {
                string plaintext = RsaKeyService.DecryptOaepSha256(
                    CiphertextField.Value,
                    GetKeyDirectory());

                DecryptedLiteral.Text = plaintext;
                ResultPanel.Visible = true;
                ServerMessagePanel.Visible = false;
                CiphertextField.Value = string.Empty;
            }
            catch (Exception ex) when (
                ex is FormatException ||
                ex is IOException ||
                ex is UnauthorizedAccessException ||
                ex is CryptographicException)
            {
                Trace.Warn("RSA", "No se pudo descifrar el criptograma: " + ex.GetType().Name);
                ShowServerError("No se pudo descifrar la solicitud. Confirma que utiliza la llave pública activa y RSA-OAEP/SHA-256.");
            }
        }

        private bool IsRequestAllowed()
        {
            bool allowRemote;
            bool.TryParse(ConfigurationManager.AppSettings["AllowRemoteRequests"], out allowRemote);
            return allowRemote || Request.IsLocal;
        }

        private static string GetKeyDirectory()
        {
            //se debe crear una variable de entorno
            string configuredDirectory = Environment.GetEnvironmentVariable("RSA_KEY_DIRECTORY");

            if (string.IsNullOrWhiteSpace(configuredDirectory))
                configuredDirectory = ConfigurationManager.AppSettings["RsaKeyDirectory"];

            if (string.IsNullOrWhiteSpace(configuredDirectory))
                throw new ConfigurationErrorsException("RsaKeyDirectory no está configurado.");

            string fullPath = Path.GetFullPath(configuredDirectory);

            if (!Directory.Exists(fullPath))
                throw new DirectoryNotFoundException("No existe el directorio configurado para las llaves RSA.");

            return fullPath;
        }

        private static void EnsurePrivateKeyIsOutsideWebRoot(string keyDirectory)
        {
            string webRoot = HostingEnvironment.MapPath("~/");

            if (string.IsNullOrWhiteSpace(webRoot))
                return;

            string normalizedWebRoot = Path.GetFullPath(webRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string privateKeyPath = Path.GetFullPath(Path.Combine(keyDirectory, "privateKey.pem"));

            if (privateKeyPath.StartsWith(normalizedWebRoot, StringComparison.OrdinalIgnoreCase))
                throw new ConfigurationErrorsException("La llave privada no puede estar dentro del directorio web.");
        }

        private void ShowServerError(string message)
        {
            ServerMessageLiteral.Text = message;
            ServerMessagePanel.Visible = true;
        }
    }
}
