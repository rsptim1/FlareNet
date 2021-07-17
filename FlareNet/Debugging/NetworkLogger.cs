using Flags = System.FlagsAttribute;

namespace FlareNet.Debug
{
	public static class NetworkLogger
	{
		/// <summary>
		/// Enable output from the logger.
		/// </summary>
		public static bool Enabled { get; private set; } = true;

		/// <summary>
		/// The minimum level to output. Anything below this value will be discarded.
		/// </summary>
		public static LogLevel OutputLevel { get; set; } = LogLevel.Message;

		public static LogCategory CategoryWhitelist { get; set; } = LogCategory.All;

		/// <summary>
		/// The output interface used by the logger. This is set to a default value
		/// and only needs to be changed if a different output method is desired.
		/// </summary>
		public static ILoggerOutput Output { get; set; } = new ConsoleLoggerOutput();

		private const string ServerStart = "FlareNet server started";
		private const string ServerStop = "FlareNet server stopped";
		private const string ClientStart = "FlareNet client started";
		private const string ClientStop = "FlareNet client stopped";
		private const string ClientDisconnect = "FlareNet client disconnected";
		private const string ClientConnect = "FlareNet client connected";
		private const string ClientTimeout = "FlareNet client timed out";

		/// <summary>
		/// Output a message to the NetworkLogger.
		/// </summary>
		/// <param name="msg">The message to log</param>
		/// <param name="level">The importance of the message</param>
		public static void Log(string msg, LogLevel level = LogLevel.Message)
		{
			if (!Enabled || Output == null || string.IsNullOrEmpty(msg) || level < OutputLevel)
				return;

			switch (level)
			{
				case LogLevel.Message:
					Output.Log(msg);
					break;
				case LogLevel.Warning:
					Output.LogWarning(msg);
					break;
				case LogLevel.Error:
					Output.LogError(msg);
					break;
			}
		}

		/// <summary>
		/// Output a message in a log category to the NetworkLogger
		/// </summary>
		/// <param name="msg">The message to log</param>
		/// <param name="category">The message log category</param>
		/// <param name="level">The importance of the message</param>
		internal static void Log(string msg, LogCategory category, LogLevel level = LogLevel.Message)
		{
			if (CategoryWhitelist.HasFlag(category) || level >= LogLevel.Error)
				Log(msg, level);
		}

		/// <summary>
		/// Output an event to the NetworkLogger.
		/// </summary>
		/// <param name="logEvent">The event to log</param>
		/// <param name="level">The importance of the event</param>
		internal static void Log(NetworkLogEvent logEvent, LogLevel level = LogLevel.Message)
		{
			if (!Enabled || level < OutputLevel)
				return;

			string message = "";

			switch (logEvent)
			{
				case NetworkLogEvent.ServerStart:
					message = ServerStart;
					break;
				case NetworkLogEvent.ServerStop:
					message = ServerStop;
					break;
				case NetworkLogEvent.ClientStart:
					message = ClientStart;
					break;
				case NetworkLogEvent.ClientStop:
					message = ClientStop;
					break;
				case NetworkLogEvent.ClientConnect:
					message = ClientConnect;
					break;
				case NetworkLogEvent.ClientDisconnect:
					message = ClientDisconnect;
					break;
				case NetworkLogEvent.ClientTimeout:
					message = ClientTimeout;
					break;
				default: break;
			}

			Log(message, LogCategory.Connections, level);
		}
	}

	internal enum NetworkLogEvent
	{
		ServerStart,
		ServerStop,

		ClientStart,
		ClientStop,
		ClientConnect,
		ClientDisconnect,
		ClientTimeout,
	}

	public enum LogLevel
	{
		Message,
		Warning,
		Error
	}

	[Flags]
	public enum LogCategory
	{
		None = 0,
		Connections = 1,
		PayloadCallbacks = 1 << 1,
		PayloadProcessing = 1 << 2,
		Packets = 1 << 3,
		All = (Packets << 1) - 1
	}
}
