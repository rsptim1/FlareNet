namespace FlareNet.Debug
{
	/// <summary>
	/// The default logger output. This class is intended to provide a very simple log output.
	/// It can't possibly get any less complex than this.
	/// </summary>
	public class ConsoleLoggerOutput : ILoggerOutput
	{
		public void Log(string message) => ConCatLog("[LOG] ", message);

		public void LogError(string message) => ConCatLog("[ERROR] ", message);

		public void LogWarning(string message) => ConCatLog("[WARNING] ", message);

		private void ConCatLog(string prefix, string msg) => System.Console.WriteLine(prefix + msg);
	}
}
