using aesth_clic.Views.Roles.Admin.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

namespace aesth_clic.Views.Roles.Admin.Modals
{
    public sealed partial class DeactivateUser : ContentDialog
    {
        private readonly StaffUserItem _user;
        public bool Confirmed { get; private set; }

        private TextBlock ClientNameText;
        private Border SavingOverlay;

        public DeactivateUser(StaffUserItem user)
        {
            _user = user ?? throw new ArgumentNullException(nameof(user));
            Title = "Deactivate User";
            PrimaryButtonText = "Yes, Deactivate";
            CloseButtonText = "Cancel";
            DefaultButton = ContentDialogButton.Close;

            PrimaryButtonClick += OnConfirmClicked;

            BuildContent();
            ClientNameText.Text = _user.FullName;
        }

        private void BuildContent()
        {
            ClientNameText = new TextBlock { Text = "User Name", FontSize = 16, FontWeight = Microsoft.UI.Text.FontWeights.SemiBold, HorizontalAlignment = HorizontalAlignment.Center, TextAlignment = TextAlignment.Center, Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black) };

            var icon = new Border { Width = 72, Height = 72, CornerRadius = new CornerRadius(36), Background = new SolidColorBrush(Color.FromArgb(0x1A, 0xD8, 0x3B, 0x01)), HorizontalAlignment = HorizontalAlignment.Center };
            icon.Child = new FontIcon { Glyph = "\uEA8C", FontSize = 32, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0x3B, 0x01)), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

            var headerStack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Spacing = 12 };
            headerStack.Children.Add(icon);
            headerStack.Children.Add(ClientNameText);

            var divider = new Border { Height = 1, Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xEA, 0xF8)) };

            var msgStack = new StackPanel { Spacing = 8 };
            msgStack.Children.Add(new TextBlock { Text = "Are you sure you want to deactivate this user account?", FontSize = 13, TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(Microsoft.UI.Colors.Black) });
            msgStack.Children.Add(new TextBlock { Text = "The user will no longer be able to log in to the system. You can reactivate this account at any time from the user management page.", FontSize = 12, TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x9B, 0x80, 0xC4)) });

            var infoBorder = new Border { Background = new SolidColorBrush(Color.FromArgb(0x1A, 0xD8, 0x3B, 0x01)), BorderBrush = new SolidColorBrush(Color.FromArgb(0x40, 0xD8, 0x3B, 0x01)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6), Padding = new Thickness(12, 10, 12, 10) };
            var infoInner = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
            infoInner.Children.Add(new FontIcon { Glyph = "\uE783", FontSize = 14, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0x3B, 0x01)), VerticalAlignment = VerticalAlignment.Top });
            infoInner.Children.Add(new TextBlock { Text = "This action is reversible. The user's data and settings will be preserved.", FontSize = 12, TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xD8, 0x3B, 0x01)) });
            infoBorder.Child = infoInner;

            var mainStack = new StackPanel { Spacing = 16, Width = 420 };
            mainStack.Children.Add(headerStack);
            mainStack.Children.Add(divider);
            mainStack.Children.Add(msgStack);
            mainStack.Children.Add(infoBorder);

            SavingOverlay = new Border { Visibility = Visibility.Collapsed, Background = new SolidColorBrush(Microsoft.UI.Colors.White), Opacity = 0.9, CornerRadius = new CornerRadius(8) };
            var overlayStack = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            overlayStack.Children.Add(new ProgressRing { IsActive = true, Width = 20, Height = 20 });
            overlayStack.Children.Add(new TextBlock { Text = "Deactivating…", VerticalAlignment = VerticalAlignment.Center, FontSize = 13, Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0x9B, 0x80, 0xC4)) });
            SavingOverlay.Child = overlayStack;

            var grid = new Grid();
            grid.Children.Add(mainStack);
            grid.Children.Add(SavingOverlay);

            Content = grid;
        }

        private void OnConfirmClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Confirmed = true;
            SetSavingState(true);
        }

        private void SetSavingState(bool isSaving)
        {
            IsPrimaryButtonEnabled = !isSaving;
            IsSecondaryButtonEnabled = !isSaving;
            SavingOverlay.Visibility = isSaving ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
