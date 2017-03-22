﻿using Mapsui.Samples.XamarinForms.Views;
using Xamarin.Forms;

namespace Mapsui.Samples.XamarinForms
{
	public partial class App
	{
		public App ()
		{
			InitializeComponent();
		    var navPage = new NavigationPage(new MapView())
		    {
		        BarBackgroundColor = (Color) Resources["AccentColor2"],
                BarTextColor = (Color)Resources["TextColorLight"]
            };

		    MainPage = navPage;
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