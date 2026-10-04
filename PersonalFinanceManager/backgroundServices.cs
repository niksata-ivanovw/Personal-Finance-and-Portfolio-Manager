namespace test;
using test.Pages;

public class PriceUpdateService : BackgroundService
{
    private readonly ILogger<PriceUpdateService> _logger;

    public PriceUpdateService(ILogger<PriceUpdateService> logger)
    {
        _logger = logger;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Price Update Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                MarketsModel.UpdateAllPrices();
                _logger.LogInformation("Prices updated at: {time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prices");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}