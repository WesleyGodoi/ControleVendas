using System.Text.Json;
using ControleVendasLajes.Models;
using Microsoft.JSInterop;

namespace ControleVendasLajes.PWA.Services;

public class StorageDatabaseService
{
    private readonly IJSRuntime _js;
    private bool _initialized = false;

    private List<Client> _clients = new();
    private List<Product> _products = new();
    private List<Sale> _sales = new();
    private List<SalePayment> _payments = new();

    private const string KeyClients = "cvl_clients";
    private const string KeyProducts = "cvl_products";
    private const string KeySales = "cvl_sales";
    private const string KeyPayments = "cvl_payments";
    private const string KeyLastBackup = "cvl_last_backup";

    public StorageDatabaseService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            _clients = await LoadListAsync<Client>(KeyClients);
            _products = await LoadListAsync<Product>(KeyProducts);
            _sales = await LoadListAsync<Sale>(KeySales);
            _payments = await LoadListAsync<SalePayment>(KeyPayments);

            // Seed default products if empty
            if (_products.Count == 0)
            {
                _products = new List<Product>
                {
                    new() { Id = 1, Name = "Laje Treliçada H8 EPS", DefaultPricePerSqm = 38.00m },
                    new() { Id = 2, Name = "Laje Treliçada H8 Cerâmica", DefaultPricePerSqm = 36.00m },
                    new() { Id = 3, Name = "Laje Treliçada H12 EPS", DefaultPricePerSqm = 44.00m },
                    new() { Id = 4, Name = "Laje Treliçada H12 Cerâmica", DefaultPricePerSqm = 42.00m },
                    new() { Id = 5, Name = "Laje Treliçada H16 EPS", DefaultPricePerSqm = 52.00m }
                };
                await SaveListAsync(KeyProducts, _products);
            }

            _initialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao inicializar banco local: {ex.Message}");
            _initialized = true;
        }
    }

    // --- Clientes ---
    public async Task<List<Client>> GetClientsAsync()
    {
        await InitializeAsync();
        return _clients.OrderBy(c => c.Name).ToList();
    }

    public async Task<Client?> GetClientByIdAsync(int id)
    {
        await InitializeAsync();
        return _clients.FirstOrDefault(c => c.Id == id);
    }

    public async Task<int> SaveClientAsync(Client client)
    {
        await InitializeAsync();
        if (client.Id == 0)
        {
            client.Id = _clients.Count > 0 ? _clients.Max(c => c.Id) + 1 : 1;
            _clients.Add(client);
        }
        else
        {
            var idx = _clients.FindIndex(c => c.Id == client.Id);
            if (idx >= 0) _clients[idx] = client;
            else _clients.Add(client);
        }

        await SaveListAsync(KeyClients, _clients);
        return client.Id;
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        await InitializeAsync();
        var removed = _clients.RemoveAll(c => c.Id == id) > 0;
        if (removed) await SaveListAsync(KeyClients, _clients);
        return removed;
    }

    // --- Produtos ---
    public async Task<List<Product>> GetProductsAsync()
    {
        await InitializeAsync();
        return _products.OrderBy(p => p.Name).ToList();
    }

    public async Task<int> SaveProductAsync(Product product)
    {
        await InitializeAsync();
        if (product.Id == 0)
        {
            product.Id = _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
        }
        else
        {
            var idx = _products.FindIndex(p => p.Id == product.Id);
            if (idx >= 0) _products[idx] = product;
            else _products.Add(product);
        }

        await SaveListAsync(KeyProducts, _products);
        return product.Id;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await InitializeAsync();
        var removed = _products.RemoveAll(p => p.Id == id) > 0;
        if (removed) await SaveListAsync(KeyProducts, _products);
        return removed;
    }

    // --- Vendas / Orçamentos ---
    public async Task<List<Sale>> GetSalesAsync()
    {
        await InitializeAsync();
        return _sales.OrderByDescending(s => s.SaleDate).ToList();
    }

    public async Task<Sale?> GetSaleByIdAsync(int id)
    {
        await InitializeAsync();
        return _sales.FirstOrDefault(s => s.Id == id);
    }

    public async Task<int> SaveSaleAsync(Sale sale)
    {
        await InitializeAsync();
        if (sale.Id == 0)
        {
            sale.Id = _sales.Count > 0 ? _sales.Max(s => s.Id) + 1 : 1;
            _sales.Add(sale);
        }
        else
        {
            var idx = _sales.FindIndex(s => s.Id == sale.Id);
            if (idx >= 0) _sales[idx] = sale;
            else _sales.Add(sale);
        }

        await SaveListAsync(KeySales, _sales);
        return sale.Id;
    }

    public async Task<bool> DeleteSaleAsync(int id)
    {
        await InitializeAsync();
        var removed = _sales.RemoveAll(s => s.Id == id) > 0;
        _payments.RemoveAll(p => p.SaleId == id);
        if (removed)
        {
            await SaveListAsync(KeySales, _sales);
            await SaveListAsync(KeyPayments, _payments);
        }
        return removed;
    }

    // --- Pagamentos ---
    public async Task<List<SalePayment>> GetPaymentsBySaleIdAsync(int saleId)
    {
        await InitializeAsync();
        return _payments.Where(p => p.SaleId == saleId).OrderBy(p => p.PaymentDate).ToList();
    }

    public async Task<int> AddPaymentAsync(SalePayment payment)
    {
        await InitializeAsync();
        payment.Id = _payments.Count > 0 ? _payments.Max(p => p.Id) + 1 : 1;
        _payments.Add(payment);
        await SaveListAsync(KeyPayments, _payments);

        // Recalcular TotalPago e Status da Venda
        var sale = _sales.FirstOrDefault(s => s.Id == payment.SaleId);
        if (sale != null)
        {
            var totalPaid = _payments.Where(p => p.SaleId == payment.SaleId).Sum(p => p.Amount);
            sale.TotalPaid = totalPaid;
            if (totalPaid >= sale.TotalAmount && sale.TotalAmount > 0)
            {
                sale.PaymentStatus = PaymentStatus.Pago;
                sale.PaymentDate ??= payment.PaymentDate;
            }
            else if (totalPaid > 0)
            {
                sale.PaymentStatus = PaymentStatus.Parcial;
            }
            else
            {
                sale.PaymentStatus = PaymentStatus.Pendente;
            }
            await SaveSaleAsync(sale);
        }

        return payment.Id;
    }

    public async Task<bool> DeletePaymentAsync(int paymentId, int saleId)
    {
        await InitializeAsync();
        var removed = _payments.RemoveAll(p => p.Id == paymentId) > 0;
        if (removed)
        {
            await SaveListAsync(KeyPayments, _payments);
            var sale = _sales.FirstOrDefault(s => s.Id == saleId);
            if (sale != null)
            {
                var totalPaid = _payments.Where(p => p.SaleId == saleId).Sum(p => p.Amount);
                sale.TotalPaid = totalPaid;
                if (totalPaid >= sale.TotalAmount && sale.TotalAmount > 0)
                {
                    sale.PaymentStatus = PaymentStatus.Pago;
                }
                else if (totalPaid > 0)
                {
                    sale.PaymentStatus = PaymentStatus.Parcial;
                }
                else
                {
                    sale.PaymentStatus = PaymentStatus.Pendente;
                }
                await SaveSaleAsync(sale);
            }
        }
        return removed;
    }

    // --- Relatórios e Estatísticas ---
    public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int month, int year)
    {
        await InitializeAsync();
        var monthSales = _sales.Where(s => s.SaleDate.Month == month && s.SaleDate.Year == year).ToList();

        return new MonthlySummaryDto
        {
            TotalSqmSold = monthSales.Sum(s => s.AreaSqm),
            TotalRevenue = monthSales.Sum(s => s.TotalAmount),
            PendingDeliveriesCount = _sales.Count(s => s.DeliveryStatus != DeliveryStatus.Entregue),
            PendingPaymentsCount = _sales.Count(s => s.PaymentStatus != PaymentStatus.Pago)
        };
    }

    public async Task<DatabaseStatsDto> GetDatabaseStatsAsync()
    {
        await InitializeAsync();
        var lastBackupStr = await _js.InvokeAsync<string?>("localStorage.getItem", KeyLastBackup);
        DateTime? lastBackup = null;
        if (DateTime.TryParse(lastBackupStr, out var parsed)) lastBackup = parsed;

        var fullJson = await ExportBackupJsonAsync();
        var sizeBytes = System.Text.Encoding.UTF8.GetByteCount(fullJson);

        return new DatabaseStatsDto
        {
            ClientCount = _clients.Count,
            SaleCount = _sales.Count,
            PaymentCount = _payments.Count,
            ProductCount = _products.Count,
            DatabaseSizeBytes = sizeBytes,
            LastBackupDate = lastBackup
        };
    }

    // --- Backup & Restauração ---
    public async Task<string> ExportBackupJsonAsync()
    {
        await InitializeAsync();
        var backup = new BackupContainer
        {
            Version = 1,
            ExportedAt = DateTime.Now,
            Clients = _clients,
            Products = _products,
            Sales = _sales,
            Payments = _payments
        };

        var nowStr = DateTime.Now.ToString("o");
        await _js.InvokeVoidAsync("localStorage.setItem", KeyLastBackup, nowStr);

        return JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<bool> RestoreBackupJsonAsync(string json)
    {
        try
        {
            var backup = JsonSerializer.Deserialize<BackupContainer>(json);
            if (backup == null) return false;

            _clients = backup.Clients ?? new();
            _products = backup.Products ?? new();
            _sales = backup.Sales ?? new();
            _payments = backup.Payments ?? new();

            await SaveListAsync(KeyClients, _clients);
            await SaveListAsync(KeyProducts, _products);
            await SaveListAsync(KeySales, _sales);
            await SaveListAsync(KeyPayments, _payments);

            await _js.InvokeVoidAsync("localStorage.setItem", KeyLastBackup, DateTime.Now.ToString("o"));
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao restaurar backup: {ex.Message}");
            return false;
        }
    }

    // --- Helpers Privados ---
    private async Task<List<T>> LoadListAsync<T>(string key)
    {
        var raw = await _js.InvokeAsync<string?>("localStorage.getItem", key);
        if (string.IsNullOrWhiteSpace(raw)) return new List<T>();
        try
        {
            return JsonSerializer.Deserialize<List<T>>(raw) ?? new List<T>();
        }
        catch
        {
            return new List<T>();
        }
    }

    private async Task SaveListAsync<T>(string key, List<T> items)
    {
        var json = JsonSerializer.Serialize(items);
        await _js.InvokeVoidAsync("localStorage.setItem", key, json);
    }

    private class BackupContainer
    {
        public int Version { get; set; } = 1;
        public DateTime ExportedAt { get; set; }
        public List<Client>? Clients { get; set; }
        public List<Product>? Products { get; set; }
        public List<Sale>? Sales { get; set; }
        public List<SalePayment>? Payments { get; set; }
    }
}
