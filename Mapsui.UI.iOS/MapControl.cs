using CoreFoundation;
using Foundation;
using UIKit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CoreGraphics;
using Mapsui.Geometries;
using Mapsui.Geometries.Utilities;
using SkiaSharp.Views.iOS;

namespace Mapsui.UI.iOS
{
    [Register("MapControl"), DesignTimeVisible(true)]
    public partial class MapControl : UIView, IMapControl
    {
        private readonly SKGLView _canvas = new SKGLView();
        private double _innerRotation;
        
        public MapControl(CGRect frame)
            : base(frame)
        {
            Initialize();
        }

        [Preserve]
        public MapControl(IntPtr handle) : base(handle) // used when initialized from storyboard
        {
            Initialize();
        }

        public void Initialize()
        {
            Map = new Map();
            BackgroundColor = UIColor.White;

            _canvas.TranslatesAutoresizingMaskIntoConstraints = false;
            _canvas.MultipleTouchEnabled = true;
            _canvas.PaintSurface += OnPaintSurface;
            AddSubview(_canvas);

            AddConstraints(new[] {
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Leading, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Trailing, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Top, 1.0f, 0.0f),
                NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _canvas, NSLayoutAttribute.Bottom, 1.0f, 0.0f)
            });

            ClipsToBounds = true;
            MultipleTouchEnabled = true;
            UserInteractionEnabled = true;
            
            var doubleTapGestureRecognizer = new UITapGestureRecognizer(OnDoubleTapped)
            {
                NumberOfTapsRequired = 2,
                CancelsTouchesInView = false,
            };
            AddGestureRecognizer(doubleTapGestureRecognizer);

            var tapGestureRecognizer = new UITapGestureRecognizer(OnSingleTapped)
            {
                NumberOfTapsRequired = 1,
                CancelsTouchesInView = false,
            };
            tapGestureRecognizer.RequireGestureRecognizerToFail(doubleTapGestureRecognizer);
            AddGestureRecognizer(tapGestureRecognizer);
        }

        public float PixelDensity => (float) _canvas.ContentScaleFactor; // todo: Check if I need canvas

        private void OnDoubleTapped(UITapGestureRecognizer gesture)
        {
            var position = GetScreenPosition(gesture.LocationInView(this));
            OnInfo(InvokeInfo(Map.Layers.Where(l => l.IsMapInfoLayer), Map.Widgets, Viewport, 
                position, position, Renderer.SymbolCache, WidgetTouched, 2));
        }
        
        private void OnSingleTapped(UITapGestureRecognizer gesture)
        {
            var position = GetScreenPosition(gesture.LocationInView(this));
            OnInfo(InvokeInfo(Map.Layers.Where(l => l.IsMapInfoLayer), Map.Widgets, Viewport, 
                position, position, Renderer.SymbolCache, WidgetTouched, 1));
        }
       
        void OnPaintSurface(object sender, SKPaintGLSurfaceEventArgs args)
        {
            TryInitializeViewport(ViewportWidth, ViewportHeight);
            if (!Viewport.Initialized) return;

            // Unfortunately the SKGLView does not have a IgnorePixelScaling property,
            // so have to adjust for density with SKGLView.Scale.
            // The Scale can only be set in the render loop

            args.Surface.Canvas.Scale(PixelDensity, PixelDensity);  
            Renderer.Render(args.Surface.Canvas, Map, Viewport, _map.Layers, _map.Widgets, _map.BackColor);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);

            _innerRotation = Viewport.Rotation;
        }

        public override void TouchesMoved(NSSet touches, UIEvent evt)
        {
            base.TouchesMoved(touches, evt);

            if (evt.AllTouches.Count == 1)
            {
                if (touches.AnyObject is UITouch touch)
                {
                    var currentPos = touch.LocationInView(this);
                    var previousPos = touch.PreviousLocationInView(this);

                    Viewport.Transform(currentPos.X, currentPos.Y, previousPos.X, previousPos.Y);

                    ViewportLimiter.LimitExtent(_viewport, _map.PanMode, _map.PanLimits, _map.Envelope);

                    RefreshGraphics();

                    _innerRotation = Viewport.Rotation;
                }
            }
            else if (evt.AllTouches.Count >= 2)
            {
                var prevLocations = evt.AllTouches.Select(t => ((UITouch)t).PreviousLocationInView(this))
                                           .Select(p => new Point(p.X, p.Y)).ToList();

                var locations = evt.AllTouches.Select(t => ((UITouch)t).LocationInView(this))
                                        .Select(p => new Point(p.X, p.Y)).ToList();

                var (prevCenter, prevRadius, prevAngle) = GetPinchValues(prevLocations);
                var (center, radius, angle) = GetPinchValues(locations);

                double rotationDelta = 0;

                if (!RotationLock)
                {
                    _innerRotation += angle - prevAngle;
                    _innerRotation %= 360;

                    if (_innerRotation > 180)
                        _innerRotation -= 360;
                    else if (_innerRotation < -180)
                        _innerRotation += 360;

                    if (Viewport.Rotation == 0 && Math.Abs(_innerRotation) >= Math.Abs(UnSnapRotationDegrees))
                        rotationDelta = _innerRotation;
                    else if (Viewport.Rotation != 0)
                    {
                        if (Math.Abs(_innerRotation) <= Math.Abs(ReSnapRotationDegrees))
                            rotationDelta = -Viewport.Rotation;
                        else
                            rotationDelta = _innerRotation - Viewport.Rotation;
                    }
                }

                Viewport.Transform(center.X, center.Y, prevCenter.X, prevCenter.Y, radius / prevRadius, rotationDelta);

                ViewportLimiter.Limit(_viewport,
                    _map.ZoomMode, _map.ZoomLimits, _map.Resolutions,
                    _map.PanMode, _map.PanLimits, _map.Envelope);

                RefreshGraphics();
            }
        }

        public override void TouchesEnded(NSSet touches, UIEvent e)
        {
            Refresh();
        }

        /// <summary>
        /// Gets screen position in device independent units (or DIP or DP).
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Point GetScreenPosition(CGPoint point)
        {
            return new Point(point.X, point.Y);
        }
       
        private void RunOnUIThread(Action action)
        {
            DispatchQueue.MainQueue.DispatchAsync(action);
        }
        
        public void RefreshGraphics()
        {
            RunOnUIThread(() =>
            {
                SetNeedsDisplay();
                _canvas?.SetNeedsDisplay();
            });
        }

        public override CGRect Frame
        {
            get => base.Frame;
            set
            {
                _canvas.Frame = value;
                base.Frame = value;
                
                _viewport.Width = ViewportWidth;
                _viewport.Height = ViewportHeight;

                Refresh();
            }
        }

        public override void LayoutMarginsDidChange()
        {
            if (_canvas == null) return;

            base.LayoutMarginsDidChange();
            
            _viewport.Width = ViewportWidth;
            _viewport.Height = ViewportHeight;

            Refresh();
        }

        public void OpenBrowser(string url)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
        }

        public new void Dispose()
        {
            Unsubscribe();
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            Unsubscribe();
            base.Dispose(disposing);
        }

        private static (Point centre, double radius, double angle) GetPinchValues(List<Point> locations)
        {
            if (locations.Count < 2)
                throw new ArgumentException();

            double centerX = 0;
            double centerY = 0;

            foreach (var location in locations)
            {
                centerX += location.X;
                centerY += location.Y;
            }

            centerX = centerX / locations.Count;
            centerY = centerY / locations.Count;

            var radius = Algorithms.Distance(centerX, centerY, locations[0].X, locations[0].Y);

            var angle = Math.Atan2(locations[1].Y - locations[0].Y, locations[1].X - locations[0].X) * 180.0 / Math.PI;

            return (new Point(centerX, centerY), radius, angle);
        }

        public float ViewportWidth => (float)_canvas.Frame.Width; // todo: check if we need _canvas
        public float ViewportHeight => (float)_canvas.Frame.Height; // todo: check if we need _canvas
    }
}