﻿using System;
using System.Collections.Generic;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public static class WidgetRenderer
    {
        public static void Render(object target, Map map, IReadOnlyViewport viewport, IEnumerable<IWidget> widgets,
            IDictionary<Type, IWidgetRenderer> renders, float layerOpacity)
        {
            var canvas = (SKCanvas) target;

            foreach (var widget in widgets)
            {
                ((ISkiaWidgetRenderer)renders[widget.GetType()]).Draw(canvas, map, viewport, widget, layerOpacity);
            }
        }
    }
}