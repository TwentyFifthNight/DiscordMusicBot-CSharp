using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satescuro.Exceptions
{
	public class CommandCancelledException : Exception
	{
		/// <summary>
		/// Creates a new execution cancelled exception with default reason.
		/// </summary>
		public CommandCancelledException()
			: base("Command execution was cancelled due to unmet criteria.")
		{ }

		/// <summary>
		/// Creates a new execution cancelled exception with specified reason.
		/// </summary>
		/// <param name="message">Reason for cancellation.</param>
		public CommandCancelledException(string message)
			: base(message)
		{ }

		/// <summary>
		/// Creates a new execution cancelled exception with default reason and specified cause.
		/// </summary>
		/// <param name="innerException">Cause of the cancellation.</param>
		public CommandCancelledException(Exception innerException)
			: base("Command execution was cancelled due to unmet criteria.", innerException)
		{ }

		/// <summary>
		/// Creates a new execution cancelled exception with specified reason and cause.
		/// </summary>
		/// <param name="message">Reason for cancellation.</param>
		/// <param name="innerException">Cause of the cancellation.</param>
		public CommandCancelledException(string message, Exception innerException)
			: base(message, innerException)
		{ }
	}
}
