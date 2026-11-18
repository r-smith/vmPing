using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace vmPing.Controls
{
    // A UniformGrid that puts every cell edge on a whole screen pixel. A plain UniformGrid gives each
    // cell a fractional width, and rounding each cell on its own leaves 1px gaps or overlaps between
    // them, so the lines between probes change width as the window is resized.
    public class ProbeGrid : UniformGrid
    {
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            int count = 0;
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility != Visibility.Collapsed)
                {
                    count++;
                }
            }

            // The same column and row counts UniformGrid works out.
            int columns = Columns > 0 ? Columns : (int)Math.Ceiling(Math.Sqrt(count));
            if (columns == 0)
            {
                return arrangeSize;
            }
            int rows = Rows > 0 ? Rows : Math.Max(1, (count + FirstColumn + columns - 1) / columns);

            // Work in screen pixels, so the edges stay whole at any display scaling.
            DpiScale dpi = VisualTreeHelper.GetDpi(this);
            double width = arrangeSize.Width * dpi.DpiScaleX;
            double height = arrangeSize.Height * dpi.DpiScaleY;

            int index = FirstColumn;
            foreach (UIElement child in InternalChildren)
            {
                if (child.Visibility == Visibility.Collapsed)
                {
                    child.Arrange(new Rect());
                    continue;
                }

                int column = index % columns;
                int row = index / columns;
                double left = Math.Round(column * width / columns);
                double right = Math.Round((column + 1) * width / columns);
                double top = Math.Round(row * height / rows);
                double bottom = Math.Round((row + 1) * height / rows);
                child.Arrange(new Rect(
                    left / dpi.DpiScaleX,
                    top / dpi.DpiScaleY,
                    (right - left) / dpi.DpiScaleX,
                    (bottom - top) / dpi.DpiScaleY));
                index++;
            }

            return arrangeSize;
        }
    }
}
