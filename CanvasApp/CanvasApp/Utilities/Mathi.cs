using System;
using System.Collections.Generic;
using System.Text;

namespace CanvasApp.Utilities
{
    class Mathi
    {
        /*e.g. get the highest non-0 bit's position
         *Log2(2) = 2
         */
        public static int Log2(int x)
        {
            int y = 0;
            while (x > 0)
            {
                x >>= 1;
                y++;
            }
            return y;
        }
    }
}
