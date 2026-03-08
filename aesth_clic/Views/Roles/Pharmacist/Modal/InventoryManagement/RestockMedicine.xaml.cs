using aesth_clic.Tenant.Controller;
using aesth_clic.Views.Roles.Pharmacist.Pages;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Pharmacist.Modals
{
    public sealed partial class RestockMedicine : ContentDialog
    {
        private readonly MedicineController _controller;
        private readonly InventoryItem _item;

        /// <summary>True when restock succeeded.</summary>
        public bool Confirmed { get; private set; }

        /// <summary>Set when the controller throws. Null on success.</summary>
        public Exception? SaveError { get; private set; }

        public RestockMedicine(InventoryItem item, MedicineController controller)
        {
            InitializeComponent();
            _item = item;
            _controller = controller;

            TxtMedicineName.Text = item.MedicineName;
            TxtCurrentStock.Text = $"Current stock: {item.StockQuantity} {item.Unit}";
        }

        private async void OnRestockClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                ValidationBar.IsOpen = false;

                var amount = (int)(FieldAmount.Value is double v && !double.IsNaN(v) ? v : 0);

                if (amount <= 0)
                {
                    ValidationBar.Message = "Amount to add must be greater than zero.";
                    ValidationBar.IsOpen = true;
                    args.Cancel = true;
                    return;
                }

                SavingOverlay.Visibility = Visibility.Visible;

                if (!int.TryParse(_item.MedicineId, out var id))
                    throw new InvalidOperationException("Invalid medicine ID.");

                await _controller.RestockMedicineAsync(id, amount);
                Confirmed = true;
            }
            catch (Exception ex)
            {
                SaveError = ex;
                SavingOverlay.Visibility = Visibility.Collapsed;
                ValidationBar.Message = ex.Message;
                ValidationBar.IsOpen = true;
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}