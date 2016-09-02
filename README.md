# netduino-serial-helper
The netduino-serial-helper project is a pair of libraries to make serial port communications between a Netduino and a Windows PC easy to use. All you need to do is create an object and register an event listener and you will be sending bytes back and forth in no time!

## Setup

### Prerequisites
#### Hardware
In addition to a Netduino board, you will need a TTL to USB serial cable. Since the Netduino is a 3.3V board, we will be using a TTL-232R-3V3 cable. Connect the following together:

* TX wire (red) of the serial cable <-> RX pin (digital 0) on the Netduino
* RX wire (yellow) of the serial cable <-> TX pin (digital 1) on the Netduino
* Ground wire (black) of the serial cable <-> Ground pin (GND) on the Netduino

For production use, you can connect the 5V wire of the USB serial cable to the 5V pin on the Netduino to power the board. For development use, the Netduino will be powered from the micro USB cable used for debugging.

#### Software
Start with a working Visual Studio environment that can build and publish code for the Netduino. This project was developed with a Netduino 3, but I'm sure it will work with other versions of the Netduino. For a list of downloads to setup your environment, [please checkout the Netduino downloads page.](http://www.netduino.com/downloads/)

### How to Use the Library
#### Netduino Code
While you want to open up the serial port and maintain communications with the computer on the other side, instantiate the disposable `SerialDataHelper` class in a `using` block like this:
```C#
using (var serial = new SerialDataHelper(SerialPorts.COM1, 9600, Parity.None, 8, StopBits.One))
{
    // Code goes here
}
```
Until the `SerialDataHelper` object is disposed, the Netduino will be ready to send and receive messages over the serial connection. This also means the Netduino is ready to acknowledge the handshake from the computer.

To receive messages from the serial connection, register a `PayloadReceivedEventHandler` to the serial helper. Here is an example of a simple handler that encodes the `byte[]` payload from the helper as a `string`:
```C# 
serial.PayloadReceived += new PayloadReceivedEventHandler((src, e) =>
{
    payloadState = new string(Encoding.UTF8.GetChars(e.Payload));
});
```

To send messages to the serial connection, all you need to do is call the `SendPayload` function of the serial helper. In this example, we will send a string as UTF-8 bytes to the computer on the other side of the serial connection:
```C#
serial.SendPayload(Encoding.UTF8.GetBytes("Last message received: " + payloadState));
```

#### Computer Code
Using the netduino-serial-helper on the computer side of the serial connection works in a very similar way. It is recommended that you wrap the `using` block in a `try/catch` because the helper will throw a `TimeoutException` if it is unable to connect to the Netduino in time. The default timeout is 30 seconds, but this can be adjusted when instantiating the object:
```C#
try
{
    using (var sdk = new NetduinoSerialSDK.NetduinoSerialSDK())
    {
        //If we enter this block, the helper was able to connect to the Netduino.
    }
}
catch (TimeoutException)
{
    //Deal with the fact that a connection to the Netduino could not be established.
}
```

After the connection has been established, register an event handler to receive payloads:
```C#
sdk.PayloadReceived += new PayloadReceivedEventHandler(PayloadReceivedConsoleHandler);

// .......

public static void PayloadReceivedConsoleHandler(object sender, PayloadReceivedEventArgs e)
{
    Console.WriteLine(new string(Encoding.UTF8.GetChars(e.Payload)));
}
```
And finally, sending messages to the Netduino works the same as before:
```C#
sdk.SendPayload(Encoding.UTF8.GetBytes("Hello Netduino!"));
```

### License
Spreetail has licensed the netduino-serial-helper under the terms of the Unlicense. For more information, see [http://unlicense.org/](http://unlicense.org/).

