using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace majestic_player.core.Helpers
{
    public static class SearchQueryFormater
    {
        public static string Format(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return string.Empty;
            }

            var escapedQuery = Uri.EscapeDataString(query);
            return escapedQuery.Replace("%20", "+");
        }
    }
}
