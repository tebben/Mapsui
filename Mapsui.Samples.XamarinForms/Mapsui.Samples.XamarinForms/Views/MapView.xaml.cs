using System;
using Mapsui.Utilities;

namespace Mapsui.Samples.XamarinForms.Views
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

        private void BtnTocOnTapped(object sender, EventArgs e)
        {
            Toc.IsVisible = !Toc.IsVisible;
        }
    }
}
