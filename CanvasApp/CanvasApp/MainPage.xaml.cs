using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views;
using SkiaSharp.Views.Forms;

namespace CanvasApp
{
    public partial class MainPage : ContentPage
    {
        SKCanvasView canvas;
        Viewport vp;

        Label info,info2,info3;

        public MainPage()
        {
            InitializeComponent();

            canvas = new SKCanvasView();
            canvas.PaintSurface += Repaint;
            canvas.HorizontalOptions = LayoutOptions.Fill;
            canvas.VerticalOptions = LayoutOptions.Fill;

            vp = Viewport.Get();
            vp.SetupRendererThread(canvas);
            vp.Resize((int)this.Width, (int)this.Height);

        }

        public void Test()
        {
            if (canvas == null)
            {
                canvas = new SKCanvasView();
                canvas.PaintSurface += Repaint;
                canvas.HorizontalOptions = LayoutOptions.Fill;
                canvas.VerticalOptions = LayoutOptions.Fill;
            }
            canvas.EnableTouchEvents = true;
            canvas.Touch += Canvas_Touch;
            //canvas.
            AbsoluteLayout l = new AbsoluteLayout();
            this.Content = l;

            info = new Label();
            AbsoluteLayout.SetLayoutBounds(info, new Rectangle(0, 0, 1, 16));
            AbsoluteLayout.SetLayoutFlags(info, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info);
            info.Text = "C:"+canvas.CanvasSize+" X:"+this.Width + ","+this.Height;

            info2 = new Label();
            AbsoluteLayout.SetLayoutBounds(info2, new Rectangle(0, 16, 1, 16));
            AbsoluteLayout.SetLayoutFlags(info2, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info2);

            info3 = new Label();
            AbsoluteLayout.SetLayoutBounds(info3, new Rectangle(0, 32, 1, 16));
            AbsoluteLayout.SetLayoutFlags(info3, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info3);

            AbsoluteLayout.SetLayoutBounds(canvas, new Rectangle(0.5, 0.5, 1, 1));
            AbsoluteLayout.SetLayoutFlags(canvas, AbsoluteLayoutFlags.All);
            l.Children.Add(canvas);

        }

        private void Canvas_Touch(object sender, SKTouchEventArgs e)
        {
            if (info2 != null) {
                info2.Text = "Position:" + e.Location;
            }
            if (info3 != null)
            {
                info3.Text = "Type:" + e.ActionType +"Contact:"+ e.InContact;
                
            }
            if (e.MouseButton == SKMouseButton.Left)
            {
                //vp.SetPixelBack(Convert.ToInt32(e.Location.X), Convert.ToInt32(e.Location.Y), SKColors.Red);
                //canvas.InvalidateSurface();
                vp.AddPoint(Convert.ToInt32(e.Location.X), Convert.ToInt32(e.Location.Y));
            }

            // let the OS know we are interested
            e.Handled = true;
        }

        void Repaint(object sender, SKPaintSurfaceEventArgs args)
        {
            /*for (int x = 0; x < this.Width; x++)
            {
                for (int y = 0; y < this.Height; y++)
                {
                    vp.SetPixel(x,y, SKColors.Green);
                    //args.Surface.Canvas.
                }
            }/**/
            //vp.Fill();
            args.Surface.Canvas.DrawBitmap(vp.buffer,0,0);
        }

        private void ChangeContent(object sender, EventArgs e)
        {
            Test();
        }

        private void ContentPage_SizeChanged(object sender, EventArgs e)
        {
            Viewport.Get().Resize((int)this.Width, (int)this.Height);
            //canvas?.InvalidateSurface();
        }
    }
}
