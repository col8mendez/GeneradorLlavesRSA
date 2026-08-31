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


DEMOSTRACIÓN ASP.NET WEB FORMS
------------------------------
La solución también contiene GeneradorLlavesRSA.Web, una página ASPX que:

1. Lee publicKey.pem en el servidor y entrega solamente la parte pública al
   navegador.
2. Cifra el texto en JavaScript con Web Crypto API, RSA-OAEP y SHA-256.
3. Envía únicamente el criptograma al backend C#.
4. Lee privateKey.pem fuera del directorio web y descifra con RSACng.

El par configurado inicialmente se encuentra en:
C:\Users\Luis\Downloads\GeneradorLlavesRSA_VS2019_NET48\GeneradorLlavesRSA_VS2019\GeneradorLlavesRSA\bin\Debug\RSA_Keys_20260831_093140

Para probarlo:

1. Abrir GeneradorLlavesRSA.sln en Visual Studio 2019.
2. Marcar GeneradorLlavesRSA.Web como proyecto de inicio.
3. Confirmar RsaKeyDirectory en GeneradorLlavesRSA.Web\Web.config.
4. Ejecutar DemoRSA.aspx con IIS Express y HTTPS.

La variable de entorno RSA_KEY_DIRECTORY puede reemplazar el valor de
Web.config sin cambiar archivos del proyecto.

SEGURIDAD DE LA DEMOSTRACIÓN WEB
--------------------------------
- AllowRemoteRequests está en false: solo acepta solicitudes locales.
- No se copia privateKey.pem al proyecto web ni se envía al navegador.
- Para exponer la función en una red se requiere autenticación, autorización,
  límites de uso, HTTPS y almacenamiento de la llave en un almacén protegido
  (por ejemplo, Windows Certificate Store o un gestor de secretos).
- El límite de RSA-OAEP/SHA-256 con una llave de 3072 bits es 318 bytes por
  operación; RSA no debe utilizarse para cifrar archivos o mensajes grandes.
