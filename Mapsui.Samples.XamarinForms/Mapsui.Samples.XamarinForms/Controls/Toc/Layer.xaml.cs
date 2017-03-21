using Mapsui.Samples.XamarinForms.Models;

namespace Mapsui.Samples.XamarinForms.Controls.Toc
{
	public partial class Layer
	{
		public Layer (LayerModel layerModel)
		{
			InitializeComponent ();
		    BindingContext = layerModel;
		}
	}
}
