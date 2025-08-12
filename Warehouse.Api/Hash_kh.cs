using System.Security.Cryptography;
using System.Text;

namespace Warehouse.WebApi
{
    public class Hash_kh
    {
        static public string GetHash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
