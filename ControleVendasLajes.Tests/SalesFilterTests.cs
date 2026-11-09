using ControleVendasLajes.Models;
using ControleVendasLajes.Services;
using ControleVendasLajes.ViewModels;
using Xunit;

namespace ControleVendasLajes.Tests;

public class SalesFilterTests
{
    private List<Sale> CreateSampleSales()
    {
        return new List<Sale>
        {
            new()
            {
                Id = 1,
                ClientName = "Construtora Alfa",
                ProductName = "Laje Treliçada H8 EPS",
                AreaSqm = 100,
                TotalAmount = 5000,
                DeliveryStatus = DeliveryStatus.Pendente,
                PaymentStatus = PaymentStatus.Pendente,
                SaleDate = new DateTime(2026, 10, 1)
            },
            new()
            {
                Id = 2,
                ClientName = "Engenharia Beta",
                ProductName = "Laje Treliçada H12 EPS",
                AreaSqm = 150,
                TotalAmount = 8500,
                DeliveryStatus = DeliveryStatus.EmRota,
                PaymentStatus = PaymentStatus.Parcial,
                TotalPaid = 4000,
                SaleDate = new DateTime(2026, 10, 15)
            },
            new()
            {
                Id = 3,
                ClientName = "Wesley Godoi",
                ProductName = "Painel Treliçado Maciço",
                AreaSqm = 200,
                TotalAmount = 15000,
                DeliveryStatus = DeliveryStatus.Entregue,
                PaymentStatus = PaymentStatus.Pago,
                TotalPaid = 15000,
                SaleDate = new DateTime(2026, 11, 1)
            }
        };
    }

    [Fact]
    public void FilterByDeliveryStatus_Pendente_ReturnsOnlyPendente()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();
        
        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        vm.SelectedDeliveryFilter = "Pendente";
        vm.ApplyFilters();

        Assert.Single(vm.FilteredSales);
        Assert.Equal(DeliveryStatus.Pendente, vm.FilteredSales[0].DeliveryStatus);
        Assert.Equal("Construtora Alfa", vm.FilteredSales[0].ClientName);
    }

    [Fact]
    public void FilterByDeliveryStatus_EmTransporte_ReturnsOnlyEmRota()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();

        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        // Testa com o novo nome amigável da UI
        vm.SelectedDeliveryFilter = "Em Transporte";
        vm.ApplyFilters();

        Assert.Single(vm.FilteredSales);
        Assert.Equal(DeliveryStatus.EmRota, vm.FilteredSales[0].DeliveryStatus);
        Assert.Equal("Engenharia Beta", vm.FilteredSales[0].ClientName);
    }

    [Fact]
    public void FilterByDeliveryStatus_EntregueNaObra_ReturnsOnlyEntregue()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();

        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        // Testa com o novo nome amigável da UI
        vm.SelectedDeliveryFilter = "Entregue na Obra";
        vm.ApplyFilters();

        Assert.Single(vm.FilteredSales);
        Assert.Equal(DeliveryStatus.Entregue, vm.FilteredSales[0].DeliveryStatus);
        Assert.Equal("Wesley Godoi", vm.FilteredSales[0].ClientName);
    }

    [Fact]
    public void FilterByDeliveryIndex_SelectsCorrectFilter()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();

        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        // Index 3 = "Entregue na Obra"
        vm.SelectedDeliveryIndex = 3;

        Assert.Equal("Entregue na Obra", vm.SelectedDeliveryFilter);
        Assert.Single(vm.FilteredSales);
        Assert.Equal(DeliveryStatus.Entregue, vm.FilteredSales[0].DeliveryStatus);
    }

    [Fact]
    public void FilterByPaymentStatus_PagoParcial_ReturnsOnlyParcial()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();

        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        vm.SelectedPaymentFilter = "Pago Parcial";
        vm.ApplyFilters();

        Assert.Single(vm.FilteredSales);
        Assert.Equal(PaymentStatus.Parcial, vm.FilteredSales[0].PaymentStatus);
    }

    [Fact]
    public void FilterByPaymentStatus_Quitado_ReturnsOnlyPago()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();

        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        vm.SelectedPaymentFilter = "Quitado";
        vm.ApplyFilters();

        Assert.Single(vm.FilteredSales);
        Assert.Equal(PaymentStatus.Pago, vm.FilteredSales[0].PaymentStatus);
    }

    [Fact]
    public void FilterCombined_DeliveryEntregueAndPaymentQuitado()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();

        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        vm.SelectedDeliveryFilter = "Entregue na Obra";
        vm.SelectedPaymentFilter = "Quitado";
        vm.ApplyFilters();

        Assert.Single(vm.FilteredSales);
        Assert.Equal("Wesley Godoi", vm.FilteredSales[0].ClientName);
    }

    [Fact]
    public void FilterByText_SearchesClientOrProduct()
    {
        var vm = new SalesListViewModel(new DatabaseService());
        var sampleSales = CreateSampleSales();

        typeof(SalesListViewModel)
            .GetField("_allSales", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(vm, sampleSales);

        vm.SearchClientText = "Maciço";
        vm.ApplyFilters();

        Assert.Single(vm.FilteredSales);
        Assert.Equal("Painel Treliçado Maciço", vm.FilteredSales[0].ProductName);
    }
}
