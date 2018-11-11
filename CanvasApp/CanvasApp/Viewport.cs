using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SkiaSharp;
using System.Threading;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;

namespace CanvasApp
{

    class Viewport:IDisposable
    {
        //static Viewport _self;
        //public static Viewport Get() { if (_self == null) _self = new Viewport(); return _self; }

        public SKBitmap buffer;
        IntPtr _pixels;

        float _scale = 1;
        public int depth = 1;
        public int posx = 0, posy = 0;

        //------------------Test contents-------------------------------
        byte[,] _backPixelBuffer;
        public Types.ScalablePixelTree pixelTree = new Types.ScalablePixelTree();
        Utilities.Queue<Types.TouchPoint> points = new Utilities.Queue<Types.TouchPoint>();
        Thread renderer;
        ManualResetEventSlim renlocker = new ManualResetEventSlim(false);
        SKCanvasView canvas;
        bool _rndManRun;
        void RendererMain()
        {
            while (true)
            {
                renlocker.Wait();
               
                if (points.GetDepth() <= 0)
                    renlocker.Reset();

                if (!_rndManRun) break;
                while (points.GetDepth() > 0)
                {
                    //Get a point
                    Types.TouchPoint p = points.Pop();
                    if (p.type != SKTouchAction.Cancelled)
                    {
                        //Draw point
                        //SetPixelBack(p.x, p.y, SKColors.Red);
                        depth += pixelTree.SetPixel(p.x + posx, p.y+posy, depth, SKColors.Red);
                    }
                }
                //Render viewport
                lock (this)
                    //Lock viewport during rendering
                    Utilities.ParallelJobManager.Get().DoJob(FillFromPixelTree);
                //issue redraw
                Device.BeginInvokeOnMainThread(() => {canvas?.InvalidateSurface();});

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
                    //p[x] = _backPixelBuffer[x];
                }
            }
        }

        void FillFromPixelTree(int tindex, int total)
        {
            //if (_backPixelBuffer == null) return;
            unsafe
            {
                byte* p = (byte*)_pixels.ToPointer();

                for(int y = tindex; y <height; y += total)
                {
                    int pindex = y * width;
                    for(int x = 0; x < width; x++)
                    {
                        //pindex = (pindex + x) * 4;
                        //SKColor pixel = pixelTree.GetPixel(x, y, depth);
                        //p[pindex] = pixel.Alpha;//_backPixelBuffer[x];
                        //p[pindex++] = pixel.Blue;
                        //p[pindex++] = pixel.Green;
                        //p[pindex++] = pixel.Red;
                        //p[pindex] = pixel.Alpha;
                        SKColor c = pixelTree.GetPixel(x + posx, y + posy, depth);
                        SetPixel(x, y, c);
                    }
                }
            }
        }

        public void AddPoint(int x, int y)
        {
            points.Push(new Types.TouchPoint(x, y, SKTouchAction.Pressed));
            //lock (points)
            //    Monitor.Pulse(points);
            renlocker.Set();
        }
        //--------------------------------------------------------------

        public int width = 0;//{ get { if (buffer == null) return 0; else return (int)buffer.Width; } }
        public int height = 0;// { get { if (buffer == null) return 0; else return (int)buffer.Height; } }

        

        public Viewport()
        {
            
        }

        public Viewport(int width, int height)
        {
            Resize(width, height);
        }

        public void SetupRendererThread(SKCanvasView canvas)
        {
            this.canvas = canvas;
            _rndManRun = true;
            renderer = new Thread(RendererMain);
            renderer.Name = "RendererMan";
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
            _backPixelBuffer = new byte[width * 4, height];
            renlocker.Set();
        }

        public void Resize(int width, int height)
        {
            if (buffer == null)
            {
                _resize(width, height);
            }
            else
            {
                lock (this) _resize(width, height);
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

            int index = x * 4;
            _backPixelBuffer[index++,y] = color.Blue;
            _backPixelBuffer[index++, y] = color.Green;
            _backPixelBuffer[index++, y] = color.Red;
            _backPixelBuffer[index, y] = color.Alpha;

            //             int index = (y * width + x) * 4;
            //             _backPixelBuffer[index++] = color.Blue;
            //             _backPixelBuffer[index++] = color.Green;
            //             _backPixelBuffer[index++] = color.Red;
            //             _backPixelBuffer[index] = color.Alpha;

        }

        public void Dispose()
        {
            _rndManRun = false;
            buffer.Dispose();
            renlocker.Dispose();
        }
    }
}
