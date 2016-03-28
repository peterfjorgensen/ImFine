using System;
using System.Threading;
using IoTHubServiceBackend;
using System.Threading.Tasks;
using ImFineDatamodel;

namespace ImFine
{
    class Program
    {
        static string serviceConnectionString = "HostName=ImFineDev.azure-devices.net;SharedAccessKeyName=service;SharedAccessKey=hILqO6rCWzv+x9e7cpx/oUkOSs6I94Ciugyd1yzWzEQ=";
        static string serviceEndpoint = "messages/events";

        static IoTReceiver receiver;
        static IoTSender sender;

        static bool sendEmailResponse = false;

        static DAL dal;

        static void Main(string[] args)
        {
            Console.WriteLine("ImFine Backend Console\n");

            dal = new DAL();

            DateTime start = DateTime.UtcNow; // Discard older messages received before starting this app! - To change later...
            receiver = new IoTReceiver(serviceConnectionString, serviceEndpoint);
            receiver.DeviceDataReceived += Receiver_DeviceDataReceived;
            receiver.Initialize(start);

            Console.WriteLine("Receive started");

            sender = new IoTSender(serviceConnectionString);
            sender.DeviceMessageSent += Sender_DeviceMessageSent;
            sender.DeviceMessageStatusReceived += Sender_DeviceMessageStatusReceived;
            sender.Initialize();

            Console.ReadLine();

            sender.Stop();
            receiver.Stop();
        }

        #region Receive data from devices
        private static void Receiver_DeviceDataReceived(string DeviceId, long SequenceNumber, DateTime Timestamp, string Data)
        {
            // Debug: Write data to console...
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(string.Format("Incoming   {0};{1};{2};{3}", DeviceId, SequenceNumber, Timestamp, Data));
            Console.ResetColor();

            // Save data to data store...
            // TODO
            dal.saveDeviceDataReceived(DeviceId, SequenceNumber, Timestamp, Data);


            // Handle acknowledge messages...
            if (Data.StartsWith("Button"))
            {
                SendMessageToDevice(DeviceId, String.Format("ACK;{0};Button Acknowledge", SequenceNumber));

                if (sendEmailResponse)
                {
                    // Send Email as a test...
                    //string content = String.Format("{0} has not reported back within set time. Please give {1} a call and check up on {1}!", DeviceId, "her");
                    string content = String.Format("'{0}' has just checked in. All is Fine.!", DeviceId);
                    string subject = String.Format("'{0}' is fine. {1} just checked in.", DeviceId, "She");
                    SendEmail("peterfjorgensen@msn.com", subject, content);
                }
            }
        }
        #endregion

        #region Send and monitor sent data
        private static void SendMessageToDevice(String DeviceId, string Message)
        {
            Task.Run(() => {
                Task.WaitAll(sender.SendToDevice(DeviceId, Message, TimeSpan.FromMinutes(2)));
            });
        }

        private static void Sender_DeviceMessageSent(string DeviceId, string OriginalMessageId, DateTime EnqueuedTimeUtc, DateTime ExpiryTimeUtc, string Message)
        {
            // Save to data store...
            // Set status to WAITING
            // TODO
            dal.saveDeviceMessageSent(DeviceId, OriginalMessageId, EnqueuedTimeUtc, ExpiryTimeUtc, Message);
            

            // Debug print to console...
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Outgoing   {0};{1};{2};{3};{4}", DeviceId, OriginalMessageId, EnqueuedTimeUtc, ExpiryTimeUtc, Message);
            Console.ResetColor();
        }

        private static void Sender_DeviceMessageStatusReceived(string DeviceId, string OriginalMessageId, string StatusCode, DateTime EnqueuedTimeUtc)
        {
            // Find and update device data in the data store...
            // Set status according to received status and time
            // TODO
            dal.saveDeviceMessageStatusReceived(DeviceId, OriginalMessageId, StatusCode, EnqueuedTimeUtc);

            // Debug print to console...
            switch (StatusCode)
            {
                case "Success":
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;

                case "Rejected":
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
            }
            Console.WriteLine("Feedback   {0};{1};{2};{3}", DeviceId, OriginalMessageId, StatusCode, EnqueuedTimeUtc);
            Console.ResetColor();
        }
        #endregion

        #region Debugging and test
        static Timer timer = new Timer(timerHandler, null, Timeout.Infinite, Timeout.Infinite);
        static int counter = 199;

        private static void StartTestSendMessageToDevices()
        {
            timer.Change(0, 5000);
        }

        private static void timerHandler(object state)
        {
            counter++;
            try
            {
                SendMessageToDevice("Grandma", "CMD;" + counter.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in 'timerHandler'!");
            }
        }
        #endregion


        #region Email
        private static void SendEmail(string Receiver, string Subject, string Message)
        {
            string host = "smtp.gmail.com";
            int port = 587;
            string user = "imfinedev@gmail.com";
            string password = "1Q2w3E4r";
            string from = "ImFineDev@gmail.com";

            EmailController emailController = new EmailController(host, port, user, password, from);
            emailController.UseSSL = true;

            string res1 = emailController.SendEmail(Receiver, Subject, Message);
        }
        #endregion
    }
}
