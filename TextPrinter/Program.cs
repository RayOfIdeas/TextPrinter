using System.Runtime.InteropServices;

namespace TextPrinter
{
    public class Program
    {
        static void Main(string[] args)
        {
            var text = "Hello, world !!!\nWow";
            CopyToClipboard(text);
            SendInputKey(VK_CONTROL, true);
            SendInputKey(VK_V, true);
            SendInputKey(VK_V, false);
            SendInputKey(VK_CONTROL, false);
            Console.ReadLine();
        }

        #region [INPUT]

        #region [Structs]

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        #endregion

        [DllImport("user32.dll")]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        static extern short VkKeyScan(char ch);

        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_SCANCODE = 0x0008;
        const int INPUT_KEYBOARD = 1;
        const int VK_SHIFT = 0x10;
        const int VK_CONTROL = 0x11;
        const int VK_V = 0x56;

        static void SendInputKeyboard(string text)
        {
            foreach (var character in text)
            {
                var vk = GetVirtualKeyCode(character, out var shiftRequired);
                if (shiftRequired)
                    SendInputKey(VK_SHIFT, true);

                SendInputKey(vk, true);
                SendInputKey(vk, false);

                if (shiftRequired)
                    SendInputKey(VK_SHIFT, false);
            }
        }

        static void SendInputKey(ushort vk, bool press)
        {
            var inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = vk;
            inputs[0].u.ki.wScan = 0;
            inputs[0].u.ki.dwFlags = press ? 0 : KEYEVENTF_KEYUP;
            inputs[0].u.ki.time = 0;
            inputs[0].u.ki.dwExtraInfo = IntPtr.Zero;
            SendInput(1, inputs, Marshal.SizeOf<INPUT>());
        }

        static ushort GetVirtualKeyCode(char ch, out bool shiftRequired)
        {
            short vk = VkKeyScan(ch);
            shiftRequired = false;

            if (vk == -1) // special characters or non-keyboard characters.
                return 0;

            ushort keyCode = (ushort)(vk & 0xFF); // Extract the virtual key code (low-order byte).
            if (((byte)(vk >> 8) & 0x01) != 0)
                shiftRequired = true;

            return keyCode;
        }

        #endregion

        #region [CLIPBOARD]

        [DllImport("user32.dll")]
        static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll")]
        static extern bool CloseClipboard();

        [DllImport("user32.dll")]
        static extern bool SetClipboardData(uint uFormat, IntPtr data);

        static void CopyToClipboard(string text)
        {
            OpenClipboard(IntPtr.Zero);
            var ptr = Marshal.StringToHGlobalUni(text);
            SetClipboardData(13, ptr);
            CloseClipboard();
            Marshal.FreeHGlobal(ptr);
        }

        #endregion
    }
}
