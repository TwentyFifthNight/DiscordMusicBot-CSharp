using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;
using DSharpPlus.SlashCommands;
using Satescuro.Exceptions;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using Satescuro.Extensions;
using System.Diagnostics;
using Satescuro.Data;
using Satescuro.Players.CustomPlayer;
using Satescuro.Database.Context;
using Satescuro.Database.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace Satescuro.Commands.Slash
{
    public class MusicCommands : ApplicationCommandModule
    {
        private readonly IAudioService _audioService;
		private readonly MyDbContext _context;

		public MusicCommands(IAudioService audioService, MyDbContext context)
        {
            ArgumentNullException.ThrowIfNull(audioService);
			ArgumentNullException.ThrowIfNull(context);

			_audioService = audioService;
			_context = context;
        }

        public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext interactionContext)
        {	
            var chanel = interactionContext.Member.VoiceState?.Channel;
            if (chanel == null)
            {
                await interactionContext.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Description = "You need to be in a voice channel."
                }));

                throw new CommandCancelledException();
            }

            var member = interactionContext.Guild.CurrentMember?.VoiceState?.Channel;
            if (member != null && chanel != member)
            {
                await interactionContext.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Description = "You need to be in the same voice channel."
                }));
                throw new CommandCancelledException();
            }

            return await base.BeforeSlashExecutionAsync(interactionContext);
        }

        [SlashCommand("play", description: "Plays music")]
        public async Task Play(InteractionContext interactionContext, [Option("URL", "Music URL")] string url)
        {
            await interactionContext.DeferAsync().ConfigureAwait(false);
			DiscordEmbedBuilder embed = new()
			{
				Color = DiscordColor.Red
			};

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: true).ConfigureAwait(false);

            if (player is null)
            {
                return;
            }

			try
			{
				Uri uri = new(url);
			}
			catch (Exception)
			{
				embed.Description = $"Invalid URL: {url}.";
				await interactionContext.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed));
				return;
			}


			var tracksResult = await _audioService.Tracks
                .LoadTracksAsync(url, TrackSearchMode.YouTube)
                .ConfigureAwait(false);

            if (!tracksResult.IsSuccess)
            {
				embed.Description = $"Track search failed for {url}.";

                await interactionContext
                    .FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed))
                    .ConfigureAwait(false);
				return;
			}


			embed.Color = DiscordColor.Black;
			if (tracksResult.IsPlaylist)
			{
				foreach(var track in tracksResult.Tracks)
					await player.PlayAsync(track).ConfigureAwait(false);

				embed.Description = $"Added {tracksResult.Tracks.Length:#,##0} tracks to playback queue!";
			}
			else
			{
				var track = tracksResult.Track;
				await player.PlayAsync(track).ConfigureAwait(false);

				embed.Description = $"Added {track.Title} to the playback queue.";
			}

			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
		}

		[SlashCommand("leave", "Leave channel")]
		public async Task Leave(InteractionContext interactionContext)
		{
			await interactionContext.DeferAsync().ConfigureAwait(false);

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			await player.DisconnectAsync().ConfigureAwait(false);

			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
				{
					Color = DiscordColor.Black,
					Description = $"Leaving!"
				}));
		}

		[SlashCommand("skip", description: "Skips current track.")]
        public async Task Skip(InteractionContext interactionContext)
        {
			await interactionContext.DeferAsync().ConfigureAwait(false);
			

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			if(player.CurrentTrack is null)
			{
				await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
					new DiscordEmbedBuilder(){
						Color = DiscordColor.Red,
						Description = "Nothing to skip."
					}));
				return;
			}


			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
				new DiscordEmbedBuilder()
				{
					Color = DiscordColor.Black,
					Description = $"{player.CurrentTrack.Title} skipped."
				}));
			await player.SkipAsync().ConfigureAwait(false);
		}

		[SlashCommand("volume", "Sets playback volume.")]
		public async Task Volume(InteractionContext interactionContext,
			[Option("Volume", "0-200")]
			[Minimum(0)]
			[Maximum(200)] long volume)
		{
			await interactionContext.DeferAsync();

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			await player.SetVolumeAsync((float)volume / 100).ConfigureAwait(false);

			await UpdatePlayerDB(player);

			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
				{
					Color = DiscordColor.Black,
					Description = $"Volume set to {volume}."
				}));
		}

		[SlashCommand("pause", "Pauses playback.")]
		public async Task Pause(InteractionContext interactionContext)
		{
			await interactionContext.DeferAsync();

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);
            
            if(player is null)
            {
                return;
            }

			if(player.State  == PlayerState.Paused) 
			{
				await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
					new DiscordEmbedBuilder()
					{
						Color = DiscordColor.Red,
						Description = "Player is already paused."
					}));
				return;
			}

            await player.PauseAsync().ConfigureAwait(false);


			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
				new DiscordEmbedBuilder()
				{
					Color = DiscordColor.Black,
					Description = "Playback paused. Use resume command to resume playback."
				}));
		}

		[SlashCommand("resume", "Resume playback.")]
		public async Task Resume(InteractionContext interactionContext)
		{
			await interactionContext.DeferAsync();

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			if (player.State == PlayerState.Playing)
			{
				await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
					new DiscordEmbedBuilder()
					{
						Color = DiscordColor.Red,
						Description = "Player is already playing."
					}));
				return;
			}

			await player.ResumeAsync().ConfigureAwait(false);
			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(
				new DiscordEmbedBuilder()
				{
					Color = DiscordColor.Black,
					Description = "Playback resumed."
				}));
		}

		[SlashCommand("shuffle", "Shuffles the queue.")]
		public async Task Shuffle(InteractionContext interactionContext)
		{
			await interactionContext.DeferAsync();

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			await player.ShuffleAsync().ConfigureAwait(false);

			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
				{
					Color = DiscordColor.Black,
					Description = "Queue shuffled."
				}));
		}

		[SlashCommand("remove", "Removes a track from playback queue.")]
		public async Task Remove(InteractionContext interactionContext, [Option("Index", "Track index")][Minimum(1)] long index)
		{
			await interactionContext.DeferAsync();

			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			var track = player.Queue.ElementAt((int)index - 1);
			await player.Queue.RemoveAtAsync((int)index - 1);

			DiscordEmbedBuilder embed = new()
			{
				Color = DiscordColor.Black,
			};

			if (track == null)
				embed.Description = "No such track.";
			else
			{
				embed.Description = $"{track.Track?.Title} removed.";
			}

			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder()
				.AddEmbed(embed));
		}

		[SlashCommand("queue", "Displays current playback queue.")]
		public async Task Queue(InteractionContext interactionContext)
		{
			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}
			
			var pageCount = player.Queue.Count / 10 + 1;
			if (player.Queue.Count % 10 == 0) pageCount--;

			var track = player.CurrentItem;
			if (pageCount == 0)
			{
				DiscordEmbedBuilder embed = new()
				{
					Color = DiscordColor.Black,
				};

				if (track == null)
					embed.Description = "Queue is empty!";
				else
					embed.Description = $"Now playing: {track.Track.ToTrackString()}";

				await interactionContext.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(embed));
				return;
			}

			await interactionContext.CreateResponseAsync(new DiscordInteractionResponseBuilder().AddEmbed(new DiscordEmbedBuilder()
			{
				Description = "Generating queue"
			}));

			Page[] pages = new Page[pageCount];

			int index = 1;
			int page = 1;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"Now playing: {track.Track.ToTrackString()}\n");
			foreach (var item in player.Queue)
			{
				sb.AppendLine($"{index} {item.Track.ToTrackString()}");
				if (index % 10 == 0)
				{
					sb.AppendLine($"Page {page}/{pageCount}");
					var embed = new DiscordEmbedBuilder()
					{
						Description = sb.ToString(),
						Title = "Queue"
					};
					pages[page - 1] = new Page(embed: embed);
					sb.Clear();
					sb.AppendLine($"Now playing: {track.Track.ToTrackString()}\n");
					page++;
				}
				index++;
			}
			if (--index % 10 != 0)
			{
				sb.AppendLine($"Page {page}/{pageCount}");
				var embed = new DiscordEmbedBuilder()
				{
					Description = sb.ToString(),
					Title = "Queue"
				};
				pages[page - 1] = new Page(embed: embed);
			}

			var emojis = new PaginationEmojis
			{
				SkipLeft = null,
				SkipRight = null,
				Stop = DiscordEmoji.FromUnicode("⏹"),
				Left = DiscordEmoji.FromUnicode("◀"),
				Right = DiscordEmoji.FromUnicode("▶")
			};
			await interactionContext.Client.GetInteractivity()
				.SendPaginatedMessageAsync(interactionContext.Channel, interactionContext.User, pages, emojis,
				PaginationBehaviour.Ignore, PaginationDeletion.DeleteEmojis, TimeSpan.FromSeconds(30));
		}
		
		[SlashCommand("nowplaying", "Displays information about currently-played track.")]
		public async Task NowPlaying(InteractionContext interactionContext)
		{
			await interactionContext.DeferAsync();
			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			var track = player.CurrentItem;

			DiscordEmbedBuilder embed = new()
			{
				Color = DiscordColor.Black,
			};


			if (track == null || track.Track == null)
				embed.Description = "Nothing is playing.";
			else
				embed.Description = $"Now playing: {track.Track.Title} " +
					$"[{player.Position.Value.Position.ToDurationString()}/{track.Track.Duration.ToDurationString()}].";


			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder()
				.AddEmbed(embed));
		}
		
		[SlashCommand("loop", "Set queue loop mode.")]
		public async Task Repeat(InteractionContext interactionContext, [Option("mode", "Repeat mode")] LoopMode mode = LoopMode.Disable)
		{
			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			
			if (!(player.RepeatMode == (TrackRepeatMode) mode))
			{
				player.RepeatMode = (TrackRepeatMode) mode;

				await UpdatePlayerDB(player);

				await interactionContext.CreateResponseAsync(new DiscordInteractionResponseBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					{
						Description = $"Repeat mode changed to {mode}"
					}));
			}
			else
				await interactionContext.CreateResponseAsync(new DiscordInteractionResponseBuilder()
					.AddEmbed(new DiscordEmbedBuilder()
					{
						Description = $"Repeat mode already set to {mode}"
					}));
		}
		
		[SlashCommand("playerinfo", "Displays information about current player.")]
		public async Task PlayerInfo(InteractionContext interactionContext)
		{
			await interactionContext.DeferAsync();
			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}


			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
				{
					Description = $"Queue length: {player.Queue.Count}" +
					$"\nRepeat mode: {player.RepeatMode}" +
					$"\nVolume: {(int)(player.Volume * 100)}%" +
					$"\nTrack message: {(player.TrackStartedMessage ? "Enabled" : "Disabled")}"
				}));
		}

		[SlashCommand("trackmessage", "Toggle showing message at the beginning of a track.")]
		public async Task TrackMessage(InteractionContext interactionContext)
		{
			await interactionContext.DeferAsync();
			var player = await GetPlayerAsync(interactionContext, connectToVoiceChannel: false).ConfigureAwait(false);

			if (player is null)
			{
				return;
			}

			player.TrackStartedMessage = !player.TrackStartedMessage;

			await UpdatePlayerDB(player);

			await interactionContext.EditResponseAsync(new DiscordWebhookBuilder()
				.AddEmbed(new DiscordEmbedBuilder()
				{
					Description = $"Track message: {(player.TrackStartedMessage ? "Enabled" : "Disabled")}"
				}));
		}

		private async ValueTask<CustomPlayer?> GetPlayerAsync(InteractionContext interactionContext, bool connectToVoiceChannel = true)
        {
            ArgumentNullException.ThrowIfNull(interactionContext);

            var retrieveOptions = new PlayerRetrieveOptions(
                ChannelBehavior: connectToVoiceChannel ? PlayerChannelBehavior.Join : PlayerChannelBehavior.None);

            var playerOptions = new CustomPlayerOptions { HistoryCapacity = 1000 , DisconnectOnStop = false};

			var options = new CustomPlayerOptions()
			{
				TextChannel = interactionContext.Channel,
				GuildId = interactionContext.Guild.Id
			};

			var result = await _audioService.Players
				.RetrieveAsync<CustomPlayer, CustomPlayerOptions>(interactionContext.Guild.Id, interactionContext.Member?.VoiceState.Channel.Id, 
					CreatePlayerAsync, Options.Create(options), retrieveOptions)
				.ConfigureAwait(false);

            if (!result.IsSuccess)
            {
                var errorMessage = result.Status switch
                {
                    PlayerRetrieveStatus.UserNotInVoiceChannel => "You are not connected to a voice channel.",
                    PlayerRetrieveStatus.BotNotConnected => "The bot is currently not connected.",
                    _ => "Unknown error.",
                };

                var errorResponse = new DiscordFollowupMessageBuilder()
                    .WithContent(errorMessage)
                    .AsEphemeral();

                await interactionContext
                    .FollowUpAsync(errorResponse)
                    .ConfigureAwait(false);

                return null;
            }

			return result.Player;
        }

		private async ValueTask<CustomPlayer> CreatePlayerAsync(IPlayerProperties<CustomPlayer, 
			CustomPlayerOptions> properties, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ArgumentNullException.ThrowIfNull(properties);

			CustomPlayer player = new(properties);


			if (_context.Players == null)
				return player;


			PlayerEntity? playerEntity = await _context.Players.
				FirstOrDefaultAsync(x => x.GuildId == properties.Options.Value.GuildId);

			if (playerEntity == null)
			{
				playerEntity = new()
				{
					Id = 0,
					GuildId = properties.Options.Value.GuildId,
					RepeatMode = player.RepeatMode,
					TrackStartedMessage = player.TrackStartedMessage,
					Volume = player.Volume
				};

				_context.Players.Add(playerEntity);
				await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}
			else
			{
				player.RepeatMode = playerEntity.RepeatMode;
				player.TrackStartedMessage = playerEntity.TrackStartedMessage;
				await player.SetVolumeAsync(playerEntity.Volume, cancellationToken).ConfigureAwait(false);
			}

			return player;
		}

		private async Task UpdatePlayerDB(CustomPlayer player)
		{
			PlayerEntity? playerEntity = await _context.Players.
				FirstOrDefaultAsync(p => p.GuildId == player.GuildId);

			if (playerEntity != null)
			{
				playerEntity.RepeatMode = player.RepeatMode;
				playerEntity.TrackStartedMessage = player.TrackStartedMessage;
				playerEntity.Volume = player.Volume;

				try
				{
					_context.Update(playerEntity);
					await _context.SaveChangesAsync();
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
				}
			}
		}
	}
}
