using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

using Emgu.CV;
using Emgu.CV.Structure;

using ScreenSpec_INI;

namespace NS_baseCalculation
{
    class CL_baseCalculation
    {
        LOG_space.LOG log = new LOG_space.LOG();
        public Image<Gray, byte> thresholdImg;
        public Image<Gray, byte> originYcorp;
        public Image<Gray, byte> originbackupA;
        public Image<Gray, byte> originbackupB;
        public Image<Gray, byte> APIAverageImg;
        public Image<Gray, byte> img_corp;
        public Bitmap bit_corp;
        public double PixelAverage;
        static public double luma_base;

        public void base_Calculation(Image<Gray, byte> grayImage, out int lumaMax, out int pu, out int pv)
        {            
            //### 模糊化 -> 計算平均值         
            APIAverageImg = grayImage.SmoothBlur(classINI.Mask, classINI.Mask);
            PixelAverage = APIAverageImg.GetAverage().Intensity;

            //### 銳利化 -> 計算平均值 -> 二值化 -> 侵蝕 擴張
            Image<Gray, byte> OgrayImage = grayImage.Copy();
            //銳利化
            float[,] matrixKernel = new float[3, 3] { 
                { 0, -1, 0 }, 
                {-1, 5, -1 }, 
                { 0, -1, 0 } };
            ConvolutionKernelF matrix = new ConvolutionKernelF(matrixKernel);
            Image<Gray, float> Oresult = OgrayImage.Convolution(matrix);
            Image<Gray, byte> Yresult = Oresult.ConvertScale<byte>(1, 0);

            //計算平均值
            double YresultAverage = Yresult.GetAverage().Intensity;
            log.OutLog(YresultAverage.ToString("f2"));

            //二值化 
            int threshold = Convert.ToInt32(YresultAverage);
            thresholdImg = Yresult.ThresholdBinary(new Gray(threshold), new Gray(255));

            //侵蝕 擴張: find screen
            int EDcount = 0;
            thresholdImg = thresholdImg.Dilate(1); 
            thresholdImg = thresholdImg.Erode(1);
            for (int ED = 0; ED < 36; ED += 5)
            {
                if (YresultAverage < 9) { ED -= EDcount; }
                thresholdImg = thresholdImg.Erode(3 + ED);
                thresholdImg = thresholdImg.Dilate(3 + ED);
                if (YresultAverage < 9) { ED += EDcount; }
                EDcount += 3;
            }

            //確認切割範圍
            int max_i = 0, max_j = 0;
            int min_i = thresholdImg.Height, min_j = thresholdImg.Width;
            int draw_max_j = 0, draw_max_i = 0,
                draw_min_j = 0, draw_min_i = 0; //紀錄最小最大寬高值時候的另一個平面座標 畫圖用
            int DrawRange_h = 0, DrawRange_w = 0;

            for (int i = 0; i < thresholdImg.Height; i++)
            {
                for (int j = 0; j < thresholdImg.Width; j++)
                {
                    if (thresholdImg.Data[i, j, 0] > 1)
                    {
                        if (i > max_i) { max_i = i; draw_max_j = j; }
                        if (j > max_j) { max_j = j; draw_max_i = i; }
                        if (i < min_i) { min_i = i; draw_min_j = j; }
                        if (j < min_j) { min_j = j; draw_min_i = i; }
                    }
                }
            }

            //draw picture
            bit_corp = new Bitmap(grayImage.Bitmap);
            if (max_i - min_i <= 0) { DrawRange_h = 0; min_i = 0; } else { DrawRange_h = max_i - min_i; }
            if (max_j - min_j <= 0) { DrawRange_w = 0; min_j = 0; } else { DrawRange_w = max_j - min_j; }
            switch ((DrawRange_h + 1) % 2) { case 1: min_i += 1; break; }
            switch ((DrawRange_w + 1) % 2) { case 1: min_j += 1; break; }
            DrawRange_h = max_i - min_i;
            DrawRange_w = max_j - min_j;
            Rectangle DrawRange = new Rectangle(min_j, min_i, DrawRange_w, DrawRange_h);
            
            //### 找luma MAX和複製切割後影像
            img_corp = new Image<Gray, byte>(DrawRange_w + 1, DrawRange_h + 1);
            originYcorp = new Image<Gray, byte>(DrawRange_w + 1, DrawRange_h + 1);
            originbackupA = new Image<Gray, byte>(DrawRange_w + 1, DrawRange_h + 1);
            originbackupB = new Image<Gray, byte>(DrawRange_w + 1, DrawRange_h + 1);

            pu = 0; pv = 0; //position u & v  
            double Ratioprocess = 0.98;
            int radius = 100;
            lumaMax = 0;

            for (int i = min_i; i <= max_i; i++)
            {
                for (int j = min_j; j <= max_j; j++)
                {
                    byte aa = grayImage.Data[i, j, 0];

                    originYcorp.Data[i - min_i, j - min_j, 0] = 
                        originbackupA.Data[i - min_i, j - min_j, 0] = 
                        originbackupB.Data[i - min_i, j - min_j, 0] = aa;

                    byte a = APIAverageImg.Data[i, j, 0];
                    img_corp.Data[i - min_i, j - min_j, 0] = a;
                    if (a > lumaMax)
                    {
                        pu = j; pv = i;
                        lumaMax = a;
                    }
                }
            }

            float spu = pu, spv = pv;
            if (pu >= bit_corp.Width * Ratioprocess) { spu = pu - (radius / 2); }
            if (pu <= bit_corp.Width * (1 - Ratioprocess)) { spu = pu + (radius / 2); }
            if (pv >= bit_corp.Height * Ratioprocess) { spv = pv - (radius / 2); }
            if (pv <= bit_corp.Height * (1 - Ratioprocess)) { spv = pv + (radius / 2); }

            //### luma base計算
            float[] grayHist = new float[256];
            int RatioTreshold = classINI.Threshold_Ratio; // 30%
            int MINThreshold = 0;
            double Count = 0;
            double TotalCount = img_corp.Height * img_corp.Width;
            double ratioCount = TotalCount * RatioTreshold / 100;//占整體比例 n% 的數量(排除為10的pixel數) 
            double lightvalue = 0;
            luma_base = 0;

            //Create a histogram
            DenseHistogram baseHistogram = new DenseHistogram(256, //number of bins
                new RangeF(0, 255));  //pixel value range
            //Compute histogram
            baseHistogram.Calculate(new Image<Gray, Byte>[] { img_corp },  //input image
                false, //If it is true, the histogram is not cleared in the beginning
                null //no mask is used
                );
            baseHistogram.MatND.ManagedArray.CopyTo(grayHist, 0);

            for (int i = MINThreshold; i < 256; i++)
            {
                Count += grayHist[i];
                lightvalue += i * grayHist[i]; //計算亮度 lightvalue = lightvalue + i * grayHist[i];
                if (Count >= ratioCount) //特定的數量
                {
                    if (Math.Abs(ratioCount - Count) > 0)
                    {
                        int diff = (int)Math.Abs(ratioCount - Count);
                        Count = Count - diff; // 等於ratioCount
                        lightvalue = lightvalue - (i * diff); //扣除多算的
                    }
                    if (ratioCount == 0) { luma_base = 0; } else { luma_base = lightvalue / ratioCount; }
                    break;
                }
            }

            lumaMax -= Convert.ToInt32(luma_base);

            Graphics gg = Graphics.FromImage(bit_corp);
            Pen myPen = new Pen(Color.Red);
            gg.DrawRectangle(myPen, DrawRange);
            gg.DrawString(lumaMax.ToString("f0"), new Font("Arial", 28.0f), Brushes.Red, new PointF(spu, spv));
            gg.DrawEllipse(myPen, pu - (radius / 2), pv - (radius / 2), radius, radius);
            gg.DrawEllipse(new Pen(Color.GreenYellow), draw_max_j - 5, max_i - 5, 11, 11);
            gg.DrawEllipse(new Pen(Color.GreenYellow), max_j - 5, draw_max_i - 5, 11, 11);
            gg.DrawEllipse(new Pen(Color.White), draw_min_j - 5, min_i - 5, 11, 11);
            gg.DrawEllipse(new Pen(Color.White), min_j - 5, draw_min_i - 5, 11, 11);
            gg.Dispose();
            myPen.Dispose();
            log.OutLog("Get Luma_Average : " + PixelAverage.ToString("f2") + ", Get Luma_max : " + lumaMax.ToString("f2") + ", Get Luma_base : " + luma_base.ToString("f2"));
        }
    }
}
