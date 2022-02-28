using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace QuestonsAndAnswers.Model
{
    public static class Extensions
    {
        private static readonly string globalSalt = ConfigurationManager.AppSettings["globalSalt"] ?? string.Empty;

        public static string ToHash(this string input, string inputsalt)
        {

            var stringContainer = new StringBuilder();

            using (var hasher = SHA256.Create())
            {
                var encoding = Encoding.UTF8;
                var byteHash = hasher.ComputeHash(encoding.GetBytes(string.Concat(inputsalt, input, globalSalt)));

                foreach (byte b in byteHash)
                    stringContainer.Append(b.ToString("x2"));
            }

            return stringContainer.ToString();
        }



    }
}
