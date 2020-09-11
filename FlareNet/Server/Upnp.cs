using FlareNet.Debug;
using Open.Nat;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FlareNet.Server
{
	internal static class Upnp
	{
		private static Dictionary<int, NatDevice> activeMappings = new Dictionary<int, NatDevice>(1);

		public static async Task OpenPort(string portMapName, ushort port, Action<string> onCompleted)
		{
			var discoverer = new NatDiscoverer();
			var cts = new CancellationTokenSource(10000); // Timeout after 10 seconds

			// Check for UPNP device
			NatDevice device = await discoverer.DiscoverDeviceAsync(PortMapper.Upnp, cts);
			
			if (device == null)
			{
				NetworkLogger.Log("Failed to find NAT device.", LogLevel.Error);
				throw new System.Exception();
				return;
			}

			var ip = await device.GetExternalIPAsync();

			//await ClosePort(portMapName, port, device);

			// Open both TCP and UDP ports
			await device.CreatePortMapAsync(new Mapping(Protocol.Udp, port, port, portMapName));
			//AddDevice(portMapName, device);

			// Invoke callback with external IP
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

			RemoveDevice(portMapName);
			return;
		}

		public static async Task ClosePort(string portMapName, ushort port)
		{
			if (TryGetDevice(portMapName, out var device))
			{
				await ClosePort(portMapName, port, device);
			}
		}

		private static void AddDevice(string tag, NatDevice device)
		{
			int hash = tag.GetHashCode();

			if (!activeMappings.ContainsKey(hash))
			{
				activeMappings.Add(hash, device);
			}
		}

		private static void RemoveDevice(string tag)
		{
			int hash = tag.GetHashCode();

			if (activeMappings.ContainsKey(hash))
			{
				activeMappings.Remove(hash);
			}
		}

		private static bool TryGetDevice(string tag, out NatDevice device)
		{
			return activeMappings.TryGetValue(tag.GetHashCode(), out device);
		}
	}
}
