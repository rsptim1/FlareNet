# Understanding `Message`

The `Message` class is the core for all of FlareNet's read and write activities. When sending data over a network, the user never really has to touch this class, but understanding how it works can prove helpful.

## Reading and writing

Whether the message is assigning values to variables through `Process` or writing values to its internal bitbuffer depends on how the message is instantiatied.

Instantiating with a buffer to read from will create a message that assigns values with `Process`. An optional *length* parameter can be included if the buffer being read from is bigger than the actual message being deserialized.

```cs
byte[] readFromHere = new byte[arbitraryLength];

using (Message m = new Message(readFromHere))
	m.Process(arbitrarySerializable); // m writes to serializable
```

Instantiating with the empty constructor or with a `ushort` value will create a message that reads values with `Process` and writes them to its buffer.

The optional `ushort` can be used to identify what kind of data a message carries with the exposed parameter, `Message.Tag`.

```cs
using (Message m = new Message())
	m.Process(arbitrarySerializable); // m reads from serializable
```

## Using `Message` outside of FlareNet

`Message.Process` can be used outside the context of networking to just quickly and cleanly serialize some data. Data can be written to an empty message and the output can be taken with the exposed function, `Message.GetBufferArray()`.

```cs
using (Message m = new Message())
{
	m.Process(players);
	m.Process(worldState);
	byte[] buffer = m.GetBufferArray();
	
	// Write buffer to file
}
```

## Deserializing classes

One thing to keep note of is how `Message` deserializes information for reference types. In deserializing a generic `ISerializable` or an array or list of any type, `Message` will internally create a new instance of the object if the object does not exist. Sending a payload containing an array over FlareNet every tick is not recommended because an array will have to be recreated on the other end with every payload sent.

With this in mind, it is recommended where possible to have `struct` serializables to minimize garbage collection.

## Serializing `float` values

The `Process(float)` overload uses half precision for compression. If you don't like that, then read up on [how to extend Message.Process()](ExtendingMessage.md). If you don't care, then it's not a concern.
