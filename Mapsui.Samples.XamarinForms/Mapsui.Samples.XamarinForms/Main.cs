using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Mapsui.Samples.XamarinForms.Models;
using Mapsui.UI.XamarinForms;

namespace Mapsui.Samples.XamarinForms
{
    public class Main : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MapControl _mapControl;
        private ObservableCollection<LayerModel> _layerModels;

        public Main()
        {
            MapControl = new MapControl();

            Layers = new ObservableCollection<LayerModel>();
            Layers.CollectionChanged += LayersCollectionChanged;         
            foreach (var layerModel in TestData.GetLayers())
            {                
                Layers.Add(layerModel);
            }
        }

        public MapControl MapControl
        {
            get { return _mapControl; }
            set
            {
                _mapControl = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LayerModel> Layers
        {
            get { return _layerModels; }
            set
            {
                _layerModels = value;
                OnPropertyChanged();
            }
        }

        private void LayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    if (e.NewItems != null)
                        foreach (var item in e.NewItems)
                        {
                            MapControl.Map.Layers.Add(((LayerModel)item).Layer);
                        }
                }
                    break;
                case NotifyCollectionChangedAction.Remove:
                {
                    if (e.OldItems != null)
                        foreach (var item in e.OldItems)
                        {
                            var layerModel = (LayerModel) item;
                            foreach (var mapLayer in MapControl.Map.Layers)
                            {
                                if (!mapLayer.Tag.ToString().Equals(layerModel.Id))
                                        continue;

                                MapControl.Map.Layers.Remove(layerModel.Layer);
                                break;
                            }
                        }
                }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    MapControl.Map.Layers.Clear();
                    break;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
