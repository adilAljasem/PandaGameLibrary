using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http.Connections.Client;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.DependencyInjection;

namespace PandaGameLibrary.System
{
    public class NetworkSystem
    {
        public long Ping { get; private set; }
        public HubConnection hubConnection { get; private set; }
        private const double UpdateInterval = 1.0 / 3.0;
        private double updateAccumulator;
        private bool isConnecting = false;

        public async Task StartConnectionAsync(string url)
        {
            if (isConnecting) return;
            isConnecting = true;

            hubConnection = new HubConnectionBuilder()
                .WithUrl(url, options =>
                {
                    options.Transports = HttpTransportType.WebSockets;
                    options.WebSocketConfiguration = conf =>
                    {
                        conf.RemoteCertificateValidationCallback = (message, cert, chain, errors) => true;
                    };
                    options.HttpMessageHandlerFactory = factory => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                    };
                    options.CloseTimeout = TimeSpan.FromSeconds(10);
                })
                .AddMessagePackProtocol()
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                .Build();

            hubConnection.Closed += async (error) =>
            {
                Console.WriteLine($"Connection closed. Error: {error?.Message}");
                await ConnectWithRetryAsync();
            };

          

            hubConnection.Reconnected += connectionId =>
            {
                Console.WriteLine($"Reconnected with ConnectionId: {connectionId}");
                return Task.CompletedTask;
            };

            hubConnection.On("ReceivePing", ReceivePing);

            await ConnectWithRetryAsync();
            isConnecting = false;
        }

        private async Task ConnectWithRetryAsync(int maxAttempts = 5)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                try
                {
                    await hubConnection.StartAsync();
                    Console.WriteLine("Connection started successfully.");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Connection attempt {i + 1} failed: {ex.Message}");
                    if (i == maxAttempts - 1)
                    {
                        Console.WriteLine($"Failed to connect after {maxAttempts} attempts.");
                        throw;
                    }
                    await Task.Delay(5000);
                }
            }
        }

        public void ReceivePing()
        {
            _pingCompletionSource?.TrySetResult(0);
        }

        public void Update(GameTime gameTime)
        {
            double elapsedTime = gameTime.ElapsedGameTime.TotalSeconds;
            updateAccumulator += elapsedTime;
            while (updateAccumulator >= UpdateInterval)
            {
                _ = MeasurePingAsync();
                updateAccumulator -= UpdateInterval;
            }
        }

        private TaskCompletionSource<long> _pingCompletionSource;

        public async Task<long> MeasurePingAsync()
        {
            if (hubConnection.State != HubConnectionState.Connected)
            {
                //Console.WriteLine("Cannot measure ping: connection is not active.");
                return -1L;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                await hubConnection.InvokeAsync("SendPing");
                stopwatch.Stop();
                Ping = stopwatch.ElapsedMilliseconds;
                return Ping;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error measuring ping: " + ex.Message);
                return -1L;
            }
        }
    }
}