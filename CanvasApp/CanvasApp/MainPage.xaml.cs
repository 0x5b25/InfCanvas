﻿using System;
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

            vp = new Viewport();
            vp.SetupRendererThread(canvas);
            //vp.Resize((int)this.Width, (int)this.Height);

        }

        public void Test()
        {
            if (canvas == null)
            {
                canvas = new SKCanvasView();
                canvas.PaintSurface += Repaint;
                canvas.HorizontalOptions = LayoutOptions.Fill;
                canvas.VerticalOptions = LayoutOptions.Fill;
                canvas.SizeChanged += ContentPage_SizeChanged;
                //canvas.
            }
            canvas.EnableTouchEvents = true;
            canvas.Touch += Canvas_Touch;
            //canvas.
            AbsoluteLayout l = new AbsoluteLayout();
            this.Content = l;

            info = new Label();
            AbsoluteLayout.SetLayoutBounds(info, new Rectangle(0, 0, 1, 20));
            AbsoluteLayout.SetLayoutFlags(info, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info);
            

            info2 = new Label();
            AbsoluteLayout.SetLayoutBounds(info2, new Rectangle(0, 20, 1, 20));
            AbsoluteLayout.SetLayoutFlags(info2, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info2);

            info3 = new Label();
            AbsoluteLayout.SetLayoutBounds(info3, new Rectangle(0, 40, 1, 20));
            AbsoluteLayout.SetLayoutFlags(info3, AbsoluteLayoutFlags.WidthProportional);
            l.Children.Add(info3);

            AbsoluteLayout.SetLayoutBounds(canvas, new Rectangle(0.5, 0.5, 1, 1));
            AbsoluteLayout.SetLayoutFlags(canvas, AbsoluteLayoutFlags.All);
            l.Children.Add(canvas);

            //vp.Resize((int)canvas.CanvasSize.Width, (int)canvas.CanvasSize.Height);
        }

        private void Canvas_Touch(object sender, SKTouchEventArgs e)
        {
            info.Text = "C:" + canvas.CanvasSize + " X:" + scaleX + "Y:" + scaleY;
            if (info2 != null) {
                info2.Text = "Position:" + e.Location;
            }
            if (info3 != null)
            {
                info3.Text = "Type:" + e.ActionType +"Contact:"+ e.InContact;
                
            }
            if (e.MouseButton == SKMouseButton.Left||
                e.ActionType == SKTouchAction.Pressed||
                e.ActionType == SKTouchAction.Moved)
            {
                //vp.SetPixelBack(Convert.ToInt32(e.Location.X), Convert.ToInt32(e.Location.Y), SKColors.Red);
                //canvas.InvalidateSurface();
                vp.AddPoint(Convert.ToInt32(e.Location.X/scaleX), Convert.ToInt32(e.Location.Y/scaleY));
            }

            // let the OS know we are interested
            e.Handled = true;
        }

        void Repaint(object sender, SKPaintSurfaceEventArgs args)
        {
            //May get called for the first time when canvas is added to layout tree,
            //So we initalize viewport buffer and set up correct scale
            if (vp.buffer == null)
                RefreshBufferRes();
            //vp.Resize((int)canvas.CanvasSize.Width, (int)canvas.CanvasSize.Height);
            
            args.Surface.Canvas.DrawBitmap(vp.buffer,bmpRect);
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
        void RefreshBufferRes()
        {
            scaleX = (float)(canvas.CanvasSize.Width / Width);
            scaleY = (float)(canvas.CanvasSize.Height / Height);
            bmpRect.Left = 0;
            bmpRect.Top = 0;
            bmpRect.Size = canvas.CanvasSize;
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
