#ifndef IQTEST_H
#define	IQTEST_H

#include <opencv2/opencv.hpp>
#include <opencv2/core/core.hpp>
#include <opencv2/highgui/highgui.hpp>


#include <boost/filesystem.hpp>
namespace bfs = boost::filesystem;

#include <iostream>
#include <string>
#include <stdio.h>
#include <stdarg.h>
#include <math.h>
#include <algorithm>
#include <vector>
#include <numeric>
using namespace std;

class IQtest
{
public:
	int	margin = 12;

	cv::Mat src_img,
			res_img;
};

#endif