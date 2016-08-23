using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using NetduinoSerialSDK;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Connecting to the Netduino.....");
                using (var sdk = new NetduinoSerialSDK.NetduinoSerialSDK())
                {
                    Console.WriteLine("Connected!");
                    sdk.PayloadReceived += new PayloadReceivedEventHandler(PayloadReceivedConsoleHandler);
                    while (true)
                    {
                        Console.WriteLine("Please type a message you would like to send to the Netduino.");
                        var line = Console.ReadLine();
                        var bytes = Encoding.UTF8.GetBytes(line);
                        sdk.SendPayload(bytes);
                    }
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine("Unable to connect to the Netduino. Operation timed out. Bye!");
                Thread.Sleep(5000);
            }
        }

        public static void PayloadReceivedConsoleHandler(object sender, PayloadReceivedEventArgs e)
        {
            Console.WriteLine(new string(Encoding.UTF8.GetChars(e.Payload)));
        }

    }
}
