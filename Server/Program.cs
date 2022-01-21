using ENet;
using NetStack.Serialization;

ENet.Library.Initialize();

using (Host server = new Host())
{
	Address address = new Address();

	address.Port = 1337;
	server.Create(address, 12);

	Event netEvent;

	while (!Console.KeyAvailable)
	{
		bool polled = false;

		while (!polled)
		{
			if (server.CheckEvents(out netEvent) <= 0)
			{
				if (server.Service(15, out netEvent) <= 0)
					break;

				polled = true;
			}

			switch (netEvent.Type)
			{
				case EventType.None:
					break;

				case EventType.Connect:
					Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
					break;

				case EventType.Disconnect:
					Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
					break;

				case EventType.Timeout:
					Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
					break;

				case EventType.Receive:
					Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
					byte[] data = new byte[64];
					netEvent.Packet.CopyTo(data);
					BitBuffer buffer = new BitBuffer(128);
					buffer.FromArray(data, netEvent.Packet.Length);
					String username = buffer.ReadString();
					Console.WriteLine(username);
					netEvent.Packet.Dispose();
					break;
			}
		}
	}

	server.Flush();
	ENet.Library.Deinitialize();
}