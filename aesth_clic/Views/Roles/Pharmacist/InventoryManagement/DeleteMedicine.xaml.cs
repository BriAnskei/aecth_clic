using aesth_clic.Tenant.Controller;
using aesth_clic.Views.Roles.Pharmacist.Pages;
using Microsoft.UI.Xaml.Controls;
using System;

namespace aesth_clic.Views.Roles.Pharmacist.Modals
{
    public sealed partial class DeleteMedicine : ContentDialog
    {
        private readonly MedicineController _controller;
        private readonly InventoryItem _item;

        /// <summary>True when the user confirmed and the delete succeeded.</summary>
        public bool Confirmed { get; private set; }

        /// <summary>Set when the controller throws. Null on success.</summary>
        public Exception? SaveError { get; private set; }

        public DeleteMedicine(InventoryItem item, MedicineController controller)
        {
            InitializeComponent();
            _item = item;
            _controller = controller;

            TxtMedicineName.Text = $"Are you sure you want to delete \"{item.MedicineName}\"?";
        }

        private async void OnDeleteClicked(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                ErrorBar.IsOpen = false;

                if (!int.TryParse(_item.MedicineId, out var id))
                    throw new InvalidOperationException("Invalid medicine ID.");

                await _controller.DeleteMedicineAsync(id);
                Confirmed = true;
            }
            catch (Exception ex)
            {
                SaveError = ex;
                ErrorBar.Message = ex.Message;
                ErrorBar.IsOpen = true;
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
            }
        }
    }
}