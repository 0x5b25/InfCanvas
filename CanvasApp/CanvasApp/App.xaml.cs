using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CanvasApp
{
    public partial class App : Application
    {
        Action exit;

        public App(Action exitMethod)
        {
            InitializeComponent();
            exit = exitMethod;
            MainPage = new MainPage();
        }

        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        } 

        public void Exit()
        {
            exit?.Invoke();
        }
    }
}
