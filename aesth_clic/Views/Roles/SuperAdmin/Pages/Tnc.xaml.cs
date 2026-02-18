using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    // ─────────────────────────────────────────────────────────────
    // Simple model – swap for your real EF / DB model later
    // ─────────────────────────────────────────────────────────────
    internal class TncEntry
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public sealed partial class TNC : Page
    {
        // ── In-memory list (replace with DB calls once ready) ──
        private readonly List<TncEntry> _entries = new()
        {
            new TncEntry
            {
                Id = 1,
                Title = "Acceptance of Terms",
                Description =
                    "By accessing or using the services provided by Aesthetic Clinic, you agree to be " +
                    "bound by these Terms and Conditions. If you do not agree with any part of these " +
                    "terms, you must not use our services. Continued use of our services following the " +
                    "posting of any changes constitutes your acceptance of those changes."
            },
            new TncEntry
            {
                Id = 2,
                Title = "Appointment & Cancellation Policy",
                Description =
                    "Appointments must be scheduled at least 24 hours in advance. Cancellations made " +
                    "less than 12 hours before the scheduled time may incur a cancellation fee equivalent " +
                    "to 20% of the service cost. No-shows without prior notice will be charged the full " +
                    "service amount. The clinic reserves the right to reschedule appointments due to " +
                    "unforeseen circumstances."
            },
            new TncEntry
            {
                Id = 3,
                Title = "Payment Terms",
                Description =
                    "Full payment is due at the time of service unless a pre-approved payment plan is in " +
                    "place. We accept cash, major credit/debit cards, and approved digital wallets. " +
                    "Promotional rates and discounts cannot be combined unless explicitly stated. Refunds " +
                    "are only applicable for pre-paid packages and are subject to management approval."
            },
            new TncEntry
            {
                Id = 4,
                Title = "Privacy & Confidentiality",
                Description =
                    "All personal and medical information collected during your visit is treated as " +
                    "strictly confidential and is used solely for the purpose of providing our services. " +
                    "We do not share, sell, or disclose your information to third parties without your " +
                    "explicit written consent, except as required by law. You may request access to or " +
                    "deletion of your data at any time by contacting our Data Privacy Officer."
            }
        };

        private int _nextId = 5;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────
        public TNC()
        {
            InitializeComponent();
            Loaded += OnPageLoaded;
        }

        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            foreach (var entry in _entries)
                TncList.Children.Add(BuildEntryCard(entry));

            RefreshEntryCount();
        }

        // ─────────────────────────────────────────────────────────────
        // ADD NEW TNC  →  appends a blank card already in edit mode
        // ─────────────────────────────────────────────────────────────
        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            var newEntry = new TncEntry { Id = _nextId++, Title = "", Description = "" };
            _entries.Add(newEntry);

            var card = BuildEntryCard(newEntry, startInEditMode: true);
            TncList.Children.Add(card);

            // Auto-expand the new card
            if (card is Border border && border.Child is Expander expander)
                expander.IsExpanded = true;

            RefreshEntryCount();
        }

        // ─────────────────────────────────────────────────────────────
        // BUILD ENTRY CARD
        // Each card is a Border > Expander.
        // The Expander header   = title (view) or TextBox (edit)
        // The Expander content  = description text + action buttons
        // ─────────────────────────────────────────────────────────────
        private Border BuildEntryCard(TncEntry entry, bool startInEditMode = false)
        {
            // ── Root border ──────────────────────────────────────────
            var border = new Border
            {
                Style = (Style)Resources["TncCardStyle"],
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(8),
                Tag = entry.Id          // easy lookup later
            };

            // ── Expander ─────────────────────────────────────────────
            var expander = new Expander
            {
                IsExpanded = startInEditMode,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16, 12, 16, 12)
            };

            // ── Header: TextBlock (view) + TextBox (edit) ─────────────
            var headerView = new TextBlock
            {
                Text = entry.Title.Length > 0 ? entry.Title : "(New entry — add a title)",
                Style = (Style)Resources["EntryTitleStyle"],
                Opacity = entry.Title.Length > 0 ? 1.0 : 0.5
            };

            var headerEdit = new TextBox
            {
                Text = entry.Title,
                PlaceholderText = "Enter title…",
                Style = (Style)Resources["EditTitleBoxStyle"],
                Visibility = Visibility.Collapsed
            };

            var headerPanel = new Grid();
            headerPanel.Children.Add(headerView);
            headerPanel.Children.Add(headerEdit);
            expander.Header = headerPanel;

            // ── Content: description + buttons ───────────────────────

            // View mode
            var descView = new TextBlock
            {
                Text = entry.Description,
                Style = (Style)Resources["DescriptionStyle"],
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
            };

            // Edit mode
            var descEdit = new TextBox
            {
                Text = entry.Description,
                PlaceholderText = "Enter description…",
                Style = (Style)Resources["EditDescBoxStyle"],
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            // Action buttons (row beneath description)
            var editBtn = new Button
            {
                Content = "Edit",
                Style = (Style)Resources["SubtleButtonStyle"],
                Margin = new Thickness(0, 14, 8, 0),
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
            };

            var saveBtn = new Button
            {
                Content = "Save",
                Style = (Style)Resources["PrimaryButtonStyle"],
                Margin = new Thickness(0, 14, 8, 0),
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            var cancelBtn = new Button
            {
                Content = "Cancel",
                Style = (Style)Resources["SubtleButtonStyle"],
                Margin = new Thickness(0, 14, 0, 0),
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            var buttonRow = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            buttonRow.Children.Add(editBtn);
            buttonRow.Children.Add(saveBtn);
            buttonRow.Children.Add(cancelBtn);

            // Separator line above buttons
            var separator = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Height = 1,
                Margin = new Thickness(0, 14, 0, 0),
                Fill = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128))
            };

            var contentPanel = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };
            contentPanel.Children.Add(descView);
            contentPanel.Children.Add(descEdit);
            contentPanel.Children.Add(separator);
            contentPanel.Children.Add(buttonRow);

            expander.Content = contentPanel;
            border.Child = expander;

            // ── Wire up button events ─────────────────────────────────

            // EDIT → switch to edit mode
            editBtn.Click += (s, e) =>
            {
                headerView.Visibility = Visibility.Collapsed;
                headerEdit.Visibility = Visibility.Visible;
                headerEdit.Text = entry.Title;

                descView.Visibility = Visibility.Collapsed;
                descEdit.Visibility = Visibility.Visible;
                descEdit.Text = entry.Description;

                editBtn.Visibility = Visibility.Collapsed;
                saveBtn.Visibility = Visibility.Visible;
                cancelBtn.Visibility = Visibility.Visible;
            };

            // SAVE → persist changes, switch back to view mode
            saveBtn.Click += (s, e) =>
            {
                var newTitle = headerEdit.Text.Trim();
                var newDesc = descEdit.Text.Trim();

                // Basic validation — title must not be empty
                if (newTitle.Length == 0)
                {
                    ShowValidationError(expander, "Title cannot be empty.");
                    return;
                }

                // Commit to the in-memory model
                entry.Title = newTitle;
                entry.Description = newDesc;

                // TODO: UPDATE entries SET title=@title, description=@desc WHERE id=@id

                // Switch back to view mode
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
            };

            // CANCEL → discard changes (or remove if brand-new empty entry)
            cancelBtn.Click += (s, e) =>
            {
                bool isNewEmpty = entry.Title.Length == 0;

                if (isNewEmpty)
                {
                    // Remove from both the list model and the UI
                    _entries.Remove(entry);
                    TncList.Children.Remove(border);
                    RefreshEntryCount();
                    return;
                }

                // Restore original values and go back to view mode
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
            };

            return border;
        }

        // ─────────────────────────────────────────────────────────────
        // HELPERS
        // ─────────────────────────────────────────────────────────────

        private void RefreshEntryCount()
        {
            int n = _entries.Count;
            TxtEntryCount.Text = $"{n} {(n == 1 ? "entry" : "entries")}";
        }

        /// <summary>Shows a transient red InfoBar inside the expander's content area.</summary>
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
                Margin = new Thickness(0, 8, 0, 0)
            };

            panel.Children.Insert(0, bar);

            // Auto-dismiss after 4 seconds
            await System.Threading.Tasks.Task.Delay(4000);
            bar.IsOpen = false;
            panel.Children.Remove(bar);
        }
    }
}