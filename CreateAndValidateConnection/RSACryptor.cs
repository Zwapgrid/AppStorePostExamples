using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace CreateAndValidateConnections
{
    public class RSACryptor
    {
        const int KeySize = 4096;

        public RSAParameters PEMStringToRSAKey(string keyString)
        {
            if (keyString == null)
                throw new ArgumentNullException(nameof(keyString));

            // https://stackoverflow.com/a/27743515
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (keyString.StartsWith(_byteOrderMarkUtf8, StringComparison.Ordinal))
            {
                keyString = keyString.Remove(0, _byteOrderMarkUtf8.Length);
            }

            using (var stringReader = new StringReader(keyString))
            {
                var obj = new PemReader(stringReader).ReadObject();
                if (obj is AsymmetricCipherKeyPair keyPair)
                {
                    return DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);
                }
                else if (obj is RsaKeyParameters publicKey)
                {
                    return DotNetUtilities.ToRSAParameters(publicKey);
                }
                else
                {
                    throw new Exception("Invalid key value");
                }
            }
        }

        public string Encrypt(string toEncrypt, RSAParameters publicKey)
        {
            if (toEncrypt == null)
                throw new ArgumentNullException(nameof(toEncrypt));

            using (var cryptoProvider = new RSACryptoServiceProvider(KeySize))
            {
                cryptoProvider.ImportParameters(publicKey);
                var encryptedBytes = cryptoProvider.Encrypt(Encoding.UTF8.GetBytes(toEncrypt), false);
                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }
}
