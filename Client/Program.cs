using ENet;
using NetStack.Serialization;
using System.Text.RegularExpressions;

bool initialize = ENet.Library.Initialize();

using (Host client = new Host())
{
	Address address = new Address();

	address.SetHost("127.0.0.1");
	address.Port = 1337;
	client.Create();
	Peer peer = client.Connect(address);

	Event netEvent;
	do
    {
		while (!Console.KeyAvailable)
		{
			bool polled = false;

			while (!polled)
			{
				if (client.CheckEvents(out netEvent) <= 0)
				{
					if (client.Service(15, out netEvent) <= 0)
						if (peer.State == PeerState.Connected)
						{
							break;
						}
						else
						{
							Console.WriteLine("Couldn't connect to the server. Press any key to try again.");
							Console.ReadKey(true);
						}

					polled = false;
				}

				switch (netEvent.Type)
				{
					case EventType.None:

						break;

					case EventType.Connect:
						Console.Write("Enter your username: ");
						String username = Console.ReadLine() ?? throw new Exception();
						if (!(Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$")))
                        {
							Console.WriteLine("Username should be alphanumeric!");
							Console.ReadKey(true);
							return;
                        }
						byte[] data = new byte[64];
						BitBuffer buffer = new BitBuffer(128);
						buffer.AddString(username).ToArray(data);
						Packet packet = default(Packet);
						packet.Create(data);
						peer.Send(netEvent.ChannelID, ref packet);
						Console.WriteLine("Client connected to server.");
						break;

					case EventType.Disconnect:
						Console.WriteLine("Client disconnected from server.");
						break;

					case EventType.Timeout:
						Console.WriteLine("Client connection timeout.");
						break;

					case EventType.Receive:
						Console.WriteLine("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
						netEvent.Packet.Dispose();
						break;
				}
			}
		}
	} while (Console.ReadKey(true).Key != ConsoleKey.Escape);
	peer.Disconnect(0);
	client.Flush();
	ENet.Library.Deinitialize();
}