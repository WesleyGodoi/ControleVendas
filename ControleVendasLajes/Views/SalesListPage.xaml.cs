using ControleVendasLajes.ViewModels;

namespace ControleVendasLajes.Views;

public partial class SalesListPage : ContentPage
{
    private readonly SalesListViewModel _viewModel;

    public SalesListPage(SalesListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadSalesCommand.ExecuteAsync(null);
    }

    private void OnDeliveryFilterSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && BindingContext is SalesListViewModel vm)
        {
            if (picker.SelectedIndex >= 0 && picker.SelectedIndex < vm.DeliveryFilterOptions.Count)
            {
                vm.SelectedDeliveryFilter = vm.DeliveryFilterOptions[picker.SelectedIndex];
                vm.ApplyFilters();
            }
        }
    }

    private void OnPaymentFilterSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && BindingContext is SalesListViewModel vm)
        {
            if (picker.SelectedIndex >= 0 && picker.SelectedIndex < vm.PaymentFilterOptions.Count)
            {
                vm.SelectedPaymentFilter = vm.PaymentFilterOptions[picker.SelectedIndex];
                vm.ApplyFilters();
            }
        }
    }
}
