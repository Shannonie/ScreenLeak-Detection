#pragma once
#ifndef UNIFORMITY_H
#define	UNIFORMITY_H

#include "IQtest.h"

class Uniformity :public IQtest
{
public:
	int		rect[2] = {3,3};
	int		rect_unit[2] = { 20,45 };

	int		grid_ref = 1;
	int		neighbor_dis = 20;

	double	filter_y = 0.5;
	double	y_diff[2] = { 0.75,0.75 };

	int		CountMask[8][8] = {
		0,0,0,0,1,0,1,1, //lt
		0,0,0,1,0,1,1,0, //rt
		0,0,0,1,0,1,1,0, //top
		0,1,1,0,1,0,0,0, //lb
		1,1,0,1,0,0,0,0, //rb
		1,1,1,1,1,0,0,0, //bottom
		0,1,1,0,1,0,1,1, //left
		1,1,0,1,0,1,1,0  //right
	};
	
	int		offsets[8][2] = { 
		//row,col coordinate related to the grid pos 
		-1, -1,
		-1, 0,
		-1, 1,
		0, -1,
		0, 1,
		1, -1,
		1, 0,
		1, 1 
	};

	cv::Mat UniformityTest(cv::Mat image, int cen_w, int cen_h);
	vector<vector<int>> DoTest(cv::Mat img, int cen_w, int cen_h);
	bool	Detection(cv::Mat img, int ind, int row, int col);
};

#endif	