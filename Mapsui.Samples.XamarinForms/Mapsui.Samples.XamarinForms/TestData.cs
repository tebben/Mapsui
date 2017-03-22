using System.Collections.ObjectModel;
using Mapsui.Samples.XamarinForms.Models;
using Mapsui.Utilities;

namespace Mapsui.Samples.XamarinForms
{
    public class TestData
    {
        public static ObservableCollection<LayerModel> GetLayers()
        {
            return new ObservableCollection<LayerModel>
            {
                new LayerModel
                {
                    Title = "OSM",
                    Enabled = true,
                    Layer = OpenStreetMap.CreateTileLayer()
                },
                new LayerModel
                {
                    Title = "OSM",
                    Enabled = false,
                    Layer = OpenStreetMap.CreateTileLayer()
                }
            };
        }
    }
}
