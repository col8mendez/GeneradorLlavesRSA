<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DemoRSA.aspx.cs" Inherits="GeneradorLlavesRSA.Web.DemoRSA" %>

<!DOCTYPE html>
<html lang="es">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Demostración RSA-OAEP</title>
    <link rel="stylesheet" href="Content/site.css" />
    <script src="Scripts/rsa-demo.js" defer></script>
</head>
<body>
    <main class="shell">
        <section class="card" aria-labelledby="page-title">
            <p class="eyebrow">ASP.NET Web Forms · .NET Framework 4.8</p>
            <h1 id="page-title">RSA-OAEP con backend C#</h1>
            <p class="intro">
                El navegador cifra el texto con la llave pública. El servidor recibe
                únicamente el criptograma y lo descifra con la llave privada.
            </p>

            <asp:Panel ID="AccessDeniedPanel" runat="server" Visible="false" CssClass="notice error" role="alert">
                Esta demostración está configurada para aceptar solamente solicitudes locales.
            </asp:Panel>

            <asp:Panel ID="RsaPanel" runat="server">
                <form id="RsaForm" runat="server">
                    <asp:HiddenField ID="PublicKeyDerField" runat="server" ClientIDMode="Static" />
                    <asp:HiddenField ID="CiphertextField" runat="server" ClientIDMode="Static" />

                    <div class="key-summary">
                        <span>Llave activa</span>
                        <strong>RSA 3072 · OAEP SHA-256</strong>
                        <code><asp:Literal ID="FingerprintLiteral" runat="server" Mode="Encode" /></code>
                    </div>

                    <label for="plainText">Texto para cifrar</label>
                    <textarea id="plainText" rows="5" autocomplete="off" spellcheck="false"
                        placeholder="Escribe un texto de hasta 318 bytes"></textarea>
                    <p class="hint">
                        El campo no tiene atributo <code>name</code>, por lo que el texto claro
                        no forma parte del POST.
                    </p>

                    <div id="ClientStatus" class="notice" role="status" aria-live="polite"></div>
                    <div class="actions">
                        <button id="EncryptButton" type="button">Cifrar y enviar al servidor</button>
                        <asp:Button ID="SubmitCipherButton" runat="server" ClientIDMode="Static"
                            CssClass="server-submit" Text="Enviar criptograma"
                            OnClick="SubmitCipherButton_Click" />
                    </div>

                    <asp:Panel ID="ServerMessagePanel" runat="server" Visible="false" CssClass="notice error" role="alert">
                        <asp:Literal ID="ServerMessageLiteral" runat="server" Mode="Encode" />
                    </asp:Panel>

                    <asp:Panel ID="ResultPanel" runat="server" Visible="false" CssClass="result" aria-live="polite">
                        <span>Texto recuperado por C#</span>
                        <pre><asp:Literal ID="DecryptedLiteral" runat="server" Mode="Encode" /></pre>
                    </asp:Panel>
                </form>
            </asp:Panel>
        </section>
    </main>
</body>
</html>
