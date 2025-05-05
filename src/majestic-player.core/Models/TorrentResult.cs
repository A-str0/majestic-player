using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace majestic_player.core.Models
{
    public class TorrentResult
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string InfoHash { get; set; }
        public string MagnetLink { get; set; }
        public ulong Size { get; set; }
        public uint Seeders { get; set; }
        public uint Leechers { get; set; }
        public ushort NumFiles { get; set; }
        public string Username { get; set; }
        public string Added { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string Imdb { get; set; }
    }
}
