using System;
using System.Collections.Generic;
using System.Windows.Interop;

namespace Newgen.Base
{
    /// <summary>
    /// Provides methods to send string formatted messages to Newgen and widgets.
    /// </summary>
    public static class MessagingHelper
    {
        #region Common

        /// <summary>
        /// Message key for Newgen
        /// </summary>
        public const string NewgenKey = "Newgen.Message";

        /// <summary>
        /// Message key for Newgen widget
        /// </summary>
        public const string NewgenWidgetKey = "Newgen.WidgetMessage";

        private static List<IntPtr> listners = new List<IntPtr>();
        private static readonly SerializerHelper serializerHelper = new SerializerHelper();

        /// <summary>
        /// Message handler
        /// </summary>
        /// <param name="e">The <see cref="Newgen.Base.MessageEventArgs"/> instance containing the event data.</param>
        public delegate void MessageHandler(MessageEventArgs e);

        /// <summary>
        /// Occurs when [message received].
        /// </summary>
        public static event MessageHandler MessageReceived;

        #endregion Common

        #region Broadcasting

        /// <summary>
        /// Sends the message to newgen.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="message">The message.</param>
        public static void SendMessageToNewgen(string key, string message)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message cannot be null");
            }

            SendMessage(NewgenKey, key + ":" + message);
        }

        /// <summary>
        /// Sends the message to widget.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="message">The message.</param>
        public static void SendMessageToWidget(string name, string message)
        {
            if (name == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message cannot be null");
            }

            SendMessage(NewgenWidgetKey, name + ":" + message);
        }

        internal static void SendMessage(string key, object message)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message cannot be null");
            }

            SendMessage(key, serializerHelper.Serialize(message));
        }

        internal static void SendMessage(string key, string message)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key", "The key cannot be null");
            }
            if (message == null)
            {
                throw new ArgumentNullException("message", "The message packet cannot be null");
            }

            using (var dataGram = new WinMsgData(key, message))
            {
                // Allocate the DataGram to a memory address contained in COPYDATASTRUCT
                Native.COPYDATASTRUCT dataStruct = dataGram.ToStruct();
                // Use a filter with the EnumWindows class to get a list of windows containing
                // a property name that matches the destination channel. These are the listening
                // applications.

                foreach (var hWnd in listners)
                {
                    IntPtr outPtr;
                    // For each listening window, send the message data. Return if hang or unresponsive within 1 sec.
                    Native.SendMessageTimeout(hWnd, Native.WM_COPYDATA, IntPtr.Zero, ref dataStruct,
                                              Native.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 1000, out outPtr);
                }
            }
        }

        #endregion Broadcasting

        #region Listening

        /// <summary>
        /// Adds the listener.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        public static void AddListener(IntPtr hWnd)
        {
            try
            {
                HwndSource src = HwndSource.FromHwnd(hWnd);
                src.AddHook(new HwndSourceHook(WndProc));
                listners.Add(hWnd);
            }
            catch { }
        }

        /// <summary>
        /// Removes the listener.
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        public static void RemoveListener(IntPtr hWnd)
        {
            try
            {
                HwndSource src = HwndSource.FromHwnd(hWnd);
                src.RemoveHook(new HwndSourceHook(WndProc));
                listners.Remove(hWnd);
            }
            catch { }
        }

        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != Native.WM_COPYDATA)
            {
                return IntPtr.Zero;
            }

            using (var dataGram = WinMsgData.FromPointer(lParam))
            {
                if (MessageReceived != null && dataGram.IsValid)
                {
                    MessageReceived.Invoke(new MessageEventArgs(dataGram));
                }
            }
            return IntPtr.Zero;
        }

        #endregion Listening
    }
}