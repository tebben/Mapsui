﻿using System.Windows.Controls;
using Mapsui.Widgets;

namespace Mapsui.Rendering.Xaml.XamlWidgets
{
    interface IXamlWidgetRenderer : IWidgetRenderer
    {
        void Draw(Canvas canvas, Map map, IReadOnlyViewport viewport, IWidget widget);
    }
}
