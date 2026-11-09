using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleVendasLajes.Helpers;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;

namespace ControleVendasLajes.ViewModels;

public partial class ProductsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    public ObservableCollection<Product> Products { get; } = new();

    [ObservableProperty]
    private int _editingProductId = 0;

    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private string _formTitle = "NOVO TIPO DE LAJE";

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _defaultPricePerSqm;

    [ObservableProperty]
    private string _defaultPricePerSqmText = string.Empty;

    partial void OnDefaultPricePerSqmTextChanged(string value)
    {
        var formatted = MaskHelper.FormatCurrency(value, out var parsed);
        DefaultPricePerSqm = parsed;
        if (formatted != value)
        {
            DefaultPricePerSqmText = formatted;
        }
    }

    public ProductsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        var list = await _databaseService.GetProductsAsync();
        Products.Clear();
        foreach (var product in list)
        {
            Products.Add(product);
        }
    }

    [RelayCommand]
    public void StartEdit(Product? product)
    {
        if (product is null) return;

        EditingProductId = product.Id;
        IsEditing = true;
        FormTitle = $"EDITANDO: {product.Name.ToUpper()}";

        Name = product.Name;
        DefaultPricePerSqm = product.DefaultPricePerSqm;
        DefaultPricePerSqmText = MaskHelper.FormatCurrency(product.DefaultPricePerSqm);
    }

    [RelayCommand]
    public void CancelEdit()
    {
        ResetForm();
    }

    private void ResetForm()
    {
        EditingProductId = 0;
        IsEditing = false;
        FormTitle = "NOVO TIPO DE LAJE";

        Name = string.Empty;
        DefaultPricePerSqm = 0;
        DefaultPricePerSqmText = string.Empty;
    }

    [RelayCommand]
    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Atenção", "Informe a descrição do tipo de laje.", "OK");
            return;
        }

        if (DefaultPricePerSqm <= 0)
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Atenção", "Informe um valor padrão do m² válido.", "OK");
            return;
        }

        var product = new Product
        {
            Id = EditingProductId,
            Name = Name.Trim(),
            DefaultPricePerSqm = DefaultPricePerSqm
        };

        await _databaseService.SaveProductAsync(product);

        var actionMsg = IsEditing ? "Tipo de laje atualizado com sucesso!" : "Tipo de laje cadastrado com sucesso!";
        ResetForm();
        await LoadProductsAsync();

        if (Shell.Current is not null)
            await Shell.Current.DisplayAlert("Sucesso", actionMsg, "OK");
    }

    [RelayCommand]
    private async Task DeleteProductAsync(Product? product)
    {
        if (product is null || Shell.Current is null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Confirmar Exclusão", 
            $"Deseja realmente excluir a laje '{product.Name}'?", 
            "Excluir", 
            "Cancelar");

        if (confirm)
        {
            await _databaseService.DeleteProductAsync(product);
            if (EditingProductId == product.Id)
            {
                ResetForm();
            }
            await LoadProductsAsync();
        }
    }
}
