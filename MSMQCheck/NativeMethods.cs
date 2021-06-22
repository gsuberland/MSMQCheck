using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MSMQCheck
{
    static class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct tagMQQUEUEPROPS
        {
            public uint cProp;
            /*[MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
            public uint[] aPropID;*/
            public IntPtr aPropID;
            /*[MarshalAs(UnmanagedType.ByValArray, ArraySubType  = UnmanagedType.Struct, SizeConst = 1)]
            public tagMQPROPVARIANT[] aPropVar;*/
            public IntPtr aPropVar;
            public IntPtr aStatus;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct tagMQPROPVARIANT
        {
            public ushort vt;
            public ushort wReserved1;
            public ushort wReserved2;
            public ushort wReserved3;
            public byte bVal;

            public T GetValue<T>(IntPtr thisPtr)
            {
                if (!typeof(T).IsValueType)
                    throw new NotImplementedException();

                if (typeof(T) == typeof(Int16))
                {
                    return (T)(object)Marshal.ReadInt16(thisPtr, Marshal.OffsetOf(typeof(tagMQPROPVARIANT), nameof(bVal)).ToInt32());
                }
                if (typeof(T) == typeof(Int32))
                {
                    return (T)(object)Marshal.ReadInt32(thisPtr, Marshal.OffsetOf(typeof(tagMQPROPVARIANT), nameof(bVal)).ToInt32());
                }
                if (typeof(T) == typeof(Int64))
                {
                    return (T)(object)Marshal.ReadInt64(thisPtr, Marshal.OffsetOf(typeof(tagMQPROPVARIANT), nameof(bVal)).ToInt32());
                }

                throw new NotImplementedException();
            }
        }

        [Flags]
        public enum MQ_QUEUE_ACCESS_MASK : uint
        {
            DELETE_MESSAGE = 0x00000001,
            PEEK_MESSAGE = 0x00000002,
            WRITE_MESSAGE = 0x00000004,
            DELETE_JOURNAL_MESSAGE = 0x00000008,
            SET_QUEUE_PROPERTIES = 0x00000010,
            GET_QUEUE_PROPERTIES = 0x00000020,
            DELETE_QUEUE = 0x00010000,
            GET_QUEUE_PERMISSIONS = 0x00020000,
            CHANGE_QUEUE_PERMISSIONS = 0x00040000,
            TAKE_QUEUE_OWNERSHIP = 0x00080000,
            RECEIVE_MESSAGE = (DELETE_MESSAGE | PEEK_MESSAGE),
            RECEIVE_JOURNAL_MESSAGE = (DELETE_JOURNAL_MESSAGE | PEEK_MESSAGE),
            QUEUE_GENERIC_READ = (GET_QUEUE_PROPERTIES | GET_QUEUE_PERMISSIONS | RECEIVE_MESSAGE | RECEIVE_JOURNAL_MESSAGE),
            QUEUE_GENERIC_WRITE = (GET_QUEUE_PROPERTIES | GET_QUEUE_PERMISSIONS | WRITE_MESSAGE),
            QUEUE_GENERIC_ALL = (RECEIVE_MESSAGE | RECEIVE_JOURNAL_MESSAGE | WRITE_MESSAGE | SET_QUEUE_PROPERTIES | GET_QUEUE_PROPERTIES | DELETE_QUEUE | GET_QUEUE_PERMISSIONS | CHANGE_QUEUE_PERMISSIONS | TAKE_QUEUE_OWNERSHIP)

        }


        public const int OWNER_SECURITY_INFORMATION = 0x01;
        public const int GROUP_SECURITY_INFORMATION = 0x02;
        public const int DACL_SECURITY_INFORMATION = 0x04;
        public const int SACL_SECURITY_INFORMATION = 0x08;

        public const int MQ_OK = 0x0;

        public const int PROPID_Q_INSTANCE = 101;
        public const int PROPID_Q_TYPE = 102;
        public const int PROPID_Q_PATHNAME = 103;
        public const int PROPID_Q_JOURNAL = 104;
        public const int PROPID_Q_QUOTA = 105;
        public const int PROPID_Q_BASEPRIORITY = 106;
        public const int PROPID_Q_JOURNAL_QUOTA = 107;
        public const int PROPID_Q_LABEL = 108;
        public const int PROPID_Q_CREATE_TIME = 109;
        public const int PROPID_Q_MODIFY_TIME = 110;
        public const int PROPID_Q_AUTHENTICATE = 111;
        public const int PROPID_Q_PRIV_LEVEL = 112;
        public const int PROPID_Q_TRANSACTION = 113;
        public const int PROPID_Q_PATHNAME_DNS = 124;
        public const int PROPID_Q_MULTICAST_ADDRESS = 125;
        public const int PROPID_Q_ADS_PATH = 126;
        public const int MQ_TRANSACTIONAL_NONE = 0;
        public const int MQ_TRANSACTIONAL = 1;
        public const int MQ_AUTHENTICATE_NONE = 0;
        public const int MQ_AUTHENTICATE = 1;
        public const int MQ_PRIV_LEVEL_NONE = 0;
        public const int MQ_PRIV_LEVEL_OPTIONAL = 1;
        public const int MQ_PRIV_LEVEL_BODY = 2;

        public const uint MQ_ERROR_SECURITY_DESCRIPTOR_TOO_SMALL = 0xC00E0023;

        [DllImport("mqrt.dll", CharSet = CharSet.Unicode)]
        public static extern uint MQGetQueueSecurity(
            [MarshalAs(UnmanagedType.LPWStr)] string lpwcsFormatName,
            int SecurityInformation,
            IntPtr pSecurityDescriptor,
            int nLength,
            ref int lpnLengthNeeded
        );

        /*
        [DllImport("mqrt.dll", CharSet = CharSet.Unicode)]
        public static extern uint MQGetQueueProperties(
            [MarshalAs(UnmanagedType.LPWStr)] string lpwcsFormatName,
            ref tagMQQUEUEPROPS pQueueProps
        );*/
        [DllImport("mqrt.dll", CharSet = CharSet.Unicode)]
        public static extern int MQGetQueueProperties([MarshalAs(UnmanagedType.LPWStr)] string formatName, MessagePropertyVariants.MQPROPS queueProperties);

        [DllImport("mqrt.dll", CharSet = CharSet.Unicode)]
        public static extern void MQFreeMemory(IntPtr memory);
    }
}
