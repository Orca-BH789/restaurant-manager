using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


namespace BASIC_PROJECT.Controllers.helper
{
	public static class SHA256Helper
	{
        public static string GetSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);

                // Chuyển thành chuỗi hexa
                StringBuilder sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // x2: viết thường
                }

                return sb.ToString();
            }
        }
    }
}