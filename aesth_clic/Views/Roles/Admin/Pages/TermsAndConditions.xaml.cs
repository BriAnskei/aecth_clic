using aesth_clic.Tenant.Controller;
using aesth_clic.Tenant.Model;
using aesth_clic.Utils;
using aesth_clic.ViewModels.Admin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;
using Windows.UI;

namespace aesth_clic.Views.Roles.Admin.Pages
{
    public sealed partial class TermsAndConditions : Page
    {
        private readonly TncTenantViewModel _vm = new();
        private readonly TncTenantController _tncController;

        public TermsAndConditions()
        {
            InitializeComponent();

            _tncController = App.Services.GetRequiredService<TncTenantController>();

            _ = LoadFromDbAsync();
        }

        // ──────────────────────────────────────────────────────────────────────
        // LOADING STATE
        // ──────────────────────────────────────────────────────────────────────

        private void UpdateLoadingState(bool isLoading)
        {
            _vm.IsLoading = isLoading;

            // Clinic toolbar
            ClinicCardToolbar.IsHitTestVisible = !isLoading;
            ClinicCardToolbar.Opacity = isLoading ? 0.4 : 1.0;

            // Master section skeletons
            MasterSkeletonList.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            MasterTncList.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;

            // Clinic section skeletons
            ClinicSkeletonList.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            ClinicTncList.Visibility = isLoading ? Visibility.Collapsed : Visibility.Visible;
        }

        // ──────────────────────────────────────────────────────────────────────
        // DATA LOADING
        // ──────────────────────────────────────────────────────────────────────

        private async Task LoadFromDbAsync()
        {
            UpdateLoadingState(true);

            try
            {
                // Fetch both in parallel
                var masterTask = _tncController.FetchMasterTncsAsync();
                var tenantTask = _tncController.GetAllTncsAsync();

                await Task.WhenAll(masterTask, tenantTask);

                // ── Master (read-only) ──
                _vm.LoadMasterFromDb(masterTask.Result);

                MasterTncList.Children.Clear();
                bool isFirst = true;
                foreach (var entry in _vm.MasterEntries)
                {
                    if (!isFirst)
                    {
                        MasterTncList.Children.Add(new Border
                        {
                            Height = 1,
                            Background = new SolidColorBrush(Color.FromArgb(255, 240, 234, 248)),
                            Margin = new Thickness(16, 0, 16, 0)
                        });
                    }
                    MasterTncList.Children.Add(BuildMasterEntryRow(entry));
                    isFirst = false;
                }

                UpdateMasterEntryCount();

                // ── Tenant (editable) ──
                _vm.LoadTenantFromDb(tenantTask.Result);

                ClinicTncList.Children.Clear();
                foreach (var entry in _vm.TenantEntries)
                    ClinicTncList.Children.Add(BuildEntryCard(entry));

                UpdateTenantEntryCount();
            }
            catch (Exception ex)
            {
                ToastHelper.Error(ToastBar, "Failed to load terms", ex.Message);
            }
            finally
            {
                UpdateLoadingState(false);
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // KPI / COUNT LABELS
        // ──────────────────────────────────────────────────────────────────────

        private void UpdateMasterEntryCount() =>
            TxtMasterEntryCount.Text = _vm.MasterEntryCountDisplay;

        private void UpdateTenantEntryCount() =>
            TxtEntryCount.Text = _vm.TenantEntryCountDisplay;

        // ──────────────────────────────────────────────────────────────────────
        // ADD NEW PROVISION
        // ──────────────────────────────────────────────────────────────────────

        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            var newEntry = new TncTenant { Id = 0, Title = "", Description = "" };

            var card = BuildEntryCard(newEntry, startInEditMode: true);
            ClinicTncList.Children.Add(card);

            // Track in VM so count is accurate while editing
            _vm.AddTenantEntry(newEntry);
            UpdateTenantEntryCount();

            if (card is Border border && border.Child is Expander expander)
                expander.IsExpanded = true;
        }

        // ──────────────────────────────────────────────────────────────────────
        // BUILD MASTER ENTRY ROW (read-only)
        // ──────────────────────────────────────────────────────────────────────

        private static Border BuildMasterEntryRow(TncTenant entry)
        {
            var titleBlock = new TextBlock
            {
                Text = entry.Title,
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 45, 21, 84))
            };

            var descBlock = new TextBlock
            {
                Text = entry.Description,
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 122, 106, 154)),
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20,
                Margin = new Thickness(0, 5, 0, 0)
            };

            var content = new StackPanel { Spacing = 0 };
            content.Children.Add(titleBlock);
            content.Children.Add(descBlock);

            return new Border
            {
                Padding = new CornerRadius(8).Equals(default) ? new Thickness(16, 18, 16, 18) : new Thickness(16, 18, 16, 18),
                CornerRadius = new CornerRadius(8),
                Child = content
            };
        }

        // ──────────────────────────────────────────────────────────────────────
        // BUILD CLINIC ENTRY CARD (editable)
        // ──────────────────────────────────────────────────────────────────────

        private Border BuildEntryCard(TncTenant entry, bool startInEditMode = false)
        {
            // ── Root border ───────────────────────────────────────────
            var border = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(255, 253, 251, 255)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(255, 228, 218, 245)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Tag = entry.Id
            };

            // ── Expander ──────────────────────────────────────────────
            var expander = new Expander
            {
                IsExpanded = startInEditMode,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(16, 12, 16, 12)
            };

            // ── Header: view label overlaid with edit TextBox (Admin pattern) ──
            var headerView = new TextBlock
            {
                Text = entry.Title.Length > 0 ? entry.Title : "(New provision — add a title)",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromArgb(255, 45, 21, 84)),
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = entry.Title.Length > 0 ? 1.0 : 0.5,
                Visibility = startInEditMode ? Visibility.Collapsed : Visibility.Visible
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
                Visibility = startInEditMode ? Visibility.Visible : Visibility.Collapsed
            };

            var headerPanel = new Grid();
            headerPanel.Children.Add(headerView);
            headerPanel.Children.Add(headerEdit);

            expander.Header = headerPanel;

            // ── Description ───────────────────────────────────────────
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

            // ── Buttons ───────────────────────────────────────────────
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
                Fill = new SolidColorBrush(Color.FromArgb(40, 180, 150, 220))
            };

            var contentPanel = new StackPanel { Margin = new Thickness(0, 4, 0, 4) };
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

            // ──────────────────────────────────────────────────────────
            // SAVE  (Create if Id == 0, otherwise Update)
            // ──────────────────────────────────────────────────────────
            saveBtn.Click += async (s, e) =>
            {
                var newTitle = headerEdit.Text.Trim();
                var newDesc = descEdit.Text.Trim();

                if (newTitle.Length == 0)
                {
                    ShowValidationError(expander, "Provision title cannot be empty.");
                    return;
                }

                SetCardBusy(saveBtn, cancelBtn, editBtn, deleteBtn, true);

                try
                {
                    if (entry.Id == 0)
                    {
                        // ── CREATE ──
                        _vm.RemoveTenantEntry(entry);

                        entry.Title = newTitle;
                        entry.Description = newDesc;

                        var created = await _tncController.CreateTncAsync(entry);

                        entry.Id = created.Id;
                        border.Tag = entry.Id;

                        _vm.AddTenantEntry(entry);

                        ToastHelper.Success(ToastBar, "Provision added",
                            $"\"{entry.Title}\" has been created.");
                    }
                    else
                    {
                        // ── UPDATE ──
                        entry.Title = newTitle;
                        entry.Description = newDesc;

                        await _tncController.UpdateTncAsync(entry);

                        ToastHelper.Success(ToastBar, "Provision updated",
                            $"\"{entry.Title}\" has been saved.");
                    }

                    // Switch to view mode
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

                    UpdateTenantEntryCount();
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to save provision", ex.Message);
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
                    _vm.RemoveTenantEntry(entry);
                    ClinicTncList.Children.Remove(border);
                    UpdateTenantEntryCount();
                    return;
                }

                // Restore and return to view mode
                headerView.Text = entry.Title;
                headerView.Opacity = 1.0;
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

            // ──────────────────────────────────────────────────────────
            // DELETE
            // ──────────────────────────────────────────────────────────
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
                if (result != ContentDialogResult.Primary) return;

                SetCardBusy(saveBtn, cancelBtn, editBtn, deleteBtn, true);

                try
                {
                    await _tncController.DeleteTncAsync(entry.Id);

                    _vm.RemoveTenantEntry(entry);
                    ClinicTncList.Children.Remove(border);
                    UpdateTenantEntryCount();

                    ToastHelper.Success(ToastBar, "Provision deleted",
                        $"\"{entry.Title}\" has been removed.");
                }
                catch (Exception ex)
                {
                    ToastHelper.Error(ToastBar, "Failed to delete provision", ex.Message);
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
                Margin = new Thickness(0, 0, 0, 8)
            };

            panel.Children.Insert(0, bar);

            await Task.Delay(4000);
            bar.IsOpen = false;
            panel.Children.Remove(bar);
        }
    }
}