# FlareNet

FlareNet is intended to be a set of high-level tools built on ENet C#, providing a basic and easy-to-use server-client structure.

**This is an ongoing project and will continue to be updated.**

### TODO

- [x] Server-client structure
- [ ] Unit tests
- [ ] Dedicated FlareSystem classes
- [ ] Callback registration attribute
- [ ] Encryption
- [ ] Proper channel management
- [ ] Proper handling of packet flags
- [ ] Simplified serializable objects
- [ ] Client metadata syncing
- [ ] Network-synced entities
- [ ] Unity extension
- [ ] Stride extension
- [ ] UPNP
- [x] Logging
- [ ] Documentation

# Building

I would like to say "just open the solution and build," but I don't think it's quite there yet.

# Getting Started

## Processing a serializable object

To send data between server and client, a struct or class implementing the `ISerializable` interface must be set up to process the variables to be sent or synced.

```cs
class ExampleClass : ISerializable
{
    private int exampleInt;
    private string exampleString;
    private ISerializable nestedSerializable;

    public ExampleClass(ISerializable nested)
    {
        exampleInt = 5;
        exampleString = "speeen";
        nestedSerializable = nested;
    }

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

Each unique network tag, represented as a `ushort`, can have a single callback method to be invoked when a message with the respective tag is received from the server.

```csharp
using FlareNet;

ExampleClass exampleVar;

void ExampleCallback(Message message, IClient client)
{
    // The message is incoming, so the variable is given a value
    message.Process(ref exampleVar);
}

// ...

// To add the tag and callback to be invoked
FlareNetwork.RegisterCallback((ushort)tag, (ServerMessageCallback) exampleCallback);

// To remove the callback so it is no longer invoked
FlareNetwork.RemoveCallback((ushort)tag);
```

## Sending a message

The tag attached to the message must be a unique `ushort` as to distinguish the data being sent and to invoke the correct registered callback.

Create a message as such, process the data to be sent, and send through the active server or client.

```csharp
const ushort Tag = 18;

// Instantiate and initialize variables
var exampleVar = new ExampleClass(new OtherSerializable());

// Create and send the message through the client or server
using (Message message = new Message(Tag))
{
    // Process the data to be sent
    message.Process(ref exampleVar);

    FlareNetwork.SendMessageReliable(message);
}
```

