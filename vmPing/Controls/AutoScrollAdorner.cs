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
            const double imageWidth = 13.0;
            const double imageHeight = 11.0;
            const double xOffset = 36.0;
            const double yOffset = 14.0;
            Rect rect = new Rect(
                AdornedElement.RenderSize.Width - xOffset,
                AdornedElement.RenderSize.Height - yOffset,
                imageWidth,
                imageHeight);
            drawingContext.DrawImage((DrawingImage)Application.Current.Resources["icon.down-arrow-scroll-indicator"], rect);

            base.OnRender(drawingContext);
        }
    }
}
