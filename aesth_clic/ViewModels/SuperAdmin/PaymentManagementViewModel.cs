using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI;

namespace aesth_clic.ViewModels.SuperAdmin
{
    public static class DisplayPaymentStatus
    {
        public const string Paid = "Paid";
        public const string DueSoon = "Due Soon";
        public const string Overdue = "Overdue";
    }

    public class SubscriptionDbRow
    {
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string ClinicName { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;

        public decimal MonthlyAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class SubscriptionItem : INotifyPropertyChanged
    {
        // ── Identity ──────────────────────────────────────────────────────────
        public int SubscriptionId { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
        public string Tier { get; set; } = string.Empty;
        public decimal MonthlyAmount { get; set; }

        // ── Dates ─────────────────────────────────────────────────────────────
        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StartDateDisplay));
            }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EndDateDisplay));
                OnPropertyChanged(nameof(DisplayStatus));
                OnPropertyChanged(nameof(StatusForeground));
                OnPropertyChanged(nameof(StatusIcon));
                OnPropertyChanged(nameof(NextDueForeground));
            }
        }

        // ── Formatted display strings ─────────────────────────────────────────
        public string MonthlyAmountDisplay => MonthlyAmount.ToString("₱#,##0.00");
        public string EndDateDisplay => EndDate.ToString("MMM d, yyyy");
        public string StartDateDisplay => StartDate == default ? "—" : StartDate.ToString("MMM d, yyyy");

        // ── Tier display helpers ──────────────────────────────────────────────
        public string TierDisplay => Tier.ToLower() switch
        {
            "basic" => "Basic",
            "standard" => "Standard",
            "premium" => "Premium",
            _ => Tier,
        };

        public SolidColorBrush TierForeground => Tier.ToLower() switch
        {
            "basic" => new(Color.FromArgb(255, 0, 120, 212)),
            "standard" => new(Color.FromArgb(255, 135, 100, 184)),
            "premium" => new(Color.FromArgb(255, 192, 120, 0)),
            _ => new(Color.FromArgb(255, 100, 100, 100)),
        };

        // ── Status derived purely from EndDate ────────────────────────────────
        /// <summary>
        /// EndDate > 7 days away  → Paid
        /// EndDate 0–7 days away  → Due Soon  (includes today)
        /// EndDate in the past    → Overdue
        /// </summary>
        public string DisplayStatus
        {
            get
            {
                var daysUntil = (EndDate.Date - DateTime.Today).TotalDays;
                return daysUntil > 7 ? DisplayPaymentStatus.Paid :
                       daysUntil >= 0 ? DisplayPaymentStatus.DueSoon :
                                        DisplayPaymentStatus.Overdue;
            }
        }

        public string StatusIcon => DisplayStatus switch
        {
            DisplayPaymentStatus.Paid => "\uE73E",  // checkmark
            DisplayPaymentStatus.Overdue => "\uE814",  // error
            DisplayPaymentStatus.DueSoon => "\uEA8F",  // clock warning
            _ => "\uE9CE",
        };

        public SolidColorBrush StatusForeground => DisplayStatus switch
        {
            DisplayPaymentStatus.Paid => new(Color.FromArgb(255, 14, 164, 122)),
            DisplayPaymentStatus.Overdue => new(Color.FromArgb(255, 216, 59, 1)),
            DisplayPaymentStatus.DueSoon => new(Color.FromArgb(255, 192, 120, 0)),
            _ => new(Color.FromArgb(255, 120, 120, 120)),
        };

        public SolidColorBrush NextDueForeground => DisplayStatus switch
        {
            DisplayPaymentStatus.Overdue => new(Color.FromArgb(255, 216, 59, 1)),
            DisplayPaymentStatus.DueSoon => new(Color.FromArgb(255, 192, 120, 0)),
            _ => new(Color.FromArgb(255, 60, 60, 60)),
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    internal class PaymentManagementViewModel : INotifyPropertyChanged
    {
        private readonly List<SubscriptionItem> _allSubscriptions = new();
        public ObservableCollection<SubscriptionItem> DisplayedSubscriptions { get; } = new();

        // ── Loading state ─────────────────────────────────────────────────────
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ── Search / filter ───────────────────────────────────────────────────
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedStatus = "All";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _selectedTier = "All";
        public string SelectedTier
        {
            get => _selectedTier;
            set { _selectedTier = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ── KPI raw values ────────────────────────────────────────────────────
        public decimal MRR => _allSubscriptions.Sum(s => s.MonthlyAmount);

        // "Collected this month" = subscriptions currently in Paid status
        public decimal CollectedThisMonth => _allSubscriptions
            .Where(s => s.DisplayStatus == DisplayPaymentStatus.Paid)
            .Sum(s => s.MonthlyAmount);

        public decimal OverdueAmount => _allSubscriptions
            .Where(s => s.DisplayStatus == DisplayPaymentStatus.Overdue)
            .Sum(s => s.MonthlyAmount);

        public int OverdueCount => _allSubscriptions
            .Count(s => s.DisplayStatus == DisplayPaymentStatus.Overdue);

        public int ActiveSubscriberCount => _allSubscriptions.Count;

        // ── KPI formatted strings ─────────────────────────────────────────────
        public string MrrDisplay => MRR.ToString("₱#,##0.00");
        public string CollectedThisMonthDisplay => CollectedThisMonth.ToString("₱#,##0.00");
        public string OverdueAmountDisplay => OverdueAmount.ToString("₱#,##0.00");

        public string CollectionRateDisplay
        {
            get
            {
                if (MRR == 0) return "0% collection rate";
                var rate = Math.Round((CollectedThisMonth / MRR) * 100);
                return $"{rate}% collection rate";
            }
        }

        public string OverdueCountDisplay =>
            OverdueCount == 1 ? "1 overdue invoice" : $"{OverdueCount} overdue invoices";

        public string SubscriberCountDisplay =>
            ActiveSubscriberCount == 1
                ? "1 active subscriber"
                : $"{ActiveSubscriberCount} active subscribers";

        public string RowCountDisplay =>
            $"{DisplayedSubscriptions.Count} invoice{(DisplayedSubscriptions.Count != 1 ? "s" : "")}";

        // ── Load from DB ──────────────────────────────────────────────────────
        public void LoadFromDb(List<SubscriptionDbRow> rows)
        {
            _allSubscriptions.Clear();

            foreach (var row in rows)
            {
                _allSubscriptions.Add(new SubscriptionItem
                {
                    SubscriptionId = row.SubscriptionId,
                    UserId = row.UserId,
                    FullName = row.FullName,
                    Email = row.Email,
                    ClinicName = row.ClinicName,
                    Tier = row.Tier,
                    MonthlyAmount = row.MonthlyAmount,
                    StartDate = row.StartDate,
                    EndDate = row.EndDate,
                });
            }

            ApplyFilters();
            RefreshKpis();
        }

        // ── Filters ───────────────────────────────────────────────────────────
        public void ApplyFilters()
        {
            var filtered = _allSubscriptions.Where(s =>
                (string.IsNullOrEmpty(SearchText)
                    || s.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || s.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                    || s.ClinicName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                && (SelectedStatus == "All" || s.DisplayStatus == SelectedStatus)
                && (SelectedTier == "All" || string.Equals(s.Tier, SelectedTier, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            DisplayedSubscriptions.Clear();
            foreach (var item in filtered)
                DisplayedSubscriptions.Add(item);

            RefreshKpis();
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private void RefreshKpis()
        {
            OnPropertyChanged(nameof(MRR));
            OnPropertyChanged(nameof(MrrDisplay));
            OnPropertyChanged(nameof(CollectedThisMonth));
            OnPropertyChanged(nameof(CollectedThisMonthDisplay));
            OnPropertyChanged(nameof(OverdueAmount));
            OnPropertyChanged(nameof(OverdueAmountDisplay));
            OnPropertyChanged(nameof(OverdueCount));
            OnPropertyChanged(nameof(OverdueCountDisplay));
            OnPropertyChanged(nameof(ActiveSubscriberCount));
            OnPropertyChanged(nameof(SubscriberCountDisplay));
            OnPropertyChanged(nameof(CollectionRateDisplay));
            OnPropertyChanged(nameof(RowCountDisplay));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}