using SQLite;
using ControleVendasLajes.Models;

namespace ControleVendasLajes.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;

    private string DatabasePath => 
        Path.Combine(FileSystem.AppDataDirectory, "controle_lajes_v1.db3");

    private async Task InitAsync()
    {
        if (_database is not null)
            return;

        var flags = SQLiteOpenFlags.ReadWrite | 
                    SQLiteOpenFlags.Create | 
                    SQLiteOpenFlags.SharedCache;

        _database = new SQLiteAsyncConnection(DatabasePath, flags);

        await _database.CreateTableAsync<Client>();
        await _database.CreateTableAsync<Product>();
        await _database.CreateTableAsync<Sale>();
        await _database.CreateTableAsync<SalePayment>();

        var userVersion = await _database.ExecuteScalarAsync<int>("PRAGMA user_version;");
        if (userVersion < 2)
        {
            string[] clientCols = ["Street", "Number", "Neighborhood", "City", "Notes"];
            foreach (var col in clientCols)
            {
                try { await _database.ExecuteAsync($"ALTER TABLE Clients ADD COLUMN {col} TEXT"); } catch { }
            }

            try { await _database.ExecuteAsync("ALTER TABLE Sales ADD COLUMN ClientPhone TEXT"); } catch { }
            try { await _database.ExecuteAsync("ALTER TABLE Sales ADD COLUMN DeliveryAddress TEXT"); } catch { }
            try { await _database.ExecuteAsync("ALTER TABLE Sales ADD COLUMN DeliveryStatus INTEGER DEFAULT 0"); } catch { }
            try { await _database.ExecuteAsync("ALTER TABLE Sales ADD COLUMN DeliveryDate TEXT"); } catch { }
            try { await _database.ExecuteAsync("ALTER TABLE Sales ADD COLUMN PaymentStatus INTEGER DEFAULT 0"); } catch { }
            try { await _database.ExecuteAsync("ALTER TABLE Sales ADD COLUMN PaymentDate TEXT"); } catch { }
            try { await _database.ExecuteAsync("ALTER TABLE Sales ADD COLUMN TotalPaid NUMERIC DEFAULT 0"); } catch { }

            await _database.ExecuteAsync("PRAGMA user_version = 2;");
        }

        await SeedDefaultDataAsync();
    }

    private async Task SeedDefaultDataAsync()
    {
        if (await _database!.Table<Product>().CountAsync() == 0)
        {
            await _database.InsertAllAsync(new List<Product>
            {
                new() { Name = "Laje Treliçada c/ EPS (Isopor) H8", DefaultPricePerSqm = 45.00m },
                new() { Name = "Laje Treliçada c/ Cerâmica H8", DefaultPricePerSqm = 48.00m },
                new() { Name = "Laje Treliçada c/ EPS (Isopor) H12", DefaultPricePerSqm = 58.00m },
                new() { Name = "Painel Treliçado Maciço", DefaultPricePerSqm = 75.00m }
            });
        }

        if (await _database.Table<Client>().CountAsync() == 0)
        {
            await _database.InsertAsync(new Client
            {
                Name = "Construtora Silva (Obra Centro)",
                Phone = "(11) 99999-0000",
                Street = "Rua das Palmeiras",
                Number = "450",
                Neighborhood = "Centro",
                City = "São Paulo",
                Notes = "Entregar no portão dos fundos",
                DeliveryAddress = "Rua das Palmeiras, 450 - Centro - São Paulo (Obs: Entregar no portão dos fundos)"
            });
        }
    }

    public async Task<List<Client>> GetClientsAsync()
    {
        await InitAsync();
        return await _database!.Table<Client>().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<Client?> GetClientByIdAsync(int id)
    {
        await InitAsync();
        return await _database!.Table<Client>().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<int> SaveClientAsync(Client client)
    {
        await InitAsync();
        return client.Id != 0 
            ? await _database!.UpdateAsync(client) 
            : await _database!.InsertAsync(client);
    }

    public async Task<int> DeleteClientAsync(Client client)
    {
        await InitAsync();
        return await _database!.DeleteAsync(client);
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await InitAsync();
        return await _database!.Table<Product>().OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<int> SaveProductAsync(Product product)
    {
        await InitAsync();
        return product.Id != 0 
            ? await _database!.UpdateAsync(product) 
            : await _database!.InsertAsync(product);
    }

    public async Task<int> DeleteProductAsync(Product product)
    {
        await InitAsync();
        return await _database!.DeleteAsync(product);
    }

    public async Task<int> SaveSaleAsync(Sale sale)
    {
        await InitAsync();
        return sale.Id != 0 
            ? await _database!.UpdateAsync(sale) 
            : await _database!.InsertAsync(sale);
    }

    public async Task<int> DeleteSaleAsync(Sale sale)
    {
        await InitAsync();
        try
        {
            await _database!.ExecuteAsync("DELETE FROM SalePayments WHERE SaleId = ?", sale.Id);
        }
        catch { }
        return await _database!.DeleteAsync(sale);
    }

    public async Task<List<SalePayment>> GetPaymentsBySaleIdAsync(int saleId)
    {
        await InitAsync();
        return await _database!.Table<SalePayment>()
                               .Where(p => p.SaleId == saleId)
                               .OrderByDescending(p => p.PaymentDate)
                               .ToListAsync();
    }

    public async Task AddPaymentAsync(SalePayment payment)
    {
        await InitAsync();
        await _database!.InsertAsync(payment);
        await RecalculateSalePaymentAsync(payment.SaleId);
    }

    public async Task DeletePaymentAsync(SalePayment payment)
    {
        await InitAsync();
        await _database!.DeleteAsync(payment);
        await RecalculateSalePaymentAsync(payment.SaleId);
    }

    public async Task RecalculateSalePaymentAsync(int saleId)
    {
        var sale = await _database!.Table<Sale>().FirstOrDefaultAsync(s => s.Id == saleId);
        if (sale is null) return;

        var payments = await _database.Table<SalePayment>()
                                      .Where(p => p.SaleId == saleId)
                                      .ToListAsync();

        var totalPaid = payments.Sum(p => p.Amount);
        sale.TotalPaid = totalPaid;

        if (totalPaid <= 0)
        {
            sale.PaymentStatus = PaymentStatus.Pendente;
            sale.PaymentDate = null;
        }
        else if (totalPaid < sale.TotalAmount)
        {
            sale.PaymentStatus = PaymentStatus.Parcial;
            sale.PaymentDate = payments.Max(p => (DateTime?)p.PaymentDate);
        }
        else
        {
            sale.PaymentStatus = PaymentStatus.Pago;
            sale.PaymentDate = payments.Max(p => (DateTime?)p.PaymentDate) ?? DateTime.Now;
        }

        await _database.UpdateAsync(sale);
    }

    public async Task<List<Sale>> GetSalesAsync()
    {
        await InitAsync();
        var sales = await _database!.Table<Sale>()
                                   .OrderByDescending(s => s.SaleDate)
                                   .ToListAsync();

        var missingInfoSales = sales.Where(s => string.IsNullOrWhiteSpace(s.DeliveryAddress) || string.IsNullOrWhiteSpace(s.ClientPhone)).ToList();
        if (missingInfoSales.Count > 0)
        {
            var clients = await _database.Table<Client>().ToListAsync();
            var clientDict = clients.ToDictionary(c => c.Id);

            foreach (var sale in missingInfoSales)
            {
                if (clientDict.TryGetValue(sale.ClientId, out var client))
                {
                    if (string.IsNullOrWhiteSpace(sale.DeliveryAddress))
                        sale.DeliveryAddress = client.DeliveryAddress;
                    if (string.IsNullOrWhiteSpace(sale.ClientPhone))
                        sale.ClientPhone = client.Phone;
                }
            }
        }

        return sales;
    }

    public async Task<MonthlySummaryDto> GetCurrentMonthDashboardAsync()
    {
        await InitAsync();

        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0);
        var endOfMonth = startOfMonth.AddMonths(1).AddTicks(-1);

        var monthlySales = await _database!.Table<Sale>()
            .Where(s => s.SaleDate >= startOfMonth && s.SaleDate <= endOfMonth)
            .ToListAsync();

        // Considera faturamento e m² das vendas que foram Fechadas (não orçamentos perdidos)
        var closedSales = monthlySales
            .Where(s => s.Status == SaleStatus.Fechado || (int)s.Status == 1 || (int)s.Status == 2 || (int)s.Status == 3)
            .ToList();

        var allActiveSales = await _database!.Table<Sale>().ToListAsync();

        return new MonthlySummaryDto
        {
            TotalSqmSold = closedSales.Sum(s => s.AreaSqm),
            TotalRevenue = closedSales.Sum(s => s.TotalPaid > 0 ? s.TotalPaid : (s.PaymentStatus == PaymentStatus.Pago ? s.TotalAmount : 0)),
            PendingDeliveriesCount = allActiveSales.Count(s => s.DeliveryStatus != DeliveryStatus.Entregue && s.Status == SaleStatus.Fechado),
            PendingPaymentsCount = allActiveSales.Count(s => s.PaymentStatus != PaymentStatus.Pago && s.Status == SaleStatus.Fechado)
        };
    }

    public async Task<DatabaseStatsDto> GetDatabaseStatsAsync()
    {
        await InitAsync();

        var clients = await _database!.Table<Client>().CountAsync();
        var sales = await _database.Table<Sale>().CountAsync();
        var payments = await _database.Table<SalePayment>().CountAsync();
        var products = await _database.Table<Product>().CountAsync();

        long size = 0;
        if (File.Exists(DatabasePath))
        {
            size = new FileInfo(DatabasePath).Length;
        }

        DateTime? lastBackup = null;
        var lastBackupStr = Preferences.Default.Get<string?>("LastBackupDateIso", null);
        if (DateTime.TryParse(lastBackupStr, out var parsed))
        {
            lastBackup = parsed;
        }

        return new DatabaseStatsDto
        {
            ClientCount = clients,
            SaleCount = sales,
            PaymentCount = payments,
            ProductCount = products,
            DatabaseSizeBytes = size,
            LastBackupDate = lastBackup
        };
    }

    public async Task<string> CreateBackupAsync()
    {
        await InitAsync();

        try
        {
            await _database!.ExecuteAsync("PRAGMA wal_checkpoint(FULL);");
        }
        catch { }

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = $"Backup_ControleLajes_{timestamp}.db3";
        var backupPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        File.Copy(DatabasePath, backupPath, true);

        Preferences.Default.Set("LastBackupDateIso", DateTime.Now.ToString("o"));

        return backupPath;
    }

    public async Task<(int clients, int sales, int payments, int products)> RestoreBackupAsync(string sourceBackupPath)
    {
        if (!File.Exists(sourceBackupPath))
        {
            throw new FileNotFoundException("O arquivo de backup não foi encontrado.", sourceBackupPath);
        }

        int clientsCount, salesCount, paymentsCount, productsCount;
        var tempConnection = new SQLiteAsyncConnection(sourceBackupPath, SQLiteOpenFlags.ReadOnly);
        try
        {
            clientsCount = await tempConnection.Table<Client>().CountAsync();
            salesCount = await tempConnection.Table<Sale>().CountAsync();
            productsCount = await tempConnection.Table<Product>().CountAsync();
            paymentsCount = await tempConnection.Table<SalePayment>().CountAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "O arquivo selecionado não contém uma base de dados válida do Controle de Vendas de Lajes.", ex);
        }
        finally
        {
            await tempConnection.CloseAsync();
        }

        var rollbackPath = DatabasePath + ".rollback";
        if (File.Exists(DatabasePath))
        {
            File.Copy(DatabasePath, rollbackPath, true);
        }

        try
        {
            if (_database != null)
            {
                await _database.CloseAsync();
                _database = null;
            }

            File.Copy(sourceBackupPath, DatabasePath, true);
            await InitAsync();

            return (clientsCount, salesCount, paymentsCount, productsCount);
        }
        catch
        {
            if (File.Exists(rollbackPath))
            {
                File.Copy(rollbackPath, DatabasePath, true);
                await InitAsync();
            }
            throw;
        }
    }
}
