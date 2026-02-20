using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.UI;

namespace aesth_clic.Views.Roles.Admin.Pages
{
    internal class ClinicTncEntry
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public sealed partial class TermsAndConditions : Page
    {
        // TODO: Replace with your repository/service calls
        private readonly List<ClinicTncEntry> _entries = new()
        {
            new ClinicTncEntry
            {
                Id = 1,
                Title = "Photography Policy",
                Description = "Patients must obtain written consent from the clinic before capturing any photos or videos within clinic premises. Unauthorized recording of staff or other patients is strictly prohibited."
            },
            new ClinicTncEntry
            {
                Id = 2,
                Title = "Post-Procedure Care",
                Description = "Patients are required to follow all post-procedure care instructions provided by their attending clinician. The clinic assumes no liability for complications arising from failure to comply with aftercare guidelines."
            }
        };

        private int _nextId = 3;

        public TermsAndConditions()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var entry in _entries)
                ClinicTncList.Children.Add(BuildEntryCard(entry));

            RefreshEntryCount();
        }

        // ── ADD ─────────────────────────────────────────────────────
        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            var newEntry = new ClinicTncEntry { Id = _nextId++, Title = "", Description = "" };
            _entries.Add(newEntry);

            var card = BuildEntryCard(newEntry, startInEditMode: true);
            ClinicTncList.Children.Add(card);

            if (card is Border border && border.Child is Expander expander)
                expander.IsExpanded = true;

            RefreshEntryCount();
        }

        // ── BUILD CARD ──────────────────────────────────────────────
        private Border BuildEntryCard(ClinicTncEntry entry, bool startInEditMode = false)
        {
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 253, 251, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 228, 218, 245)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Tag = entry.Id
            };

            var expander = new Expander
            {
                IsExpanded = startInEditMode,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16, 12, 16, 12)
            };

            // ── Header ───────────────────────────────────────────────
            var headerView = new TextBlock
            {
                Text = entry.Title.Length > 0 ? entry.Title : "(New provision — add a title)",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 45, 21, 84)),
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = entry.Title.Length > 0 ? 1.0 : 0.5
            };

            var headerEdit = new TextBox
            {
                Text = entry.Title,
                PlaceholderText = "Enter provision title…",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 201, 168, 232)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 6, 10, 6),
                MaxLength = 120,
                Visibility = Visibility.Collapsed
            };

            var headerPanel = new Grid();
            headerPanel.Children.Add(headerView);
            headerPanel.Children.Add(headerEdit);
            expander.Header = headerPanel;

            // ── Content ──────────────────────────────────────────────
            var descView = new TextBlock
            {
                Text = entry.Description,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 122, 106, 154)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
            };

            var descEdit = new TextBox
            {
                Text = entry.Description,
                PlaceholderText = "Enter description…",
                FontSize = 13,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                MinHeight = 100,
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 201, 168, 232)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10, 8, 10, 8),
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            // Buttons
            var editBtn = new Button
            {
                Content = "Edit",
                Background = new SolidColorBrush(Color.FromArgb(255, 237, 228, 249)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 91, 45, 142)),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 7, 14, 7),
                FontSize = 13,
                Margin = new Thickness(0, 14, 8, 0),
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
            };

            var saveBtn = new Button
            {
                Content = "Save",
                Background = new SolidColorBrush(Color.FromArgb(255, 123, 63, 204)),
                Foreground = new SolidColorBrush(Colors.White),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 7, 14, 7),
                FontSize = 13,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 14, 8, 0),
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            var cancelBtn = new Button
            {
                Content = "Cancel",
                Background = new SolidColorBrush(Colors.Transparent),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 91, 45, 142)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 201, 168, 232)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 7, 14, 7),
                FontSize = 13,
                Margin = new Thickness(0, 14, 8, 0),
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            var deleteBtn = new Button
            {
                Content = "Delete",
                Background = new SolidColorBrush(Color.FromArgb(255, 254, 242, 242)),
                Foreground = new SolidColorBrush(Color.FromArgb(255, 220, 38, 38)),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(14, 7, 14, 7),
                FontSize = 13,
                Margin = new Thickness(0, 14, 0, 0),
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
            };

            var buttonRow = new StackPanel { Orientation = Orientation.Horizontal };
            buttonRow.Children.Add(editBtn);
            buttonRow.Children.Add(saveBtn);
            buttonRow.Children.Add(cancelBtn);
            buttonRow.Children.Add(deleteBtn);

            var separator = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Height = 1,
                Margin = new Thickness(0, 14, 0, 0),
                Fill = new SolidColorBrush(Color.FromArgb(40, 180, 150, 220))
            };

            var contentPanel = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };
            contentPanel.Children.Add(descView);
            contentPanel.Children.Add(descEdit);
            contentPanel.Children.Add(separator);
            contentPanel.Children.Add(buttonRow);

            expander.Content = contentPanel;
            border.Child = expander;

            // ── Wire events ──────────────────────────────────────────

            editBtn.Click += (s, e) =>
            {
                headerView.Visibility = Visibility.Collapsed;
                headerEdit.Text = entry.Title;
                headerEdit.Visibility = Visibility.Visible;

                descView.Visibility = Visibility.Collapsed;
                descEdit.Text = entry.Description;
                descEdit.Visibility = Visibility.Visible;

                editBtn.Visibility = Visibility.Collapsed;
                deleteBtn.Visibility = Visibility.Collapsed;
                saveBtn.Visibility = Visibility.Visible;
                cancelBtn.Visibility = Visibility.Visible;
            };

            saveBtn.Click += (s, e) =>
            {
                var newTitle = headerEdit.Text.Trim();
                var newDesc = descEdit.Text.Trim();

                if (newTitle.Length == 0)
                {
                    ShowValidationError(expander, "Provision title cannot be empty.");
                    return;
                }

                entry.Title = newTitle;
                entry.Description = newDesc;

                // TODO: UPDATE clinic_tnc SET title=@title, description=@desc WHERE id=@id

                headerView.Text = entry.Title;
                headerView.Opacity = 1.0;
                headerView.Visibility = Visibility.Visible;
                headerEdit.Visibility = Visibility.Collapsed;

                descView.Text = entry.Description;
                descView.Visibility = Visibility.Visible;
                descEdit.Visibility = Visibility.Collapsed;

                saveBtn.Visibility = Visibility.Collapsed;
                cancelBtn.Visibility = Visibility.Collapsed;
                editBtn.Visibility = Visibility.Visible;
                deleteBtn.Visibility = Visibility.Visible;
            };

            cancelBtn.Click += (s, e) =>
            {
                bool isNewEmpty = entry.Title.Length == 0;

                if (isNewEmpty)
                {
                    _entries.Remove(entry);
                    ClinicTncList.Children.Remove(border);
                    RefreshEntryCount();
                    return;
                }

                headerEdit.Text = entry.Title;
                headerView.Text = entry.Title;
                headerView.Visibility = Visibility.Visible;
                headerEdit.Visibility = Visibility.Collapsed;

                descEdit.Text = entry.Description;
                descView.Text = entry.Description;
                descView.Visibility = Visibility.Visible;
                descEdit.Visibility = Visibility.Collapsed;

                saveBtn.Visibility = Visibility.Collapsed;
                cancelBtn.Visibility = Visibility.Collapsed;
                editBtn.Visibility = Visibility.Visible;
                deleteBtn.Visibility = Visibility.Visible;
            };

            deleteBtn.Click += async (s, e) =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Delete Provision",
                    Content = $"Are you sure you want to delete \"{entry.Title}\"? This action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = XamlRoot,
                    RequestedTheme = ElementTheme.Light
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    // TODO: DELETE FROM clinic_tnc WHERE id=@id
                    _entries.Remove(entry);
                    ClinicTncList.Children.Remove(border);
                    RefreshEntryCount();
                }
            };

            return border;
        }

        // ── HELPERS ─────────────────────────────────────────────────

        private void RefreshEntryCount()
        {
            int n = _entries.Count;
            TxtEntryCount.Text = $"{n} {(n == 1 ? "entry" : "entries")}";
        }

        private static async void ShowValidationError(Expander expander, string message)
        {
            if (expander.Content is not StackPanel panel) return;

            var bar = new InfoBar
            {
                Severity = InfoBarSeverity.Error,
                Title = "Validation Error",
                Message = message,
                IsOpen = true,
                IsClosable = true,
                Margin = new Thickness(0, 0, 0, 8)
            };

            panel.Children.Insert(0, bar);

            await System.Threading.Tasks.Task.Delay(4000);
            bar.IsOpen = false;
            panel.Children.Remove(bar);
        }
    }
}