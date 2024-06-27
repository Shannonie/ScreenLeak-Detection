using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;//DllImport

namespace ScLeakageCmd
{
    public partial class Form1 : Form
    {
        const uint FILE_DEVICE_VIDEO = 0x00000023;
        const uint FILE_ANY_ACCESS = 0;
        const uint METHOD_BUFFERED = 0;

        static uint IOCTL_VIDEO_QUERY_SUPPORTED_BRIGHTNESS =
                            CTL_CODE(FILE_DEVICE_VIDEO, 293, METHOD_BUFFERED, FILE_ANY_ACCESS);
        static uint IOCTL_VIDEO_SET_DISPLAY_BRIGHTNESS =
                            CTL_CODE(FILE_DEVICE_VIDEO, 295, METHOD_BUFFERED, FILE_ANY_ACCESS);
        static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
        { return ((DeviceType) << 16) | ((Access) << 14) | ((Function) << 2) | (Method); }
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(
                                    IntPtr hDevice,
                                    uint dwIoControlCode,
                                    byte[] lpInBuffer,
                                    uint nInBufferSize,
                                    [Out] byte[] lpOutBuffer,
                                    uint nOutBufferSize,
                                    out uint lpBytesReturned,
                                    IntPtr lpOverlapped
                                );
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateFile(
                                           [MarshalAs(UnmanagedType.LPStr)]
                                            string strName,
                                            uint nAccess,
                                            uint nShareMode,
                                            IntPtr lpSecurity,
                                            uint nCreationFlags,
                                            uint nAttributes,
                                            IntPtr lpTemplate
                                        );
        private IntPtr OpenLCDDevice()
        {
            IntPtr hDevice;
            hDevice = CreateFile(
                            "\\\\.\\LCD",                  // open LCD device   @\\.\LCD or \\\\.\LCD                      
                            0x80000000 | 0x40000000,       // no access to the drive
                                                           // FILE_SHARE_READ | FILE_SHARE_WRITE,     // share mode
                            0,
                            IntPtr.Zero,                   // default security attributes
                            3,                             // disposition
                            0,                                      // file attributes                         
                            IntPtr.Zero
                        );
            return hDevice;
        }
        private bool QueryDisplayState(IntPtr hDevice, out byte[] StateBuffer, out uint lpByte)
        {
            bool Ret;
            lpByte = 0;
            StateBuffer = new byte[512];
            Ret = DeviceIoControl(hDevice, IOCTL_VIDEO_QUERY_SUPPORTED_BRIGHTNESS, null, 0, StateBuffer, (uint)sizeof(byte) * 512, out lpByte, IntPtr.Zero);

            return Ret;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct DISPLAY_BRIGHTNESS
        {
            public byte DisplayPolicy;
            public byte ACBRightness;
            public byte DCBrightness;
        }
        private bool SetDisplayBrightness(IntPtr hDevice, DISPLAY_BRIGHTNESS DB)
        {
            byte[] InputBuffer = new byte[3];
            bool Ret;
            uint lpByte;

            InputBuffer[0] = DB.DisplayPolicy;
            InputBuffer[1] = DB.ACBRightness;
            InputBuffer[2] = DB.DCBrightness;

            Ret = DeviceIoControl(hDevice, IOCTL_VIDEO_SET_DISPLAY_BRIGHTNESS, InputBuffer, (uint)sizeof(byte) * 3, null, 0, out lpByte, IntPtr.Zero);

            return Ret;
        }
        IntPtr hDevice;
        byte[] BacklightIndex;
        DISPLAY_BRIGHTNESS NowBrightness;
        uint lpByte;
        

        private void Form1_Load(object sender, EventArgs e)
        {
            Cursor.Hide();
            hDevice = OpenLCDDevice();
            QueryDisplayState(hDevice, out BacklightIndex, out lpByte);
            DISPLAY_BRIGHTNESS DB = new DISPLAY_BRIGHTNESS();
            DB.DisplayPolicy = NowBrightness.DisplayPolicy;
            DB.ACBRightness = BacklightIndex[BacklightIndex.Max()];
            DB.DCBrightness = BacklightIndex[BacklightIndex.Max()];
            SetDisplayBrightness(hDevice, DB);
        }
        private void Form1_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
