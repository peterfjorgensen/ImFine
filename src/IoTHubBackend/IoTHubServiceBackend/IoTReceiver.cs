using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;

namespace IoTHubServiceBackend
{
    public delegate void DeviceDataReceivedArgs(string DeviceId, long SequenceNumber, DateTime Timestamp, string Data);


    public class IoTReceiver
    {
        string connectionString = "";
        string iotHubD2cEndpoint = "messages/events";
        EventHubClient eventHubClient;

        public event DeviceDataReceivedArgs DeviceDataReceived;

        public IoTReceiver(string ConnectionString, string EndPoint)
        {
            connectionString = ConnectionString;
            iotHubD2cEndpoint = EndPoint;
        }

        private void onDeviceDataReceived(string DeviceId, long SequenceNumber, DateTime Timestamp, string Data)
        {
            if (DeviceDataReceived != null)
                DeviceDataReceived(DeviceId, SequenceNumber, Timestamp, Data);
        }

        public void Initialize(DateTime StartTime)
        {
            eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var partitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            foreach (string partition in partitions)
            {
                ReceiveMessagesFromDevices(partition, StartTime);
            }
        }

        public void Stop()
        {
            if (eventHubClient != null)
                eventHubClient = null;
        }


        private void ReceiveMessagesFromDevices(string Partition, DateTime StartTime)
        {
            Task.Run(() => {
                Task.WaitAll(ReceiveMessagesFromDevicesAsync(Partition, StartTime));
            });

        }

        private async Task ReceiveMessagesFromDevicesAsync(string Partition, DateTime StartTime)
        {
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(Partition, StartTime);

            while (true)
            {
                var eventData = await eventHubReceiver.ReceiveAsync(TimeSpan.FromMilliseconds(1000));

                if (eventData != null)
                {
                    // Pick data from the received Message
                    var deviceId = eventData.SystemProperties["iothub-connection-device-id"].ToString();
                    var sequenceNumber = eventData.SequenceNumber;
                    var enqueuedTime = eventData.EnqueuedTimeUtc;
                    var payload = Encoding.UTF8.GetString(eventData.GetBytes());

                    // Do something with the received data... Raise event
                    this.onDeviceDataReceived(deviceId, sequenceNumber, enqueuedTime, payload);
                }
            }
        }
    }
}
