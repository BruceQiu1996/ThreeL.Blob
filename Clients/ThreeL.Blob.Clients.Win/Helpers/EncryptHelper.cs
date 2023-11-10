using System;

namespace ThreeL.Blob.Clients.Win.Helpers
{
    public class EncryptHelper
    {
        public string Encrypt(string plainText)
        {
            byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(encbuff);
        }

        public string Decrypt(string cipherText)
        {
            byte[] decbuff = Convert.FromBase64String(cipherText);
            return System.Text.Encoding.UTF8.GetString(decbuff);
        }
    }
}
