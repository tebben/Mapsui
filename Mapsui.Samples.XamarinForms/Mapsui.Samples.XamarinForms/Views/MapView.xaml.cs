using Mapsui.Utilities;

namespace Mapsui.Samples.XamarinForms
{
    public partial class MapView
    {
        public MapView()
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
