using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using RSG;
using SimoraUnity.Http;
using UnityEngine;
using Newtonsoft.Json;

namespace SimoraUnity.Activation
{
    public class Kms : Client
    {
        // 配置文件名（放在_Data文件夹内）
        private const string CONFIG_FILE = "kms_config.txt";

        private string productName;
        private string productSecret;

        [TextArea]
        public string publicKey;


        // 确保配置已加载
        private void EnsureConfigLoaded()
        {

            string configPath = Path.Combine(Application.dataPath, CONFIG_FILE);
            Debug.Log($"Loading KMS config from: {configPath}");

            // 如果配置文件不存在则创建默认配置
            if (!File.Exists(configPath))
            {
                CreateDefaultConfig(configPath);
                Debug.Log($"Created default KMS config at {configPath}");
            }

            try
            {
                string[] lines = File.ReadAllLines(configPath);
                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    int separatorIndex = line.IndexOf('=');
                    if (separatorIndex <= 0) continue;

                    string key = line.Substring(0, separatorIndex).Trim();
                    string value = line.Substring(separatorIndex + 1).Trim();

                    switch (key)
                    {
                        case "baseUrl":
                            baseUrl = value;
                            Debug.LogError("baseUrl = " + value);
                            break;
                        case "enableRequestLogs":
                            if (int.TryParse(value, out int intValue))
                            {
                                enableRequestLogs = intValue != 0;
                                Debug.LogError("enableRequestLogs = " + value);
                            }
                            break;
                        case "productName":
                            productName = value;
                            Debug.LogError("productName = " + value);
                            break;
                        case "productSecret":
                            productSecret = value;
                            Debug.LogError("productSecret = " + value);
                            break;
                    }
                }
                Debug.Log("Loaded KMS configuration successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load KMS config: {ex.Message}");
            }
        }

        // 创建默认配置文件
        private void CreateDefaultConfig(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine("baseUrl=");
                writer.WriteLine("enableRequestLogs=0");
                writer.WriteLine("productName=");
                writer.WriteLine("productSecret=");
            }
        }

        public IPromise<string> GetDeviceLicense()
        {
            EnsureConfigLoaded(); // 确保已加载配置

            if (string.IsNullOrWhiteSpace(publicKey))
            {
                throw new Exception("Missing public key");
            }
            else if (string.IsNullOrWhiteSpace(productSecret))
            {
                throw new Exception("Missing product secret");
            }

            return Post<Dictionary<string, string>>("/v1/devices", new Dictionary<string, string>
            {
                {"product", productName ?? Application.productName},
                {"secret", Encrypt(publicKey, productSecret)},
                {"device_id", SystemInfo.deviceUniqueIdentifier}
            }).Then((response) =>
            {
                if (!response.Success)
                {
                    throw new Exception(response.Message);
                }
                return response.Data["key"];
            });
        }

        public License DecryptLicenseFromKeyString(string key)
        {
            EnsureConfigLoaded();
            var json = Decrypt(publicKey, key);

            // 使用Newtonsoft.Json替代JsonUtility
            var jsonString = Encoding.UTF8.GetString(json);
            var license = JsonConvert.DeserializeObject<License>(jsonString);

            license.rightProduct = productName == "" ? Application.productName : productName;
            return license;
        }

        public static string Encrypt(string publicKey, string content)
        {
            string encryptedContent;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(RsaPublicKey2Xml(publicKey));
                var encryptedData = rsa.Encrypt(Encoding.Default.GetBytes(content), false);
                encryptedContent = Convert.ToBase64String(encryptedData);
            }

            return encryptedContent;
        }

        public static byte[] Decrypt(String publicKey, String data)
        {
            String xmlPublicKey = RsaPublicKey2Xml(publicKey.Trim());

            RSACryptoServiceProvider publicRsa = new RSACryptoServiceProvider();
            publicRsa.FromXmlString(xmlPublicKey);
            AsymmetricKeyParameter keyPair = DotNetUtilities.GetRsaPublicKey(publicRsa);
            IBufferedCipher c = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
            c.Init(false, keyPair);
            var dataToEncrypt = Convert.FromBase64String(data);
            var outBytes = c.DoFinal(dataToEncrypt);
            return outBytes;
        }

        private static string RsaPublicKey2Xml(string publicKey)
        {
            var publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
            return
                $"<RSAKeyValue><Modulus>{Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned())}</Modulus><Exponent>{Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned())}</Exponent></RSAKeyValue>";
        }
    }
}