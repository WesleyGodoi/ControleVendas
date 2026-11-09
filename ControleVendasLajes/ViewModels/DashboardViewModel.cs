using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;

namespace ControleVendasLajes.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private double _monthTotalSqm;

    [ObservableProperty]
    private decimal _monthTotalRevenue;

    [ObservableProperty]
    private int _pendingDeliveriesCount;

    [ObservableProperty]
    private int _pendingPaymentsCount;

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _isRefreshing;

    public ObservableCollection<Sale> RecentSales { get; } = new();

    public DashboardViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        try
        {
            if (RecentSales.Count == 0 && MonthTotalSqm == 0 && MonthTotalRevenue == 0)
            {
                IsLoading = true;
            }

            var summary = await _databaseService.GetCurrentMonthDashboardAsync();
            MonthTotalSqm = summary.TotalSqmSold;
            MonthTotalRevenue = summary.TotalRevenue;
            PendingDeliveriesCount = summary.PendingDeliveriesCount;
            PendingPaymentsCount = summary.PendingPaymentsCount;

            var sales = await _databaseService.GetSalesAsync();
            RecentSales.Clear();
            foreach (var sale in sales)
            {
                RecentSales.Add(sale);
            }
        }
        finally
        {
            IsLoading = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    public async Task RefreshDashboardAsync()
    {
        IsRefreshing = true;
        await LoadDashboardAsync();
    }

    [RelayCommand]
    public async Task OpenSaleDetailAsync(Sale? sale)
    {
        if (sale is null || Shell.Current is null) return;
        await Shell.Current.GoToAsync($"SaleDetailPage?id={sale.Id}");
    }
}
