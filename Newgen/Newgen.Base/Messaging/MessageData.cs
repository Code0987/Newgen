using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;

namespace Newgen.Base
{
    public class MessageData
    {
        private readonly string key;

        private readonly string message;

        public MessageData(string key, string message)
        {
            this.key = key;
            this.message = message;
        }

        internal MessageData()
        {
        }

        public string MessageKey
        {
            get { return key; }
        }

        public string Message
        {
            get { return message; }
        }

        internal bool IsValid
        {
            get { return !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(Message); }
        }

        public override string ToString()
        {
            return string.Concat(key, ":", Message);
        }

        internal static MessageData ExpandFromRaw(string rawmessage)
        {
            // if the message contains valid data
            if (!string.IsNullOrEmpty(rawmessage) && rawmessage.Contains(":"))
            {
                // extract the key name and message data
                string[] parts = rawmessage.Split(new[] { ':' }, 2);
                return new MessageData(parts[0], parts[1]);
            }
            return new MessageData();
        }
    }

    internal class WinMsgData : IDisposable
    {
        private readonly MessageData dataGram;

        private bool allocatedMemory;

        private Native.COPYDATASTRUCT dataStruct;

        public WinMsgData(string key, string message)
        {
            allocatedMemory = false;
            dataStruct = new Native.COPYDATASTRUCT();
            dataGram = new MessageData(key, message);
        }

        private WinMsgData(IntPtr lpParam)
        {
            allocatedMemory = false;
            dataStruct = (Native.COPYDATASTRUCT)Marshal.PtrToStructure(lpParam, typeof(Native.COPYDATASTRUCT));
            var bytes = new byte[dataStruct.cbData];
            Marshal.Copy(dataStruct.lpData, bytes, 0, dataStruct.cbData);
            string rawmessage;
            using (var stream = new MemoryStream(bytes))
            {
                var b = new BinaryFormatter();
                rawmessage = (string)b.Deserialize(stream);
            }
            // use helper method to expand the raw message
            dataGram = MessageData.ExpandFromRaw(rawmessage);
        }

        public string Key
        {
            get { return dataGram.MessageKey; }
        }

        public string Message
        {
            get { return dataGram.Message; }
        }

        internal bool IsValid
        {
            get { return dataGram.IsValid; }
        }

        public static implicit operator MessageData(WinMsgData dataGram)
        {
            return dataGram.dataGram;
        }

        public override string ToString()
        {
            return dataGram.ToString();
        }

        public void Dispose()
        {
            // clean up unmanaged resources
            if (dataStruct.lpData != IntPtr.Zero)
            {
                // only free memory if this instance created it (broadcast instance)
                // don't free if we are just reading shared memory
                if (allocatedMemory)
                {
                    Marshal.FreeCoTaskMem(dataStruct.lpData);
                }
                dataStruct.lpData = IntPtr.Zero;
                dataStruct.dwData = IntPtr.Zero;
                dataStruct.cbData = 0;
            }
        }

        internal static WinMsgData FromPointer(IntPtr lpParam)
        {
            return new WinMsgData(lpParam);
        }

        internal Native.COPYDATASTRUCT ToStruct()
        {
            string raw = dataGram.ToString();

            byte[] bytes;

            // serialize data into stream
            var b = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                b.Serialize(stream, raw);
                stream.Flush();
                var dataSize = (int)stream.Length;

                // create byte array and get pointer to mem location
                bytes = new byte[dataSize];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytes, 0, dataSize);
            }
            IntPtr ptrData = Marshal.AllocCoTaskMem(bytes.Length);
            // flag that this instance dispose method needs to clean up the memory
            allocatedMemory = true;
            Marshal.Copy(bytes, 0, ptrData, bytes.Length);

            dataStruct.cbData = bytes.Length;
            dataStruct.dwData = IntPtr.Zero;
            dataStruct.lpData = ptrData;

            return dataStruct;
        }
    }
}