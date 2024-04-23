using Lavalink4NET.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satescuro.Extensions
{
	public static class LavalinkTrackExtensions
	{
		public static string ToTrackString(this LavalinkTrack item)
		{
			return $"**{item.Title}** [{item.Duration.ToDurationString()}]";
		}

		public static string ToDurationString(this TimeSpan timeSpan)
		{
			if (timeSpan.TotalHours >= 1)
				return timeSpan.ToString(@"h\:mm\:ss");
			return timeSpan.ToString(@"m\:ss");
		}
	}
}
