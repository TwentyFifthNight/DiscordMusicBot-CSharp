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

namespace Satescuro.Players.CustomPlayer
{
    public sealed class CustomPlayer : QueuedLavalinkPlayer, IInactivityPlayerListener
    {
		public CustomPlayer(IPlayerProperties<CustomPlayer, CustomPlayerOptions> properties)
            : base(properties)
        {
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
    }
}
