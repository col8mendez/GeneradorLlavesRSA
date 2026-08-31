(function () {
    "use strict";

    var maximumPayloadBytes = 318;

    function setStatus(message, isError) {
        var status = document.getElementById("ClientStatus");
        status.textContent = message;
        status.classList.toggle("error", Boolean(isError));
        status.classList.toggle("visible", Boolean(message));
    }

    function base64ToBytes(value) {
        var binary = window.atob(value);
        var bytes = new Uint8Array(binary.length);

        for (var index = 0; index < binary.length; index += 1) {
            bytes[index] = binary.charCodeAt(index);
        }

        return bytes;
    }

    function bytesToBase64(value) {
        var bytes = new Uint8Array(value);
        var binary = "";

        for (var index = 0; index < bytes.length; index += 1) {
            binary += String.fromCharCode(bytes[index]);
        }

        return window.btoa(binary);
    }

    async function encryptAndSubmit() {
        var encryptButton = document.getElementById("EncryptButton");
        var plaintextInput = document.getElementById("plainText");
        var publicKeyField = document.getElementById("PublicKeyDerField");
        var ciphertextField = document.getElementById("CiphertextField");
        var submitButton = document.getElementById("SubmitCipherButton");

        if (!window.crypto || !window.crypto.subtle || !window.TextEncoder) {
            setStatus("Este navegador no ofrece Web Crypto API en el contexto actual. Usa HTTPS o localhost.", true);
            return;
        }

        var plaintextBytes = new TextEncoder().encode(plaintextInput.value);

        if (plaintextBytes.length === 0) {
            setStatus("Escribe un texto antes de cifrar.", true);
            return;
        }

        if (plaintextBytes.length > maximumPayloadBytes) {
            setStatus("El texto ocupa " + plaintextBytes.length + " bytes; el máximo es 318.", true);
            return;
        }

        encryptButton.disabled = true;
        setStatus("Cifrando con la llave pública...", false);

        try {
            var publicKey = await window.crypto.subtle.importKey(
                "spki",
                base64ToBytes(publicKeyField.value),
                { name: "RSA-OAEP", hash: "SHA-256" },
                false,
                ["encrypt"]);

            var ciphertext = await window.crypto.subtle.encrypt(
                { name: "RSA-OAEP" },
                publicKey,
                plaintextBytes);

            ciphertextField.value = bytesToBase64(ciphertext);
            plaintextInput.value = "";
            setStatus("Criptograma listo. Enviando al backend C#...", false);
            submitButton.click();
        } catch (error) {
            setStatus("No fue posible cifrar el texto con la llave pública configurada.", true);
            encryptButton.disabled = false;
        }
    }

    document.addEventListener("DOMContentLoaded", function () {
        var encryptButton = document.getElementById("EncryptButton");

        if (encryptButton) {
            encryptButton.addEventListener("click", encryptAndSubmit);
        }
    });
}());
