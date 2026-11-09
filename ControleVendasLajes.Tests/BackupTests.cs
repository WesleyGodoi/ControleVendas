using System.IO;
using ControleVendasLajes.Models;
using ControleVendasLajes.Services;
using Xunit;

namespace ControleVendasLajes.Tests;

public class BackupTests
{
    [Theory]
    [InlineData(500, "500 B")]
    [InlineData(1024, "1,0 KB")]
    [InlineData(2048, "2,0 KB")]
    [InlineData(1048576, "1,00 MB")]
    [InlineData(2621440, "2,50 MB")]
    public void DatabaseStatsDto_FormattedSize_ReturnsExpectedUnit(long bytes, string expectedFormat)
    {
        var stats = new DatabaseStatsDto
        {
            DatabaseSizeBytes = bytes
        };

        // Note: Culture might use comma or period depending on locale
        var result = stats.FormattedSize.Replace(".", ",");
        Assert.Equal(expectedFormat, result);
    }

    [Fact]
    public async Task RestoreBackupAsync_ThrowsFileNotFound_WhenFileDoesNotExist()
    {
        var service = new DatabaseService();
        var fakePath = Path.Combine(Path.GetTempPath(), "inexistent_backup_12345.db3");

        await Assert.ThrowsAsync<FileNotFoundException>(() => service.RestoreBackupAsync(fakePath));
    }

    [Fact]
    public async Task RestoreBackupAsync_ThrowsInvalidOperation_WhenFileIsNotValidDatabase()
    {
        var service = new DatabaseService();
        var tempCorruptFile = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempCorruptFile, "Este arquivo nao e um banco de dados valido!");

            await Assert.ThrowsAsync<InvalidOperationException>(() => service.RestoreBackupAsync(tempCorruptFile));
        }
        finally
        {
            if (File.Exists(tempCorruptFile))
            {
                File.Delete(tempCorruptFile);
            }
        }
    }
}
