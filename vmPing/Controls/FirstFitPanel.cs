using System.Windows;
using System.Windows.Controls;

namespace vmPing.Controls
{
    // Shows the first child that fits in the available width and hides the rest.
    // List the children from widest to narrowest. If none of them fit, the last one is shown.
    public class FirstFitPanel : Panel
    {
        private UIElement _shownChild;

        protected override Size MeasureOverride(Size availableSize)
        {
            _shownChild = null;
            foreach (UIElement child in InternalChildren)
            {
                // Measure with unlimited width to get the width the child needs.
                child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
                if (_shownChild == null && child.DesiredSize.Width <= availableSize.Width)
                {
                    _shownChild = child;
                }
            }

            if (_shownChild == null && InternalChildren.Count > 0)
            {
                _shownChild = InternalChildren[InternalChildren.Count - 1];
            }

            if (_shownChild == null)
            {
                return new Size();
            }

            // Measure again with the available width, so the child can fill it.
            _shownChild.Measure(availableSize);
            return _shownChild.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in InternalChildren)
            {
                // A child arranged with no space is clipped away, which hides it.
                child.Arrange(child == _shownChild ? new Rect(finalSize) : new Rect());
            }
            return finalSize;
        }
    }
}
