namespace FlareNet.Debug
{
	public interface ILoggerOutput
	{
		void Log(string message);
		void LogWarning(string message);
		void LogError(string message);
	}
}
