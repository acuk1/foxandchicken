using System.Collections.Generic;

namespace foxandchicken.Models
{
    public class FoxMove
    {
        public int FromRow { get; set; }
        public int FromCol { get; set; }
        public int ToRow { get; set; }
        public int ToCol { get; set; }
        public List<(int, int)> EatenChickens { get; set; } = new List<(int, int)>();
    }
}