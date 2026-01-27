using StackExchange.Redis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;


public class RedisRecoveryService : BackgroundService
{
    private readonly IConnectionMultiplexer _mux;
    private readonly ILogger<RedisRecoveryService> _logger;

    private bool _wasConnected = true; // status sebelumnya

    public RedisRecoveryService(IConnectionMultiplexer mux, ILogger<RedisRecoveryService> logger)
    {
        _mux = mux;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(60000, stoppingToken);

            bool connected = _mux.IsConnected;

            try
            {
                if (!connected)
                {
                    if (_wasConnected)
                    {
                        _logger.LogWarning("❌ Redis is DISCONNECTED. Attempting to reconnect...");
                    }

                    await _mux.ConfigureAsync();

                    if (_mux.IsConnected)
                    {
                        _logger.LogInformation("Redis successfully RECONNECTED.");
                        _wasConnected = true;
                    }
                    else
                    {
                        if (_wasConnected)
                        {
                            _logger.LogError("Redis reconnect FAILED. Still disconnected.");
                        }
                        _wasConnected = false;
                    }
                }
                else
                {
                    if (!_wasConnected)
                    {
                        _logger.LogInformation("✔ Redis connection restored and stable.");
                    }

                    _wasConnected = true;
                }
            }
            catch (Exception ex)
            {
                if (_wasConnected) // hanya log sekali ketika terjadi
                {
                    _logger.LogError(ex, "Redis reconnect error.");
                }
                _wasConnected = false;
            }
        }
    }
}
