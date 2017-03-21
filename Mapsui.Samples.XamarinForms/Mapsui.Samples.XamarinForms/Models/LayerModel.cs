using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mapsui.Layers;

namespace Mapsui.Samples.XamarinForms.Models
{
    public class LayerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _title;
        private bool _visible;
        private double _opacity;
        private ILayer _layer;

        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public bool Visible
        {
            get
            {
                return _visible;
            }
            set
            {
                _visible = value;
                OnPropertyChanged();
            }
        }

        public double Opacity
        {
            get
            {
                return _opacity;
            }
            set
            {
                _opacity = value;
                OnPropertyChanged();
            }
        }

        public ILayer Layer
        {
            get
            {
                return _layer;
            }
            set
            {
                _layer = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
