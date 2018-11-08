using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp.Views.Forms;

namespace CanvasApp.Types
{
    class TouchPoint
    {
        public SKTouchAction type;
        public int x, y;
        public TouchPoint() { x = y = 0;type = SKTouchAction.Cancelled; }
        public TouchPoint(int x,int y, SKTouchAction type)
        {
            this.x = x;
            this.y = y;
            this.type = type;
        }
    }
}
