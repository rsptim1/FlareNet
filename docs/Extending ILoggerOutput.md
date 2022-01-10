# Extending the logging system

FlareNet's barebones logging system can be extended to output into any desired form or external logging system via the `ILoggerOutput` interface.

For example, compatability with Unity's debug system can be accomplished by attaching the following `MonoBehaviour` to an active object:

```cs
using FlareNet.Debug;
using UnityEngine;

public class UnityOutputLogger : MonoBehaviour, ILoggerOutput
{
	private void Awake()
	{
		// Set FlareNet's logger output to this
		NetworkLogger.Output = this;

		// Log levels can be muted if they are unnecessary or annoying
		NetworkLogger.CategoryWhitelist -= LogCategory.PayloadCallbacks;
	}

	public void Log(string message)
	{
		Debug.Log(message);
	}

	public void LogError(string message)
	{
		Debug.LogError(message);
	}

	public void LogWarning(string message)
	{
		Debug.LogWarning(message);
	}
}
```