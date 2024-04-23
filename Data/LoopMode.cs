using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satescuro.Data
{
	public enum LoopMode
	{
		/// <summary>
		/// Defines that items are not to be repeated.
		/// </summary>
		[ChoiceName("Disabled")]
		Disable,
		/// <summary>
		/// Defines that a single song is to be repeated.
		/// </summary>
		[ChoiceName("Song")]
		Song,

		/// <summary>
		/// Defines that the entire queue is to be looped.
		/// </summary>
		[ChoiceName("Queue")]
		Queue
	}
}
