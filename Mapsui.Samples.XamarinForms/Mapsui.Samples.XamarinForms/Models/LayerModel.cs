using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Mapsui.Layers;

namespace Mapsui.Samples.XamarinForms.Models
{
    public class LayerModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Id { get; }

        private string _title;
        private bool _enabled;
        private ILayer _layer;        

        public LayerModel()
        {
            Id = Guid.NewGuid().ToString();
        }

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

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged();

                if (Layer != null)
                {
                    Layer.Enabled = _enabled;
                }
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
                _layer.Tag = Id;
                _layer.Enabled = _enabled;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
