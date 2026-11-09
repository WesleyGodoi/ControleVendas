using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;

namespace ControleVendasLajes.ViewModels;

public partial class ClientsViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    public ObservableCollection<Client> Clients { get; } = new();

    [ObservableProperty]
    private int _editingClientId = 0;

    [ObservableProperty]
    private bool _isEditing = false;

    [ObservableProperty]
    private string _formTitle = "NOVO CLIENTE";

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _street = string.Empty;

    [ObservableProperty]
    private string _number = string.Empty;

    [ObservableProperty]
    private string _neighborhood = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _notes = string.Empty;

    public ClientsViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    partial void OnPhoneChanged(string value)
    {
        var formatted = FormatPhoneNumber(value);
        if (formatted != value)
        {
            Phone = formatted;
        }
    }

    private static string FormatPhoneNumber(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var digits = Regex.Replace(input, @"\D", "");

        if (digits.Length > 11)
            digits = digits[..11];

        if (digits.Length == 0)
            return string.Empty;

        if (digits.Length <= 2)
            return $"({digits}";

        if (digits.Length <= 6)
            return $"({digits[..2]}) {digits[2..]}";

        if (digits.Length <= 10)
            return $"({digits[..2]}) {digits[2..6]}-{digits[6..]}";

        return $"({digits[..2]}) {digits[2..7]}-{digits[7..]}";
    }

    [RelayCommand]
    public async Task LoadClientsAsync()
    {
        var list = await _databaseService.GetClientsAsync();
        Clients.Clear();
        foreach (var client in list)
        {
            Clients.Add(client);
        }
    }

    [RelayCommand]
    public void StartEdit(Client? client)
    {
        if (client is null) return;

        EditingClientId = client.Id;
        IsEditing = true;
        FormTitle = $"EDITANDO: {client.Name.ToUpper()}";

        Name = client.Name;
        Phone = client.Phone;
        Street = client.Street;
        Number = client.Number;
        Neighborhood = client.Neighborhood;
        City = client.City;
        Notes = client.Notes;
    }

    [RelayCommand]
    public void CancelEdit()
    {
        ResetForm();
    }

    private void ResetForm()
    {
        EditingClientId = 0;
        IsEditing = false;
        FormTitle = "NOVO CLIENTE";

        Name = string.Empty;
        Phone = string.Empty;
        Street = string.Empty;
        Number = string.Empty;
        Neighborhood = string.Empty;
        City = string.Empty;
        Notes = string.Empty;
    }

    [RelayCommand]
    private async Task SaveClientAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Atenção", "Informe o nome do cliente.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(Street))
        {
            if (Shell.Current is not null)
                await Shell.Current.DisplayAlert("Atenção", "Informe a rua/logradouro da obra.", "OK");
            return;
        }

        var addressParts = new List<string>();
        var streetAndNum = string.IsNullOrWhiteSpace(Number) ? Street.Trim() : $"{Street.Trim()}, {Number.Trim()}";
        addressParts.Add(streetAndNum);

        if (!string.IsNullOrWhiteSpace(Neighborhood))
            addressParts.Add(Neighborhood.Trim());

        if (!string.IsNullOrWhiteSpace(City))
            addressParts.Add(City.Trim());

        var formattedAddress = string.Join(" - ", addressParts);

        if (!string.IsNullOrWhiteSpace(Notes))
            formattedAddress += $" (Obs: {Notes.Trim()})";

        var client = new Client
        {
            Id = EditingClientId,
            Name = Name.Trim(),
            Phone = Phone.Trim(),
            Street = Street.Trim(),
            Number = Number.Trim(),
            Neighborhood = Neighborhood.Trim(),
            City = City.Trim(),
            Notes = Notes.Trim(),
            DeliveryAddress = formattedAddress
        };

        await _databaseService.SaveClientAsync(client);

        var actionMsg = IsEditing ? "Cliente atualizado com sucesso!" : "Cliente cadastrado com sucesso!";
        ResetForm();
        await LoadClientsAsync();

        if (Shell.Current is not null)
            await Shell.Current.DisplayAlert("Sucesso", actionMsg, "OK");
    }

    [RelayCommand]
    private async Task DeleteClientAsync(Client? client)
    {
        if (client is null || Shell.Current is null) return;

        bool confirm = await Shell.Current.DisplayAlert(
            "Confirmar Exclusão", 
            $"Deseja realmente excluir o cliente '{client.Name}'?", 
            "Excluir", 
            "Cancelar");

        if (confirm)
        {
            await _databaseService.DeleteClientAsync(client);
            if (EditingClientId == client.Id)
            {
                ResetForm();
            }
            await LoadClientsAsync();
        }
    }
}
