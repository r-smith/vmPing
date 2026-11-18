using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using vmPing.Probes;

namespace vmPing.Controls
{
    // Shows a probe's output as selectable text. New output scrolls into view
    // unless the user has scrolled up or is selecting text.
    public class ProbeOutputBox : TextBox
    {
        public static readonly DependencyProperty ProbeProperty = DependencyProperty.Register(
            nameof(Probe), typeof(Probe), typeof(ProbeOutputBox), new PropertyMetadata(null, OnProbeChanged));

        private bool _isAutoScrollEnabled = true;
        private bool _hasText;
        private AutoScrollAdorner _adorner;
        private ScrollViewer _scrollViewer;

        public ProbeOutputBox()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(OnScrollChanged));
            SelectionChanged += OnSelectionChanged;
        }

        public Probe Probe
        {
            get => (Probe)GetValue(ProbeProperty);
            set => SetValue(ProbeProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _scrollViewer = GetTemplateChild("PART_ContentHost") as ScrollViewer;
        }

        private static void OnProbeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProbeOutputBox outputBox = (ProbeOutputBox)d;
            if (outputBox.IsLoaded)
            {
                outputBox.Detach(e.OldValue as Probe);
                outputBox.Attach(e.NewValue as Probe);
            }
        }

        // Only listen to the probe while loaded, so the probe doesn't keep old controls alive.
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Attach(Probe);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Detach(Probe);
        }

        private void Attach(Probe probe)
        {
            _isAutoScrollEnabled = true;
            if (probe == null)
            {
                ShowText(string.Empty);
                return;
            }

            Detach(probe);
            probe.HistoryAdded += Probe_HistoryAdded;
            probe.HistoryReset += Probe_HistoryReset;
            ShowText(probe.GetHistoryText());
        }

        private void Detach(Probe probe)
        {
            if (probe == null)
            {
                return;
            }

            probe.HistoryAdded -= Probe_HistoryAdded;
            probe.HistoryReset -= Probe_HistoryReset;
        }

        private void Probe_HistoryAdded(object sender, string line)
        {
            AppendText(_hasText ? Environment.NewLine + line : line);
            _hasText = true;
        }

        private void Probe_HistoryReset(object sender, EventArgs e)
        {
            ShowText(((Probe)sender).GetHistoryText());
        }

        private void ShowText(string text)
        {
            Text = text;
            _hasText = text.Length > 0;
            if (_isAutoScrollEnabled)
            {
                ScrollToBottom();
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0 && e.ViewportHeightChange == 0)
            {
                // The user scrolled. Auto-scroll turns back on at the bottom.
                _isAutoScrollEnabled = IsScrolledToBottom();
            }
            else if (_isAutoScrollEnabled)
            {
                // New output was added or the control was resized.
                ScrollToBottom();
            }

            UpdateAdorner();
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            // Catch up on output that arrived while text was selected.
            if (_isAutoScrollEnabled)
            {
                ScrollToBottom();
            }
        }

        private void ScrollToBottom()
        {
            // Don't move text the user is selecting or has selected.
            // TextBox.ScrollToEnd forces a layout of the whole window, which is slow with many probes.
            // The ScrollViewer waits for the next layout instead.
            if (SelectionLength == 0 && !IsMouseCaptureWithin)
            {
                _scrollViewer?.ScrollToEnd();
            }
        }

        private bool IsScrolledToBottom()
        {
            return VerticalOffset + ViewportHeight >= ExtentHeight - 1;
        }

        // Shows a badge while there is output below the visible area.
        private void UpdateAdorner()
        {
            const double MinimumAdornerHeight = 21.0;

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            if (layer == null)
            {
                return;
            }

            if (_adorner == null)
            {
                _adorner = new AutoScrollAdorner(this);
                _adorner.MouseLeftButtonDown += Adorner_MouseLeftButtonDown;
            }

            bool isShown = layer.GetAdorners(this) != null;
            bool shouldShow = !IsScrolledToBottom() && ActualHeight >= MinimumAdornerHeight;
            if (shouldShow && !isShown)
            {
                layer.Add(_adorner);
            }
            else if (!shouldShow && isShown)
            {
                layer.Remove(_adorner);
            }
        }

        // Clicking the badge clears any selection, which would pause auto-scroll,
        // then jumps to the newest output and turns auto-scroll back on.
        private void Adorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isAutoScrollEnabled = true;
            Select(Text.Length, 0);
            _scrollViewer?.ScrollToEnd();
            e.Handled = true;
        }

        // Let probes be dropped onto the output. A text box would otherwise take the drag.
        protected override void OnDragEnter(DragEventArgs e)
        {
        }

        protected override void OnDragOver(DragEventArgs e)
        {
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
        }

        protected override void OnDrop(DragEventArgs e)
        {
        }
    }
}
