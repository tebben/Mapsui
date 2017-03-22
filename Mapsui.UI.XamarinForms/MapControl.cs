using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Mapsui.UI.XamarinForms
{
    public class MapControl : Grid, IMapControl
    {
       // private static readonly BindableProperty ResolutionProperty =
    //BindableProperty.Create("Resolution", typeof(double), typeof(MapControl), null, propertyChanged: OnResolutionChanged);

        private bool _invalid = true;
        private Map _map;
        private double _toResolution = double.NaN;
        private bool _viewportInitialized;
        private double _previousWidth;
        private double _previousHeight;

        public MapControl()
        {
            Children.Add(RenderElement);
            RenderElement.PaintSurface += SKElementOnPaintSurface;
            RenderElement.BackgroundColor = Color.Fuchsia;
            Map = new Map();

            var panRecognizer = new PanGestureRecognizer();
            panRecognizer.PanUpdated += PanRecognizerPanUpdated;
            GestureRecognizers.Add(panRecognizer);

            //var tapGestureRecognizer = new TapGestureRecognizer{ NumberOfTapsRequired = 1 };
            var pinchGestureRecognizer = new PinchGestureRecognizer();
            pinchGestureRecognizer.PinchUpdated += PinchGestureRecognizerOnPinchUpdated;
            GestureRecognizers.Add(pinchGestureRecognizer);
        }

        private Point _previousPoint = new Point();

        private void PinchGestureRecognizerOnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Started)
            {
                _previousPoint = e.ScaleOrigin;
                return;
            }
            if (e.Status == GestureStatus.Canceled)
            {
                _previousPoint = new Point();    
                return;
            }
            if (e.Status == GestureStatus.Running)
            {
                if (!_previousPoint.IsEmpty)
                {
                    _map.Viewport.Transform(
                        Width * e.ScaleOrigin.X,
                        Height * e.ScaleOrigin.Y,
                        Width * _previousPoint.X,
                        Height * _previousPoint.Y,
                        //_previousPoint.X,
                        //_previousPoint.Y,
                        //e.ScaleOrigin.Y,
                        //e.ScaleOrigin.X,
                        e.Scale);

                    RefreshGraphics();

                    _previousPoint = e.ScaleOrigin;
                }
            }
            else if (e.Status == GestureStatus.Completed)
            {
                _previousPoint = new Point();
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            if (_previousWidth!= width || _previousHeight != height)
            {
                _previousWidth = width;
                _previousHeight = height;
                RefreshGraphics();
            }
        }

        private void PanRecognizerPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine($"Action Pos x: {e.TotalX} y: { e.TotalY}");


            //var pos = new Point(Math.Round(e.TotalX, 3), Math.Round(e.TotalY, 3));
            var pos = new Point(e.TotalX, e.TotalY);

            if (pos == default(Point))
            {
                return;
            }

            if (e.StatusType == GestureStatus.Completed)
            {
                _mouseDown = false;
                return;
            }

            if (!_mouseDown)
            {
                HandlerActionDown(pos);
            }

            if (e.StatusType == GestureStatus.Running)
            {
                if (_previousMousePosition == default(Point))
                {
                    // It turns out that sometimes MouseMove+Pressed is called before MouseDown
                    return;
                }

                _currentMousePosition = pos;
                Map.Viewport.Transform(_currentMousePosition.X, _currentMousePosition.Y, _previousMousePosition.X,
                    _previousMousePosition.Y);
                _previousMousePosition = _currentMousePosition;
                _map.ViewChanged(false);
                OnViewChanged(true);
                RefreshGraphics();
            }
        }

        private Point _previousMousePosition;
        private Point _currentMousePosition;
        private Point _downMousePosition;
        private bool _mouseDown;

        private void HandlerActionDown(Point actionPos)
        {
            _previousMousePosition = actionPos;
            _currentMousePosition = actionPos;
            _downMousePosition = actionPos;
            _mouseDown = true;
        }

        public void Init()
        {
            MapControlLoaded();
        }

        public IRenderer Renderer { get; set; } = new MapRenderer();

        public Map Map
        {
            get { return _map; }
            set
            {
                if (_map != null)
                {
                    var temp = _map;
                    _map = null;
                    temp.DataChanged -= MapDataChanged;
                    temp.PropertyChanged -= MapPropertyChanged;
                    temp.RefreshGraphics -= MapRefreshGraphics;
                    temp.Dispose();
                }

                _map = value;

                if (_map != null)
                {
                    _viewportInitialized = false;
                    _map.DataChanged += MapDataChanged;
                    _map.PropertyChanged += MapPropertyChanged;
                    _map.RefreshGraphics += MapRefreshGraphics;
                    _map.ViewChanged(true);
                }

                RefreshGraphics();
            }
        }

        public string ErrorMessage { get; private set; }

        public bool ZoomLocked { get; set; }

        private SKCanvasView RenderElement { get; } = CreateSkiaRenderElement();

        private static SKCanvasView CreateSkiaRenderElement()
        {
            return new SKCanvasView();
        }

        public event EventHandler ErrorMessageChanged;
        public event EventHandler<ViewChangedEventArgs> ViewChanged;
        public event EventHandler ViewportInitialized;

        private void MapRefreshGraphics(object sender, EventArgs eventArgs)
        {
            RefreshGraphics();
        }

        private void MapPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Layer.Enabled):
                    RefreshGraphics();
                    break;
                case nameof(Layer.Opacity):
                    RefreshGraphics();
                    break;
            }
        }

        private void OnViewChanged(bool userAction = false)
        {
            if (_map == null)
                return;

            ViewChanged?.Invoke(this, new ViewChangedEventArgs { Viewport = Map.Viewport, UserAction = userAction });
        }

        public void Refresh()
        {
            _map.ViewChanged(true);
            RefreshGraphics();
        }

        public void RefreshGraphics()
        {
            _invalid = true;
            //Dispatcher.BeginInvoke(new Action(InvalidateVisual));

            // nodig?? invoke nodig??
            Device.BeginInvokeOnMainThread(() =>
            {
                RenderElement.InvalidateSurface();
            });
        }

        public void Clear()
        {
            _map?.ClearCache();
            RefreshGraphics();
        }

        public void ZoomIn()
        {
            if (ZoomLocked)
                return;

            if (double.IsNaN(_toResolution))
                _toResolution = Map.Viewport.Resolution;

            _toResolution = ZoomHelper.ZoomIn(_map.Resolutions, _toResolution);
            //ZoomMiddle();
        }

        public void ZoomOut()
        {
            if (double.IsNaN(_toResolution))
                _toResolution = Map.Viewport.Resolution;

            _toResolution = ZoomHelper.ZoomOut(_map.Resolutions, _toResolution);
            //ZoomMiddle();
        }

        private void OnErrorMessageChanged(EventArgs e)
        {
            ErrorMessageChanged?.Invoke(this, e);
        }

        private static void OnResolutionChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var newResolution = (double)newvalue;
            ((MapControl)bindable).ZoomToResolution(newResolution);
        }

        private void ZoomToResolution(double resolution)
        {
            //var current = _currentMousePosition;

            //Map.Viewport.Transform(current.X, current.Y, current.X, current.Y, Map.Viewport.Resolution / resolution);

            //_map.ViewChanged(true);
            //OnViewChanged();
            //RefreshGraphics();
        }



        private void MapControlLoaded()
        {
            if (!_viewportInitialized) InitializeViewport();
            UpdateSize();
        }

        private void UpdateSize()
        {
            if (Map.Viewport != null)
            {
                Map.Viewport.Width = Width;
                Map.Viewport.Height = Height;
            }
        }

        public void MapDataChanged(object sender, DataChangedEventArgs e) // todo: make private?
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (e == null)
                {
                    ErrorMessage = "Unexpected error: DataChangedEventArgs can not be null";
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Cancelled)
                {
                    ErrorMessage = "Cancelled";
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Error is WebException)
                {
                    ErrorMessage = "WebException: " + e.Error.Message;
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else if (e.Error != null)
                {
                    ErrorMessage = e.Error.GetType() + ": " + e.Error.Message;
                    OnErrorMessageChanged(EventArgs.Empty);
                }
                else // no problems
                {
                    RefreshGraphics();
                }
            });
        }

        private void InitializeViewport()
        {
            if (!ViewportHelper.TryInitializeViewport(_map, Width, Height)) return;
            _viewportInitialized = true;
            Map.ViewChanged(true);
            OnViewportInitialized();
        }

        private void OnViewportInitialized()
        {
            ViewportInitialized?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomToFullEnvelope()
        {
            if (Map.Envelope == null) return;
            if (Width.IsNanOrZero()) return;
            Map.Viewport.Resolution = Math.Max(Map.Envelope.Width / Width, Map.Envelope.Height / Height);
            Map.Viewport.Center = Map.Envelope.GetCentroid();
        }

        //private static void OnManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        //{
        //    e.TranslationBehavior.DesiredDeceleration = 25 * 96.0 / (1000.0 * 1000.0);
        //}

        //private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        //{
        //    var previousX = e.ManipulationOrigin.X;
        //    var previousY = e.ManipulationOrigin.Y;
        //    var currentX = e.ManipulationOrigin.X + e.DeltaManipulation.Translation.X;
        //    var currentY = e.ManipulationOrigin.Y + e.DeltaManipulation.Translation.Y;
        //    var deltaScale = GetDeltaScale(e.DeltaManipulation.Scale);

        //    Map.Viewport.Transform(currentX, currentY, previousX, previousY, deltaScale);

        //    _invalid = true;
        //    OnViewChanged(true);
        //    e.Handled = true;
        //}

        //private double GetDeltaScale(XamlVector scale)
        //{
        //    if (ZoomLocked) return 1;
        //    var deltaScale = (scale.X + scale.Y) / 2;
        //    if (Math.Abs(deltaScale) < Constants.Epsilon)
        //        return 1; // If there is no scaling the deltaScale will be 0.0 in Windows Phone (while it is 1.0 in wpf)
        //    if (!(Math.Abs(deltaScale - 1d) > Constants.Epsilon)) return 1;
        //    return deltaScale;
        //}

        //private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        //{
        //    Refresh();
        //}

        private void OnPaintSurface(SKCanvas canvas, int width, int height)
        {
            if (double.IsNaN(Map.Viewport.Resolution)) return;

            Map.Viewport.Width = Width;
            Map.Viewport.Height = Height;
            canvas.Scale((float)(width / Width), (float)(height / Height));

            Renderer.Render(canvas, Map.Viewport, Map.Layers, Map.BackColor);
            _invalid = false;
        }

        private void SKElementOnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (!_viewportInitialized) InitializeViewport();
            if (!_viewportInitialized) return; // Stop if the line above failed. 
            if (!_invalid) return; // Don't render when nothing has changed

            OnPaintSurface(e.Surface.Canvas, e.Info.Width, e.Info.Height);
        }
    }
}