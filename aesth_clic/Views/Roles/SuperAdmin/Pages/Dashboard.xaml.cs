using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    public sealed partial class Dashboard : Page
    {
        private List<string>? _chartLabels;
        private List<int>? _chartValues;

        public Dashboard()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            LoadKpiMetrics();
            LoadAppointmentChart();
        }

        // Redraws chart whenever the card resizes (window resize or first layout)
        private void ChartCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_chartLabels != null && _chartValues != null)
                DrawLineChart(_chartLabels, _chartValues);
        }

        // ─────────────────────────────────────────
        // KPI CARDS
        // ─────────────────────────────────────────
        private void LoadKpiMetrics()
        {
            // TODO: SELECT SUM(amount) FROM payments
            // WHERE status = 'paid'
            // AND MONTH(payment_date) = MONTH(CURDATE())
            // AND YEAR(payment_date) = YEAR(CURDATE())
            TxtMonthlyRevenue.Text = "₱84,500.00";
            TxtRevenueTrend.Text = "↑ 12% vs last month";
            TxtRevenueTrend.Foreground = new SolidColorBrush(Color.FromArgb(255, 14, 164, 122));

            // TODO: SELECT COUNT(*) FROM users
            int totalUsers = 8;
            TxtTotalUsers.Text = totalUsers.ToString();
            TxtUserRoles.Text = "5 roles in system";

            // TODO: SELECT COUNT(*) FROM users WHERE is_active = TRUE
            int activeUsers = 6;
            int inactiveUsers = totalUsers - activeUsers;
            TxtActiveUsers.Text = activeUsers.ToString();
            TxtInactiveUsers.Text = $"{inactiveUsers} inactive account{(inactiveUsers != 1 ? "s" : "")}";

            TxtInactiveUsers.Foreground = inactiveUsers == 0
                ? new SolidColorBrush(Color.FromArgb(255, 14, 164, 122))
                : new SolidColorBrush(Color.FromArgb(255, 216, 59, 1));
        }

        // ─────────────────────────────────────────
        // MONTHLY APPOINTMENTS LINE CHART
        // ─────────────────────────────────────────
        private void LoadAppointmentChart()
        {
            // TODO: SELECT MONTH(appointment_date), COUNT(*)
            // FROM appointments
            // WHERE appointment_date >= DATE_SUB(CURDATE(), INTERVAL 12 MONTH)
            // GROUP BY MONTH(appointment_date)
            // ORDER BY appointment_date ASC

            _chartLabels = new List<string>
            {
                "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct",
                "Nov", "Dec", "Jan", "Feb"
            };

            _chartValues = new List<int>
            {
                18, 24, 30, 27,
                35, 40, 38, 45,
                42, 50, 47, 55
            };

            DrawLineChart(_chartLabels, _chartValues);
        }

        private void DrawLineChart(List<string> labels, List<int> values)
        {
            AppointmentChartCanvas.Children.Clear();

            // Read actual card size — subtract padding (20 each side = 40)
            // Also subtract the chart header height (~52px) + RowSpacing (12)
            double canvasWidth = ChartCard.ActualWidth - 40;
            double canvasHeight = ChartCard.ActualHeight - 40 - 52 - 12;

            // Guard: skip if layout hasn't happened yet
            if (canvasWidth <= 0 || canvasHeight <= 0) return;

            AppointmentChartCanvas.Width = canvasWidth;
            AppointmentChartCanvas.Height = canvasHeight;

            const double paddingLeft = 48;
            const double paddingRight = 16;
            const double paddingTop = 16;
            const double paddingBottom = 36;

            double chartWidth = canvasWidth - paddingLeft - paddingRight;
            double chartHeight = canvasHeight - paddingTop - paddingBottom;

            int count = values.Count;
            int maxValue = 0;
            foreach (var v in values) if (v > maxValue) maxValue = v;
            int gridMax = (int)(System.Math.Ceiling(maxValue / 10.0) * 10) + 10;

            // ── Grid lines + Y labels ──
            int gridLines = 5;
            for (int i = 0; i <= gridLines; i++)
            {
                double y = paddingTop + chartHeight - (chartHeight * i / gridLines);
                int yValue = (int)(gridMax * i / gridLines);

                AppointmentChartCanvas.Children.Add(new Line
                {
                    X1 = paddingLeft,
                    Y1 = y,
                    X2 = paddingLeft + chartWidth,
                    Y2 = y,
                    Stroke = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128)),
                    StrokeThickness = 1
                });

                var yLabel = new TextBlock
                {
                    Text = yValue.ToString(),
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromArgb(180, 128, 128, 128))
                };
                Canvas.SetLeft(yLabel, 0);
                Canvas.SetTop(yLabel, y - 8);
                AppointmentChartCanvas.Children.Add(yLabel);
            }

            // ── Point coordinates ──
            double stepX = chartWidth / (count - 1);
            var points = new List<Point>();

            for (int i = 0; i < count; i++)
            {
                double x = paddingLeft + i * stepX;
                double y = paddingTop + chartHeight - (chartHeight * values[i] / (double)gridMax);
                points.Add(new Point(x, y));
            }

            // ── Filled area under the line ──
            var areaFigure = new PathFigure
            {
                StartPoint = new Point(points[0].X, paddingTop + chartHeight)
            };
            areaFigure.Segments.Add(new LineSegment { Point = points[0] });
            for (int i = 1; i < points.Count; i++)
                areaFigure.Segments.Add(new LineSegment { Point = points[i] });
            areaFigure.Segments.Add(new LineSegment { Point = new Point(points[count - 1].X, paddingTop + chartHeight) });
            areaFigure.IsClosed = true;

            AppointmentChartCanvas.Children.Add(new Path
            {
                Data = new PathGeometry { Figures = { areaFigure } },
                Fill = new LinearGradientBrush
                {
                    StartPoint = new Point(0, 0),
                    EndPoint = new Point(0, 1),
                    GradientStops =
                    {
                        new GradientStop { Color = Color.FromArgb(60, 0, 120, 212), Offset = 0 },
                        new GradientStop { Color = Color.FromArgb(0,  0, 120, 212), Offset = 1 }
                    }
                }
            });

            // ── Line segments ──
            for (int i = 0; i < count - 1; i++)
            {
                AppointmentChartCanvas.Children.Add(new Line
                {
                    X1 = points[i].X,
                    Y1 = points[i].Y,
                    X2 = points[i + 1].X,
                    Y2 = points[i + 1].Y,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)),
                    StrokeThickness = 2.5
                });
            }

            // ── Data point dots ──
            foreach (var pt in points)
            {
                var dot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = new SolidColorBrush(Color.FromArgb(255, 0, 120, 212)),
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)),
                    StrokeThickness = 2
                };
                Canvas.SetLeft(dot, pt.X - 4);
                Canvas.SetTop(dot, pt.Y - 4);
                AppointmentChartCanvas.Children.Add(dot);
            }

            // ── X-axis labels ──
            for (int i = 0; i < count; i++)
            {
                var xLabel = new TextBlock
                {
                    Text = labels[i],
                    FontSize = 11,
                    Foreground = new SolidColorBrush(Color.FromArgb(180, 128, 128, 128))
                };
                Canvas.SetLeft(xLabel, points[i].X - 10);
                Canvas.SetTop(xLabel, paddingTop + chartHeight + 8);
                AppointmentChartCanvas.Children.Add(xLabel);
            }
        }
    }
}