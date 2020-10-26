using System;
using System.Text;
using System.Transactions;

namespace LiveShareHub.Core.Extensions
{
    static public class StringExtensions
    {
        static public string RemoveNonNumericChars(this string str)
        {
            if (str == null)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder();

            for (int i = 0, to = str.Length; i < to; i++)
            {
                var c = str[i];
                if (c >= '0' && c <= '9')
                {
                    sb.Append(c.ToString());
                }
            }

            return sb.ToString();
        }

        static public (string username, string password) BasicAuthUsernamePassword(this string authValue)
        {
            if(authValue.StartsWith("Basic ", System.StringComparison.InvariantCultureIgnoreCase))
            {
                authValue = authValue.Substring("Basic ".Length);
            }

            authValue = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(authValue));

            if(!authValue.Contains(":"))
            {
                throw new Exception("Invalid authorization value");
            }

            return (authValue.Substring(0, authValue.IndexOf(":")), authValue.Substring(authValue.IndexOf(":") + 1));
        }
    }
}
