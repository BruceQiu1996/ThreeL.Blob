using System.Text.RegularExpressions;

namespace ThreeL.Blob.Infra.Core.Extensions.System
{
    public static class StringExtension
    {
        public static bool ValidPassword(this string pw)
        {
            Regex regex = new Regex("^(?=.*[a-zA-Z])(?=.*\\d).{6,16}$");

            return regex.IsMatch(pw);
        }

        public static bool ValidUserName(this string userName)
        {
            Regex regex = new Regex("^(?=.*\\d)(?=.*[A-z])[\\da-zA-Z]{6,16}$");

            return regex.IsMatch(userName);
        }
    }
}
