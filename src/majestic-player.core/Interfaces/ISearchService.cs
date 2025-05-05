using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using majestic_player.core.Models;

namespace majestic_player.core.Interfaces
{
    /// <summary>
    /// Interface for search
    /// </summary>
    public interface ISearchService<T>
    {
        public Task SearchAsync(string query, string category = "101");
    }
}
