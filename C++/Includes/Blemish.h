#ifndef BLEMISH_H
#define	BLEMISH_H

#include "IQtest.h"

typedef union
{
	vector<char>    Rc;
	vector<double>	Rr;
	vector<char>	Bc;
	vector<double>	Br;
} RBratios;


class Blemish :public IQtest
{
public:
	/* --- spec for r/b gain: {W,B,G,R}[bratio, rratio] ---*/
	double	RBratio[4][2] = { 0.9,0.9, //W
							0.72,11.4, //B
							4.9,1.5 , //G
							0.66,0.13 //R
							};

	int	blk_size = 80;
	int	diff = 15;
	int	seg_diff = 20;
	int	Y_thr = 0;

	cv::Mat b_grayImage,
			b_resImage;

	cv::Mat	BlemishTest(cv::Mat image, int bgcolor);
	cv::Mat	ColorLeak(vector<cv::Mat> BGRreg, int bgcolor);
};

#endif	