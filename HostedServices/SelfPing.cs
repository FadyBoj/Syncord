namespace Syncord.HostedServices;

public class SelfPing : IHostedService
{
    private readonly ILogger<SelfPing> _logger;
    private readonly HttpClient _httpClient;
    private Timer _timer;
    public SelfPing(ILogger<SelfPing> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(new TimerCallback(Pinging),null,TimeSpan.Zero,TimeSpan.FromMinutes(5));
        return Task.CompletedTask;
    }

    private async void Pinging(Object state)
    {
        try
        {
            var response = await _httpClient.GetAsync("https://syncord.onrender.com/user/ping");
            _logger.LogInformation(response.ToString());
        }
        catch (Exception err)
        {
            _logger.LogInformation("Error while self pinging");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;

    }


}