using aesth_clic.Master.Controller;
using aesth_clic.Utils;
using aesth_clic.ViewModels.SuperAdmin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    public sealed partial class Dashboard : Page
    {
        private readonly SuperAdminDashboardViewModel _vm = new();
        private readonly DashboardController _dashboardController;

        public Dashboard()
        {
            InitializeComponent();
            _dashboardController = App.Services.GetRequiredService<DashboardController>();
            Loaded += OnPageLoaded;
        }

        // ── Page loaded ───────────────────────────────────────────────────────
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var dto = await _dashboardController.GetSuperAdminDashboardAsync();
                _vm.LoadFromDto(dto);
                ApplyViewModelToUi();
                DrawLineChart(_vm.ChartLabels, _vm.ChartValues);
            }
            catch (Exception ex)
            {
                TxtMonthlyRevenue.Text = "—";
                TxtTotalUsers.Text = "—";
                TxtActiveUsers.Text = "—";
                System.Diagnostics.Debug.WriteLine($"Dashboard load error: {ex}");
            }
        }

        // ── Redraws chart whenever the card resizes ───────────────────────────
        private void ChartCard_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_vm.ChartLabels.Count > 0 && _vm.ChartValues.Count > 0)
                DrawLineChart(_vm.ChartLabels, _vm.ChartValues);
        }

        // ── Bind ViewModel → named XAML elements ──────────────────────────────
        private void ApplyViewModelToUi()
        {
            TxtMonthlyRevenue.Text = _vm.MonthlyRevenue;

            TxtTotalUsers.Text = _vm.TotalClients;
            TxtUserRoles.Text = $"{_vm.TotalClients} clinic{(_vm.TotalClients != "1" ? "s" : "")} registered";

            TxtActiveUsers.Text = _vm.ActiveClients;
            TxtInactiveUsers.Text = _vm.InactiveClientsLabel;
            TxtInactiveUsers.Foreground = _vm.InactiveClientsForeground;

            TxtRevenueTrend.Visibility = Visibility.Collapsed;
        }

        // ── KPI Card click → navigate ContentFrame ────────────────────────────
        private void KpiCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;

            string tag = btn.Tag?.ToString() ?? string.Empty;

            Type? targetPage = tag switch
            {
                "PaymentManagement" => typeof(PaymentManagement),
                "UserManagement" => typeof(UserManagement),
                _ => null
            };

            if (targetPage is null) return;

            // Navigate the ContentFrame
            Frame?.Navigate(targetPage);

            // Sync the NavigationView sidebar highlight
            if (Frame?.Parent is NavigationView navView)
            {
                foreach (var item in navView.MenuItems)
                {
                    if (item is NavigationViewItem nvi && nvi.Tag?.ToString() == tag)
                    {
                        navView.SelectedItem = nvi;
                        break;
                    }
                }
            }
        }

        // ─────────────────────────────────────────
        // MONTHLY NEW CLINICS LINE CHART
        // ─────────────────────────────────────────
        private void DrawLineChart(List<string> labels, List<int> values)
        {
            AppointmentChartCanvas.Children.Clear();

            double canvasWidth = ChartCard.ActualWidth - 40;
            double canvasHeight = ChartCard.ActualHeight - 40 - 52 - 12;

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
            if (count < 2) return;

            int maxValue = 0;
            foreach (var v in values) if (v > maxValue) maxValue = v;

            int gridMax = Math.Max(
                (int)(Math.Ceiling(maxValue / 10.0) * 10) + 10,
                10);

            // ── Grid lines + Y labels ──────────────────────────────────────────
            const int gridLines = 5;
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

            // ── Point coordinates ──────────────────────────────────────────────
            double stepX = chartWidth / (count - 1);
            var points = new List<Point>();

            for (int i = 0; i < count; i++)
            {
                double x = paddingLeft + i * stepX;
                double y = paddingTop + chartHeight
                           - (chartHeight * values[i] / (double)gridMax);
                points.Add(new Point(x, y));
            }

            // ── Filled area ────────────────────────────────────────────────────
            var areaFigure = new PathFigure
            {
                StartPoint = new Point(points[0].X, paddingTop + chartHeight)
            };
            areaFigure.Segments.Add(new LineSegment { Point = points[0] });
            for (int i = 1; i < count; i++)
                areaFigure.Segments.Add(new LineSegment { Point = points[i] });
            areaFigure.Segments.Add(
                new LineSegment { Point = new Point(points[count - 1].X, paddingTop + chartHeight) });
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
                        new GradientStop { Color = Color.FromArgb(60,  0, 120, 212), Offset = 0 },
                        new GradientStop { Color = Color.FromArgb(0,   0, 120, 212), Offset = 1 }
                    }
                }
            });

            // ── Line segments ──────────────────────────────────────────────────
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

            // ── Data point dots ────────────────────────────────────────────────
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

            // ── X-axis labels ──────────────────────────────────────────────────
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