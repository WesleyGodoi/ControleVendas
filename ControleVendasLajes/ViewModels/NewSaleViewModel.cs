using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleVendasLajes.Helpers;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;

namespace ControleVendasLajes.ViewModels;

public partial class NewSaleViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    private readonly List<Client> _allClients = new();
    private readonly List<Product> _allProducts = new();

    public ObservableCollection<Client> FilteredClients { get; } = new();
    public ObservableCollection<Product> FilteredProducts { get; } = new();
    public IReadOnlyList<SaleStatus> StatusList { get; } = Enum.GetValues<SaleStatus>();

    // Busca digitada de cliente
    [ObservableProperty]
    private string _clientSearchText = string.Empty;

    [ObservableProperty]
    private bool _showClientSuggestions = false;

    // Busca digitada de laje
    [ObservableProperty]
    private string _productSearchText = string.Empty;

    [ObservableProperty]
    private bool _showProductSuggestions = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedClient))]
    private Client? _selectedClient;

    public bool HasSelectedClient => SelectedClient is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedProduct))]
    private Product? _selectedProduct;

    public bool HasSelectedProduct => SelectedProduct is not null;

    [ObservableProperty]
    private double _areaSqm;

    [ObservableProperty]
    private string _areaSqmText = string.Empty;

    [ObservableProperty]
    private decimal _unitPriceSqm;

    [ObservableProperty]
    private string _unitPriceSqmText = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private DateTime _saleDate = DateTime.Now;

    [ObservableProperty]
    private SaleStatus _selectedStatus = SaleStatus.Orcamento;

    [ObservableProperty]
    private double _monthTotalSqm;

    [ObservableProperty]
    private decimal _monthTotalRevenue;

    public NewSaleViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    // Filtra clientes conforme digita
    partial void OnClientSearchTextChanged(string value)
    {
        FilterClients(value);
    }

    private void FilterClients(string query)
    {
        FilteredClients.Clear();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _allClients
            : _allClients.Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                     c.Phone.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var c in matches) FilteredClients.Add(c);

        // Abre a lista suspensa apenas se o usuário estiver digitando para buscar
        ShowClientSuggestions = SelectedClient is null && !string.IsNullOrWhiteSpace(query) && FilteredClients.Count > 0;
    }

    // Filtra lajes conforme digita
    partial void OnProductSearchTextChanged(string value)
    {
        FilterProducts(value);
    }

    private void FilterProducts(string query)
    {
        FilteredProducts.Clear();
        var matches = string.IsNullOrWhiteSpace(query)
            ? _allProducts
            : _allProducts.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var p in matches) FilteredProducts.Add(p);

        ShowProductSuggestions = SelectedProduct is null && !string.IsNullOrWhiteSpace(query) && FilteredProducts.Count > 0;
    }

    [RelayCommand]
    public void SelectClient(Client? client)
    {
        if (client is null) return;
        SelectedClient = client;
        ClientSearchText = client.Name;
        ShowClientSuggestions = false;
    }

    [RelayCommand]
    public void ClearClientSelection()
    {
        SelectedClient = null;
        ClientSearchText = string.Empty;
        FilterClients(string.Empty);
    }

    [RelayCommand]
    public void SelectProduct(Product? product)
    {
        if (product is null) return;
        SelectedProduct = product;
        ProductSearchText = product.Name;
        UnitPriceSqm = product.DefaultPricePerSqm;
        UnitPriceSqmText = MaskHelper.FormatCurrency(product.DefaultPricePerSqm);
        ShowProductSuggestions = false;
        RecalculateTotal();
    }

    [RelayCommand]
    public void ClearProductSelection()
    {
        SelectedProduct = null;
        ProductSearchText = string.Empty;
        UnitPriceSqm = 0;
        UnitPriceSqmText = string.Empty;
        FilterProducts(string.Empty);
        RecalculateTotal();
    }

    partial void OnAreaSqmTextChanged(string value)
    {
        var formatted = MaskHelper.FormatArea(value, out var parsed);
        AreaSqm = parsed;
        if (formatted != value)
        {
            AreaSqmText = formatted;
        }
        RecalculateTotal();
    }

    partial void OnUnitPriceSqmTextChanged(string value)
    {
        var formatted = MaskHelper.FormatCurrency(value, out var parsed);
        UnitPriceSqm = parsed;
        if (formatted != value)
        {
            UnitPriceSqmText = formatted;
        }
        RecalculateTotal();
    }

    partial void OnAreaSqmChanged(double value) => RecalculateTotal();
    partial void OnUnitPriceSqmChanged(decimal value) => RecalculateTotal();

    private void RecalculateTotal()
    {
        if (AreaSqm <= 0 || UnitPriceSqm <= 0)
        {
            TotalAmount = 0;
            return;
        }

        TotalAmount = (decimal)AreaSqm * UnitPriceSqm;
    }

    [RelayCommand]
    public async Task LoadDataAsync()
    {
        var clients = await _databaseService.GetClientsAsync();
        _allClients.Clear();
        _allClients.AddRange(clients);
        FilterClients(ClientSearchText);

        var products = await _databaseService.GetProductsAsync();
        _allProducts.Clear();
        _allProducts.AddRange(products);
        FilterProducts(ProductSearchText);

        await RefreshDashboardAsync();
    }

    private async Task RefreshDashboardAsync()
    {
        var summary = await _databaseService.GetCurrentMonthDashboardAsync();
        MonthTotalSqm = summary.TotalSqmSold;
        MonthTotalRevenue = summary.TotalRevenue;
    }

    [RelayCommand]
    private async Task SaveSaleAsync()
    {
        if (SelectedClient is null)
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Atenção", "Busque e selecione um cliente da lista.", "OK");
            return;
        }

        if (SelectedProduct is null)
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Atenção", "Busque e selecione o tipo de laje.", "OK");
            return;
        }

        if (AreaSqm <= 0)
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Atenção", "Informe uma metragem (m²) válida.", "OK");
            return;
        }

        var newSale = new Sale
        {
            ClientId = SelectedClient.Id,
            ClientName = SelectedClient.Name,
            ClientPhone = SelectedClient.Phone,
            DeliveryAddress = SelectedClient.DeliveryAddress,
            ProductId = SelectedProduct.Id,
            ProductName = SelectedProduct.Name,
            AreaSqm = AreaSqm,
            UnitPriceSqm = UnitPriceSqm,
            TotalAmount = TotalAmount,
            SaleDate = SaleDate,
            Status = SelectedStatus
        };

        await _databaseService.SaveSaleAsync(newSale);

        if (Shell.Current is not null)
            await Shell.Current.DisplayAlert("Sucesso", "Pedido registrado com sucesso!", "OK");

        // Reseta campos
        ClearClientSelection();
        ClearProductSelection();
        AreaSqm = 0;
        AreaSqmText = string.Empty;
        UnitPriceSqm = 0;
        UnitPriceSqmText = string.Empty;
        SelectedStatus = SaleStatus.Orcamento;
        await RefreshDashboardAsync();
    }
}
