using Microsoft.SPOT;
using System;
using System.IO.Ports;
using System.Threading;
using System.Collections;

namespace System.Diagnostics
{
    public enum DebuggerBrowsableState
    {
        Collapsed,
        Never,
        RootHidden
    }
}

namespace SerialAbstraction
{
    public delegate void PayloadReceivedEventHandler(object source, PayloadReceivedEventArgs e);

    public class PayloadReceivedEventArgs : EventArgs
    {
        public byte[] Payload { get; set; }
    }

    public class SerialDataHelper : IDisposable
    {
        private static readonly byte[] StartHandshake = { 255, 254, 255 };
        private readonly byte[] EndHandshake = { 253, 254, 253 };
        private readonly byte[] ConnectCall = { 253, 254, 255 };
        private SerialPort serial;
        private Stack payloadBuffer = new Stack();
        private LimitQueue recentBytes = new LimitQueue(StartHandshake.Length);

        object myLock = new object();

        private PayloadReceivedEventHandler _handler;

        public SerialDataHelper(string port, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            serial = new SerialPort(port, baudRate, parity, dataBits, stopBits);
            serial.DataReceived += new SerialDataReceivedEventHandler(InternalDataReceivedHandler);
            serial.Open();
        }

        public event PayloadReceivedEventHandler PayloadReceived
        {
            add { lock (myLock) _handler += value; }
            remove { lock (myLock) _handler -= value; }
        }

        private void InternalDataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            while (serial.BytesToRead > 0)
            {
                var myInt = serial.ReadByte();
                if (myInt != -1)
                {
                    var myByte = (byte)myInt;
                    payloadBuffer.Push(myByte);
                    recentBytes.Enqueue(myByte);
                }
                else
                {
                    throw new UnknownTypeException();
                }

                // Answer connection for communication
                if (ByteAndObjectArraysEqual(ConnectCall, recentBytes.ToArray()))
                {
                    SendBytes(ConnectCall);
                }
                
                //If we see a new start handshake, prepare to receive the message
                if (ByteAndObjectArraysEqual(StartHandshake, recentBytes.ToArray()))
                {
                    payloadBuffer = new Stack();
                }

                //If we see the end handshake, wrap it up and give payload to helper user
                if (ByteAndObjectArraysEqual(EndHandshake, recentBytes.ToArray()))
                {
                    foreach (var ele in EndHandshake)
                    {
                        payloadBuffer.Pop();
                    }
                    byte[] payload = new byte[payloadBuffer.Count];
                    var payloadArray = payloadBuffer.ToArray();
                    for (var i = 0; i < payloadBuffer.Count; i++)
                    {
                        payload[i] = (byte)payloadArray[(payloadBuffer.Count - 1) - i];
                    }
                    var eventArgs = new PayloadReceivedEventArgs { Payload = payload };
                    if (_handler != null) _handler(this, eventArgs);
                }
            }
        }

        public void SendPayload(byte[] data)
        {
            var buffer = new byte[data.Length + StartHandshake.Length + EndHandshake.Length];

            for (var i = 0; i < StartHandshake.Length; i++)
            {
                buffer[i] = StartHandshake[i];
            }

            for (var i = StartHandshake.Length; i < data.Length + StartHandshake.Length; i++)
            {
                buffer[i] = data[i - StartHandshake.Length];
            }
            
            for (var i = StartHandshake.Length + data.Length; i < StartHandshake.Length + data.Length + EndHandshake.Length; i++)
            {
                buffer[i] = EndHandshake[i - StartHandshake.Length - data.Length];
            }

            SendBytes(buffer);
        }

        private void SendBytes(byte[] data)
        {
            serial.Write(data, 0, data.Length);
        }

        public void Dispose()
        {
            if (serial != null && serial.IsOpen) serial.Close();
        }

        //This is the class destructor method, make sure we don't leave open serial ports hanging around
        ~SerialDataHelper()
        {
            Dispose();
        }

        //From StackOverflow http://stackoverflow.com/questions/713341/comparing-arrays-in-c-sharp
        static bool ByteAndObjectArraysEqual(byte[] a1, object[] a2)
        {
            if (ReferenceEquals(a1, a2))
                return true;

            if (a1 == null || a2 == null)
                return false;

            if (a1.Length != a2.Length)
                return false;

            for (int i = 0; i < a1.Length; i++)
            {
                if (a1[i] != (byte)a2[i]) return false;
            }
            return true;
        }
    }
}
