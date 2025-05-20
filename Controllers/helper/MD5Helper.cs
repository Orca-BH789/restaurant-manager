using System.Security.Cryptography;
using System.Text;

public static class MD5Helper
{
    public static string GetMD5Hash(string input)
    {
        using (var md5 = MD5.Create())
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = md5.ComputeHash(inputBytes);

            // Chuyển thành chuỗi hexa
            StringBuilder sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // x2: viết thường, X2: viết hoa
            }

            return sb.ToString();
        }
    }
}
