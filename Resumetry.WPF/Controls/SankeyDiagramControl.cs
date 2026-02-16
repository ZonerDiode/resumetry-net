using System.Collections.Immutable;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Resumetry.Application.DTOs;

namespace Resumetry.WPF.Controls
{
    /// <summary>
    /// Custom control that renders a Sankey diagram for application status flow visualization.
    /// </summary>
    public class SankeyDiagramControl : FrameworkElement
    {
        private const double NodeWidth = 15;
        private const double NodeSpacing = 8;
        private const double DiagramMargin = 20;
        private const double FlowOpacity = 0.4;
        private const double LabelMargin = 8;

        public static readonly DependencyProperty ReportDataProperty =
            DependencyProperty.Register(
                nameof(ReportData),
                typeof(ImmutableList<SankeyReportData>),
                typeof(SankeyDiagramControl),
                new FrameworkPropertyMetadata(
                    ImmutableList<SankeyReportData>.Empty,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the Sankey report data to visualize.
        /// </summary>
        public ImmutableList<SankeyReportData> ReportData
        {
            get => (ImmutableList<SankeyReportData>)GetValue(ReportDataProperty);
            set => SetValue(ReportDataProperty, value);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (ReportData == null || ReportData.Count == 0)
            {
                DrawEmptyState(dc);
                return;
            }

            var width = ActualWidth;
            var height = ActualHeight;

            if (width <= 0 || height <= 0)
                return;

            var totalApplications = ReportData
                .Where(r => r.From == "Applied")
                .Sum(r => r.Count);

            if (totalApplications == 0)
            {
                DrawEmptyState(dc);
                return;
            }

            var nodes = new Dictionary<string, NodeInfo>
            {
                ["Applied"] = new NodeInfo("Applied", 0, Colors.CornflowerBlue),
                ["No Response"] = new NodeInfo("No Response", 3, Colors.Teal),
                ["Responded"] = new NodeInfo("Responded", 1, Colors.Olive),
                ["Interview"] = new NodeInfo("Interview", 2, Colors.MediumPurple),
                ["Rejected"] = new NodeInfo("Rejected", 3, Colors.Crimson),
                ["Offer"] = new NodeInfo("Offer", 3, Colors.MediumSeaGreen),
                ["No Offer"] = new NodeInfo("No Offer", 3, Colors.SlateGray)
            };

            foreach (var data in ReportData)
            {
                if (nodes.TryGetValue(data.From, out var fromNode))
                    fromNode.OutgoingCount += data.Count;
                if (nodes.TryGetValue(data.To, out var toNode))
                    toNode.IncomingCount += data.Count;
            }

            // Column X positions: left edge, ~35%, ~60%, right edge
            var columnX = new[]
            {
                DiagramMargin,
                width * 0.35,
                width * 0.58,
                width - DiagramMargin - NodeWidth
            };

            var usableHeight = height - DiagramMargin * 2;
            var heightScale = usableHeight / Math.Max(1, totalApplications);

            // Arrange nodes vertically within columns
            var columnNodes = nodes.Values
                .Where(n => n.IncomingCount > 0 || n.OutgoingCount > 0)
                .GroupBy(n => n.Column)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(n => Math.Max(n.IncomingCount, n.OutgoingCount)).ToList());

            foreach (var column in columnNodes.Keys)
            {
                var nodesInColumn = columnNodes[column];
                foreach (var node in nodesInColumn)
                    node.Height = Math.Max(4, Math.Max(node.IncomingCount, node.OutgoingCount) * heightScale);

                var totalHeight = nodesInColumn.Sum(n => n.Height);
                var spacing = nodesInColumn.Count > 1
                    ? Math.Max(NodeSpacing, (usableHeight - totalHeight) / (nodesInColumn.Count - 1))
                    : 0;

                var y = DiagramMargin;
                foreach (var node in nodesInColumn)
                {
                    node.X = columnX[node.Column];
                    node.Y = y;
                    y += node.Height + spacing;
                }
            }

            // Define flow order to control stacking (largest flows first)
            var orderedFlows = ReportData
                .Where(d => d.Count > 0 && nodes.ContainsKey(d.From) && nodes.ContainsKey(d.To))
                .OrderByDescending(d => d.Count)
                .ToList();

            // Draw flows (behind nodes)
            foreach (var data in orderedFlows)
            {
                var fromNode = nodes[data.From];
                var toNode = nodes[data.To];
                var flowHeight = Math.Max(2, data.Count * heightScale);

                var startX = fromNode.X + NodeWidth;
                var startY = fromNode.Y + fromNode.OutgoingOffset;
                var endX = toNode.X;
                var endY = toNode.Y + toNode.IncomingOffset;

                fromNode.OutgoingOffset += flowHeight;
                toNode.IncomingOffset += flowHeight;

                DrawFlow(dc, startX, startY, endX, endY, flowHeight, fromNode.Color);
            }

            // Draw nodes on top
            foreach (var node in nodes.Values)
            {
                if (node.IncomingCount > 0 || node.OutgoingCount > 0)
                    DrawNode(dc, node);
            }
        }

        private void DrawFlow(DrawingContext dc, double startX, double startY, double endX, double endY, double flowHeight, Color color)
        {
            var controlOffset = (endX - startX) / 2;

            var figure = new PathFigure { StartPoint = new Point(startX, startY) };

            // Top edge curve
            figure.Segments.Add(new BezierSegment(
                new Point(startX + controlOffset, startY),
                new Point(endX - controlOffset, endY),
                new Point(endX, endY),
                true));

            // Right edge down
            figure.Segments.Add(new LineSegment(new Point(endX, endY + flowHeight), true));

            // Bottom edge curve (reversed)
            figure.Segments.Add(new BezierSegment(
                new Point(endX - controlOffset, endY + flowHeight),
                new Point(startX + controlOffset, startY + flowHeight),
                new Point(startX, startY + flowHeight),
                true));

            figure.IsClosed = true;

            var geometry = new PathGeometry([figure]);
            var brush = new SolidColorBrush(color) { Opacity = FlowOpacity };
            dc.DrawGeometry(brush, null, geometry);
        }

        private void DrawNode(DrawingContext dc, NodeInfo node)
        {
            var rect = new Rect(node.X, node.Y, NodeWidth, node.Height);
            var brush = new SolidColorBrush(node.Color);

            dc.DrawRectangle(brush, null, rect);

            // Draw label outside the node bar
            var typeface = new Typeface("Segoe UI");
            var count = Math.Max(node.IncomingCount, node.OutgoingCount);
            var labelText = $"{node.Name} ({count})";

            var label = new FormattedText(
                labelText,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                13,
                new SolidColorBrush(node.Color),
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            double labelX;
            if (node.Column == 3)
            {
                // Right-column nodes: label to the left of the node
                labelX = node.X - label.Width - LabelMargin;
            }
            else
            {
                // Left/middle nodes: label to the right of the node
                labelX = node.X + NodeWidth + LabelMargin;
            }

            var labelY = node.Y + (node.Height - label.Height) / 2;
            dc.DrawText(label, new Point(labelX, labelY));
        }

        private void DrawEmptyState(DrawingContext dc)
        {
            var typeface = new Typeface("Segoe UI");
            var text = new FormattedText(
                "No application data available",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                16,
                Brushes.Gray,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            var x = (ActualWidth - text.Width) / 2;
            var y = (ActualHeight - text.Height) / 2;

            dc.DrawText(text, new Point(x, y));
        }

        private class NodeInfo(string name, int column, Color color)
        {
            public string Name { get; } = name;
            public int Column { get; } = column;
            public Color Color { get; } = color;
            public int IncomingCount { get; set; }
            public int OutgoingCount { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Height { get; set; }
            public double OutgoingOffset { get; set; }
            public double IncomingOffset { get; set; }
        }
    }
}
