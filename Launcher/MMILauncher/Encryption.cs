// SPDX-License-Identifier: MIT
// The content of this file has been developed in the context of the MOSIM research project.
// Original author(s): Adam Klodowski

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Encryption
{
    public class Encrypt
    {
        private const string regPassFile = ".taskEditorUnity";
        private string regPass = "";

        public Encrypt()
        {
            if (File.Exists(UserDir() + regPassFile))
                ReadRegPass();
            else
                CreateRegPass();
        }

        public string UserDir()
        {
            string userdir = Environment.GetEnvironmentVariable("userprofile");
            if (userdir.LastIndexOf('\\') != userdir.Length - 1)
            userdir += "\\";
            return userdir;
        }

        private void ReadRegPass()
        {
            if (File.Exists(UserDir() + regPassFile))
            {
               regPass = System.IO.File.ReadAllText(UserDir() + regPassFile);
            }
        }

        private void CreateRegPass()
        {
            const string chars = "QWERTYUIOPASDFGHJKLMNBVCXZqwertyuioplkjhgfdaszxcvbnm!@#$%^&*()_+=-{}{}|:;?/><,.1234567890";
            var rand = new System.Random();
            regPass = "";
            for (int i = 0; i < 32; i++)
            regPass += chars[rand.Next(0, chars.Length)];

            System.IO.File.WriteAllText(UserDir() + regPassFile, regPass);
            try
            {
                File.Encrypt(UserDir() + regPassFile);
            }
            catch
            {

            }

        }

        public string EncryptString(string plainText, string key = "")
        {
            if (key == "")
                key = regPass;
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(array);
        }

        public string DecryptString(string cipherText, string key="")
        {
            if (key == "")
                key = regPass;
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

    }
}
