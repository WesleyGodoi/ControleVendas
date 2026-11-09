using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;

namespace ControleVendasLajes.ViewModels;

public partial class SalesListViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    private readonly List<Sale> _allSales = new();

    public ObservableCollection<Sale> FilteredSales { get; } = new();

    [ObservableProperty]
    private Sale? _selectedSale;

    [ObservableProperty]
    private string _searchClientText = string.Empty;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Now.AddMonths(-3);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Now;

    [ObservableProperty]
    private bool _filterByDate = false;

    // Filtros rápidos de Entrega e Pagamento
    [ObservableProperty]
    private string _selectedDeliveryFilter = "Todos";

    [ObservableProperty]
    private int _selectedDeliveryIndex = 0;

    [ObservableProperty]
    private string _selectedPaymentFilter = "Todos";

    [ObservableProperty]
    private int _selectedPaymentIndex = 0;

    public IReadOnlyList<string> DeliveryFilterOptions { get; } = new[] 
    { 
        "Todos", 
        "Pendente", 
        "Em Transporte", 
        "Entregue na Obra" 
    };

    public IReadOnlyList<string> PaymentFilterOptions { get; } = new[] 
    { 
        "Todos", 
        "Pendente", 
        "Pago Parcial", 
        "Quitado" 
    };

    [ObservableProperty]
    private int _totalSalesCount;

    [ObservableProperty]
    private double _totalFilteredSqm;

    [ObservableProperty]
    private decimal _totalFilteredRevenue;

    public SalesListViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    partial void OnSearchClientTextChanged(string value) => ApplyFilters();
    partial void OnStartDateChanged(DateTime value) { if (FilterByDate) ApplyFilters(); }
    partial void OnEndDateChanged(DateTime value) { if (FilterByDate) ApplyFilters(); }
    partial void OnFilterByDateChanged(bool value) => ApplyFilters();
    
    partial void OnSelectedDeliveryFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedDeliveryIndexChanged(int value)
    {
        if (value >= 0 && value < DeliveryFilterOptions.Count)
        {
            SelectedDeliveryFilter = DeliveryFilterOptions[value];
            ApplyFilters();
        }
    }

    partial void OnSelectedPaymentFilterChanged(string value) => ApplyFilters();
    partial void OnSelectedPaymentIndexChanged(int value)
    {
        if (value >= 0 && value < PaymentFilterOptions.Count)
        {
            SelectedPaymentFilter = PaymentFilterOptions[value];
            ApplyFilters();
        }
    }

    [RelayCommand]
    public async Task LoadSalesAsync()
    {
        var sales = await _databaseService.GetSalesAsync();
        _allSales.Clear();
        _allSales.AddRange(sales);
        ApplyFilters();
    }

    [RelayCommand]
    public void ApplyFilters()
    {
        var query = _allSales.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchClientText))
        {
            query = query.Where(s => s.ClientName.Contains(SearchClientText, StringComparison.OrdinalIgnoreCase) ||
                                     s.ProductName.Contains(SearchClientText, StringComparison.OrdinalIgnoreCase));
        }

        if (FilterByDate)
        {
            var start = StartDate.Date;
            var end = EndDate.Date.AddDays(1).AddTicks(-1);
            query = query.Where(s => s.SaleDate >= start && s.SaleDate <= end);
        }

        // Filtro de Entrega
        if (SelectedDeliveryFilter != "Todos" && !string.IsNullOrWhiteSpace(SelectedDeliveryFilter))
        {
            if (SelectedDeliveryFilter == "Pendente" || SelectedDeliveryFilter == "Entrega Pendente")
                query = query.Where(s => s.DeliveryStatus == DeliveryStatus.Pendente);
            else if (SelectedDeliveryFilter == "Em Transporte" || SelectedDeliveryFilter == "EmRota" || SelectedDeliveryFilter == "Em Rota")
                query = query.Where(s => s.DeliveryStatus == DeliveryStatus.EmRota);
            else if (SelectedDeliveryFilter == "Entregue" || SelectedDeliveryFilter == "Entregue na Obra")
                query = query.Where(s => s.DeliveryStatus == DeliveryStatus.Entregue);
            else if (Enum.TryParse<DeliveryStatus>(SelectedDeliveryFilter, out var ds))
                query = query.Where(s => s.DeliveryStatus == ds);
        }

        // Filtro de Pagamento
        if (SelectedPaymentFilter != "Todos" && !string.IsNullOrWhiteSpace(SelectedPaymentFilter))
        {
            if (SelectedPaymentFilter == "Pendente" || SelectedPaymentFilter == "Pagamento Pendente")
                query = query.Where(s => s.PaymentStatus == PaymentStatus.Pendente);
            else if (SelectedPaymentFilter == "Pago Parcial" || SelectedPaymentFilter == "Parcial")
                query = query.Where(s => s.PaymentStatus == PaymentStatus.Parcial);
            else if (SelectedPaymentFilter == "Quitado" || SelectedPaymentFilter == "Pago")
                query = query.Where(s => s.PaymentStatus == PaymentStatus.Pago);
            else if (Enum.TryParse<PaymentStatus>(SelectedPaymentFilter, out var ps))
                query = query.Where(s => s.PaymentStatus == ps);
        }

        var result = query.ToList();

        FilteredSales.Clear();
        foreach (var sale in result)
        {
            FilteredSales.Add(sale);
        }

        TotalSalesCount = result.Count;
        TotalFilteredSqm = result.Sum(s => s.AreaSqm);
        TotalFilteredRevenue = result.Sum(s => s.TotalAmount);
    }

    [RelayCommand]
    public void ClearFilters()
    {
        SearchClientText = string.Empty;
        FilterByDate = false;
        SelectedDeliveryIndex = 0;
        SelectedPaymentIndex = 0;
        SelectedDeliveryFilter = "Todos";
        SelectedPaymentFilter = "Todos";
        StartDate = DateTime.Now.AddMonths(-3);
        EndDate = DateTime.Now;
        ApplyFilters();
    }

    [RelayCommand]
    public async Task OpenSaleDetailAsync(Sale? sale)
    {
        if (sale is null || Shell.Current is null) return;
        SelectedSale = null;
        await Shell.Current.GoToAsync($"SaleDetailPage?id={sale.Id}");
    }

    [RelayCommand]
    public async Task NavigateToNewSaleAsync()
    {
        if (Shell.Current is null) return;
        await Shell.Current.GoToAsync("NewSalePage");
    }
}
