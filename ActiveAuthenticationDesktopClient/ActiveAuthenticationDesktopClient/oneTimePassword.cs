using System;
using System.Collections.Generic;
using System.Text;

namespace ActiveAuthenticationDesktopClient
{
    class oneTimePassword
    {
        // Chars that can appear in passwords
        private static char[] chars = {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'v', 'w', 'x', 'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        public string otp { get; private set; }
        private DateTime expiry;
        /// <summary>
        /// Creates a one time password to be used for authentication
        /// </summary>
        public oneTimePassword()
        {
            StringBuilder sb = new StringBuilder();
            Random r = new Random();
            sb.Append(chars[r.Next(chars.Length)]);
            sb.Append(chars[r.Next(chars.Length)]);
            sb.Append(chars[r.Next(chars.Length)]);
            sb.Append(chars[r.Next(chars.Length)]);
            sb.Append(chars[r.Next(chars.Length)]);
            sb.Append(chars[r.Next(chars.Length)]);
            sb.Append(chars[r.Next(chars.Length)]);
            otp = sb.ToString();
            expiry = DateTime.Now.AddMinutes(10);
        }
        /// <summary>
        /// Checks if the one time password is expired
        /// </summary>
        /// <returns>Whether or not the password is expired</returns>
        public bool IsExpired()
        {
            return (expiry < DateTime.Now);
        }
        /// <summary>
        /// Checks if the password entered matches the one that was sent
        /// </summary>
        /// <param name="password"></param>
        /// <returns>Whether or not the password matches</returns>
        public bool PasswordMatch(string password)
        {
            if (!IsExpired())
            {
                return (password == otp);
            }
            return false;
        }
    }
}
