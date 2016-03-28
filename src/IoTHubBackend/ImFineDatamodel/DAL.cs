using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ImFineDatamodel
{
    public class DAL
    {

        public DAL()
        {

        }

        // Save DeviceMessage data sent from Cloud to Device
        public bool saveDeviceMessageSent(string DeviceId, string OriginalMessageId, DateTime EnqueuedTimeUtc, DateTime ExpiryTimeUtc, string Message)
        {
            using (ImFineEntities db = new ImFineEntities())
            {
                DeviceMessages dm = new DeviceMessages()
                {
                    DeviceAlias = DeviceId,
                    MessageId = OriginalMessageId,
                    Timestamp = EnqueuedTimeUtc,
                    ExpireTime = ExpiryTimeUtc,
                    Direction = "Outgoing",
                    Message = Message,
                    Status = "Waiting",
                    StatusTime = EnqueuedTimeUtc
                };

                try
                {
                    db.DeviceMessages.Add(dm);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    // Primitive error handling for test purposes only
                    string msg = ex.Message;
                    Console.WriteLine(msg);
                }
            }

            return true;
        }

        // Update a previously sent DeviceMessage from a status feedback from the IoTHub
        public bool saveDeviceMessageStatusReceived(string DeviceId, string OriginalMessageId, string StatusCode, DateTime EnqueuedTimeUtc)
        {
            using (ImFineEntities db = new ImFineEntities())
            {
                var qry = from message in db.DeviceMessages
                          where message.MessageId == OriginalMessageId
                          select message;

                if (qry != null && qry.Count() > 0)
                {
                    var dm = qry.First<DeviceMessages>();

                    dm.Status = StatusCode;
                    dm.StatusTime = EnqueuedTimeUtc;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        // Primitive error handling for test purposes only
                        string msg = ex.Message;
                        Console.WriteLine(msg);
                    }
                }
            }

            return true;
        }

        // Save DeviceMessage data received from a device
        public bool saveDeviceDataReceived(string DeviceId, long SequenceNumber, DateTime Timestamp, string Data)
        {
            using (ImFineEntities db = new ImFineEntities())
            {
                DeviceMessages dm = new DeviceMessages()
                {
                    DeviceAlias = DeviceId,
                    MessageId = SequenceNumber.ToString(),
                    Timestamp = Timestamp,
                    ExpireTime = Timestamp,
                    Direction = "Incoming",
                    Message = Data,
                    Status = "Received",
                    StatusTime = Timestamp
                };

                try
                {
                    db.DeviceMessages.Add(dm);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    // Primitive error handling for test purposes only
                    string msg = ex.Message;
                    Console.WriteLine(msg);
                }
            }
            return true;
        }
    }
}
