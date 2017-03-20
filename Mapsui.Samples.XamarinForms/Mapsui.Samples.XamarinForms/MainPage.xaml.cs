using Mapsui.Utilities;

namespace Mapsui.Samples.XamarinForms
{
	public partial class MainPage
	{
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            MapControl.Init();
            MapControl.Map.Layers.Add(OpenStreetMap.CreateTileLayer());
            MapControl.Refresh();
        }
    }
}
