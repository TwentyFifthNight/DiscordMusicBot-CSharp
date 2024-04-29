using Lavalink4NET.Players.Queued;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satescuro.Database.Models
{
	public class PlayerEntity : Entity
	{
		public ulong GuildId { get; set; }
		public TrackRepeatMode RepeatMode { get; set; }
		public float Volume { get; set; }
		public bool TrackStartedMessage { get; set; }
	}
}
