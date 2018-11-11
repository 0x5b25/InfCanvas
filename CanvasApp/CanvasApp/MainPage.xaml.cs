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

        Label info,info2,info3,info4;
        Button b, b2, b3, b4, b5, b6;

        public MainPage()
        {
            InitializeComponent();

//             canvas = new SKCanvasView();
//             canvas.PaintSurface += Repaint;
//             canvas.HorizontalOptions = LayoutOptions.Fill;
//             canvas.VerticalOptions = LayoutOptions.Fill;

            vp = new Viewport();
            vp.posx = -100;
            vp.posy = -200;
            //vp.Resize((int)this.Width, (int)this.Height);

        }

        public void Test()
        {

            canvas = new SKCanvasView();
            canvas.PaintSurface += Repaint;
            canvas.HorizontalOptions = LayoutOptions.Fill;
            canvas.VerticalOptions = LayoutOptions.Fill;
            canvas.SizeChanged += ContentPage_SizeChanged;
            //canvas

            canvas.EnableTouchEvents = true;
            canvas.Touch += Canvas_Touch;
            //canvas.
            AbsoluteLayout l = new AbsoluteLayout();
            this.Content = l;

            AbsoluteLayout.SetLayoutBounds(canvas, new Rectangle(0.5, 0.5, 1, 1));
            AbsoluteLayout.SetLayoutFlags(canvas, AbsoluteLayoutFlags.All);
            l.Children.Add(canvas);
            vp.SetupRendererThread(canvas);

            info = new Label
            {
                FontSize = 10,
                Text = "I_1"
            };
            AbsoluteLayout.SetLayoutBounds(info, new Rectangle(0, 0, 0.5, 15));
            AbsoluteLayout.SetLayoutFlags(info, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info);


            info2 = new Label
            {
                FontSize = 10,
                Text = "I_2"
            };
            AbsoluteLayout.SetLayoutBounds(info2, new Rectangle(0, 15, 0.5, 15));
            AbsoluteLayout.SetLayoutFlags(info2, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info2);

            info3 = new Label
            {
                FontSize = 10,
                Text = "I_3"
            };
            AbsoluteLayout.SetLayoutBounds(info3, new Rectangle(0, 30, 0.5, 15));
            AbsoluteLayout.SetLayoutFlags(info3, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info3);

            info4 = new Label
            {
                FontSize = 10,
                Text = "I_4"
            };
            AbsoluteLayout.SetLayoutBounds(info4, new Rectangle(0, 45, 0.5, 15));
            AbsoluteLayout.SetLayoutFlags(info4, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info4);



            b = new Button
            {
                Text = "+"
            };
            b.Pressed += (object sender,EventArgs e) => { if(vp.depth <= vp.pixelTree.totalDepth) vp.depth++;vp.NotifyRedraw(); };
            AbsoluteLayout.SetLayoutBounds(b, new Rectangle(0, 60, 40, 40));
            AbsoluteLayout.SetLayoutFlags(b, AbsoluteLayoutFlags.None);
            l.Children.Add(b);

            b2 = new Button
            {
                Text = "-"
            };
            b2.Pressed += (object sender, EventArgs e) => { if (vp.depth > 0) vp.depth--; vp.NotifyRedraw(); };
            AbsoluteLayout.SetLayoutBounds(b2, new Rectangle(40, 60, 40, 40));
            AbsoluteLayout.SetLayoutFlags(b2, AbsoluteLayoutFlags.None);
            l.Children.Add(b2);

            var moveSensitivity = 8;

            b3 = new Button
            {
                Text = "/\\"
            };
            b3.Pressed += (object sender, EventArgs e) => { vp.posy-= moveSensitivity; vp.NotifyRedraw(); };
            AbsoluteLayout.SetLayoutBounds(b3, new Rectangle(120, 60, 40, 40));
            AbsoluteLayout.SetLayoutFlags(b3, AbsoluteLayoutFlags.None);
            l.Children.Add(b3);

            b4 = new Button
            {
                Text = "<"
            };
            b4.Pressed += (object sender, EventArgs e) => { vp.posx-= moveSensitivity; vp.NotifyRedraw(); };
            AbsoluteLayout.SetLayoutBounds(b4, new Rectangle(80, 100, 40, 40));
            AbsoluteLayout.SetLayoutFlags(b4, AbsoluteLayoutFlags.None);
            l.Children.Add(b4);

            b5 = new Button
            {
                Text = "\\/"
            };
            b5.Pressed += (object sender, EventArgs e) => { vp.posy+= moveSensitivity; vp.NotifyRedraw(); };
            AbsoluteLayout.SetLayoutBounds(b5, new Rectangle(120, 100, 40, 40));
            AbsoluteLayout.SetLayoutFlags(b5, AbsoluteLayoutFlags.None);
            l.Children.Add(b5);

            b6 = new Button
            {
                Text = ">"
            };
            b6.Pressed += (object sender, EventArgs e) => { vp.posx += moveSensitivity; vp.NotifyRedraw(); };
            AbsoluteLayout.SetLayoutBounds(b6, new Rectangle(160, 100, 40, 40));
            AbsoluteLayout.SetLayoutFlags(b6, AbsoluteLayoutFlags.None);
            l.Children.Add(b6);
            //vp.Resize((int)canvas.CanvasSize.Width, (int)canvas.CanvasSize.Height);
            //RefreshBufferRes();
        }

        private void Canvas_Touch(object sender, SKTouchEventArgs e)
        {
            if (scaleX == 0 || scaleY == 0)
            {
                GetScreenInfo();
            }
            info.Text = "C:" + canvas.CanvasSize + " X:" + scaleX + "Y:" + scaleY;
            if (info2 != null) {
                info2.Text = "Position:" + e.Location;
            }
            if (info3 != null)
            {
                info3.Text = "Type:" + e.ActionType +" Contact:"+ e.InContact;
            }
            
            if (e.MouseButton == SKMouseButton.Left||
                e.ActionType == SKTouchAction.Pressed||
                e.ActionType == SKTouchAction.Moved)
            {
                //vp.SetPixelBack(Convert.ToInt32(e.Location.X), Convert.ToInt32(e.Location.Y), SKColors.Red);
                //canvas.InvalidateSurface();
                if (e.InContact)
                {
                    
                    vp.AddPoint(Convert.ToInt32(e.Location.X / scaleX), Convert.ToInt32(e.Location.Y / scaleY));
                }
            }

            // let the OS know we are interested
            e.Handled = true;
        }

        void Repaint(object sender, SKPaintSurfaceEventArgs args)
        {
            //May get called for the first time when canvas is added to layout tree,
            //So we initalize viewport buffer and set up correct scale
            if (info4 != null)
            {
                info4.Text = "vp.dp:" + vp.depth + " tree.dp:" + vp.pixelTree.totalDepth;
            }
            if (vp.buffer == null)
                RefreshBufferRes();
            //vp.Resize((int)canvas.CanvasSize.Width, (int)canvas.CanvasSize.Height);

            args.Surface.Canvas.DrawBitmap(vp.buffer, bmpRect);

        }

        private void ChangeContent(object sender, EventArgs e)
        {
            Test();
        }

        private void ContentPage_SizeChanged(object sender, EventArgs e)
        {
            //vp.Resize((int)canvas.CanvasSize.Width, (int)canvas.CanvasSize.Height);
            //Viewport.Get().Resize((int)this.Width, (int)this.Height);
            //canvas?.InvalidateSurface();
            RefreshBufferRes();
        }

        float scaleX = 1, scaleY = 1;
        SKRect bmpRect = new SKRect();

        private void GetScreenInfo()
        {
            scaleX = (float)(canvas.CanvasSize.Width / Width);
            scaleY = (float)(canvas.CanvasSize.Height / Height);
            bmpRect.Left = 0;
            bmpRect.Top = 0;
            bmpRect.Size = canvas.CanvasSize;
        }

        void RefreshBufferRes()
        {
            //if (canvas == null) return;
            GetScreenInfo();

            //vp.Resize((int)canvas.CanvasSize.Width, (int)canvas.CanvasSize.Height);
            vp.Resize((int)this.Width, (int)this.Height);
                //if(info != null)
        }

        protected override bool OnBackButtonPressed()
        {
            //paren
            (Application.Current as App).Exit();
            return true;
        }
    }
}
