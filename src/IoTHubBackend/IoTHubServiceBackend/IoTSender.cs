using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTHubServiceBackend
{
    public delegate void DeviceMessageSentArgs(string DeviceId, string OriginalMessageId, DateTime EnqueuedTimeUtc, DateTime ExpiryTimeUtc, string Message);
    public delegate void DeviceMessageStatusReceivedArgs(string DeviceId, string OriginalMessageId, string StatusCode, DateTime EnqueuedTimeUtc);
    

    public class IoTSender
    {
        string connectionString = "";
        ServiceClient serviceClient;

        public event DeviceMessageSentArgs DeviceMessageSent;
        public event DeviceMessageStatusReceivedArgs DeviceMessageStatusReceived;

        public IoTSender(string ConnectionString)
        {
            connectionString = ConnectionString;
        }

        private void onDeviceMessageSent(string DeviceId, string OriginalMessageId, DateTime EnqueuedTimeUtc, DateTime ExpiryTimeUtc, string Message)
        {
            if (DeviceMessageSent != null)
                DeviceMessageSent(DeviceId, OriginalMessageId, EnqueuedTimeUtc, ExpiryTimeUtc, Message);
        }

        private void onDeviceMessageStatusReceived(string DeviceId, string OriginalMessageId, string StatusCode, DateTime EnqueuedTimeUtc)
        {
            if (DeviceMessageStatusReceived != null)
                DeviceMessageStatusReceived(DeviceId, OriginalMessageId, StatusCode, EnqueuedTimeUtc);
        }

        public void Initialize()
        {
            serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

            ReceiveFeedbackAsync();
        }

        public void Stop()
        {
            if (serviceClient != null)
            {
                serviceClient.CloseAsync();
                serviceClient = null;
            }
        }

        public async Task SendToDevice(string DeviceId, string Message, TimeSpan ExpiryTime)
        {
            if (serviceClient != null)
            {
                try
                {
                    var serviceMessage = new Message(Encoding.UTF8.GetBytes(Message));
                    serviceMessage.Ack = DeliveryAcknowledgement.Full;
                    serviceMessage.MessageId = Guid.NewGuid().ToString();

                    serviceMessage.ExpiryTimeUtc = DateTime.UtcNow.Add(ExpiryTime);

                    // Tell application that a message will be sent to device
                    onDeviceMessageSent(DeviceId, serviceMessage.MessageId, DateTime.UtcNow, serviceMessage.ExpiryTimeUtc, Message);

                    // Perform actual "send to device"
                    await serviceClient.SendAsync(DeviceId, serviceMessage);
                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                    Console.WriteLine(err);
                }
            }
        }

        private async void ReceiveFeedbackAsync()
        {
            var feedbackReceiver = serviceClient.GetFeedbackReceiver();
            while (true)
            {
                var feedbackBatch = await feedbackReceiver.ReceiveAsync();
                if (feedbackBatch == null) continue;

                List<FeedbackRecord> records = feedbackBatch.Records.ToList();
                foreach(var record in records)
                {
                    onDeviceMessageStatusReceived(record.DeviceId, record.OriginalMessageId, record.StatusCode.ToString(), record.EnqueuedTimeUtc);
                }

                await feedbackReceiver.CompleteAsync(feedbackBatch);
            }
        }
    }
}

