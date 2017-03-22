using System.Collections.ObjectModel;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.XamarinForms.Models;
using Mapsui.Styles;
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
                    Title = "Points",
                    Enabled = true,
                    Layer = CreatePointLayer()
                }
            };

        }

        private static ILayer CreatePointLayer()
        {
            return new MemoryLayer
            {
                Style = null,
                DataSource = CreateProviderWithPointsWithVectorStyle(),
                Name = "Points"
            };
        }

        private static MemoryProvider CreateProviderWithPointsWithVectorStyle()
        {
            var features = new Features
            {
                new Feature
                {
                    Geometry = new Point(541108.987373, 6861616.40385),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Red)}}
                },
                new Feature
                {
                    Geometry = new Point(2194659.956124, -3999234.795973),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Yellow), Outline = new Pen(Color.Black, 2)}}
                },
                new Feature
                {
                    Geometry = new Point(-8134436.80516, 5001559.515575),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Blue), Outline = new Pen(Color.White, 2)}}
                },
                new Feature
                {
                    Geometry = new Point(16357243.63836, -4378934.290906),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Green), Outline = null}}
                }
            };
            var provider = new MemoryProvider(features);
            return provider;
        }
    }
}
