using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;

namespace MSMQCheck
{
    static class MessageQueueSecurity
    {
        public static RawSecurityDescriptor GetQueueSecurity(string formatName)
        {
            int lengthNeeded = 0;

            uint result = NativeMethods.MQGetQueueSecurity(formatName, NativeMethods.OWNER_SECURITY_INFORMATION | NativeMethods.GROUP_SECURITY_INFORMATION | NativeMethods.DACL_SECURITY_INFORMATION, IntPtr.Zero, 0, ref lengthNeeded);

            if (result == NativeMethods.MQ_OK)
            {
                throw new Exception($"MQGetQueueSecurity call succeeded with zero-length buffer; something went wrong. Result: {result}");
            }
            if (result != NativeMethods.MQ_ERROR_SECURITY_DESCRIPTOR_TOO_SMALL)
            {
                throw new Exception($"MQGetQueueSecurity call failed. Result: {result}");
            }

            if (lengthNeeded <= 0)
            {
                throw new Exception($"MQGetQueueSecurity call returned zero or negative value {lengthNeeded} as the required buffer length. Result: {result}");
            }

            int length = lengthNeeded;
            byte[] descriptorBytes = new byte[length];
            GCHandle hDescriptor = GCHandle.Alloc(descriptorBytes, GCHandleType.Pinned);
            try
            {
                IntPtr ptrDescriptor = hDescriptor.AddrOfPinnedObject();
                result = NativeMethods.MQGetQueueSecurity(formatName, NativeMethods.OWNER_SECURITY_INFORMATION | NativeMethods.GROUP_SECURITY_INFORMATION | NativeMethods.DACL_SECURITY_INFORMATION, ptrDescriptor, length, ref lengthNeeded);
                if (result != NativeMethods.MQ_OK)
                {
                    throw new Exception($"MQGetQueueSecurity call failed. Result: {result}");
                }
            }
            finally
            {
                hDescriptor.Free();
            }
            
            var descriptor = new RawSecurityDescriptor(descriptorBytes, 0);
            return descriptor;
        }

        public static bool IsQueueAuthenticated(string formatName)
        {
            var properties = new QueuePropertyVariants();
            properties.SetUI1(NativeMethods.PROPID_Q_AUTHENTICATE, 0);
            int status = NativeMethods.MQGetQueueProperties(formatName, properties.Lock());
            properties.Unlock();
            if (MessageQueueHelpers.IsMQFatalError(status))
            {
                throw new Exception($"MQGetQueueProperties call failed. Status: {status} (0x{status:X8})");
            }
            bool auth = properties.GetUI1(NativeMethods.PROPID_Q_AUTHENTICATE) != 0;
            properties.Remove(NativeMethods.PROPID_Q_AUTHENTICATE);
            return auth;
        }

        public static QueuePrivacyLevel GetQueuePrivacyLevel(string formatName)
        {
            var properties = new QueuePropertyVariants();
            properties.SetUI4(NativeMethods.PROPID_Q_PRIV_LEVEL, 0);
            int status = NativeMethods.MQGetQueueProperties(formatName, properties.Lock());
            properties.Unlock();
            if (MessageQueueHelpers.IsMQFatalError(status))
            {
                throw new Exception($"MQGetQueueProperties call failed. Status: {status} (0x{status:X8})");
            }
            var privLevel = (QueuePrivacyLevel)properties.GetUI4(NativeMethods.PROPID_Q_PRIV_LEVEL);
            properties.Remove(NativeMethods.PROPID_Q_PRIV_LEVEL);
            return privLevel;
        }

        public static string GetMulticastAddress(string formatName)
        {
            var properties = new QueuePropertyVariants();
            properties.SetNull(NativeMethods.PROPID_Q_MULTICAST_ADDRESS);
            int status = NativeMethods.MQGetQueueProperties(formatName, properties.Lock());
            properties.Unlock();
            if (MessageQueueHelpers.IsMQFatalError(status))
            {
                throw new Exception($"MQGetQueueProperties call failed. Status: {status} (0x{status:X8})");
            }
            string multicastAddr = null;
            IntPtr ptrMulticast = properties.GetIntPtr(NativeMethods.PROPID_Q_MULTICAST_ADDRESS);
            if (ptrMulticast != IntPtr.Zero)
            {
                multicastAddr = Marshal.PtrToStringUni(ptrMulticast);
                NativeMethods.MQFreeMemory(ptrMulticast);
            }
            properties.Remove(NativeMethods.PROPID_Q_MULTICAST_ADDRESS);
            return multicastAddr;
        }
    }
}
