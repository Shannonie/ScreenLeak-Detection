using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;//DllImport

namespace ScreenSpec_INI
{  
    class classINI
    {
        [DllImport("kernel32.dll")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        static public string mainpath = "C:\\";
        static public string camera_VID = "04F2";
        static public string camera_PID = "B59C";
        static public string CamName = "Logitech Webcam C930e";

        static public int Mask = 8;
        static public int Threshold_Spec = 200;
        static public int Threshold_Ratio = 30;
        static public int Threshold_Brighthness = 10;
        static public int Lumabase_Min = 10;
        static public int sub_m = 4;
        static public int sub_n = 8;

        static public int LumaBaseUpperLimit = 150;
        static public double overBaseRatio = 90;

        static public bool SpecB_on_off = false;
        static public double GammaCorrect = 1.3;
        static public double GammaCorrectB = 1.5;
        static public double CircleContrastSpec = 1.11;
        static public double CircleContrastSpecB = 5;
        static public double ContrastSpecConstant = 1.06;
        static public double ContrastSpecConstantB = 3;

        static public int SN_length = 15;
        static public int factoryID = 886;
        static public bool DetailsLOG = false;
        static public bool ImagePASSLOG = false;
        static public bool Selftest = false;
        public bool SetINI()
        {
            string spc_path = System.Windows.Forms.Application.StartupPath + "\\ScreenSpec.ini";
            if (System.IO.File.Exists(spc_path))
            { 
                try
                {
                    StringBuilder temp = new StringBuilder(255);
                    GetPrivateProfileString("PATH", "Mainpath", "", temp, 255, spc_path);
                    mainpath = temp.ToString();

                    GetPrivateProfileString("CameraParameter", "VID", "", temp, 255, spc_path);
                    camera_VID = temp.ToString();
                    GetPrivateProfileString("CameraParameter", "PID", "", temp, 255, spc_path);
                    camera_PID = temp.ToString();
                    GetPrivateProfileString("CameraParameter", "CamName", "", temp, 255, spc_path);
                    CamName = temp.ToString();

                    GetPrivateProfileString("Parameter", "Block", "", temp, 255, spc_path);
                    Mask = int.Parse(temp.ToString());
                    GetPrivateProfileString("Parameter", "Threshold_Spec", "", temp, 255, spc_path);
                    Threshold_Spec = int.Parse(temp.ToString());
                    GetPrivateProfileString("Parameter", "Threshold_Ratio", "", temp, 255, spc_path);
                    Threshold_Ratio = int.Parse(temp.ToString());
                    GetPrivateProfileString("Parameter", "Threshold_Brighthness", "", temp, 255, spc_path);
                    Threshold_Brighthness = int.Parse(temp.ToString());
                    GetPrivateProfileString("Parameter", "Lumabase_Min", "", temp, 255, spc_path);
                    Lumabase_Min = int.Parse(temp.ToString());
                    GetPrivateProfileString("Parameter", "sub_m", "", temp, 255, spc_path);
                    sub_m = Convert.ToInt32(temp.ToString());
                    GetPrivateProfileString("Parameter", "sub_n", "", temp, 255, spc_path);
                    sub_n = Convert.ToInt32(temp.ToString());

                    GetPrivateProfileString("Parameter", "LumaBaseUpperLimit", "", temp, 255, spc_path);
                    LumaBaseUpperLimit = int.Parse(temp.ToString());
                    GetPrivateProfileString("Parameter", "overBaseRatio", "", temp, 255, spc_path);
                    overBaseRatio = int.Parse(temp.ToString());

                    GetPrivateProfileString("Parameter", "SpecB_on_off", "", temp, 255, spc_path);
                    if (Convert.ToInt32(temp.ToString()) == 1) { SpecB_on_off = true; } else { SpecB_on_off = false; }

                    GetPrivateProfileString("Parameter", "GammaCorrect", "", temp, 255, spc_path);
                    GammaCorrect = Convert.ToDouble(temp.ToString());
                    GetPrivateProfileString("Parameter", "GammaCorrectB", "", temp, 255, spc_path);
                    GammaCorrectB = Convert.ToDouble(temp.ToString());

                    GetPrivateProfileString("Parameter", "CircleContrastSpec", "", temp, 255, spc_path);
                    CircleContrastSpec = Convert.ToDouble(temp.ToString());
                    GetPrivateProfileString("Parameter", "CircleContrastSpecB", "", temp, 255, spc_path);
                    CircleContrastSpecB = Convert.ToDouble(temp.ToString());

                    GetPrivateProfileString("Parameter", "ContrastSpecConstant", "", temp, 255, spc_path);
                    ContrastSpecConstant = Convert.ToDouble(temp.ToString());
                    GetPrivateProfileString("Parameter", "ContrastSpecConstantB", "", temp, 255, spc_path);
                    ContrastSpecConstantB = Convert.ToDouble(temp.ToString());

                    GetPrivateProfileString("OtherSetting", "SN_length", "", temp, 255, spc_path);
                    SN_length = int.Parse(temp.ToString());//SN 長度
                    GetPrivateProfileString("OtherSetting", "factoryID", "", temp, 255, spc_path);
                    factoryID = int.Parse(temp.ToString());//factoryID
                    GetPrivateProfileString("OtherSetting", "DetailsLOG", "", temp, 255, spc_path);
                    if (Convert.ToInt32(temp.ToString()) == 1) { DetailsLOG = true; } else { DetailsLOG = false; }
                    GetPrivateProfileString("OtherSetting", "ImagePASSLOG", "", temp, 255, spc_path);
                    if (Convert.ToInt32(temp.ToString()) == 1) { ImagePASSLOG = true; } else { ImagePASSLOG = false; }
                    GetPrivateProfileString("OtherSetting", "Selftest", "", temp, 255, spc_path);
                    if (Convert.ToInt32(temp.ToString()) == 1) { Selftest = true; } else { Selftest = false; }
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to read INI file."); Console.ReadLine();
                    return false;
                }  
            }
            else
            {
                Console.WriteLine("Can not find INI file."); Console.ReadLine();
                return false;
            }
            return true;
        }
    }
}
