using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using System.Runtime.InteropServices; //DllImport
using DirectShowLib;

using System.Management;
using ScreenSpec_INI;
using LOG_space;
using NS_baseCalculation;

namespace ScLeakageCmd
{
    class Program
    {
        //PMS variable
        static string PMSlog, errorCode;
        static string projectName;
        //static int factoryID;
        static int LCD_LEAKAGE_method = -1;
        static double LCD_LEAKAGE_Exposure = 88.88;
        static int LCD_LEAKAGE_MAX = 0; //byte lumaMax
        static int LCD_LEAKAGE_MAX_SPEC = 0;
        static int LCD_LEAKAGE_ScRange_HEIGHT = 0;
        static int LCD_LEAKAGE_ScRange_WIDTH = 0;
        static int LCD_LEAKAGE_MAXPOSITION_X = 0;
        static int LCD_LEAKAGE_MAXPOSITION_Y = 0;
        static double SPEC_Contrast_A = 0;
        static double LCD_LEAKAGE_Contrast_A = 0;
        static int LCD_LEAKAGE_POSITION_X_A = 0;
        static int LCD_LEAKAGE_POSITION_Y_A = 0;
        static double LCD_LEAKAGE_CIRCLE_A = 0;
        static double LCD_LEAKAGE_BIGCIRCLE_A = 0;
        static double SPEC_Contrast_B = 0;
        static double LCD_LEAKAGE_Contrast_B = 0;
        static int LCD_LEAKAGE_POSITION_X_B = 0;
        static int LCD_LEAKAGE_POSITION_Y_B = 0;
        static double LCD_LEAKAGE_CIRCLE_B = 0;
        static double LCD_LEAKAGE_Contrast_P = 0;
        static int LCD_LEAKAGE_POSITION_X_P = 0;
        static int LCD_LEAKAGE_POSITION_Y_P = 0;
        static double LCD_LEAKAGE_CIRCLE_P = 0;
        static double LCD_LEAKAGE_BIGCIRCLE_B = 0;
        static int LCD_LEAKAGE_DIFFENECE = 0;
        static string LCD_LEAKAGE_TIME = "0";
        //PMS variable
        //bool CircleContrastAnalysisPASS = true;
        //static DenseHistogram histogram;
        static double luma_base = 0;
        static string SSN = "";
        static string resultLog = "";
        //static string mainpath = "";
        static string LogDateTime = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        static Bitmap BitDrawLine, DrawCircle, DrawCircleA, DrawCircleB, DrawLine2, DrawLine3;
        //static ScreenSpecINI readINI = new ScreenSpecINI();
        private delegate void FlushClient(); //thread
                                             //static string help = "Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n";

        static int Main(string[] args)
        {
            /**/args[0] = "/T";
            args[1] = "100.jpg";
            args[2] = "/A"; args[3] = "-SSN[L1NTKD000000039]";

            bool NotonlyT = true, cmdhaveProject = false;
            if (args.Count() == 0) { Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine(); return -1; }
            if (args[0] == "/T")
            {
                LCD_LEAKAGE_method = 0;
                switch (args.Count())
                {
                    case 2:
                        if (!args[1].Contains(".jpg") && !args[1].Contains(".JPG")) { Console.WriteLine("Only support JPG."); Console.ReadLine(); return -1; }
                        NotonlyT = false;
                        break;
                    case 4:
                        if (!args[1].Contains(".jpg") && !args[1].Contains(".JPG")) { Console.WriteLine("Only support JPG."); Console.ReadLine(); return -1; }
                        if (args[2] != "/A") { Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine(); return -1; }
                        if (!args[3].Contains("-SSN[")) { Console.WriteLine("Can not find '-SSN'."); Console.ReadLine(); return -1; }
                        if (args[3].Substring(0, 5) != "-SSN[") { Console.WriteLine("Can not find '-SSN['."); Console.ReadLine(); return -1; } 
                        if (!args[3].Contains("]")) { Console.WriteLine("Can not find SSN ']'."); Console.ReadLine(); return -1; }
                        break;
                    case 5:
                        if (!args[1].Contains(".jpg") && !args[1].Contains(".JPG"))
                        {
                            Console.WriteLine("Only support JPG.");
                            Console.ReadLine();
                            return -1;
                        }
                        if (args[2] != "/A") { Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine(); return -1; }
                        if (!args[3].Contains("-SSN[")) { Console.WriteLine("Can not find '-SSN'."); Console.ReadLine(); return -1; }         
                        if (args[3].Substring(0, 5) != "-SSN[") { Console.WriteLine("Can not find '-SSN['."); Console.ReadLine(); return -1; } 
                        if (!args[3].Contains("]")) { Console.WriteLine("Can not find SSN ']'."); Console.ReadLine(); return -1; }
                        cmdhaveProject = true;
                        if (!args[4].Contains("-PROJECT[")) { Console.WriteLine("Can not find '-PROJECT['."); Console.ReadLine(); return -1; }
                        if (args[4].Substring(0, 9) != "-PROJECT[") { Console.WriteLine("Can not find '-PROJECT['."); Console.ReadLine(); return -1; } 
                        if (!args[4].Contains("]")) { Console.WriteLine("Can not find PROJECT ']'."); Console.ReadLine(); return -1; }
                        break;
                    default:
                        Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine();
                        return -1;
                }
            } 
            else if (args[0] == "/R")
            {
                LCD_LEAKAGE_method = 1;
                switch (args.Count())
                {
                    case 4:
                        if (!args[1].Contains(".jpg") && !args[1].Contains(".JPG")) { Console.WriteLine("Only support JPG."); Console.ReadLine(); return -1; }
                        if (!File.Exists(args[1])) { Console.WriteLine("The Image Path is error."); Console.ReadLine(); return -1; }
                        if (args[2] != "/A") { Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine(); return -1; }
                        if (!args[3].Contains("-SSN[")) { Console.WriteLine("Can not find '-SSN['."); Console.ReadLine(); return -1; }
                        if (args[3].Substring(0, 5) != "-SSN[") { Console.WriteLine("Can not find '-SSN['."); Console.ReadLine(); return -1; }
                        if (!args[3].Contains("]")) { Console.WriteLine("Can not find SSN ']'."); Console.ReadLine(); return -1; }
                        break;
                    case 5:
                        if (!args[1].Contains(".jpg") && !args[1].Contains(".JPG")) { Console.WriteLine("Only support JPG."); Console.ReadLine(); return -1; }
                        if (!File.Exists(args[1])) { Console.WriteLine("The Image Path is error."); Console.ReadLine(); return -1; }
                        if (args[2] != "/A") { Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine(); return -1; }
                        if (!args[3].Contains("-SSN[")) { Console.WriteLine("Can not find '-SSN['."); Console.ReadLine(); return -1; }
                        if (args[3].Substring(0, 5) != "-SSN[") { Console.WriteLine("Can not find '-SSN['."); Console.ReadLine(); return -1; }
                        if (!args[3].Contains("]")) { Console.WriteLine("Can not find SSN ']'."); Console.ReadLine(); return -1; }
                        cmdhaveProject = true;
                        if (!args[4].Contains("-PROJECT[")) { Console.WriteLine("Can not find '-PROJECT['."); Console.ReadLine(); return -1; }
                        if (args[4].Substring(0, 9) != "-PROJECT[") { Console.WriteLine("Can not find '-PROJECT['."); Console.ReadLine(); return -1; } 
                        if (!args[4].Contains("]")) { Console.WriteLine("Can not find PROJECT ']'."); Console.ReadLine(); return -1; }
                        break;
                    default:
                        Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine();
                        return -1;
                }
            }
            else { Console.WriteLine("Please input the correct command-line Argument/parameters.\nex.\n/T C:/123.jpg /A -SSN[XXX]\n"); Console.ReadLine(); return -1; }

            string[] dirs = Directory.GetFiles(Application.StartupPath, "*.log");
            foreach (string dir in dirs)
            {
                try { File.Delete(dir);}
                catch (Exception) { }
            }

            int index = 0, index_end = 0;
            string cmd_project = "";
            if (cmdhaveProject)
            {
                index = args[4].IndexOf("[");
                index_end = args[4].IndexOf("]");
                cmd_project = args[4].Substring(index + 1, index_end - index - 1);
                projectName = cmd_project;
            }
            if (NotonlyT)
            {
                index = args[3].IndexOf("[");
                index_end = args[3].IndexOf("]");
                SSN = args[3].Substring(index + 1, index_end - index - 1);
            }

            classINI programINI = new classINI();
            if (!programINI.SetINI()) { return -1; }
            LCD_LEAKAGE_MAX_SPEC = classINI.Threshold_Spec;

            LOG log = new LOG();
            if (NotonlyT)
            {
                string Product = "";
                string SerialNumberBIOS = "";

                if (classINI.Selftest) //Test self
                {
                    FlushClient fc = new FlushClient(ThreadForm); fc.BeginInvoke(null, null);
                    ManagementClass Win32_BaseBoard = new ManagementClass("Win32_BaseBoard");
                    ManagementObjectCollection instances_Win32_BaseBoard = Win32_BaseBoard.GetInstances();
                    foreach (ManagementBaseObject instance in instances_Win32_BaseBoard)
                    {
                        Product = instance.Properties["Product"].Value.ToString();
                    }
                    ManagementClass Win32_BIOS = new ManagementClass("Win32_BIOS");
                    ManagementObjectCollection instances_Win32_BIOS = Win32_BIOS.GetInstances();
                    foreach (ManagementBaseObject instance in instances_Win32_BIOS)
                    {
                        SerialNumberBIOS = instance.Properties["SerialNumber"].Value.ToString();
                    }
                    SerialNumberBIOS = SerialNumberBIOS.Replace(" ", "");
                    projectName = Product;
                    SSN = SerialNumberBIOS;
                }
                if (!CreateLog()) { Console.WriteLine("Failed to create Log file."); Console.ReadLine(); return -1; }
                if (classINI.Selftest) { log.OutLog("The value read by the system for PMS log. Project:" + Product + ", SSN:" + SerialNumberBIOS); }
                else
                {
                    log.OutLog("It's not DUT self test.");
                    if (SSN.Length < classINI.SN_length)
                    {
                        Console.WriteLine("It's not DUT self test. SSN length is not enough " + classINI.SN_length + "."); Console.ReadLine();
                        log.OutLog("SSN length is not enough " + classINI.SN_length + ".");
                        Finaladdress(false, null, null, null, null, null, null);
                        return -1;
                    }
                }
                string log_para = "";
                log_para = "Mask:" + classINI.Mask + ", MAX Spec:" + classINI.Threshold_Spec + ", Ratio:" + classINI.Threshold_Ratio + ", Brighthness:" + classINI.Threshold_Brighthness + ", Lumabase_Min:" + classINI.Lumabase_Min;
                log.OutLog(log_para);
                log_para = "Luma Base Upper Limit:" + classINI.LumaBaseUpperLimit + ", Over Base Ratio:" + classINI.overBaseRatio + ", SpecB_on_off:" + classINI.SpecB_on_off;
                log.OutLog(log_para);
                log_para = "Circle Contrast Spec :" + classINI.CircleContrastSpec + ", Contrast Spec Constant :" + classINI.ContrastSpecConstant;
                log.OutLog(log_para);
                log_para = "Circle Contrast Spec B:" + classINI.CircleContrastSpecB + ", Contrast Spec Constant B:" + classINI.ContrastSpecConstantB;
                log.OutLog(log_para);
                log_para = "SN_length:" + classINI.SN_length + ", factoryID:" + classINI.factoryID + ", DetailsLOG:" + classINI.DetailsLOG + ", ImagePASSLOG:" + classINI.ImagePASSLOG + ", Selftest:" + classINI.Selftest;
                log.OutLog(log_para);

            }
            System.Diagnostics.Stopwatch swt = new System.Diagnostics.Stopwatch();
            swt.Reset();
            swt.Start();

            if (args[0] == "/T")
            {
                if (!TurnOnOffCamera(classINI.CamName, args[1]))
                {
                    log.OutLog("Program close."); Finaladdress(false, null, null, null, null, null, null); return -1;
                }
                else
                {
                    log.OutLog("Take picture: " + swt.Elapsed.TotalMilliseconds.ToString());
                    if (classINI.Selftest && NotonlyT)
                    {
                        Form1.ActiveForm.Close();
                        /*System.Media.SoundPlayer player = new System.Media.SoundPlayer();
                        player.SoundLocation = Application.StartupPath + "\\2.wav";
                        System.Threading.Thread.Sleep(10); player.PlayLooping();*/
                    }
                }
            }
            if (args.Count() == 2)
            {
                return 0;
            }//Just take pictures         

            string imgName = args[1];

            /*=========== image pre-processing ===========*/
            Image<Bgr, byte> colorImage = new Image<Bgr, byte>(imgName);
            Image<Gray, byte> grayImage = new Image<Gray, byte>(imgName);
            Image<Gray, byte> thresholdImg0 = new Image<Gray, byte>(imgName);

            CL_baseCalculation CL_baseCalculation = new CL_baseCalculation();
            CL_baseCalculation.base_Calculation(grayImage, out LCD_LEAKAGE_MAX, out LCD_LEAKAGE_MAXPOSITION_X, out LCD_LEAKAGE_MAXPOSITION_Y);
            luma_base = NS_baseCalculation.CL_baseCalculation.luma_base;
            Image<Gray, byte> originYcorp = CL_baseCalculation.originYcorp; //screen image        
            Image<Gray, byte> originbackupA = CL_baseCalculation.originbackupA; //screen image
            Image<Gray, byte> originbackupB = CL_baseCalculation.originbackupB; //screen image
            Image<Gray, byte> img_corp = CL_baseCalculation.img_corp; //screen image
            Image<Gray, byte> APIAverageImg = CL_baseCalculation.APIAverageImg; //blurred input image
            Bitmap bit_corp = CL_baseCalculation.bit_corp; //result show screen image

            /*=========== fail:luma base threshold ===========*/
            if (luma_base < (CL_baseCalculation.PixelAverage) / 2)
            {
                log.OutLog("luma_base < " + ((CL_baseCalculation.PixelAverage) / 2).ToString("f2"));
                Finaladdress(false, bit_corp, null, colorImage, "ScRange", null, "Origin");
                colorImage.Dispose();
                grayImage.Dispose();
                WritePMSlog();
                return -1;
            }

            if (luma_base > classINI.LumaBaseUpperLimit)
            {
                log.OutLog("luma_base > " + classINI.LumaBaseUpperLimit);
                Finaladdress(false, bit_corp, null, colorImage, "ScRange", null, "Origin");
                colorImage.Dispose();
                grayImage.Dispose();
                WritePMSlog();
                return -1;
            }

            /*=========== fail:screen captured threshold / luma max ===========*/
            //CreateOutLog("run times : " + swt.Elapsed.TotalMilliseconds.ToString(), null);
            bool TestResult = true;
            double yh = originYcorp.Height, yw = originYcorp.Width;
            double oWH = (yw / yh);
            log.OutLog("ScRange Height = " + originYcorp.Height + ", ScRange Width = " + originYcorp.Width + ", Width/Height = " + oWH.ToString("f2"));
            LCD_LEAKAGE_ScRange_HEIGHT = originYcorp.Height;
            LCD_LEAKAGE_ScRange_WIDTH = originYcorp.Width;

            if (originYcorp.Height > 480 && originYcorp.Width > 800 && oWH > 1.6 && oWH < 2)
            {
                if (LCD_LEAKAGE_MAX > classINI.Threshold_Spec)
                {
                    log.OutLog("Over the MAX spec " + classINI.Threshold_Spec);
                    TestResult = false;
                }
            }
            else
            {
                log.OutLog("Screen Range fail");
                TestResult = false;
            }

            /*=========== luma analyze ===========*/
            if (TestResult)
            {
                //### fail: uniformity 
                if (!unifomityAnalysis(img_corp, classINI.sub_m, classINI.sub_n))
                {
                    log.OutLog("Luma analysis error.");
                    Finaladdress(false, bit_corp, null, colorImage, "ScRange", null, "Origin");
                    TestResult = false;
                }

                if (TestResult)
                {                    
                    double gm;
                    originbackupA._GammaCorrect(classINI.GammaCorrect);
                    originbackupB._GammaCorrect(classINI.GammaCorrectB);
                    string specAB;
                    bool changeGamma_bool = classINI.SpecB_on_off;

                changeGamma: //### gamma correction
                    if (changeGamma_bool)
                    {
                        originYcorp = originbackupB;
                        gm = classINI.GammaCorrectB;
                        specAB = "GammaCorrectB:" + classINI.GammaCorrectB + ", ";
                    }
                    else
                    {
                        originYcorp = originbackupA;
                        gm = classINI.GammaCorrect;
                        specAB = "GammaCorrect:" + classINI.GammaCorrect + ", ";
                    }
                    
                    #region Matrix Kernel
                    float[,] matrixKernel = new float[5, 5]
                    {{ 0, 0,-1, 0, 0 },
                    { 0, 1,-1, 1, 0 },
                    {-1,-1, 5,-1,-1 },
                    { 0, 1,-1, 1, 0 },
                    { 0, 0,-1, 0, 0 }, };
                    //matrixKernel = new float[3, 3] {{ 0,-1, 0},{-1, 5,-1},{ 0,-1, 0} };
                    ConvolutionKernelF matrix = new ConvolutionKernelF(matrixKernel);
                    Image<Gray, float> result = originYcorp.Convolution(matrix); //銳利化 
                    result = result.SmoothGaussian(3); //去噪
                    Image<Gray, byte> Yresult = result.ConvertScale<byte>(1, 0);                   
                    originYcorp = Yresult;
                    #endregion

                    #region mini
                    Image<Gray, byte> miniOriginYcorp = new Image<Gray, byte>(originYcorp.Width / 2, originYcorp.Height / 2); 
                    for (int i = 0; i < originYcorp.Height; i += 2)
                    {
                        for (int j = 0; j < originYcorp.Width; j += 2)
                        {
                            byte[] aa = new byte[9];

                            if (j + 2 == originYcorp.Width || i + 2 == originYcorp.Height)
                            {
                                aa = new byte[9]{
                                    originYcorp.Data[i + 0, j + 0, 0],
                                    originYcorp.Data[i + 0, j + 1, 0],
                                    0,
                                    originYcorp.Data[i + 1, j + 0, 0],
                                    originYcorp.Data[i + 1, j + 1, 0],
                                    0,
                                    0, 0, 0};
                            }
                            else
                            {
                                aa = new byte[9]{
                                    originYcorp.Data[i + 0, j + 0, 0],
                                    originYcorp.Data[i + 0, j + 1, 0],
                                    originYcorp.Data[i + 0, j + 2, 0],
                                    originYcorp.Data[i + 1, j + 0, 0],
                                    originYcorp.Data[i + 1, j + 1, 0],
                                    originYcorp.Data[i + 1, j + 2, 0],
                                    originYcorp.Data[i + 2, j + 0, 0],
                                    originYcorp.Data[i + 2, j + 1, 0],
                                    originYcorp.Data[i + 2, j + 2, 0]};
                            }

                            if (aa.Max() > luma_base)
                            {
                                miniOriginYcorp.Data[i / 2, j / 2, 0] = aa.Max();
                            }
                            else
                            {
                                miniOriginYcorp.Data[i / 2, j / 2, 0] = aa.Min();
                            }
                        }
                    }
                    
                    originYcorp = miniOriginYcorp;
                    #endregion

                    #region edgeStudy
                    /*
                    //horizontal filter
                    Image<Gray, float> sobelX = originYcorp.Sobel(1, 0, 31);
                    //sobelX.Save("X.jpg");                    
                    //vertical filter
                    Image<Gray, float> sobelY = originYcorp.Sobel(0, 1, 31);
                    //sobelY.Save("Y.jpg");
                    Image<Gray, Byte> sobelXByte = sobelX.Convert<Gray, Byte>();                    
                    Image<Gray, Byte> sobelYByte = sobelY.Convert<Gray, Byte>();
                    //sobelXByte.Save("sobelXByte.jpg");
                    //sobelYByte.Save("sobelYByte.jpg");
                    //Convert negative values to positive valus
                    sobelX = sobelX.AbsDiff(new Gray(0));
                    sobelY = sobelY.AbsDiff(new Gray(0));
                    Image<Gray, float> sobel = sobelX + sobelY;
                    //sobel.Save("sobel.jpg");
                    //Find sobel min or max value
                    double[] mins, maxs;
                    //Find sobel min or max value position
                    Point[] minLoc, maxLoc;
                    sobel.MinMax(out mins, out maxs, out minLoc, out maxLoc);
                    //Conversion to 8-bit image
                    Image<Gray, Byte> sobelImage = sobel.ConvertScale<byte>(255 / maxs[0], 0);
                    //Get binary image
                    sobelImage._ThresholdBinary(new Gray(20), new Gray(255));
                    //sobelImage = sobelImage.Erode(10);
                    //sobelImage = sobelImage.Dilate(10);                     
                    sobelImage.Save("XY.jpg");
                    */
                    #endregion
                    //Y1Y2                   
                    //ContrastAnalysis(originYcorp, Contrast_sub_m, Contrast_sub_n, std_num, Contrast_Spec, Contrast_Spec1, Contrast_lumaMax, Contrast_lumaMax1);                 

                    MIplImage MIpImg = (MIplImage)System.Runtime.InteropServices.Marshal.PtrToStructure(originYcorp.Ptr, typeof(MIplImage));
                    Bitmap disp = new Bitmap(originYcorp.Width, originYcorp.Height, PixelFormat.Format16bppGrayScale);
                    BitmapData bmp = disp.LockBits(new Rectangle(0, 0, originYcorp.Width, originYcorp.Height), ImageLockMode.ReadWrite, PixelFormat.Format16bppGrayScale);
                    int tch = MIpImg.nChannels;

                    if (true)
                    {
                        bool CircleContrastAnalysisPASS = true;               
                        try
                        {
                            System.Diagnostics.Stopwatch Ctime = new System.Diagnostics.Stopwatch();//引用stopwatch物件
                            Ctime.Reset(); Ctime.Start();//碼表歸零&開始計時

                            //### full image Y_avg, Y_std
                            int h = originYcorp.Height, w = originYcorp.Width;
                            double fullImgAverage, fullImgStandard;
                            AverageCalculation(originYcorp, out fullImgAverage, out fullImgStandard);

                            //### 4 quarters-image Y_avg, Y_std
                            Image<Gray, byte> Ycorp_quarters = originYcorp.Copy();
                            bool bool_quarterAge_Std = true;
                            double[] quarterAge = new double[4]; double[] quarterStd = new double[4];
                            Ycorp_quarters.ROI = new Rectangle(0, 0, w / 2, h / 2);
                            AverageCalculation(Ycorp_quarters, out quarterAge[0], out quarterStd[0]);                            
                            Ycorp_quarters.ROI = new Rectangle(w / 2, 0, w / 2, h / 2);
                            AverageCalculation(Ycorp_quarters, out quarterAge[1], out quarterStd[1]);
                            Ycorp_quarters.ROI = new Rectangle(0, h / 2, w / 2, h / 2);
                            AverageCalculation(Ycorp_quarters, out quarterAge[2], out quarterStd[2]);
                            Ycorp_quarters.ROI = new Rectangle(w / 2, h / 2, w / 2, h / 2);
                            AverageCalculation(Ycorp_quarters, out quarterAge[3], out quarterStd[3]);

                            log.OutLog("quarterAge1:" + quarterAge[0].ToString("f3") + ", quarterStd1:" + quarterStd[0].ToString("f3"));
                            log.OutLog("quarterAge2:" + quarterAge[1].ToString("f3") + ", quarterStd2:" + quarterStd[1].ToString("f3"));
                            log.OutLog("quarterAge3:" + quarterAge[2].ToString("f3") + ", quarterStd3:" + quarterStd[2].ToString("f3"));
                            log.OutLog("quarterAge4:" + quarterAge[3].ToString("f3") + ", quarterStd4:" + quarterStd[3].ToString("f3"));
                            log.OutLog("Ctime: " + Ctime.Elapsed.TotalMilliseconds.ToString());

                            //### blocks uniformity spec: luma variety
                            double blackavgSpec_min = 0, blackavgSpec_thres = 0;
                            if (fullImgStandard / fullImgAverage < 0.8 &&
                                quarterStd[0] < 12.5 && quarterStd[0] / quarterAge[0] < 0.98 &&
                                quarterStd[1] < 12.5 && quarterStd[1] / quarterAge[1] < 0.98 &&
                                quarterStd[2] < 12.5 && quarterStd[2] / quarterAge[2] < 0.98 &&
                                quarterStd[3] < 12.5 && quarterStd[3] / quarterAge[3] < 0.98) 
                            {
                                blackavgSpec_min = 36;
                                bool_quarterAge_Std = true;
                                log.OutLog("Circle pixel average > 36.");
                            } 
                            else 
                            {
                                blackavgSpec_min = 32;
                                bool_quarterAge_Std = false;
                                log.OutLog("Circle pixel average > 32. One of the Std > 12.5 or Std > Age or Std/Age > 0.98.");
                            }

                            DenseHistogram fullImgMedian = GetHistogram(originYcorp);
                            float[] fullImgMedianH = new float[256];
                            fullImgMedian.MatND.ManagedArray.CopyTo(fullImgMedianH, 0); 
                            float sum = 0;
                            double fullImgMed75 = 0, fullImgMed80 = 0;
                            float total75 = h * w * 75 / 100;
                            float total80 = h * w * 80 / 100;

                            for (int med = 0; med < 256; med++)
                            {
                                sum += fullImgMedianH[med];
                                if (sum > total75 && fullImgMed75 == 0)
                                    fullImgMed75 = med;
                                if (sum > total80 && fullImgMed80 == 0)
                                    fullImgMed80 = med; 
                            }
                            log.OutLog(specAB + "大於75%和80%灰階值:" + fullImgMed75 + ", " + fullImgMed80 + ", Average:" + fullImgAverage.ToString("f2") + ", Standard:" + fullImgStandard.ToString("f2"));

                            double contrast = 0, contrastFinalSpec_A = 10, contrastFinalSpec_B = 10;
                            bool Spec_A = false, Spec_B = false;
                            int si = 0, sj = 0, bh = 0, bw = 0;//start end bluck height width

                            DrawCircle = new Bitmap(originYcorp.Bitmap);
                            Graphics cg3 = Graphics.FromImage(DrawCircle);
                            Image<Gray, byte> TestCircle = new Image<Gray, byte>(originYcorp.Width, originYcorp.Height);
                            Image<Gray, byte> dest = new Image<Gray, byte>(originYcorp.Width, originYcorp.Height);
                            Image<Gray, byte> destBig = new Image<Gray, byte>(originYcorp.Width, originYcorp.Height);
                            Image<Gray, byte> destConcentric = new Image<Gray, byte>(originYcorp.Width, originYcorp.Height);
                            Image<Gray, byte> aaROI = originYcorp.Copy();

                            int[] Ti = { }; int[] Tj = { };
                            double[] Tagv = { };
                            double[] TagvSpec = { };
                            double[] Tcontrast = { };
                            int[] Tci = { }; 
                            int[] Tcj = { }; 
                            int[] Tcx = { }; 
                            int[] Trx = { };

                            int array_n = 0;
                            int CircleMaxLuma = 0, CircleMaxLuma2 = 0;
                            float CircleMaxLumaN = 0, CircleMaxLumaN2 = 0;
                            double aa;
                            float[] destArray = new float[256];
                            double blackavg = 0, blackstd,
                                BigBlockavg, BigBlockstd,
                                bigAvgSpec_1 = 0, bigAvgSpec_2 = 0;
                            int pixel_n = 0, maxpixel_n = 0, avg_n = 0, std_n = 0, bigavg_n = 0,
                                temp1 = 0, temp2 = 0, temp3 = 0, temp4 = 0;
                            int loop_n = 0, base_n = 0;
                            int AutoCirclePixel_i = 4, AutoCirclePixel_j = 4;
                            DenseHistogram destHist;

                            //### detect screen leak pixels, record test results
                            unsafe
                            {                  
                                int i = 0;                                
                                while (i <= h - 1)                                
                                {
                                    int j = 0;
                                    aa = 0;

                                    while (j <= w - 1)
                                    {
                                        if (j == 0 && (i / 4) % 2 == 0) //if conor grid & side grid
                                            j += (AutoCirclePixel_j / 2);
                                        
                                        #region loop
                                        aaROI.ROI = new Rectangle(j - 1, i - 1, 2, 2);
                                        aa = aaROI.GetAverage().Intensity;
                                        
                                        if (aa > luma_base)
                                        {
                                            cg3.DrawEllipse(new Pen(Color.Brown), j - 1, i - 1, 2, 2);
                                            base_n++;
                                        }

                                        //### high luma pixels found, do circle detection to get local max
                                        if (aa > fullImgAverage + (fullImgStandard * 0) && aa > luma_base && aa > fullImgMed75)
                                        {                                            
                                            pixel_n++;
                                            bool AddArray = true;
                                            contrast = 0;

                                            for (int x = 30; x >= 6; x -= 2) //x=圓半徑                  
                                            {
                                                blackavgSpec_thres = (x * 1.8 > blackavgSpec_min) ? x * 1.8 : blackavgSpec_min; //平均亮度threshold
                                
                                                TestCircle = new Image<Gray, byte>(originYcorp.Width, originYcorp.Height, new Gray(0));
                                                CvInvoke.cvCircle(TestCircle.Ptr, new System.Drawing.Point(j, i), x, new MCvScalar(255, 255, 255), -1, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
                                                dest = originYcorp.And(originYcorp, TestCircle);

                                                si = i - x; bh = (x * 2) + 1;
                                                sj = j - x; bw = (x * 2) + 1;
                                                dest.ROI = new Rectangle(sj, si, bw, bh);
                                                destHist = null;
                                                destHist = GetHistogram(dest);
                                                destHist.MatND.ManagedArray.CopyTo(destArray, 0);

                                                //### find local max, starting from luma=30
                                                for (int hi = 30; hi < 256; hi++)
                                                {
                                                    if (destArray[hi] > 0)
                                                    {
                                                        CircleMaxLuma2 = CircleMaxLuma;
                                                        CircleMaxLumaN2 = CircleMaxLumaN;
                                                        CircleMaxLuma = hi;
                                                        CircleMaxLumaN = destArray[hi];
                                                    }
                                                }

                                                //### if found local max & luma-25>base/luma>80% luma
                                                if (CircleMaxLuma > fullImgMed80 && CircleMaxLuma > fullImgAverage + (fullImgStandard * 0.5) && CircleMaxLuma - luma_base > 25)
                                                {   
                                                    if (loop_n > temp1 || temp1 == 0)
                                                    {
                                                        maxpixel_n++; //local max count
                                                        temp1 = loop_n;
                                                    }

                                                    AverageCalculation(dest, out blackavg, out blackstd);

                                                    //### if avg luma>spec & avg luma>full luma, do screen leak test
                                                    if (blackavg > blackavgSpec_thres && blackavg > fullImgAverage)
                                                    {
                                                        if (loop_n > temp2 || temp2 == 0)
                                                        {
                                                            avg_n++; //circle region in high avg luma count
                                                            temp2 = loop_n;
                                                        }

                                                        int rx = (x * 7 / 10) + 5;                                         
                                                        CvInvoke.cvCircle(TestCircle.Ptr, new System.Drawing.Point(j, i), x + rx, new MCvScalar(255, 255, 255), -1, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
                                                        destBig = originYcorp.And(originYcorp, TestCircle);

                                                        destBig.ROI = new Rectangle(sj - rx, si - rx, bw + (rx * 2), bh + (rx * 2));                                                                    
                                                        AverageCalculation(destBig, out BigBlockavg, out BigBlockstd);

                                                        if (BigBlockstd - blackstd > 0.8 && BigBlockstd - blackstd < 20)
                                                        {
                                                            if (loop_n > temp3 || temp3 == 0)
                                                            {
                                                                std_n++; 
                                                                temp3 = loop_n;
                                                            }

                                                            if (blackavg > BigBlockavg)
                                                            {
                                                                if (loop_n > temp4 || temp4 == 0)
                                                                {
                                                                    bigavg_n++;
                                                                    temp4 = loop_n;
                                                                }

                                                                cg3.DrawEllipse(new Pen(Color.Yellow), j - 1, i - 1, 2, 2);
                                                                
                                                                contrast = blackavg / BigBlockavg;

                                                                bigAvgSpec_1 = (BigBlockavg * classINI.CircleContrastSpec) + classINI.ContrastSpecConstant;
                                                                if (!bool_quarterAge_Std)
                                                                {
                                                                    if (blackavg < 35)
                                                                        bigAvgSpec_1 *= 1.03;
                                                                }

                                                                bigAvgSpec_2 = (BigBlockavg * classINI.CircleContrastSpecB) + classINI.ContrastSpecConstantB;

                                                                if (blackavg > bigAvgSpec_1)
                                                                {
                                                                    if (contrast < contrastFinalSpec_A)
                                                                    {
                                                                        contrastFinalSpec_A = contrast;
                                                                        Spec_A = true;
                                                                    }
                                                                }
                                                                if (blackavg > bigAvgSpec_2)
                                                                {
                                                                    if (contrast < contrastFinalSpec_B)
                                                                    {
                                                                        contrastFinalSpec_B = contrast;
                                                                        Spec_B = true;
                                                                    }
                                                                }

                                                                if (classINI.DetailsLOG)
                                                                {
                                                                    log.OutLog("中心亮度:" + ", " + aa.ToString("f2") + ", " +
                                                                        ", 標準差:" + blackstd.ToString("f2") + " - " + BigBlockstd.ToString("f2") + " = " + (BigBlockstd - blackstd).ToString("f2") +
                                                                        ",  平均:" + blackavg.ToString("000.00") + ", " + BigBlockavg.ToString("000.00") + ", " + "Concentricavg.ToString()" + ", 小圓平均SPEC:" + bigAvgSpec_1.ToString("000.00") +
                                                                        ",  小圓/外圈:" + "(blackavg / Concentricavg).ToString()" +
                                                                        ",  對比值:" + (blackavg / BigBlockavg).ToString("f2") + ", (j,i)=(" + j + "," + i + ")" + ", x = " + x +
                                                                        ",  Circle Max Luma=" + CircleMaxLuma + "-base=" + (CircleMaxLuma - luma_base).ToString("f2") + ", 小圓最大亮點數:" + CircleMaxLumaN.ToString("f2"));
                                                                }

                                                                if (AddArray)
                                                                {
                                                                    Array.Resize(ref Ti, Ti.Length + 1);
                                                                    Array.Resize(ref Tj, Tj.Length + 1);
                                                                    Array.Resize(ref Tagv, Tagv.Length + 1);//TagvSpec
                                                                    Array.Resize(ref TagvSpec, TagvSpec.Length + 1);//TagvSpec
                                                                    Array.Resize(ref Tci, Tci.Length + 1);
                                                                    Array.Resize(ref Tcj, Tcj.Length + 1);
                                                                    Array.Resize(ref Tcx, Tcx.Length + 1);
                                                                    Array.Resize(ref Trx, Trx.Length + 1);
                                                                    Array.Resize(ref Tcontrast, Tcontrast.Length + 1);
                                                                    array_n = Ti.Length - 1;
                                                                    AddArray = false;
                                                                }

                                                                if (contrast > Tcontrast[array_n])
                                                                {
                                                                    Ti[array_n] = i;
                                                                    Tj[array_n] = j;
                                                                    Tagv[array_n] = blackavg;
                                                                    TagvSpec[array_n] = blackavgSpec_thres;
                                                                    Tci[array_n] = si;
                                                                    Tcj[array_n] = sj;
                                                                    Tcx[array_n] = (x * 2) + 1;
                                                                    Trx[array_n] = rx;
                                                                    Tcontrast[array_n] = contrast;
                                                                }
                                                            }                                                              
                                                        }
                                                    }
                                                }
                                                else //if not found local max, j stride += x
                                                {
                                                    j += x;
                                                    x = 0;
                                                }
                                            }
                                        }

                                        if ((j > w / 20 && j < w * 19 / 20) && (i > h / 10 && i < h * 9 / 10))
                                            j += (AutoCirclePixel_j + 6);
                                        else
                                            j += AutoCirclePixel_j; 

                                        loop_n++; //total checked pixel counts
                                        #endregion
                                    }

                                    i += (AutoCirclePixel_i);
                                }
                            }

                            //### detect screen leak pixels
                            if (contrastFinalSpec_A != 10)
                                SPEC_Contrast_A = contrastFinalSpec_A;
                            if (contrastFinalSpec_B != 10)
                                SPEC_Contrast_B = contrastFinalSpec_B;

                            log.OutLog(contrastFinalSpec_A.ToString("00.00") +", "+ contrastFinalSpec_B.ToString("00.00") +
                                ", pixels fail(圓心, luma>base & 70%亮度)數量:" + pixel_n + ", pixels fail(local max>80% & local max-25>base)數量:" + maxpixel_n + 
                                ", 平均luma(circle detection) fail數量:" + avg_n + ", 標準差(circle detection) fail數量:" + std_n + ", 大小圓平均fail數量:" + bigavg_n);
                            log.OutLog(SSN + ", Total pixels checked:" + loop_n + ", over Base Ratio:" + (Convert.ToDouble(loop_n) * (classINI.overBaseRatio / 100)).ToString("f0") + ", over base(yellow):" + base_n);

                            #region CircleContrastAnalysisPASS                            
                            //### show result
                            if (Tcontrast.Length > 0)
                            {
                                int[] Tdi = { }; int[] Tdj = { };
                                int ms = 0;

                                for (int n = 0; n < 5; n++)
                                {
                                    Pen inside = new Pen(Color.Gray);
                                    Pen outside = new Pen(Color.DarkBlue);
                                    Brush strColor = Brushes.Gray;
                                    Font strFont = new Font("Calibri", 10);
                                    bool outSideDraw = true;

                                    if (Tcontrast.Max() != 0)
                                    {
                                        ms = Array.IndexOf(Tcontrast, Tcontrast.Max());
                                        bool drawbool = true;

                                        //### avoid inner circles
                                        if (Tdi.Length > 0)
                                        {
                                            for (int dd = 0; dd < Tdi.Length; dd++)
                                            {
                                                if (Math.Sqrt(((Tdi[dd] - Ti[ms]) * (Tdi[dd] - Ti[ms])) + ((Tdj[dd] - Tj[ms]) * (Tdj[dd] - Tj[ms]))) < (Tcx[ms]))
                                                {
                                                    drawbool = false;
                                                    dd = Tdi.Length;
                                                    n -= 1;
                                                }
                                            }
                                        }

                                        if (Tcontrast[ms] >= contrastFinalSpec_A && Spec_A)
                                        {
                                            CircleContrastAnalysisPASS = false;

                                            if (LCD_LEAKAGE_Contrast_A == 0)
                                            {
                                                LCD_LEAKAGE_Contrast_A = Tcontrast[ms];
                                                LCD_LEAKAGE_POSITION_X_A = Tj[ms];
                                                LCD_LEAKAGE_POSITION_Y_A = Ti[ms];
                                                LCD_LEAKAGE_CIRCLE_A = Tagv[ms];
                                            }

                                            inside = new Pen(Color.LightBlue);
                                            outside = new Pen(Color.DarkBlue);
                                            strColor = Brushes.Blue;
                                            strFont = new Font("Calibri", 14);

                                            if (drawbool)
                                            {
                                                log.OutLog("specA 對比fail:" + Tcontrast[ms].ToString("00.00") + ", 平均亮度:" + Tagv[ms].ToString("000.00") +
                                                    ", 平均亮度最小值:" + TagvSpec[ms].ToString("000.00") + ", position(x,y)=(" + Tj[ms] + "," + Ti[ms] + ")" + ", 圓半徑x=" + ((Tcx[ms] - 1) / 2));
                                            }
                                            else
                                            {
                                                if (classINI.DetailsLOG)
                                                {
                                                    log.OutLog("A fail對比:" + Tcontrast[ms].ToString("00.00") + ", 平均:" + Tagv[ms].ToString("000.00") + ", 平均最低值:" + TagvSpec[ms].ToString("000.00") + ", (j,i)=(" + Tj[ms] + "," + Ti[ms] + ")" + ", x=" + ((Tcx[ms] - 1) / 2));


                                                }
                                            }
                                        }
                                        else if (Tcontrast[ms] >= contrastFinalSpec_B && Spec_B)
                                        {
                                            outSideDraw = false;
                                            if (LCD_LEAKAGE_Contrast_B == 0)
                                            {
                                                LCD_LEAKAGE_Contrast_B = Tcontrast[ms];
                                                LCD_LEAKAGE_POSITION_X_B = Tj[ms];
                                                LCD_LEAKAGE_POSITION_Y_B = Ti[ms];
                                                LCD_LEAKAGE_CIRCLE_B = Tagv[ms];
                                            }

                                            inside = new Pen(Color.LightGreen);
                                            strColor = Brushes.Green;

                                            if (drawbool)
                                            {
                                                log.OutLog("specB 對比fail:" + Tcontrast[ms].ToString("00.00") + ", 平均亮度:" + Tagv[ms].ToString("000.00") + ", 平均亮度最低值:" +
                                                    TagvSpec[ms].ToString("000.00") + ", position(x,y)=(" + Tj[ms] + "," + Ti[ms] + ")" + ", 圓半徑x=" + ((Tcx[ms] - 1) / 2));
                                            }
                                            else
                                            {
                                                if (classINI.DetailsLOG)
                                                {
                                                    log.OutLog("B fail對比:" + Tcontrast[ms].ToString("00.00") + ", 平均:" + Tagv[ms].ToString("000.00") + ", 平均最低值:" + TagvSpec[ms].ToString("000.00") + ", (j,i)=(" + Tj[ms] + "," + Ti[ms] + ")" + ", x=" + ((Tcx[ms] - 1) / 2));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            outSideDraw = false;
                                            strFont = new Font("Calibri", 8);

                                            if (LCD_LEAKAGE_Contrast_P == 0)
                                            {
                                                LCD_LEAKAGE_Contrast_P = Tcontrast[ms];
                                                LCD_LEAKAGE_POSITION_X_P = Tj[ms];
                                                LCD_LEAKAGE_POSITION_Y_P = Ti[ms];
                                                LCD_LEAKAGE_CIRCLE_P = Tagv[ms];
                                            }
                                            if (drawbool)
                                            {
                                                log.OutLog("Draw 對比pass:" + Tcontrast[ms].ToString("00.00") + ", 平均:" + Tagv[ms].ToString("000.00") + ", 平均最低值:" + TagvSpec[ms].ToString("000.00") + ", (j,i)=(" + Tj[ms] + "," + Ti[ms] + ")" + ", x=" + ((Tcx[ms] - 1) / 2));
                                            }
                                            else
                                            {
                                                if (classINI.DetailsLOG)
                                                {
                                                    log.OutLog("對比 pass:" + Tcontrast[ms].ToString("00.00") + ", 平均:" + Tagv[ms].ToString("000.00") + ", 平均最低值:" + TagvSpec[ms].ToString("000.00") + ", (j,i)=(" + Tj[ms] + "," + Ti[ms] + ")" + ", x=" + ((Tcx[ms] - 1) / 2));
                                                }
                                            }
                                        }

                                        if (drawbool)
                                        {
                                            Array.Resize(ref Tdi, Tdi.Length + 1);
                                            Tdi[Tdi.Length - 1] = Ti[ms];
                                            Array.Resize(ref Tdj, Tdj.Length + 1);
                                            Tdj[Tdj.Length - 1] = Tj[ms];
                                            cg3.DrawEllipse(inside, Tcj[ms], Tci[ms], Tcx[ms], Tcx[ms]);

                                            if (outSideDraw)
                                                cg3.DrawEllipse(outside, Tcj[ms] - Trx[ms], Tci[ms] - Trx[ms], Tcx[ms] + (Trx[ms] * 2), Tcx[ms] + (Trx[ms] * 2));//畫圓

                                            int pti = Ti[ms], ptj = Tj[ms];
                                            if (h - Ti[ms] < 40)
                                                pti = Ti[ms] - 20;
                                            if (w - Tj[ms] < 80)
                                                ptj = Tj[ms] - 50;

                                            cg3.DrawString(Tagv[ms].ToString("f0") + ", " + Tcontrast[ms].ToString("f2"), strFont, strColor, new PointF(ptj, pti));
                                        }
                                        Tcontrast[ms] = 0;
                                    }
                                }
                            }                  
                            #endregion

                            log.OutLog("Ctime: " + Ctime.Elapsed.TotalMilliseconds.ToString());

                            double loop_nRatio = Convert.ToDouble(loop_n) * (classINI.overBaseRatio / 100);

                            if (base_n > loop_nRatio)
                            {
                                CircleContrastAnalysisPASS = false;
                                log.OutLog("Over base fail.");
                            }

                            if (!CircleContrastAnalysisPASS)
                            {
                                if (bool_quarterAge_Std)
                                {
                                    if ((quarterStd[0] + quarterStd[1] + quarterStd[2] + quarterStd[3]) < 30)
                                    {
                                        CircleContrastAnalysisPASS = true;
                                        log.OutLog("quarterStd:" + (quarterStd[0] + quarterStd[1] + quarterStd[2] + quarterStd[3]).ToString("f3") + " < 30 : PASS.");
                                    }
                                }
                            }

                            TestCircle.Dispose();
                            dest.Dispose();
                            destBig.Dispose();
                            destConcentric.Dispose();
                        }
                        catch (Exception ex)
                        {
                            log.OutLog(ex.Message + "Auto Circle Contrast Error.");
                            return -1;
                        }
                        
                        TestResult = CircleContrastAnalysisPASS;                       

                        if (changeGamma_bool)
                        {
                            DrawCircleB = DrawCircle;
                            changeGamma_bool = false;
                            goto changeGamma;
                        }
                        else
                            DrawCircleA = DrawCircle;
                        
                        swt.Stop();
                        log.OutLog("run times : " + swt.Elapsed.TotalMilliseconds.ToString());
                        LCD_LEAKAGE_TIME = swt.Elapsed.TotalMilliseconds.ToString();
                        Finaladdress(TestResult, DrawCircleA, null, null, "DrawCircleA", null, null);
                        Finaladdress(TestResult, DrawCircleB, null, null, "DrawCircleB", null, null);
                        DrawCircle.Dispose();
                        DrawCircleA.Dispose(); 
                    }                    
                    
                    Finaladdress(TestResult, bit_corp, null, colorImage, "ScRange", null, "Origin");
                    Finaladdress(TestResult, BitDrawLine, null, null, "DrawLine", null, null);
                    BitDrawLine.Dispose();
                }
            }
            else //screen cropped size fail/screen max luma fail
            {
                log.OutLog("run times : " + swt.Elapsed.TotalMilliseconds.ToString());
                LCD_LEAKAGE_TIME = swt.Elapsed.TotalMilliseconds.ToString();
                Finaladdress(TestResult, bit_corp, null, colorImage, "ScRange", null, "Origin");              
            }

            bit_corp.Dispose();

            if (System.IO.File.Exists(Application.StartupPath + "\\" + "SubAvgImg.png"))
            {
                try { System.IO.File.Delete(Application.StartupPath + "\\" + "SubAvgImg.png"); }
                finally
                {
                    //Console.WriteLine("Delete png image fail.");
                }
            }
            
            colorImage.Dispose();
            grayImage.Dispose();
            APIAverageImg.Dispose();
            CL_baseCalculation.originYcorp.Dispose();
            CL_baseCalculation.originbackupA.Dispose();
            CL_baseCalculation.originbackupB.Dispose();
            CL_baseCalculation.bit_corp.Dispose();
            CL_baseCalculation.thresholdImg.Dispose();
            WritePMSlog();

            if (TestResult) // case PASS
                return 0;

            return -1; // case FAIL
        }

        static void ThreadForm()
        {
            Form1 f = new Form1();
            f.ShowDialog();
        }

        static void AverageCalculation(Image<Gray, byte> BA, out double BAavg, out double BAstd)
        {
            int total = BA.Height * BA.Width;
            BAavg = 0; BAstd = 0;//標準差            
            double Ysum = 0, Nsum = 0;//亮度加總，個數加總。平均=亮度加總/個數加總

            float[] HistArray = new float[256]; //the resulting histogram array             
            DenseHistogram blockHist = GetHistogram(BA);
            blockHist.MatND.ManagedArray.CopyTo(HistArray, 0); //copy array     

            double LCDoutSide = (luma_base / 6);
            LCDoutSide = (luma_base > 6)? (luma_base / 6) : 1;

            for (int hi = (int)LCDoutSide; hi < 256; hi++)
            {
                if (HistArray[hi] != 0)
                {
                    BAstd += HistArray[hi] * (Math.Pow(hi, 2));
                }//"平方和"

                Ysum += (HistArray[hi] * hi);
                Nsum += HistArray[hi];
            }

            if (Nsum == 0)
            {
                BAavg = 0;
                BAstd = 0;
            }
            else
            {
                BAavg = Ysum / Nsum;
                BAstd = Math.Sqrt((BAstd / Nsum) - Math.Pow(BAavg, 2));
            }
        }

        static double blockAverage(Image<Gray, byte> BA, bool bo)
        {
            int total = BA.Height * BA.Width;
            double BAstd0 = 0;//標準差
            double Ysum0 = 0, Nsum0 = 0;//亮度加總，個數加總。平均=亮度加總/個數加總
            float[] HistArray = new float[256];                  //the resulting histogram array    
            double avg0 = 0;
            DenseHistogram blockHist = GetHistogram(BA);
            blockHist.MatND.ManagedArray.CopyTo(HistArray, 0);//copy array     
            double LCDoutside0 = (luma_base / 5);
            for (int hi = (int)LCDoutside0; hi < 256; hi++)
            {
                if (HistArray[hi] != 0) { BAstd0 += HistArray[hi] * (Math.Pow(hi, 2)); }//"平方和"
                Ysum0 += (HistArray[hi] * hi);
                Nsum0 += HistArray[hi];
            }
            if (Nsum0 == 0) { avg0 = 0; BAstd0 = 0; }
            else
            {
                avg0 = Ysum0 / Nsum0;
                BAstd0 = Math.Sqrt((BAstd0 / Nsum0) - Math.Pow(avg0, 2));
            }
            if (bo)
            {
                if ((avg0 < (int)LCDoutside0 || total > Nsum0 * 4)) { return -1; }//若平均小於5當作LCD外
                else { return avg0; }//true 回傳平均
            }
            else { return BAstd0; } //false 回傳標準差   
        }

        static bool ContrastAnalysis(Image<Gray, byte> CA, int bm, int bn, double std_num, 
            double Contrast_Spec, double Contrast_Spec1, byte Contrast_lumaMax, byte Contrast_lumaMax1)
        {
            bool BigstdPass = true, stdPass = true;
            bool returnbool = true;
            int h = CA.Height, w = CA.Width;
            int m = bm, n = bn; //切割格子數
            int grid_height = Convert.ToInt32(h / m); //格子高
            int grid_width = Convert.ToInt32(w / n); //格子寬

            //bounding
            int bound_down = grid_height * (m - 1);
            int bound_right = grid_width * (n - 1);
            //Var
            double MaxContrast = 0;
            double Fsize = 0;

            Image<Gray, byte> block = CA.Copy();
            Image<Gray, byte> aboveblock = CA.Copy(); double ab = -1;//上面
            Image<Gray, byte> underblock = CA.Copy(); double ub = -1;//下面
            Image<Gray, byte> leftblock = CA.Copy(); double lb = -1;//左邊
            Image<Gray, byte> rightblock = CA.Copy(); double rb = -1;//右邊

            Image<Gray, byte> above_left = CA.Copy(); double al = -1;//上左
            Image<Gray, byte> above_right = CA.Copy(); double ar = -1;//上右
            Image<Gray, byte> under_left = CA.Copy(); double ul = -1;//下左
            Image<Gray, byte> under_right = CA.Copy(); double ur = -1;//下右
            Image<Gray, byte> Bigblock = CA.Copy();//九宮格區域
            //block.ROI = Rectangle.Empty;  
            LOG log = new LOG();

            try
            {                   
                DrawLine2 = new Bitmap(CA.Bitmap); //畫框寫數字的底   
                Graphics g2 = Graphics.FromImage(DrawLine2);
                DrawLine3 = new Bitmap(CA.Bitmap); //畫框寫數字的底  
                Graphics g3 = Graphics.FromImage(DrawLine3);

                Pen BluePen = new Pen(Color.Blue);
                int oci = 0, ocj = 0, ch = 0, cw = 0, fch = 0, fcw = 0;//對比度最大區域 長寬起點和長、寬 最終確定的長寬 
                double gbal = 0;
                double gbal2 = 0;
                double bstd = 0;//區塊標準差
                int MaxContrastValue = 0;
                int TorF = 0;
                for (int i = 0; i <= bound_down; i += grid_height)
                {
                    for (int j = 0; j <= bound_right; j += grid_width)
                    {
                        double aroundBlockAverage = 0;
                        double contrast = 0;
                        double bminAverage = 0;
                        ab = -1; ub = -1; lb = -1; rb = -1; al = -1; ar = -1; ul = -1; ur = -1;//歸零
                        //區塊範圍和計算
                        if (i == bound_down && j == bound_right) //右下角落
                        {
                            block.ROI = new Rectangle(j, i, w - bound_right, h - bound_down);
                            gbal2 = blockAverage(block, true);
                            g2.DrawRectangle(BluePen, new Rectangle(j, i, w - bound_right - 1, h - bound_down - 1));//畫框 
                            g3.DrawRectangle(BluePen, new Rectangle(j, i, w - bound_right - 1, h - bound_down - 1));//畫框
                            Bigblock.ROI = new Rectangle(j - grid_width, i - grid_height, (w - bound_right) + grid_width, (h - bound_down) + grid_height);//1
                            aboveblock.ROI = new Rectangle(j, i - grid_height, w - bound_right, grid_height); ab = blockAverage(aboveblock, true);
                            leftblock.ROI = new Rectangle(j - grid_width, i, grid_width, h - bound_down); lb = blockAverage(leftblock, true);
                            above_left.ROI = new Rectangle(j - grid_width, i - grid_height, grid_width, grid_height); al = blockAverage(above_left, true);
                            if (lb >= ab) { bminAverage = ab; } else { bminAverage = lb; }
                            //aroundBlockAverage = aboveblock.GetAverage().Intensity + leftblock.GetAverage().Intensity + above_left.GetAverage().Intensity;
                            aroundBlockAverage = ab + lb + al;
                            bstd = (gbal2 + aroundBlockAverage) / 4;
                            //bstd = Math.Sqrt(((Math.Pow(ab, 2) + Math.Pow(lb, 2) + Math.Pow(al, 2) + Math.Pow(gbal2, 2)) / 4) - Math.Pow(bstd, 2));
                            aroundBlockAverage = aroundBlockAverage / 3;
                            cw = w - bound_right - 1;
                            ch = h - bound_down - 1;
                        }
                        else if (j == bound_right)
                        {
                            if (i == 0)
                            { i = i + 0; }
                            block.ROI = new Rectangle(j, i, w - bound_right, grid_height);
                            gbal2 = blockAverage(block, true);
                            g2.DrawRectangle(BluePen, new Rectangle(j, i, w - bound_right - 1, grid_height));//畫框
                            g3.DrawRectangle(BluePen, new Rectangle(j, i, w - bound_right - 1, grid_height));//畫框  
                            cw = w - bound_right - 1;
                            ch = grid_height;
                            if (i == 0)
                            {
                                Bigblock.ROI = new Rectangle(j - grid_width, i, (w - bound_right) + grid_width, grid_height * 2);//2
                                underblock.ROI = new Rectangle(j, i + grid_height, w - bound_right, grid_height); ub = blockAverage(underblock, true);
                                leftblock.ROI = new Rectangle(j - grid_width, i, grid_width, grid_height); lb = blockAverage(leftblock, true);
                                under_left.ROI = new Rectangle(j - grid_width, i + grid_height, grid_width, grid_height); ul = blockAverage(under_left, true);
                                if (lb >= ub) { bminAverage = ub; } else { bminAverage = lb; }
                                //aroundBlockAverage = underblock.GetAverage().Intensity + leftblock.GetAverage().Intensity + under_left.GetAverage().Intensity;
                                aroundBlockAverage = ub + lb + ul;
                                bstd = (gbal2 + aroundBlockAverage) / 4;
                                //bstd = Math.Sqrt(((Math.Pow(ub, 2) + Math.Pow(lb, 2) + Math.Pow(ul, 2) + Math.Pow(gbal2, 2)) / 4) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 3;
                            }
                            else
                            {
                                Bigblock.ROI = new Rectangle(j - grid_width, i - grid_height, (w - bound_right) + grid_width, grid_height * 3);//3
                                aboveblock.ROI = new Rectangle(j, i - grid_height, w - bound_right, grid_height); ab = blockAverage(aboveblock, true);
                                underblock.ROI = new Rectangle(j, i + grid_height, w - bound_right, grid_height); ub = blockAverage(underblock, true);
                                leftblock.ROI = new Rectangle(j - grid_width, i, grid_width, grid_height); lb = blockAverage(leftblock, true);
                                above_left.ROI = new Rectangle(j - grid_width, i - grid_height, grid_width, grid_height); al = blockAverage(above_left, true);
                                under_left.ROI = new Rectangle(j - grid_width, i + grid_height, grid_width, grid_height); ul = blockAverage(under_left, true);
                                if (ab >= ub) { bminAverage = ub; } else { bminAverage = ab; }
                                if (bminAverage >= lb) { bminAverage = lb; }
                                //aroundBlockAverage = aboveblock.GetAverage().Intensity + underblock.GetAverage().Intensity + leftblock.GetAverage().Intensity
                                //  + above_left.GetAverage().Intensity + under_left.GetAverage().Intensity;
                                aroundBlockAverage = +ab + ub + lb + al + ul;
                                bstd = (gbal2 + aroundBlockAverage) / 6;
                                //bstd = Math.Sqrt(((Math.Pow(ab, 2) + Math.Pow(ub, 2) + Math.Pow(lb, 2) + Math.Pow(al, 2) + Math.Pow(ul, 2) + Math.Pow(gbal2, 2)) / 6) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 5;
                            }
                        }
                        else if (i == bound_down)
                        {
                            block.ROI = new Rectangle(j, i, grid_width, h - bound_down);
                            gbal2 = blockAverage(block, true);
                            g2.DrawRectangle(BluePen, new Rectangle(j, i, grid_width, h - bound_down - 1));//畫框 
                            g3.DrawRectangle(BluePen, new Rectangle(j, i, grid_width, h - bound_down - 1));//畫框
                            cw = grid_width;
                            ch = h - bound_down - 1;
                            if (j == 0)
                            {
                                Bigblock.ROI = new Rectangle(j, i - grid_height, grid_width * 2, (h - bound_down) + grid_height);//4
                                aboveblock.ROI = new Rectangle(j, i - grid_height, grid_width, grid_height); ab = blockAverage(aboveblock, true);
                                rightblock.ROI = new Rectangle(j + grid_width, i, grid_width, h - bound_down); rb = blockAverage(rightblock, true);
                                above_right.ROI = new Rectangle(j + grid_width, i - grid_height, grid_width, grid_height); ar = blockAverage(above_right, true);
                                if (ab >= rb) { bminAverage = rb; } else { bminAverage = ab; }
                                //aroundBlockAverage = aboveblock.GetAverage().Intensity + rightblock.GetAverage().Intensity + above_right.GetAverage().Intensity;
                                aroundBlockAverage = ab + rb + ar;
                                bstd = (gbal2 + aroundBlockAverage) / 4;
                                //bstd = Math.Sqrt(((Math.Pow(ab, 2) + Math.Pow(rb, 2) + Math.Pow(ar, 2) + Math.Pow(gbal2, 2)) / 4) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 3;
                            }
                            else
                            {
                                Bigblock.ROI = new Rectangle(j - grid_width, i - grid_height, grid_width * 3, (h - bound_down) + grid_height);//5
                                aboveblock.ROI = new Rectangle(j, i - grid_height, grid_width, grid_height); ab = blockAverage(aboveblock, true);
                                leftblock.ROI = new Rectangle(j - grid_width, i, grid_width, h - bound_down); lb = blockAverage(leftblock, true);
                                rightblock.ROI = new Rectangle(j + grid_width, i, grid_width, h - bound_down); rb = blockAverage(rightblock, true);
                                above_left.ROI = new Rectangle(j - grid_width, i - grid_height, grid_width, grid_height); al = blockAverage(above_left, true);
                                above_right.ROI = new Rectangle(j + grid_width, i - grid_height, grid_width, grid_height); ar = blockAverage(above_right, true);
                                if (ab >= lb) { bminAverage = lb; } else { bminAverage = ab; }
                                if (bminAverage >= rb) { bminAverage = rb; }
                                //aroundBlockAverage = aboveblock.GetAverage().Intensity + leftblock.GetAverage().Intensity + rightblock.GetAverage().Intensity
                                //  + above_left.GetAverage().Intensity + above_right.GetAverage().Intensity;
                                aroundBlockAverage = ab + lb + rb + al + ar;
                                bstd = (gbal2 + aroundBlockAverage) / 6;
                                //bstd = Math.Sqrt(((Math.Pow(ab, 2) + Math.Pow(lb, 2) + Math.Pow(rb, 2) + Math.Pow(al, 2) + Math.Pow(ar, 2) + Math.Pow(gbal2, 2)) / 6) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 5;
                            }
                        }
                        else
                        {
                            block.ROI = new Rectangle(j, i, grid_width, grid_height);
                            gbal2 = blockAverage(block, true);
                            g2.DrawRectangle(BluePen, new Rectangle(j, i, grid_width, grid_height));//畫框    
                            g3.DrawRectangle(BluePen, new Rectangle(j, i, grid_width, grid_height));//畫框
                            cw = grid_width;
                            ch = grid_height;
                            if (i == 0 && j == 0)
                            {
                                Bigblock.ROI = new Rectangle(j, i, grid_width * 2, grid_height * 2);//6
                                underblock.ROI = new Rectangle(j, i + grid_height, grid_width, grid_height); ub = blockAverage(underblock, true);
                                rightblock.ROI = new Rectangle(j + grid_width, i, grid_width, grid_height); rb = blockAverage(rightblock, true);
                                under_right.ROI = new Rectangle(j + grid_width, i + grid_height, grid_width, grid_height); ur = blockAverage(under_right, true);
                                if (rb >= ub) { bminAverage = ub; } else { bminAverage = rb; }
                                //aroundBlockAverage = underblock.GetAverage().Intensity + rightblock.GetAverage().Intensity + under_right.GetAverage().Intensity;
                                aroundBlockAverage = ub + rb + ur;
                                bstd = (gbal2 + aroundBlockAverage) / 4;
                                //bstd = Math.Sqrt(((Math.Pow(ub, 2) + Math.Pow(rb, 2) + Math.Pow(ur, 2) + Math.Pow(gbal2, 2)) / 4) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 3;
                            }
                            else if (i == 0)
                            {
                                Bigblock.ROI = new Rectangle(j - grid_width, i, grid_width * 3, grid_height * 2);//7
                                underblock.ROI = new Rectangle(j, i + grid_height, grid_width, grid_height); ub = blockAverage(underblock, true);
                                leftblock.ROI = new Rectangle(j - grid_width, i, grid_width, grid_height); lb = blockAverage(leftblock, true);
                                rightblock.ROI = new Rectangle(j + grid_width, i, grid_width, grid_height); rb = blockAverage(rightblock, true);
                                under_left.ROI = new Rectangle(j - grid_width, i + grid_height, grid_width, grid_height); ul = blockAverage(under_left, true);
                                under_right.ROI = new Rectangle(j + grid_width, i + grid_height, grid_width, grid_height); ur = blockAverage(under_right, true);
                                if (lb >= ub) { bminAverage = ub; } else { bminAverage = lb; }
                                if (bminAverage >= rb) { bminAverage = rb; }
                                //aroundBlockAverage = underblock.GetAverage().Intensity + leftblock.GetAverage().Intensity + rightblock.GetAverage().Intensity
                                //  + under_left.GetAverage().Intensity + under_right.GetAverage().Intensity;
                                aroundBlockAverage = ub + lb + rb + ul + ur;
                                bstd = (gbal2 + aroundBlockAverage) / 6;
                                //bstd = Math.Sqrt(((Math.Pow(ub, 2) + Math.Pow(lb, 2) + Math.Pow(rb, 2) + Math.Pow(ul, 2) + Math.Pow(ur, 2) + Math.Pow(gbal2, 2)) / 6) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 5;
                            }
                            else if (j == 0)
                            {
                                Bigblock.ROI = new Rectangle(j, i - grid_height, grid_width * 2, grid_height * 3);//8
                                aboveblock.ROI = new Rectangle(j, i - grid_height, grid_width, grid_height); ab = blockAverage(aboveblock, true);
                                underblock.ROI = new Rectangle(j, i + grid_height, grid_width, grid_height); ub = blockAverage(underblock, true);
                                rightblock.ROI = new Rectangle(j + grid_width, i, grid_width, grid_height); rb = blockAverage(rightblock, true);
                                above_right.ROI = new Rectangle(j + grid_width, i - grid_height, grid_width, grid_height); ar = blockAverage(above_right, true);
                                under_right.ROI = new Rectangle(j + grid_width, i + grid_height, grid_width, grid_height); ur = blockAverage(under_right, true);
                                if (ab >= ub) { bminAverage = ub; } else { bminAverage = ab; }
                                if (bminAverage >= rb) { bminAverage = rb; }
                                //aroundBlockAverage = aboveblock.GetAverage().Intensity + underblock.GetAverage().Intensity + rightblock.GetAverage().Intensity
                                //  + above_right.GetAverage().Intensity + under_right.GetAverage().Intensity;
                                aroundBlockAverage = ab + ub + rb + ar + ur;
                                bstd = (gbal2 + aroundBlockAverage) / 6;
                                //bstd = Math.Sqrt(((Math.Pow(ab, 2) + Math.Pow(ub, 2) + Math.Pow(rb, 2) + Math.Pow(ar, 2) + Math.Pow(ur, 2) + Math.Pow(gbal2, 2)) / 6) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 5;
                            }
                            else
                            {
                                if (i + (grid_height * 2) == bound_down && j + (grid_width * 2) == bound_right)
                                {
                                    Bigblock.ROI = new Rectangle(j - grid_width, i - grid_height, (w - bound_right) + (grid_width * 2), (h - bound_down) + (grid_height * 2));//9_1
                                }
                                else if (j + (grid_width * 2) == bound_right)
                                {
                                    Bigblock.ROI = new Rectangle(j - grid_width, i - grid_height, (w - bound_right) + (grid_width * 2), (grid_height * 3));//9_2
                                }
                                else if (i + (grid_height * 2) == bound_down)
                                {
                                    Bigblock.ROI = new Rectangle(j - grid_width, i - grid_height, (grid_width * 3), (h - bound_down) + (grid_height * 2));//9_3
                                }
                                else { Bigblock.ROI = new Rectangle(j - grid_width, i - grid_height, grid_width * 3, grid_height * 3); }//9_4

                                if (i + grid_height == bound_down)
                                {
                                    underblock.ROI = new Rectangle(j, i + grid_height, grid_width, h - bound_down);
                                    under_left.ROI = new Rectangle(j - grid_width, i + grid_height, grid_width, h - bound_down);
                                }
                                else
                                {
                                    underblock.ROI = new Rectangle(j, i + grid_height, grid_width, grid_height);
                                    under_left.ROI = new Rectangle(j - grid_width, i + grid_height, grid_width, grid_height);
                                }
                                ub = blockAverage(underblock, true);
                                ul = blockAverage(under_left, true);
                                if (j + grid_width == bound_right)
                                {
                                    rightblock.ROI = new Rectangle(j + grid_width, i, w - bound_right, grid_height);
                                    above_right.ROI = new Rectangle(j + grid_width, i - grid_height, w - bound_right, grid_height);
                                }
                                else
                                {
                                    rightblock.ROI = new Rectangle(j + grid_width, i, grid_width, grid_height);
                                    above_right.ROI = new Rectangle(j + grid_width, i - grid_height, grid_width, grid_height);
                                }
                                rb = blockAverage(rightblock, true);
                                ar = blockAverage(above_right, true);
                                if (i + grid_height == bound_down && j + grid_width == bound_right)
                                {
                                    under_right.ROI = new Rectangle(j + grid_width, i + grid_height, w - bound_right, h - bound_down);
                                }
                                else
                                {
                                    under_right.ROI = new Rectangle(j + grid_width, i + grid_height, grid_width, grid_height);
                                }
                                ur = blockAverage(under_right, true);
                                aboveblock.ROI = new Rectangle(j, i - grid_height, grid_width, grid_height); ab = blockAverage(aboveblock, true);
                                leftblock.ROI = new Rectangle(j - grid_width, i, grid_width, grid_height); lb = blockAverage(leftblock, true);
                                above_left.ROI = new Rectangle(j - grid_width, i - grid_height, grid_width, grid_height); al = blockAverage(above_left, true);
                                if (ab >= ub) { bminAverage = ub; } else { bminAverage = ab; }
                                if (bminAverage >= lb) { bminAverage = lb; }
                                if (bminAverage >= rb) { bminAverage = rb; }
                                //aroundBlockAverage = aboveblock.GetAverage().Intensity + underblock.GetAverage().Intensity + leftblock.GetAverage().Intensity + rightblock.GetAverage().Intensity
                                //  + above_left.GetAverage().Intensity + above_right.GetAverage().Intensity + under_left.GetAverage().Intensity + under_right.GetAverage().Intensity;
                                aroundBlockAverage = ab + ub + lb + rb + al + ar + ul + ur;
                                bstd = (gbal2 + aroundBlockAverage) / 9;
                                //bstd = Math.Sqrt(((Math.Pow(ab, 2) + Math.Pow(ub, 2) + Math.Pow(lb, 2) + Math.Pow(rb, 2) + Math.Pow(al, 2) + Math.Pow(ar, 2) + Math.Pow(ul, 2) + Math.Pow(ur, 2) + Math.Pow(gbal2, 2)) / 9) - Math.Pow(bstd, 2));
                                aroundBlockAverage = aroundBlockAverage / 8;
                            }
                        }
                        double[] array = new double[] { ab, ub, lb, rb, al, ar, ul, ur };
                        double anum = 0; int nnum = 0;
                        for (int ai = 0; ai < 8; ai++)
                        {
                            if (array[ai] != -1) { anum += array[ai]; nnum++; }
                        }
                        double testAverage = anum / nnum;
                        //testAverage = aroundBlockAverage;
                        Fsize = (grid_width / 4) + 2; //字體大小調整
                        //Gray BlockAverage = block.GetAverage();gbal = BlockAverage.Intensity;Fsize = Math.Sqrt(gbal + 10) * 1.2 + 10;
                        //gbal2 = blockAverage(block);//0不算平均

                        g2.DrawString(gbal2.ToString("f1"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i));//寫區域去除0的平均灰階
                        gbal = gbal2;//用0不算平均取代全部平均

                        if (gbal - testAverage > 10 && gbal > 20)//中間平均亮度大於四周平均10以上才做對比度計算
                        {
                            double Bigstd = blockAverage(Bigblock, false);
                            double Restd = blockAverage(block, false);
                            if (testAverage == 0) { contrast = 0; } else { contrast = gbal / testAverage; }
                            double temp_Contrast_Spec = Contrast_Spec;
                            //if (gbal <= 45) { temp_Contrast_Spec += 0.35; }
                            if (contrast > temp_Contrast_Spec)
                            {
                                if (Bigstd * std_num > gbal)
                                {
                                    g2.DrawString(Bigstd.ToString("f2"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i + (grid_height / 2)));
                                    BigstdPass = false;
                                }
                                if (Restd * std_num > gbal)
                                {
                                    g3.DrawString(Restd.ToString("f2"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i + (grid_height / 2)));//寫區域所有的平均灰階
                                    stdPass = false;
                                }

                                g3.DrawString(contrast.ToString("f3"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i));//寫第一對比值   
                                //g3.DrawString(bstd.ToString("f3"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i + (grid_height / 2)));//寫標準差                       
                                if (contrast > MaxContrast)
                                {
                                    MaxContrast = contrast;
                                }
                                //if (bminAverage == 0) { contrast = 0; } else { contrast = gbal / bminAverage; }                        
                                //Fsize = Math.Sqrt(contrast + 10) * 1.5 + 10; //字體大小調整                 
                                //g3.DrawString(contrast.ToString("f2"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i + (grid_height / 2)));//寫第二對比值                            
                                //returnbool = false;//若用Y1Y2判定 要關閉   
                                oci = i; ocj = j; fch = ch; fcw = cw;
                                MaxContrastValue = 0;
                                for (int ii = oci; ii < oci + fch; ii++)
                                {
                                    for (int jj = ocj; jj < ocj + fcw; jj++)
                                    {
                                        byte a = CA.Data[ii, jj, 0];
                                        if (a > MaxContrastValue) { MaxContrastValue = a; }
                                    }
                                }//Y1Y2
                                if (contrast > Contrast_Spec1 && MaxContrastValue - luma_base > Contrast_lumaMax1) { TorF += 1; }
                                else if (contrast > Contrast_Spec && MaxContrastValue - luma_base > Contrast_lumaMax) { TorF += 1; }
                                log.OutLog("Contrast : " + contrast.ToString("f3") + " , The block luma max: " + MaxContrastValue + " - luma_base = " + (MaxContrastValue - luma_base).ToString("f2"));
                            }
                        }
                    }
                }
                if (TorF == 0) { returnbool = true; }
                else { returnbool = false; log.OutLog("Y1Y2 fail"); }
                if (!returnbool)
                {
                    if (stdPass && BigstdPass) { returnbool = true; }
                    else { returnbool = false; log.OutLog("Standard Deviation fail"); }
                }
                log.OutLog("Max Contrast : " + MaxContrast.ToString("f3"));
            }
            catch (Exception ex)
            {
                log.OutLog(ex.Message + "Block Contrast Analysis Error.");
                return false;
            }
            return returnbool;
        }

        static bool unifomityAnalysis(Image<Gray, byte> LA, int sub_m, int sub_n)
        {
            int h = LA.Height, w = LA.Width;
            int m = sub_m, //4格子數
                n = sub_n; //8格子數
            if ((m / 2) % 2 != 0 || m == 2) { m = 4; }
            if ((n / 2) % 2 != 0 || n == 2) { n = 8; }

            int grid_height = h / m, //格子高
                grid_width = w / n; //格子寬

            //image center
            int cm = m / 2, //中間格子數2
                cn = n / 2; //中間格子數4
            int midGrid_beginIndex_m = m / 4, //格子數1
                midGrid_beginIndex_n = n / 4; //格子數2

            //bounding
            int bound_down = grid_height * (m - 1);
            int bound_right = grid_width * (n - 1);

            //Var
            float maxLuma = 0,//Y_g1
                minLuma = 255;//Y_g2
            double Y_mid = 0;
            double Fsize = 0;

            Image<Gray, byte> Block = LA.Copy(),
                BlockCenter = LA.Copy();

            Block.ROI = BlockCenter.ROI = Rectangle.Empty;
            
            LOG log = new LOG();

            try
            {                
                BitDrawLine = new Bitmap(LA.Bitmap); //畫框寫數字的底
                Graphics g = Graphics.FromImage(BitDrawLine);
                Pen RedPen = new Pen(Color.Red);

                for (int i = 0; i <= bound_down; i += grid_height)
                {
                    for (int j = 0; j <= bound_right; j += grid_width)
                    {
                        //### if i,j為中心起始pixel:計算中心亮度
                        if (i == grid_height * midGrid_beginIndex_m && j == grid_width * midGrid_beginIndex_n)
                        {
                            BlockCenter.ROI = new Rectangle(j, i, grid_width * cn, grid_height * cm);
                            Gray CenterAverageLuma = BlockCenter.GetAverage();
                            Y_mid = (float)CenterAverageLuma.Intensity;
                            Fsize = Math.Sqrt(Y_mid + 10) * 1.5 + 10; //字體大小調整     
                            g.DrawString(Y_mid.ToString("f1"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i));
                        }

                        //### if i,j為中心pixel:continue
                        if (i >= grid_height * midGrid_beginIndex_m && i < grid_height * (midGrid_beginIndex_m + cm)) //高的起始點 & 終點
                        {
                            if (j == grid_width * midGrid_beginIndex_n) //寬的起始點
                            {
                                j += grid_width * cn;
                            }
                        }

                        //### if i, j為中心之外pixel: 計算平均亮度      
                        //邊界處裡 bounding condition 
                        if (i == bound_down && j == bound_right)
                        {
                            Block.ROI = new Rectangle(j, i, w - bound_right, h - bound_down);
                            g.DrawRectangle(RedPen, new Rectangle(j, i, w - bound_right - 1, h - bound_down - 1));//畫框    
                        }
                        else if (i != bound_down && j == bound_right)
                        {
                            Block.ROI = new Rectangle(j, i, w - bound_right, grid_height);
                            g.DrawRectangle(RedPen, new Rectangle(j, i, w - bound_right - 1, grid_height));//畫框    
                        }
                        else if (i == bound_down && j != bound_right)
                        {
                            Block.ROI = new Rectangle(j, i, grid_width, h - bound_down);
                            g.DrawRectangle(RedPen, new Rectangle(j, i, grid_width, h - bound_down - 1));//畫框  
                        }
                        else
                        {
                            Block.ROI = new Rectangle(j, i, grid_width, grid_height);
                            g.DrawRectangle(RedPen, new Rectangle(j, i, grid_width, grid_height));//畫框                           
                        }

                        Gray BlockAverageLuma = Block.GetAverage();
                        float gbal = (float)BlockAverageLuma.Intensity;
                        Fsize = Math.Sqrt(gbal + 10) * 1.5 + 10; //字體大小調整                      
                        g.DrawString(Block.GetAverage().Intensity.ToString("f1"), new Font("Arial", (float)Fsize), Brushes.Red, new PointF(j, i));//寫數字

                        //get grid_Ymax, grid_Ymin
                        if (gbal > maxLuma) { maxLuma = gbal; }//最大亮度
                        if (gbal < minLuma) { minLuma = gbal; }//最小                                                                     
                    }
                }
                g.Dispose();
            }
            catch (Exception ex)
            {                
                log.OutLog(ex.Message + "計算平均亮度block error ");
                return false;
            }

            Block.Dispose();
            BlockCenter.Dispose();

            try
            {
                //比值Y_ratio
                double maxratio = 0;
                double minratio = 0;

                if (Y_mid == 0)
                {
                    maxratio = 0;
                    minratio = 0;
                }
                else
                {
                    maxratio = maxLuma / Y_mid;
                    minratio = minLuma / Y_mid;
                }

                //標準差Y_std
                double Y_std = 0;
                double sum = 0;
                for (int i = 0; i < LA.Height; i++)
                {
                    for (int j = 0; j < LA.Width; j++)
                    {
                        byte a = LA.Data[i, j, 0];
                        sum += a * a;//平方和
                    }
                }

                Gray Allavg = LA.GetAverage();
                double avg = (byte)Allavg.Intensity;
                Y_std = Math.Sqrt((sum / (LA.Height * LA.Width)) - (avg * avg));
                log.OutLog("Y_mid=" + Y_mid.ToString("f2") + " , Y_ratio1=" + maxLuma.ToString("f2") + "/" + Y_mid.ToString("f2") + " , Y_ratio2=" + minLuma.ToString("f2") + "/" + Y_mid.ToString("f2"));
                log.OutLog("Y_std=" + Y_std.ToString("f2") + " , Y_ratio1=" + maxratio.ToString("f2") + " , Y_ratio2=" + minratio.ToString("f2"));
            }
            catch (Exception ex)
            {
                log.OutLog(ex.Message + "標準計算error");
                return false;              
            }

            return true;
        }

        static DenseHistogram GetHistogram(Image<Gray, byte> image)
        {
            DenseHistogram histogram;
            //Create a histogram
            histogram = new DenseHistogram(256,                     //number of bins
                                            new RangeF(0, 255));    //pixel value range
            //Compute histogram
            histogram.Calculate(new Image<Gray, Byte>[] { image },  //input image
                                false,                               //If it is true, the histogram is not cleared in the beginning
                                null                                //no mask is used
                                );
            return histogram;
        }

        static bool TurnOnOffCamera(string CamName, string TakeimgPath)
        {
            LOG log = new LOG();
            Capture CAMERA = null;
            int CamIndex = 0;
            string Cam = "";
            try
            {
                //System.Diagnostics.Stopwatch stwt = new System.Diagnostics.Stopwatch(); //引用stopwatch物件
                //stwt.Reset(); stwt.Start(); log.OutLog("stwt: " + stwt.Elapsed.TotalMilliseconds.ToString());
                
                if (CAMERA != null) { CAMERA.Dispose(); }

                DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

                for (int i = 0; i < _SystemCamereas.Length; i++)
                {
                    CamIndex = i;
                    Cam = _SystemCamereas[i].Name;
                    if (Cam.IndexOf(CamName) >= 0) { break; }
                }
                log.OutLog("Camera device Info : " + Cam);

                CAMERA = new Capture(CamIndex);//                
                if (CAMERA == null) { log.OutLog("Open camera FAIL!"); CAMERA.Dispose(); return false; }
                              
                CAMERA.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_WIDTH, 1920);
                CAMERA.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_FRAME_HEIGHT, 1080);
                log.OutLog("before CV_CAP_PROP_MAX_DC1394: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_MAX_DC1394).ToString() + ", CV_CAP_PROP_EXPOSURE: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_EXPOSURE).ToString());
                log.OutLog("before CV_CAP_PROP_GAIN: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_GAIN).ToString()+ ", CV_CAP_PROP_BRIGHTNESS: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_BRIGHTNESS).ToString());
                CAMERA.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_MAX_DC1394, 100);
                CAMERA.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_EXPOSURE, -3);
                CAMERA.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_GAIN, 255);
                CAMERA.SetCaptureProperty(CAP_PROP.CV_CAP_PROP_BRIGHTNESS, 128);
                int Count = 0;                
                while (Count < 5)
                {
                    //log.OutLog("stwt: " + stwt.Elapsed.TotalMilliseconds.ToString());
                    System.Threading.Thread.Sleep(1); CAMERA.QueryFrame(); Count++;
                }
                LCD_LEAKAGE_Exposure = CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_EXPOSURE);
                log.OutLog("after CV_CAP_PROP_MAX_DC1394: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_MAX_DC1394).ToString() + ", CV_CAP_PROP_EXPOSURE: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_EXPOSURE).ToString());
                log.OutLog("after CV_CAP_PROP_GAIN: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_GAIN).ToString() + ", CV_CAP_PROP_BRIGHTNESS: " + CAMERA.GetCaptureProperty(CAP_PROP.CV_CAP_PROP_BRIGHTNESS).ToString());
                CAMERA.QueryFrame().Save(TakeimgPath);                
                CAMERA.Stop();
                CAMERA.Dispose();
            }
            catch (Exception) { log.OutLog("Photo failure!"); return false; }
            return true;
        }

        static bool CreateLog()
        {
            try
            {
                resultLog = Application.StartupPath + "\\" + SSN + "_" + LogDateTime + ".log";
                LOG.ResultLogPath = resultLog;
                var logFile = System.IO.File.Create(resultLog); logFile.Close();
                using (StreamWriter sw = new StreamWriter(resultLog, true))
                {
                    sw.WriteLine(" Log File Create Success ,Version " +
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());

                        //typeof(ScLeakageCmd.Program).Assembly.GetName().Version);
                }
            }
            catch (Exception) { return false; }
            return true;
        }

        static void Finaladdress(bool pass, Bitmap Bit, Image<Gray, byte> Grayimage, Image<Bgr, byte> Bgrimage,
            string Bitstr, string Graystr, string Bgrstr)
        {
            string LogFolder = classINI.mainpath + "ScreenTestResult";
            string LogDate = DateTime.Now.ToString("yyyyMMdd");
            string Logfile, Logimg;
            string resultTxt, resultIni;
            LOG log = new LOG();
            //ScreenSpecINI readINI = new ScreenSpecINI();
            try
            {
                if (pass)
                {
                    //建立捷徑
                    LogFolder = classINI.mainpath + "ScreenTestResult" + "\\PASS\\" + SSN + "\\" + LogDate + "\\" + SSN + "_" + LogDateTime;
                    if (!System.IO.Directory.Exists(LogFolder)) { System.IO.Directory.CreateDirectory(LogFolder); }
                    resultTxt = LogFolder + "\\Result.txt";
                    resultIni = LogFolder + "\\Result.ini";                    
                    if (!System.IO.File.Exists(LogFolder + "\\Result.txt"))
                    {
                        var logFile = System.IO.File.Create(resultTxt); logFile.Close();
                        using (StreamWriter sw = new StreamWriter(resultTxt)) { sw.WriteLine("PASS"); }
                    }
                    if (!System.IO.File.Exists(LogFolder + "\\Result.ini"))
                    {
                        var logFile = System.IO.File.Create(resultIni); logFile.Close();
                        using (StreamWriter sw = new StreamWriter(resultIni)) { sw.WriteLine("PASS"); }
                    }
                    if (System.IO.File.Exists(resultLog))
                    {
                        log.OutLog("End Test");
                        Logfile = LogFolder + "\\" + SSN + "_" + LogDateTime + "_PASS.log";
                        System.IO.File.Move(resultLog, Logfile); //resultLog
                    }
                    if (classINI.ImagePASSLOG)
                    {
                        if (Bit != null) { Bit.Save(Logimg = LogFolder + "\\" + Bitstr + ".jpg"); }
                        if (Grayimage != null) { Grayimage.Save(Logimg = LogFolder + "\\" + Graystr + ".jpg"); }                        
                    }
                    if (Bgrimage != null) { Bgrimage.Save(Logimg = LogFolder + "\\" + Bgrstr + ".jpg"); }
                    PMSlog = LogFolder + "\\E0_" + SSN + "_" + LogDateTime + "_SFIS_Log.txt";
                    errorCode = "0";                    
                }
                else
                {
                    //建立捷徑
                    LogFolder = classINI.mainpath + "ScreenTestResult" + "\\FAIL\\" + SSN + "\\" + LogDate + "\\" + SSN + "_" + LogDateTime;
                    if (!System.IO.Directory.Exists(LogFolder)) { System.IO.Directory.CreateDirectory(LogFolder); }
                    resultTxt = LogFolder + "\\Result.txt";
                    resultIni = LogFolder + "\\Result.ini";
                    if (!System.IO.File.Exists(LogFolder + "\\Result.txt"))
                    {
                        var logFile = System.IO.File.Create(resultTxt); logFile.Close();
                        using (StreamWriter sw = new StreamWriter(resultTxt)) { sw.WriteLine("FAIL"); }
                    }
                    if (!System.IO.File.Exists(LogFolder + "\\Result.ini"))
                    {
                        var logFile = System.IO.File.Create(resultIni); logFile.Close();
                        using (StreamWriter sw = new StreamWriter(resultIni)) { sw.WriteLine("FAIL"); }
                    }
                    if (System.IO.File.Exists(resultLog))
                    {
                        log.OutLog("End Test");
                        Logfile = LogFolder + "\\" + SSN + "_" + LogDateTime + "_FAIL.log";
                        System.IO.File.Move(resultLog, Logfile); //resultLog
                    }
                    if (Bit != null) { Bit.Save(Logimg = LogFolder + "\\" + Bitstr + ".jpg"); }
                    if (Grayimage != null) { Grayimage.Save(Logimg = LogFolder + "\\" + Graystr + ".jpg"); }
                    if (Bgrimage != null) { Bgrimage.Save(Logimg = LogFolder + "\\" + Bgrstr + ".jpg"); }
                    PMSlog = LogFolder + "\\E12019_" + SSN + "_" + LogDateTime + "_SFIS_Log.txt";
                    errorCode = "12019";                   
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex + "\nFailed to create Log folder."); Console.ReadLine();
                System.IO.File.Delete(resultLog);                
            }

            if (System.IO.File.Exists("Result.txt"))
            {
                // Use a try block to catch IOExceptions, to handle the case of the file already being opened by another process.
                try { System.IO.File.Delete("Result.txt"); } catch (System.IO.IOException e) { return; }
            }
            System.IO.StreamWriter ResultFile = new System.IO.StreamWriter("Result.txt");
            if (pass) { ResultFile.WriteLine("Screen test pass"); ResultFile.Flush(); }
            else { ResultFile.WriteLine("Screen test fail"); ResultFile.Flush(); }
            
        }

        static void WritePMSlog()
        {
            var PMSlogFile = System.IO.File.Create(PMSlog);
            using (StreamWriter PMSsw = new StreamWriter(PMSlogFile))
            {                
                string testStatus = "";
                if (int.Parse(errorCode) == 0) { testStatus = "P"; }
                else { testStatus = "F"; }
                PMSsw.WriteLine("PAT_TRACK," + SSN + "," + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + ",0,0,0," + Environment.MachineName + "," + "S-LCD_LEAKAGE," + projectName + ",END_TEST," + errorCode + "," + testStatus + "," + LCD_LEAKAGE_TIME + "," + classINI.factoryID + ",1.0");

                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "LCDleakageToolVersion," + "9999.99,-9999.99," + 0 + "," + testStatus + "," + errorCode + "," + typeof(ScLeakageCmd.Program).Assembly.GetName().Version + "," + "0");                
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "TestMethod," + "9999.99,-9999.99," + LCD_LEAKAGE_method + "," + testStatus + "," + errorCode + ",TestMethod_T:0_R:1," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "LCD_Leakage_Exposure," + "9999.99,-9999.99," + LCD_LEAKAGE_Exposure + "," + testStatus + "," + errorCode + "," + "Exposure_of_Webcam" + "," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "luma_base," + "9999.99,-9999.99," + luma_base.ToString("f2") + "," + testStatus + "," + errorCode + ",LCD_Basic_brightness," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "luma_MAX," + LCD_LEAKAGE_MAX_SPEC + ",-9999.99," + LCD_LEAKAGE_MAX + "," + testStatus + "," + errorCode + ",LCD_MAX_brightness," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "ScRange_HEIGHT," + "9999.99,480," + LCD_LEAKAGE_ScRange_HEIGHT + "," + testStatus + "," + errorCode + ",Scanning_range," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "ScRange_WIDTH," + "9999.99,800," + LCD_LEAKAGE_ScRange_WIDTH + "," + testStatus + "," + errorCode + ",Scanning_range," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "MAXposition_X," + "9999.99,-9999.99," + LCD_LEAKAGE_MAXPOSITION_X + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "MAXposition_Y," + "9999.99,-9999.99," + LCD_LEAKAGE_MAXPOSITION_Y + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "GammaCorrect," + "9999.99,-9999.99," + classINI.GammaCorrect + "," + testStatus + "," + errorCode + ",GammaCorrect," + "0");
 
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Contrast," + "9999.99,-9999.99," + LCD_LEAKAGE_Contrast_A + "," + testStatus + "," + errorCode + ",SpecContrast," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Circle_X," + "9999.99,-9999.99," + LCD_LEAKAGE_POSITION_X_A + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Circle_Y," + "9999.99,-9999.99," + LCD_LEAKAGE_POSITION_Y_A + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Average," + "9999.99,-9999.99," + LCD_LEAKAGE_CIRCLE_A + "," + testStatus + "," + errorCode + ",FirstCircle_AverageBrightness," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "CircleContrastSpec," + "9999.99,-9999.99," + classINI.CircleContrastSpec + "," + testStatus + "," + errorCode + ",CircleContrastSpec," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "ContrastSpecConstant," + "9999.99,-9999.99," + classINI.ContrastSpecConstant + "," + testStatus + "," + errorCode + ",ContrastSpecConstant," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "SPEC_Contrast," + "9999.99,-9999.99," + SPEC_Contrast_A + "," + testStatus + "," + errorCode + ",Minimum_Contrast_SPEC," + "0");

                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Contrast_B," + "9999.99,-9999.99," + LCD_LEAKAGE_Contrast_B + "," + testStatus + "," + errorCode + ",B_SpecContrast," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Circle_X_B," + "9999.99,-9999.99," + LCD_LEAKAGE_POSITION_X_B + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Circle_Y_B," + "9999.99,-9999.99," + LCD_LEAKAGE_POSITION_Y_B + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Average_B," + "9999.99,-9999.99," + LCD_LEAKAGE_CIRCLE_B + "," + testStatus + "," + errorCode + ",SecondCircle_AverageBrightness," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "CircleContrastSpecB," + "9999.99,-9999.99," + classINI.CircleContrastSpecB + "," + testStatus + "," + errorCode + ",CircleContrastSpecB," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "ContrastSpecConstantB," + "9999.99,-9999.99," + classINI.ContrastSpecConstantB + "," + testStatus + "," + errorCode + ",ContrastSpecConstantB," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "SPEC_Contrast_B," + "9999.99,-9999.99," + SPEC_Contrast_B + "," + testStatus + "," + errorCode + ",Minimum_Contrast_SPEC_B," + "0");

                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Contrast_P," + "9999.99,-9999.99," + LCD_LEAKAGE_Contrast_P + "," + testStatus + "," + errorCode + ",PassContrast," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Circle_X_P," + "9999.99,-9999.99," + LCD_LEAKAGE_POSITION_X_P + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Circle_Y_P," + "9999.99,-9999.99," + LCD_LEAKAGE_POSITION_Y_P + "," + testStatus + "," + errorCode + ",Image_coordinate_position," + "0");
                PMSsw.WriteLine("PAT_TEST," + SSN + "," + "Average_P," + "9999.99,-9999.99," + LCD_LEAKAGE_CIRCLE_P + "," + testStatus + "," + errorCode + ",PassCircle_AverageBrightness," + "0");

                PMSsw.WriteLine("#End");
            }
            PMSlogFile.Close();
        }
    }
}

