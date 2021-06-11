using ENet;

namespace FlareNet
{
	internal static class ENetLibrary
	{
		internal static bool LibraryInitialized = false;

		internal static void InitializeLibrary()
		{
			// If the library has not been initialized yet
			if (!LibraryInitialized)
			{
				if (!Library.Initialize())
					throw new System.Exception("ENet C# library unable to initialize!");

				LibraryInitialized = true;
			}
		}

		internal static void DeinitializeLibrary()
		{
			// If it's initialized
			if (LibraryInitialized)
			{
				Library.Deinitialize();
				LibraryInitialized = false;
			}
		}
	}
}
