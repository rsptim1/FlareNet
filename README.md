# FlareNet

FlareNet is a lightweight high-level wrapper around ENet C#, providing a simple server-client reliable-UDP network. The focus of FlareNet is cleanliness and readability.

**This is an ongoing project and will continue to be updated.**

## Features

- [x] Server-client connections
- [x] Channels
- [ ] Encryption
- [ ] Throttling
- [ ] Packet sending options
- [x] Logging
- [ ] Real documentation

## Building

Just open the solution and build. If it doesn't work, make an issue and complain.

FlareNet is not yet intended to be used for non-PC platforms.

## Known Issues

- Message callbacks are invoked on FlareNet's polling thread. This will be changed to dump message data for processing by a separate thread.

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

`Message.Process()` has overloads for almost every type that anyone could possibly want to be synced, and allows extension methods for types that FlareNet does not process (i.e. Unity's `Vector3` type).


To send data between server and client, an object implementing the `ISerializable` interface should be set up to process the variables to be sent or synced. It *can* be done outside of an `ISerializable` container, but I wouldn't recommend it.

```cs
class ExamplePayload : ISerializable
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

### Registering a message callback

Each unique network tag, represented as a `ushort`, can have callback methods to be invoked when a message with the respective tag is received from the server or a client.

This will be simplified in the future, **expect it to change.**

```csharp
ExamplePayload example;
const ushort Tag = 2000;

void ExampleCallback(Message message, IClient client)
{
    // The message is incoming, so the variable is given a value
    message.Process(ref example);
}

// To add the tag and callback to be invoked
client.RegisterCallback(Tag, ExampleCallback);

// To remove the callback so it is no longer invoked
client.RemoveCallback(Tag, ExampleCallback);

// To remove all callbacks for a tag
client.RemoveCallback(Tag);
```

### Sending a message

The tag attached to the message must be a unique `ushort` as to distinguish the data being sent and to invoke the correct registered callbacks. This will be changed in the future to require less manual work, expect it to change.

Create a message as such, process the data to be sent, and send through the active server or client.

Packet sending options and channel options are currently unimplemented.

```csharp
var example = new ExamplePayload();

// Create and send the message through the client or server
Message m = new Message(Tag);
m.Process(ref example);
server.SendMessage(m)
```

## Acknowledgements

Go check out [nxrighthere](https://github.com/nxrighthere). ENet C# and NetStack are both from him, and this project wouldn't exist without either of them.