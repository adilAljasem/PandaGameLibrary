using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using System.Diagnostics;

namespace PandaGameLibrary.System;

public class NetworkSystem
{

	public long Ping;
	public HubConnection hubConnection { get; private set; }
    private const double UpdateInterval = 1.0 / 3.0;
    private double updateAccumulator;

    public void StartConnection(string url)
	{
		hubConnection = new HubConnectionBuilder().WithUrl(url, delegate(HttpConnectionOptions opt)
		{
			// if any other transport used other websocket disconnect on server won't work
			opt.Transports = HttpTransportType.WebSockets;

			//these two for localhost to work
            opt.WebSocketConfiguration = conf =>
            {
                conf.RemoteCertificateValidationCallback = (message, cert, chain, errors) => { return true; };
            };
            opt.HttpMessageHandlerFactory = factory => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; }
            };

        }).AddMessagePackProtocol().Build();
		hubConnection.StartAsync().ContinueWith(delegate(Task task)
		{
			if (task.IsFaulted)
			{
                Console.WriteLine($"Error starting Connection: {task.Exception?.GetBaseException().Message}");
                Console.WriteLine($"Stack trace: {task.Exception?.GetBaseException().StackTrace}");
            }
            else
			{
				Console.WriteLine("Connection started successfully.");
			}
		});

        hubConnection.On("ReceivePing", ReceivePing);
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
            Task.Run(async () => { await PandaCore.Instance.NetworkSystem.MeasurePingAsync();}) ;
            updateAccumulator -= UpdateInterval;

        }
    }

    private TaskCompletionSource<long> _pingCompletionSource;

    public async Task<long> MeasurePingAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        try
        {
            await hubConnection.InvokeAsync("SendPing");
            stopwatch.Stop();
            Ping = stopwatch.ElapsedMilliseconds;
            return stopwatch.ElapsedMilliseconds;
        }
        catch (Exception ex2)
        {
            Exception ex = ex2;
            Console.WriteLine("Error measuring ping: " + ex.Message);
            return -1L;
        }
    }



}
