using System.Text;

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
    }
}
