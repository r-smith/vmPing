using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace vmPing.Controls
{
    class AutoScrollAdorner : Adorner
    {
        public AutoScrollAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const double ImageWidth = 13.0;
            const double ImageHeight = 11.0;
            const double XOffset = 36.0;
            const double YOffset = 14.0;
            Rect rect = new Rect(
                AdornedElement.RenderSize.Width - XOffset,
                AdornedElement.RenderSize.Height - YOffset,
                ImageWidth,
                ImageHeight);
            drawingContext.DrawImage((DrawingImage)Application.Current.Resources["icon.down-arrow-scroll-indicator"], rect);

            base.OnRender(drawingContext);
        }
    }
}
