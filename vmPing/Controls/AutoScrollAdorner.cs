using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace vmPing.Controls
{
    // A badge in the corner of a probe's output, shown while there's newer output below.
    // Clicking it is handled by the output box, which scrolls to the bottom.
    internal class AutoScrollAdorner : Adorner
    {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill), typeof(Brush), typeof(AutoScrollAdorner),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly Pen s_outlinePen = CreatePen(Color.FromArgb(0xcc, 0xff, 0xff, 0xff), 1.0);
        private static readonly Pen s_arrowPen = CreatePen(Colors.White, 1.0);

        public AutoScrollAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            Cursor = Cursors.Hand;

            // The badge takes the theme's accent color, and follows it when the theme changes.
            SetResourceReference(FillProperty, "Theme.Accent");
        }

        public Brush Fill
        {
            get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const double Radius = 9.0;
            // The half pixels put the arrow's 1px stem on a whole pixel, so it stays sharp.
            const double RightOffset = 28.5;
            const double BottomOffset = 11.5;
            Point center = new Point(
                AdornedElement.RenderSize.Width - RightOffset,
                AdornedElement.RenderSize.Height - BottomOffset);

            // Circle, with the outline just outside it
            drawingContext.DrawEllipse(Fill, null, center, Radius, Radius);
            drawingContext.DrawEllipse(null, s_outlinePen, center, Radius + 0.5, Radius + 0.5);

            // Down arrow
            Point tip = new Point(center.X, center.Y + 4.0);
            drawingContext.DrawLine(s_arrowPen, new Point(center.X, center.Y - 4.5), tip);
            drawingContext.DrawLine(s_arrowPen, new Point(center.X - 3.5, center.Y + 0.5), tip);
            drawingContext.DrawLine(s_arrowPen, new Point(center.X + 3.5, center.Y + 0.5), tip);

            base.OnRender(drawingContext);
        }

        private static Pen CreatePen(Color color, double thickness)
        {
            Pen pen = new Pen(new SolidColorBrush(color), thickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            pen.Freeze();
            return pen;
        }
    }
}
