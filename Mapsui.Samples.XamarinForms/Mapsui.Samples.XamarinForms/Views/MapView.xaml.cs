using System;

namespace Mapsui.Samples.XamarinForms.Views
{
    public partial class MapView
    {
        private bool _initialized;
        private Main Main => ((Main)BindingContext);        

        public MapView()
        {
            InitializeComponent();            
        }

        protected override void OnAppearing()
        {
            Main.MapControl.SizeChanged += MapControlViewportInitialized;
            MapControlWrapper.Children.Add(Main.MapControl);            
        }

        private void MapControlViewportInitialized(object sender, EventArgs e)
        {
            if (_initialized || Main.MapControl.Width == -1 || Main.MapControl.Height == -1)
                return;

            InitMapControl();
        }

        private void InitMapControl()
        {
            Main.MapControl.Init();
            Main.MapControl.ZoomToFullEnvelope();
            Main.MapControl.Refresh();
            _initialized = true;
        }

        private void BtnTocOnTapped(object sender, EventArgs e)
        {
            Toc.IsVisible = !Toc.IsVisible;
        }
    }
}
