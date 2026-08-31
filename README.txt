GENERADOR DE LLAVES RSA - VISUAL STUDIO 2019 / .NET FRAMEWORK 4.8
=================================================================

1. Abrir:
   GeneradorLlavesRSA.sln

2. Compilar en Visual Studio 2019:
   Build > Build Solution

3. Ejecutar el proyecto.

4. El programa crea junto al ejecutable una carpeta:
   RSA_Keys_yyyyMMdd_HHmmss

5. Archivos generados:
   publicKey.pem
      - X.509 SubjectPublicKeyInfo
      - Encabezado: -----BEGIN PUBLIC KEY-----
      - Esta llave puede entregarse al frontend.

   privateKey.pem
      - PKCS#8 PrivateKeyInfo
      - Encabezado: -----BEGIN PRIVATE KEY-----
      - Esta llave debe permanecer exclusivamente en el servidor.

CARACTERÍSTICAS
---------------
- .NET Framework 4.8
- Visual Studio 2019
- C#
- RSA 3072 bits
- Sin paquetes NuGet
- Sin dependencias externas
- Compatible con una implementación posterior de RSA-OAEP / SHA-256.

NOTA DE SEGURIDAD
-----------------
La llave privada no debe colocarse dentro del sitio web, Scripts, Content,
wwwroot ni ningún directorio que IIS pueda servir directamente.

El cifrado RSA de la contraseña es una capa adicional a nivel aplicación.
HTTPS/TLS debe continuar habilitado obligatoriamente.
