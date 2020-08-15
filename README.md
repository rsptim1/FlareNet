# FlareNet

FlareNet is intended to be a set of high-level tools built on ENet C#, providing a basic and easy-to-use server-client structure.

**This is an ongoing project and will continue to be updated.**

### TODO

- [ ] Server-client structure
- [ ] Encryption
- [ ] Simplified serializable objects
- [ ] Network-synced entities
- [ ] Unity extension
- [ ] Stride extension
- [ ] UPNP
- [ ] Logging
- [ ] Documentation

# Building

I would like to say "just open the solution and build," but I don't think it's quite there yet.

# Getting Started

## Registering a message callback

Each unique network tag, represented as a `ushort`, can have a single callback method to be invoked when a message with the respective tag is received from the server.

```csharp
using FlareNet;

FlareClient.RegisterMessage((ushort)tag, (ServerMessageCallback) callback);

// You must clean up after object is destroyed
FlareClient.RemoveMessage((ushort)tag);
```

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
    message.Process(exampleVar);

    FlareNetwork.SendMessage(message, SendMode.Reliable);
}
```

## Receiving a message

After registering a tag and its callback, the registered method will be invoked when a message with the respective tag is received.

```cs

```
