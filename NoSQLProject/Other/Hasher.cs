using System.Security.Cryptography;
using System.Text;

namespace NoSQLProject.Other
{
    public class Hasher
    {   
        private static string saltString = ""; // Stores the salt value used for password hashing
        
        public static void SetSalt(string salt) // Set the salt value
        {
            saltString = salt;
        }
        
        public static byte[] GetHash(string input) // Get the hash bytes using PBKDF2
        {      
            byte[] salt = ASCIIEncoding.ASCII.GetBytes(saltString); // Convert the salt string to bytes array

            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(input, salt, 50000, HashAlgorithmName.SHA256))
            {  
                return pbkdf2.GetBytes(32);
            }
        }
       
        public static string GetHashedString(string input) // Convert hash bytes to a hex string
        {        
            byte[] hash = GetHash(input); // Get the hashed bytes

            // Convert the bytes to a hex string
            StringBuilder hex_sb = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                hex_sb.Append(hash[i].ToString("X2")); // Convert each byte to a hex value
            }

            return hex_sb.ToString();
        }
    }
}
