# FlareNet

FlareNet is a high-level wrapper for ENet C#, providing a simple server-client reliable-UDP network. The focus of FlareNet is cleanliness and readability.

![alt text](https://i.imgur.com/qpRW2Qq.jpg)

**This is an ongoing project and will continue to be updated.**

## Features

- [x] Server-client connections
- [ ] Client connection management
- [x] Channels
- [ ] Encryption
- [ ] Throttling
- [x] Packet sending options
- [x] Logging
- [ ] Real documentation

## Building

Just open the solution and build. If it doesn't work, make an issue and complain.

FlareNet is not yet intended to be used for non-PC platforms.

## Known Issues

- PayloadHandler needs to be changed to use a ring buffer.
- Sending a network payload involves grabbing the network tag every time.

## Getting Started

### Starting a server or client

Creating a new server or connecting to an existing one is very straightforward.

```cs
using FlareNet;

// Create a server on the port 2000
FlareServer server = new FlareServer(2000);

// Create and connect a client to an IP and port
FlareClient client = new FlareClient("127.0.0.1", 2000);
```

### Processing data

`Message.Process()` has overloads for almost every type that anyone could possibly want to be synced and allows extension methods for types that FlareNet does not process (i.e. Unity's `Vector3` type).

To send data between server and client, an object implementing the `INetworkPayload` interface must be set up to process the variables to be sent or synced. This interface also requires implementation of the `ISerializable` interface, which can be used to make objects serializable.

Each network payload must have a `NetworkTag` attribute with a unique `ushort` identifier.

```cs
[NetworkTag(101, PacketOptions.Reliable)]
class ExamplePayload : INetworkPayload
{
    private int exampleInt = 18;
    private string exampleString = "speeen";
    private ISerializable nestedSerializable = new Foo();

    public void Sync(Message message)
    {
        // If reading from the buffer, the message assigns values. If writing to the buffer, the message reads values.
        message.Process(ref exampleInt);
        message.Process(ref exampleString);
        message.Process(ref nestedSerializable);
    }
}
```

### Registering a payload callback

Callback methods to handle payloads a client receives can be added through the client. When a client receives a payload of the type a method is listening for, that method will be called when the client is polled.

```cs
void ExampleCallback(ExamplePayload p)
{
    // Use payload here
}

// Add a method to be called when a payload type is received
client.AddCallback<ExamplePayload>(ExampleCallback);

// Remove the method from being called
client.RemoveCallback<ExamplePayload>(ExampleCallback);

// Clear all callbacks for a payload
client.ClearCallbacks<ExamplePayload>();
```

### Polling a client

Clients must be polled manually to ensure payload callbacks are called on the correct thread.

**The performance impact of this operation has not been benchmarked.**

```cs
client.PollMessages();
```

### Sending a network payload

Sending a payload over the network is very simple.

Overload functions for more specific behaviour are available.

```cs
byte channel = 0;
ExamplePayload p = new ExamplePayload();

client.SendMessage(p, channel);
```

## Acknowledgements

Go check out [nxrighthere](https://github.com/nxrighthere). ENet C# and NetStack are both from him, and this project wouldn't exist without either of them.
