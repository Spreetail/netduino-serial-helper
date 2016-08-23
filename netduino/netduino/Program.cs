using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO.Ports;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using SerialAbstraction;
using System.Text;

namespace System.Diagnostics
{
    public enum DebuggerBrowsableState
    {
        Collapsed,
        Never,
        RootHidden
    }
}

namespace Netduino
{
    /// <summary>
    /// A simple program for sending messages between the netduino and computer over a serial port
    /// </summary>
    public class Program
    {
        public static Cpu.Pin BUTTON_0_PIN = Pins.GPIO_PIN_D10;
        static Cpu.Pin COM_UART_RX_PIN = Pins.GPIO_PIN_D0;
        static Cpu.Pin COM_UART_TX_PIN = Pins.GPIO_PIN_D1;

        public static void Main()
        {
            var BUTTON_0 = new InputPort(BUTTON_0_PIN, false, Port.ResistorMode.PullUp);
            var buttonState = true;
            var payloadState = "";

            using (var serial = new SerialDataHelper(SerialPorts.COM1, 9600, Parity.None, 8, StopBits.One))
            {
                //Whenever we receive a payload from the computer, save it to our variable
                serial.PayloadReceived += new PayloadReceivedEventHandler((src, e) =>
                {
                    payloadState = new string(Encoding.UTF8.GetChars(e.Payload));
                });

                //We are polling the input for now because we're already using the pin with interrupt support.
                while (true)
                {
                    //If the button reads low and the button state is high
                    if (!BUTTON_0.Read() && buttonState)
                    {
                        serial.SendPayload(Encoding.UTF8.GetBytes("BUTTON WAS PUSHED! Last message received: " + payloadState));
                    }
                    buttonState = BUTTON_0.Read();
                }
            }
        }
    }
}
