using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Utils
{
    /// <summary>
    /// Utility for showing auto-dismissing InfoBar toasts on any page.
    ///
    /// USAGE (in any Page code-behind):
    ///   1. Add an InfoBar to your XAML:
    ///         &lt;InfoBar x:Name="ToastBar" IsOpen="False" IsClosable="True" /&gt;
    ///
    ///   2. Call from code-behind:
    ///         ToastHelper.Show(ToastBar, InfoBarSeverity.Success, "Client added", "Details here.");
    ///
    /// The bar auto-dismisses after <see cref="DefaultDurationSeconds"/> seconds.
    /// Calling Show() again before it dismisses resets the timer.
    /// </summary>
    public static class ToastHelper
    {
        public const int DefaultDurationSeconds = 3;

        // One timer per InfoBar instance, keyed by reference
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<InfoBar, DispatcherTimer>
            _timers = new();

        /// <summary>
        /// Shows a toast on the given InfoBar and auto-dismisses it.
        /// </summary>
        /// <param name="bar">The InfoBar control in the page XAML.</param>
        /// <param name="severity">Success | Informational | Warning | Error</param>
        /// <param name="title">Bold heading text.</param>
        /// <param name="message">Optional detail text below the title.</param>
        /// <param name="durationSeconds">Override the auto-dismiss delay (default 3 s).</param>
        public static void Show(
            InfoBar bar,
            InfoBarSeverity severity,
            string title,
            string message = "",
            int durationSeconds = DefaultDurationSeconds)
        {
            // Stop any existing timer for this bar so rapid calls reset the clock
            if (_timers.TryGetValue(bar, out var existing))
            {
                existing.Stop();
                _timers.Remove(bar);
            }

            bar.Severity = severity;
            bar.Title = title;
            bar.Message = message;
            bar.IsOpen = true;

            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationSeconds)
            };

            timer.Tick += (_, _) =>
            {
                bar.IsOpen = false;
                timer.Stop();
                _timers.Remove(bar);
            };

            _timers.Add(bar, timer);
            timer.Start();
        }

        // ── Convenience overloads ──────────────────────────────────────────

        public static void Success(InfoBar bar, string title, string message = "")
            => Show(bar, InfoBarSeverity.Success, title, message);

        public static void Error(InfoBar bar, string title, string message = "")
            => Show(bar, InfoBarSeverity.Error, title, message);

        public static void Warning(InfoBar bar, string title, string message = "")
            => Show(bar, InfoBarSeverity.Warning, title, message);

        public static void Info(InfoBar bar, string title, string message = "")
            => Show(bar, InfoBarSeverity.Informational, title, message);
    }
}