#include "Includes/Blemish.h"

cv::Mat Blemish::ColorLeak(vector<cv::Mat> BGRreg, int bgcolor)
{
	cv::Mat res;
	double bratio, rratio;

	for (int i = 0; i < BGRreg[0].rows; i++)
	{
		for (int j = 0; j < BGRreg[0].cols; j++)
		{
			bratio = (double)BGRreg[1].at<uchar>(i, j) / BGRreg[0].at<uchar>(i, j);
			rratio = (double)BGRreg[1].at<uchar>(i, j) / BGRreg[2].at<uchar>(i, j);

			switch (bgcolor)
			{
			case 1: //B
			case 2: //G
				if (bratio > RBratio[bgcolor][0] && rratio > RBratio[bgcolor][1])
				{
					BGRreg[0].at<uchar>(i, j) = BGRreg[1].at<uchar>(i, j) = 0;
					BGRreg[2].at<uchar>(i, j) = 255;
				}
				break;
			default: //W
				if (bratio < RBratio[bgcolor][0] && rratio < RBratio[bgcolor][1])
				{
					BGRreg[1].at<uchar>(i, j) = BGRreg[2].at<uchar>(i, j) = 0;
					BGRreg[0].at<uchar>(i, j) = 255;
				}
				break;
			}
		}
	}

	cv::merge(BGRreg, res);

	return res;
}

cv::Mat Blemish::BlemishTest(cv::Mat image, int bgcolor)
{
	double ind = 0.0f;
	int times,
		t_blksize = blk_size,
		r, c, row, col,
		lst_seg_r, lst_seg_c, lr, lc;

	vector<int> img_vec, seg_vec;
	vector<cv::Mat> bgrChannels(3), g_bgrChannels(3), bgr_reg;
	cv::Mat src_img = image.clone(),
		seg, g_seg,
		blemdots, g_blemdots;

	b_resImage.release();

	/*---	pre-processing input image	---*/
	// rgb image: split channels
	bgr_reg.clear();
	bgr_reg.resize(3);
	cv::split(src_img, bgrChannels);

	/*---	1) decide how many segments will we include to do the detection
			2) segmentation	---*/
			// 1)
	((r = image.rows % t_blksize) > round(t_blksize / 5.0)) ?
		lst_seg_r = floor((double)image.rows / t_blksize)*t_blksize + r : lst_seg_r = floor((double)image.rows / t_blksize)*t_blksize;
	((c = image.cols % t_blksize) > round(t_blksize / 5.0)) ?
		lst_seg_c = floor((double)image.cols / t_blksize)*t_blksize + c : lst_seg_c = floor((double)image.cols / t_blksize)*t_blksize;
	cv::Mat segs_Y(round((double)lst_seg_r / t_blksize), round((double)lst_seg_c / t_blksize), CV_8U, cv::Scalar(0.0));

	// 2)
	for (row = 0; row < lst_seg_r; row += t_blksize)
	{
		for (col = 0; col < lst_seg_c; col += t_blksize)
		{
			// 1)
			((lst_seg_r - row) > t_blksize) ? lr = t_blksize : lr = (lst_seg_r - row);
			((lst_seg_c - col) > t_blksize) ? lc = t_blksize : lc = (lst_seg_c - col);
			cv::Rect crop_seg(col, row, lc, lr);

			// 2)
			seg.release();
			seg = src_img(crop_seg);
			blemdots.release();
			bgr_reg.clear();

			for (int cn = 0; cn < src_img.channels(); cn++)
			{
				cv::Mat tmp = bgrChannels[cn](crop_seg);
				bgr_reg.push_back(tmp);
			}
			blemdots = ColorLeak(bgr_reg, bgcolor); //ColorLeak Detection
			addWeighted(seg, 0.0, blemdots, 1.0, 0, seg);
		}
	}

	return src_img;
}