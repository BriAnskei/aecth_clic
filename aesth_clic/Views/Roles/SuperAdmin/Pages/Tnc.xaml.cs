using aesth_clic.Master.Controller;
using aesth_clic.Master.Model;
using aesth_clic.Utils;
using aesth_clic.ViewModels.SuperAdmin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.UI;

namespace aesth_clic.Views.Roles.SuperAdmin.Pages
{
    public sealed partial class TNC : Page
    {
        private readonly TncViewModel _vm = new();
        private readonly TncMasterController _tncController;

        public TNC()
        {
            InitializeComponent();

            _tncController = App.Services.GetRequiredService<TncMasterController>();

            _ = LoadFromDbAsync();
        }

        // ──────────────────────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────────────────────
        private void UpdateLoadingState(bool isLoading)
        {
            _vm.IsLoading = isLoading;
            Toolbar.IsHitTestVisible = !isLoading;
            Toolbar.Opacity = isLoading ? 0.4 : 1.0;
            SkeletonList.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            TncList.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
        }

        // ──────────────────────────────────────────────────────────────────────
        // DATA LOADING
        // ──────────────────────────────────────────────────────────────────────
        private async Task LoadFromDbAsync()
        {
            _vm.IsLoading = true;
            UpdateLoadingState(true);

            try
            {
                var rows = await _tncController.GetAllTncsAsync();
                _vm.LoadFromDb(rows);

                TncList.Children.Clear();
                foreach (var entry in _vm.Entries)
                    TncList.Children.Add(BuildEntryCard(entry));

                UpdateEntryCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load terms", ex.Message);
            }
            finally
            {
                _vm.IsLoading = false;
                UpdateLoadingState(false);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // KPI / COUNT LABEL
        // ──────────────────────────────────────────────────────────────────────
        private void UpdateEntryCount()
        {
            TxtEntryCount.Text = _vm.EntryCountDisplay;
        }

        // ──────────────────────────────────────────────────────────────────────
        // ADD NEW TNC
        // ──────────────────────────────────────────────────────────────────────
        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            var newEntry = new TncMaster { Id = 0, Title = "", Description = "" };

            var card = BuildEntryCard(newEntry, startInEditMode: true);
            TncList.Children.Add(card);

            // Temporarily track it in the VM so EntryCountDisplay is accurate
            _vm.AddEntry(newEntry);
            UpdateEntryCount();

            if (card is Border border && border.Child is Expander expander)
                expander.IsExpanded = true;
        }

        // ──────────────────────────────────────────────────────────────────────
        // BUILD ENTRY CARD
        // ──────────────────────────────────────────────────────────────────────
        private Border BuildEntryCard(TncMaster entry, bool startInEditMode = false)
        {
            // ── Root border ──────────────────────────────────────────
            var border = new Border
            {
                Style = (Style)Resources["TncCardStyle"],
                Padding = new Thickness(0),
                CornerRadius = new CornerRadius(8),
                Tag = entry.Id
            };

            // ── Expander ─────────────────────────────────────────────
            var expander = new Expander
            {
                IsExpanded = startInEditMode,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16, 12, 16, 12)
            };

            // ── Header (view-only TextBlock — never editable) ─────────
            var headerView = new TextBlock
            {
                Text = entry.Title.Length > 0 ? entry.Title : "(New entry — add a title)",
                Style = (Style)Resources["EntryTitleStyle"],
                Opacity = entry.Title.Length > 0 ? 1.0 : 0.5
            };

            expander.Header = headerView;

            // ── Title edit box (lives in content, not header) ─────────
            // This is the FIX: TextBox inside Expander.Header cannot receive
            // focus because the Expander intercepts pointer events to toggle
            // expand/collapse. Moving it into the content panel resolves this.
            var headerEdit = new TextBox
            {
                Text = entry.Title,
                PlaceholderText = "Enter title…",
                Style = (Style)Resources["EditTitleBoxStyle"],
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed,
                Margin = new Thickness(0, 0, 0, 8)
            };

            // ── Description ───────────────────────────────────────────
            var descView = new TextBlock
            {
                Text = entry.Description,
                Style = (Style)Resources["DescriptionStyle"],
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
            };

            var descEdit = new TextBox
            {
                Text = entry.Description,
                PlaceholderText = "Enter description…",
                Style = (Style)Resources["EditDescBoxStyle"],
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            // ── Buttons ───────────────────────────────────────────────
            var editBtn = new Button
            {
                Content = "Edit",
                Style = (Style)Resources["SubtleButtonStyle"],
                Margin = new Thickness(0, 14, 8, 0),
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
            };

            var deleteBtn = new Button
            {
                Content = "Delete",
                Style = (Style)Resources["DangerButtonStyle"],
                Margin = new Thickness(0, 14, 0, 0),
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

            var buttonRow = new StackPanel { Orientation = Orientation.Horizontal };
            buttonRow.Children.Add(editBtn);
            buttonRow.Children.Add(saveBtn);
            buttonRow.Children.Add(cancelBtn);
            buttonRow.Children.Add(deleteBtn);

            var separator = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Height = 1,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 14, 0, 0),
                Fill = new SolidColorBrush(Color.FromArgb(30, 128, 128, 128))
            };

            // headerEdit is first so the title field appears above description
            var contentPanel = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };
            contentPanel.Children.Add(headerEdit);   // ← title edit box here
            contentPanel.Children.Add(descView);
            contentPanel.Children.Add(descEdit);
            contentPanel.Children.Add(separator);
            contentPanel.Children.Add(buttonRow);

            expander.Content = contentPanel;
            border.Child = expander;

            // ──────────────────────────────────────────────────────────
            // EDIT
            // ──────────────────────────────────────────────────────────
            editBtn.Click += (s, e) =>
            {
                // Show title edit box in content area
                headerEdit.Text = entry.Title;
                headerEdit.Visibility = Visibility.Visible;

                // Dim the expander header label while editing
                headerView.Opacity = 0.4;

                descView.Visibility = Visibility.Collapsed;
                descEdit.Visibility = Visibility.Visible;
                descEdit.Text = entry.Description;

                editBtn.Visibility = Visibility.Collapsed;
                deleteBtn.Visibility = Visibility.Collapsed;
                saveBtn.Visibility = Visibility.Visible;
                cancelBtn.Visibility = Visibility.Visible;
            };

            // ──────────────────────────────────────────────────────────
            // SAVE  (Create if Id == 0, otherwise Update)
            // ──────────────────────────────────────────────────────────
            saveBtn.Click += async (s, e) =>
            {
                var newTitle = headerEdit.Text.Trim();
                var newDesc = descEdit.Text.Trim();

                if (newTitle.Length == 0)
                {
                    ShowValidationError(expander, "Title cannot be empty.");
                    return;
                }

                SetCardBusy(saveBtn, cancelBtn, editBtn, deleteBtn, true);

                try
                {
                    if (entry.Id == 0)
                    {
                        // ── CREATE ──
                        _vm.RemoveEntry(entry);

                        entry.Title = newTitle;
                        entry.Description = newDesc;

                        var created = await _tncController.CreateTncAsync(entry);

                        entry.Id = created.Id;
                        border.Tag = entry.Id;

                        _vm.AddEntry(entry);

                        ToastHelper.Success(ToastBar, "Term added",
                            $"\"{entry.Title}\" has been created.");
                    }
                    else
                    {
                        // ── UPDATE ──
                        entry.Title = newTitle;
                        entry.Description = newDesc;

                        await _tncController.UpdateTncAsync(entry);

                        ToastHelper.Success(ToastBar, "Term updated",
                            $"\"{entry.Title}\" has been saved.");
                    }

                    // Switch to view mode
                    headerView.Text = entry.Title;
                    headerView.Opacity = 1.0;

                    headerEdit.Visibility = Visibility.Collapsed;

                    descView.Text = entry.Description;
                    descView.Visibility = Visibility.Visible;
                    descEdit.Visibility = Visibility.Collapsed;

                    saveBtn.Visibility = Visibility.Collapsed;
                    cancelBtn.Visibility = Visibility.Collapsed;
                    editBtn.Visibility = Visibility.Visible;
                    deleteBtn.Visibility = Visibility.Visible;

                    UpdateEntryCount();
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to save term", ex.Message);
                }
                finally
                {
                    SetCardBusy(saveBtn, cancelBtn, editBtn, deleteBtn, false);
                }
            };

            // ──────────────────────────────────────────────────────────
            // CANCEL
            // ──────────────────────────────────────────────────────────
            cancelBtn.Click += (s, e) =>
            {
                // Never saved to DB — remove from VM and UI
                if (entry.Id == 0)
                {
                    _vm.RemoveEntry(entry);
                    TncList.Children.Remove(border);
                    UpdateEntryCount();
                    return;
                }

                // Restore and return to view mode
                headerEdit.Visibility = Visibility.Collapsed;
                headerView.Text = entry.Title;
                headerView.Opacity = 1.0;

                descEdit.Text = entry.Description;
                descView.Text = entry.Description;
                descView.Visibility = Visibility.Visible;
                descEdit.Visibility = Visibility.Collapsed;

                saveBtn.Visibility = Visibility.Collapsed;
                cancelBtn.Visibility = Visibility.Collapsed;
                editBtn.Visibility = Visibility.Visible;
                deleteBtn.Visibility = Visibility.Visible;
            };

            // ──────────────────────────────────────────────────────────
            // DELETE
            // ──────────────────────────────────────────────────────────
            deleteBtn.Click += async (s, e) =>
            {
                var dialog = new ContentDialog
                {
                    Title = "Delete Term",
                    Content = $"Are you sure you want to delete \"{entry.Title}\"? This cannot be undone.",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary) return;

                SetCardBusy(saveBtn, cancelBtn, editBtn, deleteBtn, true);

                try
                {
                    await _tncController.DeleteTncAsync(entry.Id);

                    _vm.RemoveEntry(entry);
                    TncList.Children.Remove(border);
                    UpdateEntryCount();

                    ToastHelper.Success(ToastBar, "Term deleted",
                        $"\"{entry.Title}\" has been removed.");
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to delete term", ex.Message);
                }
                finally
                {
                    SetCardBusy(saveBtn, cancelBtn, editBtn, deleteBtn, false);
                }
            };

            return border;
        }

        // ──────────────────────────────────────────────────────────────────────
        // HELPERS
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Disables all action buttons while an async DB call is in flight.</summary>
        private static void SetCardBusy(
            Button save, Button cancel, Button edit, Button delete, bool busy)
        {
            save.IsEnabled = !busy;
            cancel.IsEnabled = !busy;
            edit.IsEnabled = !busy;
            delete.IsEnabled = !busy;
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

            await Task.Delay(4000);
            bar.IsOpen = false;
            panel.Children.Remove(bar);
        }
    }
}