using System;
using System.Reflection;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands.EventArgs;
using Satescuro.Data;
using Lavalink4NET;
using Lavalink4NET.Extensions;
using Lavalink4NET.InactivityTracking;
using Lavalink4NET.InactivityTracking.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = new HostApplicationBuilder(args);

BotConfig configuration;
try
{
	var configurationFile = new FileInfo("appsettings.json");
	configuration = await BotConfig.LoadConfig(configurationFile);
}
catch (Exception ex)
{
	Console.WriteLine(ex.Message);
	return;
}


builder.Services.AddHostedService<ApplicationHost>();
builder.Services.AddSingleton<DiscordClient>();
builder.Services.AddSingleton(new DiscordConfiguration { Token = configuration.Token, });

builder.Services.AddLavalink();
builder.Services.ConfigureLavalink(config =>
{
	config.BaseAddress = new Uri($"http://{configuration.LavalinkAddress}:{configuration.LavalinkPort}");
	config.Passphrase = configuration.LavalinkPassword;
});

builder.Services.AddInactivityTracking();
builder.Services.ConfigureInactivityTracking(options =>
{
	options.DefaultTimeout = TimeSpan.FromSeconds(120);
	options.DefaultPollInterval = TimeSpan.FromSeconds(30);
	options.TrackingMode = InactivityTrackingMode.Any;
	options.UseDefaultTrackers = true;
	options.TimeoutBehavior = InactivityTrackingTimeoutBehavior.Lowest;
	options.InactivityBehavior = PlayerInactivityBehavior.None;
});

builder.Services.AddLogging(s => s.AddConsole().SetMinimumLevel(LogLevel.Trace));

builder.Build().Run();

file sealed class ApplicationHost : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly DiscordClient _discordClient;

	public ApplicationHost(IServiceProvider serviceProvider, DiscordClient discordClient)
	{
		ArgumentNullException.ThrowIfNull(serviceProvider);
		ArgumentNullException.ThrowIfNull(discordClient);

		_serviceProvider = serviceProvider;
		_discordClient = discordClient;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_discordClient.UseInteractivity(new InteractivityConfiguration
		{
			Timeout = TimeSpan.FromSeconds(30)
		});

		var Slash = _discordClient
			.UseSlashCommands(new SlashCommandsConfiguration { Services = _serviceProvider });
		Slash.RegisterCommands(Assembly.GetExecutingAssembly());
		Slash.SlashCommandErrored += OnCommandErrored;

	
		await _discordClient
			.ConnectAsync()
			.ConfigureAwait(false);

		var readyTaskCompletionSource = new TaskCompletionSource();


		_discordClient.Ready += OnClientReady;
		//_discordClient.VoiceStateUpdated += OnDiscordVoiceStateUpdated;
		await readyTaskCompletionSource.Task.ConfigureAwait(false);
		_discordClient.Ready -= OnClientReady;

		await Task
			.Delay(Timeout.InfiniteTimeSpan, stoppingToken)
			.ConfigureAwait(false);
	}

	private async Task OnCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
	{
		if (e.Exception is SlashExecutionChecksFailedException exception)
		{
			foreach (var check in exception.FailedChecks)
			{
				var cooldown = (SlashCooldownAttribute)check;


				var cooldownEmbed = new DiscordEmbedBuilder
				{
					Title = "Cooldown",
					Description = $"Time: {cooldown.GetRemainingCooldown(e.Context).Seconds} seconds",
					Color = DiscordColor.Red
				};
				await e.Context.Channel.SendMessageAsync(embed: cooldownEmbed);
			}
		}
		else
			Console.WriteLine(e.Exception.StackTrace);
	}

	private Task OnClientReady(DiscordClient client, ReadyEventArgs eventArgs)
	{
		client.UpdateStatusAsync(new DiscordActivity("by TwentyFifthNight"), UserStatus.Online);
		return Task.CompletedTask;
	}
}