# FlareNet

FlareNet is intended to be a set of high-level tools built on ENet C#, providing a basic and easy-to-use server-client structure.

**This is an ongoing project and will continue to be updated.**

### TODO

- [x] Server-client structure
- [ ] Unit tests
- [ ] FlareSystem classes
- [ ] Callback registration attribute
- [ ] Encryption
- [ ] Channel management
- [ ] Handling of packet flags
- [ ] Unity extension
- [ ] Stride extension
- [x] Logging
- [ ] Documentation

# Building

I would like to say "just open the solution and build," but I don't think it's quite there yet.

# Getting Started

## Starting a server or client

Creating a new server or connecting to an existing one is very straightforward.

```cs
using FlareNet;

// Create a server on the port 2000
FlareServer server = new FlareServer(2000);

// Create and connect a client to an IP and port
FlareClient client = new FlareClient("127.0.0.1", 2000);
```

## Processing a serializable object

To send data between server and client, a struct or class implementing the `ISerializable` interface must be set up to process the variables to be sent or synced.

```cs
class ExampleClass : ISerializable
{
    private int exampleInt = 18;
    private string exampleString = "speeen";
    private ISerializable nestedSerializable = new OtherSerializable();

    public void Sync(Message message)
    {
        // The variables can be written to the message or assigned values as read from the message
        message.Process(ref exampleInt);
        message.Process(ref exampleString);
        message.Process(ref nestedSerializable);
    }
}
```

## Registering a message callback

Each unique network tag, represented as a `ushort`, can have callback methods to be invoked when a message with the respective tag is received from the server or a client.

This will be simplified in the future, expect it to change.

```csharp
ExampleClass exampleVar;

void ExampleCallback(Message message, IClient client)
{
    // The message is incoming, so the variable is given a value
    message.Process(ref exampleVar);
}

// ...

// To add the tag and callback to be invoked
client.RegisterCallback((ushort)tag, (FlareMessageCallback)ExampleCallback);

// To remove the callback so it is no longer invoked
client.RemoveCallback((ushort)tag, (FlareMessageCallback)ExampleCallback);

// To remove all callbacks for a tag
client.RemoveCallback((ushort)tag);
```

## Sending a message

The tag attached to the message must be a unique `ushort` as to distinguish the data being sent and to invoke the correct registered callbacks. This will be changed in the future to require less manual work, expect it to change.

Create a message as such, process the data to be sent, and send through the active server or client.

```csharp
const ushort Tag = 18;

// Instantiate and initialize variables
var exampleVar = new ExampleClass();

// Create and send the message through the client or server
Message m = new Message(Tag);
m.Process(ref exampleVar);
server.SendMessage(m)
```

