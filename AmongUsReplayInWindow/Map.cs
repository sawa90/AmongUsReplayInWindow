using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmongUsReplayInWindow
{
    public static class Map
    {
        public struct MapScale
        {
            public float hw, xs, ys, xp, yp;
        }

        static public MapScale[] Maps = new MapScale[3]
        { new MapScale
            {
                hw = 0.58f,
                xs = 0.022f,
                ys = 0.038f,
                xp = 0.548f,
                yp = 0.292f,
            },
            new MapScale
            {
                hw = 0.73f,
                xs = 0.0223f,
                ys = 0.0305f,
                xp = 0.305f,
                yp = 0.84f,
            },
            new MapScale
            {
                hw = 0.69f,
                xs = 0.0232f,
                ys = 0.0335f,
                xp = 0.02275f,
                yp = 0.0885f,
            }
        };
    }
}
