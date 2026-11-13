using Common.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using RecipeBook.Shared.Constants;
using RecipeBook.Shared.Models;

namespace RecipeBook.Services.Http
{
    public class RecipeShareProgressClient(IConfiguration configuration, ITokenProvider tokenProvider) : IRecipeShareProgressClient
    {
        private HubConnection? _connection;

        public event Action<JobStatusModel>? ProgressReceived;

        public async Task ConnectAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            var baseUrl = configuration[$"{ApiConfigNames.RecipeBook}:BaseUrl"];

            _connection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl?.TrimEnd('/')}/hubs/recipe-share-progress", options =>
                {
                    options.AccessTokenProvider = () => tokenProvider.GetTokenAsync();
                })
                .WithAutomaticReconnect()
                .Build();

            _connection.On<JobStatusModel>("ShareProgress", status => ProgressReceived?.Invoke(status));

            await _connection.StartAsync(cancellationToken);
            await _connection.InvokeAsync("JoinJobAsync", jobId, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection is not null)
                await _connection.DisposeAsync();
        }
    }
}
