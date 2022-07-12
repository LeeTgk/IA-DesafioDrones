using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INF1771_GameAI.Map
{
    public class Position
    {
        public int x { get; set; }
        public int y { get; set; }

        public Position()
        {
            x = 0;
            y = 0;
        }
        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
