# Extending `Message`

The `Message` class handles serialization and deserialization of synchronizable data through its `Process` functions. It has overloads for all native types, their array variants, `ISerializable` objects, `ISerializable` arrays, `ISerializable` lists, and two-dimensional `ISerializable` arrays. Overloads can be extended to include data types not readily-implemented.

```cs
public static class MessageExtensions
{
	public static void Process(this Message message, ref Color32 c)
	{
		message.Process(ref c.r);
		message.Process(ref c.g);
		message.Process(ref c.b);
		message.Process(ref c.a);
	}

	public static void Process(this Message message, ref Vector2 vector)
	{
		// x and y are properties, which can't be passed with ref
		float x = vector.x, y = vector.y;
		message.Process(ref x);
		message.Process(ref y);

		if (message.IsReading) // Assign to vector
			vector = new Vector2(x, y);
	}
}
```

`Message` exposes both `IsReading` and `IsWriting` for readability.
