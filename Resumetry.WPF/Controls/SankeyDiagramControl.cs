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
        private const double NodeWidth = 80;
        private const double NodePadding = 20;
        private const double MinNodeHeight = 40;
        private const double FlowOpacity = 0.5;

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

            // Calculate total applications for scaling
            var totalApplications = ReportData
                .Where(r => r.From == "Applied")
                .Sum(r => r.Count);

            if (totalApplications == 0)
            {
                DrawEmptyState(dc);
                return;
            }

            // Define node positions and colors
            var nodes = new Dictionary<string, NodeInfo>
            {
                ["Applied"] = new NodeInfo("Applied", 0, Colors.CornflowerBlue),
                ["No Response"] = new NodeInfo("No Response", 2, Colors.Teal),
                ["Responded"] = new NodeInfo("Responded", 1, Colors.Olive),
                ["Interview"] = new NodeInfo("Interview", 1, Colors.MediumPurple),
                ["Rejected"] = new NodeInfo("Rejected", 2, Colors.Crimson),
                ["Offer"] = new NodeInfo("Offer", 2, Colors.MediumSeaGreen),
                ["No Offer"] = new NodeInfo("No Offer", 2, Colors.HotPink)
            };

            // Calculate node sizes based on flow volumes
            foreach (var data in ReportData)
            {
                if (nodes.ContainsKey(data.From))
                    nodes[data.From].OutgoingCount += data.Count;
                if (nodes.ContainsKey(data.To))
                    nodes[data.To].IncomingCount += data.Count;
            }

            // Calculate column positions
            var columnSpacing = width / 4;
            var columnX = new[] { columnSpacing * 0.5, columnSpacing * 2, columnSpacing * 3.5 };

            // Arrange nodes vertically within their columns
            var columnNodes = nodes.Values
                .GroupBy(n => n.Column)
                .ToDictionary(g => g.Key, g => g.OrderBy(n => n.Name).ToList());

            var usableHeight = height - 40; // Leave margin
            var heightScale = usableHeight / Math.Max(1, totalApplications);

            foreach (var column in columnNodes.Keys)
            {
                var nodesInColumn = columnNodes[column];
                var totalHeight = nodesInColumn.Sum(n => Math.Max(MinNodeHeight, Math.Max(n.IncomingCount, n.OutgoingCount) * heightScale));
                var spacing = nodesInColumn.Count > 1 ? (usableHeight - totalHeight) / (nodesInColumn.Count - 1) : 0;

                var y = 20.0;
                foreach (var node in nodesInColumn)
                {
                    node.Height = Math.Max(MinNodeHeight, Math.Max(node.IncomingCount, node.OutgoingCount) * heightScale);
                    node.X = columnX[column] - NodeWidth / 2;
                    node.Y = y;
                    y += node.Height + spacing;
                }
            }

            // Draw flows first (behind nodes)
            foreach (var data in ReportData)
            {
                if (data.Count > 0 && nodes.ContainsKey(data.From) && nodes.ContainsKey(data.To))
                {
                    var fromNode = nodes[data.From];
                    var toNode = nodes[data.To];

                    DrawFlow(dc, fromNode, toNode, data.Count, heightScale, fromNode.Color);
                }
            }

            // Draw nodes on top
            foreach (var node in nodes.Values)
            {
                if (node.IncomingCount > 0 || node.OutgoingCount > 0)
                {
                    DrawNode(dc, node);
                }
            }
        }

        private void DrawFlow(DrawingContext dc, NodeInfo from, NodeInfo to, int count, double heightScale, Color color)
        {
            var flowHeight = Math.Max(2, count * heightScale);

            // Calculate start and end points
            var startX = from.X + NodeWidth;
            var startY = from.Y + from.Height / 2 - flowHeight / 2;
            var endX = to.X;
            var endY = to.Y + to.Height / 2 - flowHeight / 2;

            // Create bezier curve path
            var startPoint = new Point(startX, startY + flowHeight / 2);
            var endPoint = new Point(endX, endY + flowHeight / 2);
            var controlOffset = (endX - startX) / 2;

            var figure = new PathFigure { StartPoint = startPoint };

            // Top curve
            figure.Segments.Add(new BezierSegment(
                new Point(startX + controlOffset, startY),
                new Point(endX - controlOffset, endY),
                new Point(endX, endY),
                true));

            // Bottom curve (reversed)
            figure.Segments.Add(new LineSegment(new Point(endX, endY + flowHeight), true));
            figure.Segments.Add(new BezierSegment(
                new Point(endX - controlOffset, endY + flowHeight),
                new Point(startX + controlOffset, startY + flowHeight),
                new Point(startX, startY + flowHeight),
                true));

            figure.IsClosed = true;

            var geometry = new PathGeometry(new[] { figure });
            var brush = new SolidColorBrush(color) { Opacity = FlowOpacity };
            dc.DrawGeometry(brush, null, geometry);
        }

        private void DrawNode(DrawingContext dc, NodeInfo node)
        {
            var rect = new Rect(node.X, node.Y, NodeWidth, node.Height);
            var brush = new SolidColorBrush(node.Color);
            var pen = new Pen(Brushes.White, 2);

            dc.DrawRectangle(brush, pen, rect);

            // Draw label
            var typeface = new Typeface("Segoe UI");
            var label = new FormattedText(
                node.Name,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                12,
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            var count = Math.Max(node.IncomingCount, node.OutgoingCount);
            var countText = new FormattedText(
                count.ToString(),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                14,
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            var labelX = node.X + (NodeWidth - label.Width) / 2;
            var labelY = node.Y + (node.Height - label.Height - countText.Height - 4) / 2;

            dc.DrawText(label, new Point(labelX, labelY));
            dc.DrawText(countText, new Point(node.X + (NodeWidth - countText.Width) / 2, labelY + label.Height + 4));
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

        private class NodeInfo
        {
            public string Name { get; }
            public int Column { get; }
            public Color Color { get; }
            public int IncomingCount { get; set; }
            public int OutgoingCount { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
            public double Height { get; set; }

            public NodeInfo(string name, int column, Color color)
            {
                Name = name;
                Column = column;
                Color = color;
            }
        }
    }
}
