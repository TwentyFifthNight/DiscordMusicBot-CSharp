using Lavalink4NET.InactivityTracking.Players;
using Lavalink4NET.Players.Queued;
using Lavalink4NET.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lavalink4NET.InactivityTracking.Trackers;
using DSharpPlus.Entities;
using Satescuro.Extensions;

namespace Satescuro.Players.CustomPlayer
{
    public sealed class CustomPlayer : QueuedLavalinkPlayer, IInactivityPlayerListener
    {
		private readonly DiscordChannel _textChannel;
        public bool TrackStartedMessage { get; set; } = true;


		public CustomPlayer(IPlayerProperties<CustomPlayer, CustomPlayerOptions> properties)
            : base(properties)
        {
			_textChannel = properties.Options.Value.TextChannel;
		}

        public ValueTask NotifyPlayerActiveAsync(PlayerTrackingState trackingState, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return default;
        }

        public ValueTask NotifyPlayerInactiveAsync(PlayerTrackingState trackingState, CancellationToken cancellationToken = default)
        {
            DisconnectAsync().ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return default;
        }

        public ValueTask NotifyPlayerTrackedAsync(PlayerTrackingState trackingState, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return default;
        }

		protected override async ValueTask NotifyTrackStartedAsync(ITrackQueueItem track, CancellationToken cancellationToken = default)
		{
			await base.NotifyTrackStartedAsync(track, cancellationToken).ConfigureAwait(false);

            if(TrackStartedMessage)
                await _textChannel.SendMessageAsync(new DiscordEmbedBuilder()
                {
                    Description = $"Now playing: {track.Track.ToTrackString()}"
                });
		}



		/// <summary>
		/// Shuffles the playback queue.
		/// </summary>
		public async ValueTask ShuffleAsync()
        {
            await Queue.ShuffleAsync();
        }
    }
}
