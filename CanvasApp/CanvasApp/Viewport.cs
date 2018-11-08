using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SkiaSharp;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp.Views.Forms;

namespace CanvasApp
{

    class Viewport
    {
        static Viewport _self;
        public static Viewport Get() { if (_self == null) _self = new Viewport(); return _self; }

        public SKBitmap buffer;
        IntPtr _pixels;

        float _scale = 1;

        //------------------Test contents-------------------------------
        byte[] _backPixelBuffer;
        Types.ScalablePixelTree pixelTree = new Types.ScalablePixelTree();
        Utilities.Queue<Types.TouchPoint> points = new Utilities.Queue<Types.TouchPoint>();
        Task renderer;
        SKCanvasView canvas;
        void RendererMain()
        {
            while (true)
            {
                if (buffer != null)
                {
                    lock (buffer)
                    {
                        while (points.GetDepth() > 0)
                        {
                            //Get a point
                            Types.TouchPoint p = points.Pop();
                            if (p.type != SKTouchAction.Cancelled)
                            {
                                //Draw point
                                SetPixelBack(p.x, p.y, SKColors.Red);
                            }
                        }
                        //Render viewport
                        //fillJob?.DoOnce();
                        Utilities.ParallelJobManager.Get().DoJob(Fill);
                        //issue redraw
                        canvas?.InvalidateSurface();
                    }
                }
                //Pause renderer
                    lock (points)
                        Monitor.Wait(points);
            }
        }

        void Fill(int index, int total)
        {
            if (_backPixelBuffer == null) return;
            unsafe
            {
                byte* p = (byte*)_pixels.ToPointer();

                for (int x = index; x < _backPixelBuffer.Length; x += total)
                {
                    p[x] = _backPixelBuffer[x];
                }
            }
        }

        public void AddPoint(int x, int y)
        {
            points.Push(new Types.TouchPoint(x, y, SKTouchAction.Pressed));
            lock (points)
                Monitor.Pulse(points);
        }
        //--------------------------------------------------------------

        public int width = 0;//{ get { if (buffer == null) return 0; else return (int)buffer.Width; } }
        public int height = 0;// { get { if (buffer == null) return 0; else return (int)buffer.Height; } }

        

        Viewport()
        {
            
        }

        Viewport(int width, int height)
        {
            Resize(width, height);
            _self = this;
        }

        public void SetupRendererThread(SKCanvasView canvas)
        {
            this.canvas = canvas;
            renderer = new Task(RendererMain);
            renderer.Start();
            //SetupRendererSubThread(1);
        }

        private void _resize(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                buffer = null; this.width = 0; this.height = 0;
                return;
            }

            this.width = width; this.height = height;
            buffer = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _pixels = buffer.GetPixels();
            _backPixelBuffer = new byte[width * height * 4];
            lock (points)
                Monitor.Pulse(points);
        }

        public void Resize(int width, int height)
        {
            if (buffer == null)
            {
                _resize(width, height);
            }
            else
            {
                lock (buffer) _resize(width, height);
            }
        }

        bool CheckPos(int x, int y)
        {
            if (_pixels == null||
                x<0||x>= width||
                y<0||y>=height) return false;

            return true;
        }

        public void SetPixel(int x, int y, SKColor color)
        {
            if (!CheckPos(x,y)) return;
            //Int32 rawColor = color.Alpha << 24 | color.Red << 16 | color.Green << 8 | color.Blue;
            //Marshal.WriteInt32(IntPtr.Add(_pixels,( y * width + x)*4), rawColor);

            unsafe
            {
                byte* p = (byte*)_pixels.ToPointer();
                int index = (y * width + x) * 4;
                p[index++] = color.Blue;
                p[index++] = color.Green;
                p[index++] = color.Red;
                p[index] = color.Alpha;
            }
            //Marshal.WriteByte(IntPtr.Add(_pixels, (y * width + x) * 4), color.Blue);
            //Marshal.WriteByte(IntPtr.Add(_pixels, (y * width + x) * 4 + 1), color.Green);
            //Marshal.WriteByte(IntPtr.Add(_pixels, (y * width + x) * 4 + 2), color.Red);
            //Marshal.WriteByte(IntPtr.Add(_pixels, (y * width + x) * 4 + 3), color.Alpha);
            //_pixels[y * width + x] = color;
        }

        public void SetPixelBack(int x, int y, SKColor color)
        {
            if (!CheckPos(x, y)) return;

            int index = (y * width + x) * 4;
            _backPixelBuffer[index++] = color.Blue;
            _backPixelBuffer[index++] = color.Green;
            _backPixelBuffer[index++] = color.Red;
            _backPixelBuffer[index] = color.Alpha;

        }
    }
}
