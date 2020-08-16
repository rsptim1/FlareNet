using FlareNet.Debug;
using Open.Nat;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FlareNet.Server
{
	public class Upnp
	{
		public static async void OpenPort(string portMapName, ushort port, Action<string> onCompleted)
		{
			var discoverer = new NatDiscoverer();
			var cts = new CancellationTokenSource(10000); // Timeout after 10 seconds

			// Check for UPNP device
			NatDevice device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);

			if (device == null)
			{
				NetworkLogger.Log("Failed to find NAT device.", LogLevel.Error);
				return;
			}

			var ip = await device.GetExternalIPAsync();

			await ClosePort(portMapName, port, device);

			// Open both TCP and UDP ports
			await device.CreatePortMapAsync(new Mapping(Protocol.Udp, port, port, portMapName));

			onCompleted?.Invoke(ip.MapToIPv4().ToString());
		}

		/// <summary>
		/// Closes a port if a mapping for it exists.
		/// </summary>
		/// <param name="portMapName"></param>
		/// <param name="port"></param>
		public static async Task ClosePort(string portMapName, ushort port, NatDevice device)
		{
			var mappings = await device.GetAllMappingsAsync();

			foreach (var mapping in mappings)
			{
				if (mapping.Description == portMapName || mapping.PublicPort == port)
				{
					await device.DeletePortMapAsync(mapping);
				}
			}
		}
	}
}
