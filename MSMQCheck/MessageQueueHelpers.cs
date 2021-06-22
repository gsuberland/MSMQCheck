using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;

namespace MSMQCheck
{
    static class MessageQueueHelpers
    {
        public static string[] GetQueues()
        {
            string query = @"SELECT Name FROM Win32_PerfRawData_MSMQ_MSMQQueue";
            var searcher = new ManagementObjectSearcher(query);
            var results = searcher.Get();
            var queues = new List<string>();
            foreach (ManagementObject obj in results)
            {
                queues.Add(obj["Name"].ToString());
            }
            return queues.ToArray();
        }

        public static string TranslateQueueNameToFormat(string queueName)
        {
            if (queueName.StartsWith("."))
            {
                return @"DIRECT=OS:.\" + queueName;
            }
            if (queueName.StartsWith("tcp:") || queueName.StartsWith("os:") || queueName.StartsWith("http:") || queueName.StartsWith("https:"))
            {
                return "DIRECT=" + queueName;
            }
            string machinePrefix = Environment.MachineName + @"\";
            if (queueName.StartsWith(machinePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return @"DIRECT=OS:.\" + queueName.Substring(machinePrefix.Length);
            }
            return queueName;
        }

        public static bool IsMQFatalError(int value)
        {
            bool flag = value == 0;
            if ((value & -1073741824) == 1073741824)
            {
                return false;
            }
            return !flag;
        }
    }
}
