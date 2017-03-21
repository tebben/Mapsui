using Xamarin.Forms;

namespace Mapsui.Samples.XamarinForms
{
	public partial class App
	{
		public App ()
		{
			InitializeComponent();
            MainPage = new NavigationPage(new MapView());
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
