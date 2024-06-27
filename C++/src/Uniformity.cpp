#include "Includes/Uniformity.h"

bool Uniformity::Detection(cv::Mat img, int ind, int row, int col)
{
	bool isd = false;

	for (int i = 0; i < sizeof(offsets) / sizeof(offsets[0]); i++)
	{
		if (CountMask[ind][i] != 0)
		{
			if ( (double)img.at<uchar>(row + offsets[i][0], col + offsets[i][1])/ ((double)img.at<uchar>(row, col))<=y_diff[0])
			{
				isd = true;
				break;
			}
		}
	}

	return isd;
}

vector<vector<int>> Uniformity::DoTest(cv::Mat img, int cen_w, int cen_h)
{
	vector<vector<int>> def_regs;

	//x-cord., y-cord., width, height
	int starts[4][4] = { 0,0,img.cols,round((double)(img.rows - cen_h) / 2), 
		0,round((double)(img.rows - cen_h) / 2) + cen_h,img.cols,round((img.rows - cen_h) / 2), 
		0,round((double)(img.rows - cen_h) / 2),round((double)(img.cols - cen_w) / 2),cen_h, 
		round((double)(img.cols - cen_w) / 2) + cen_w,round((double)(img.rows - cen_h) / 2),round((double)(img.cols - cen_w) / 2),cen_h }; 
	int offsets[8][2] = { -1,-1, -1,0, -1,1, 0,-1, 0,1, 1,-1, 1,0, 1,1 };
	int avgY;

	cv::Mat recti_img(round(img.rows / (double)rect_unit[0]), round(img.cols/ (double)rect_unit[0]), CV_8U, cv::Scalar(0));
	vector<uchar> reg_tmp_vec, reg_tmp_vec_st, reg_vec;

	for (int reg = 0; reg < 4; reg++)
	{
		// cropping 4 corner regions
		cv::Rect crop(starts[reg][0], starts[reg][1], starts[reg][2], starts[reg][3]);
		cv::Mat crop_img = img(crop),
			tmp_img = crop_img.clone();

		/*=== rectified rec*rec blocks ===*/
		reg_tmp_vec.clear();
		reg_tmp_vec.resize(tmp_img.rows*tmp_img.cols);
		reg_tmp_vec.assign(tmp_img.datastart, tmp_img.dataend);

		reg_tmp_vec_st.clear();
		reg_tmp_vec_st.resize(tmp_img.rows*tmp_img.cols);
		reg_tmp_vec_st.assign(tmp_img.datastart, tmp_img.dataend);
		sort(reg_tmp_vec_st.begin(), reg_tmp_vec_st.end());
		int	reg_max_tr = round((int)reg_tmp_vec_st[reg_tmp_vec_st.size() - 1] * filter_y);

		int l_rows = round((double)tmp_img.rows / rect_unit[0]),
			l_cols = round((double)tmp_img.cols / rect_unit[0]);

		cv::Mat reg_tmp_img(l_cols, tmp_img.rows, CV_8U, cv::Scalar(0)),
			reg_img(l_rows, l_cols, CV_8U);

		for (int blk_r = 0; blk_r < tmp_img.rows; blk_r++)
		{
			for (int blk_c = 0; blk_c < l_cols; blk_c++)
			{
				if (blk_c == (l_cols - 1) && (tmp_img.cols% rect_unit[0]) >= floor(rect_unit[0] / 2.0))
					avgY = accumulate(reg_tmp_vec.begin() + blk_r * tmp_img.cols + blk_c * rect_unit[0],
						reg_tmp_vec.begin() + blk_r * tmp_img.cols + blk_c * rect_unit[0] + (tmp_img.cols% rect_unit[0]), 0)
					/ (tmp_img.cols% rect_unit[0]);
				else
					avgY = accumulate(reg_tmp_vec.begin() + blk_r * tmp_img.cols + blk_c * rect_unit[0],
						reg_tmp_vec.begin() + blk_r * tmp_img.cols + (blk_c + 1) * rect_unit[0], 0)
					/ rect_unit[0];

				reg_tmp_img.at<uchar>(blk_c, blk_r) = avgY;
			}
		}

		reg_vec.clear();
		reg_vec.resize(reg_tmp_img.rows*reg_tmp_img.cols);
		reg_vec.assign(reg_tmp_img.datastart, reg_tmp_img.dataend);

		for (int blk_r = 0; blk_r < l_cols; blk_r++)
		{
			for (int blk_c = 0; blk_c < l_rows; blk_c++)
			{
				if ((blk_c > l_rows - 1) && (reg_tmp_img.cols% rect_unit[0]) >= floor(rect_unit[0] / 2.0))
					avgY = accumulate(reg_vec.begin() + blk_r * reg_tmp_img.cols + blk_c * rect_unit[0],
						reg_vec.begin() + blk_r * reg_tmp_img.cols + blk_c * rect_unit[0] + (reg_tmp_img.cols% rect_unit[0]), 0)
					/ (reg_tmp_img.cols% rect_unit[0]);
				else
					avgY = accumulate(reg_vec.begin() + blk_r * reg_tmp_img.cols + blk_c * rect_unit[0],
						reg_vec.begin() + blk_r * reg_tmp_img.cols + (blk_c + 1) * rect_unit[0], 0)
					/ rect_unit[0];

				reg_img.at<uchar>(blk_c, blk_r) = avgY;
			}
		}

		/*===	1)diagnose position of each rect
				2)neighbors comparison ===*/
		bool isdefect;

		for (int i = 0; i < reg_img.rows; i++)
		{
			for (int j = 0; j < reg_img.cols; j++)
			{
				if ((int)reg_img.at<uchar>(i, j) > reg_max_tr)
				{
					int pos = 8;

					if (i == 0) // top corner
					{
						if (j == 0)
							pos = 0;
						else if (j == (reg_img.cols - 1))
							pos = 1;
						else
							pos = 2;
					}
					else if (i == (reg_img.rows - 1)) // bottom coner
					{
						if (j == 0)
							pos = 3;
						else if (j == (reg_img.cols - 1))
							pos = 4;
						else
							pos = 5;
					}
					else // sides
					{
						if (j == 0)
							pos = 6;
						else if (j == (reg_img.cols - 1))
							pos = 7;
					}

					if (isdefect = Detection(reg_img, pos, i, j))
					{
						vector<int> regs;
						regs.push_back(j* rect_unit[0] + starts[reg][0]); //col
						regs.push_back(starts[reg][1] + i * rect_unit[0]); //row
						(j*rect_unit[0] > crop_img.cols) ? regs.push_back((crop_img.cols% rect_unit[0])) : regs.push_back(rect_unit[0]); //width
						(i*rect_unit[0] > crop_img.rows) ? regs.push_back((crop_img.rows% rect_unit[0])) : regs.push_back(rect_unit[0]); //height

						def_regs.push_back(regs);
					}
				}
			}
		}
	}

	return def_regs;
}

cv::Mat Uniformity::UniformityTest(cv::Mat image, int cen_w, int cen_h)
{
	cv::Mat tmp_img(image.cols, image.rows, CV_8U, cv::Scalar(0)),
		avg_img(image.rows, image.cols, CV_8U, cv::Scalar(0)),
		res_img = image.clone();

	vector<int> img_vec, img_tmp_vec;
	vector<vector<int>> uniform_res;

	/*===== 1.1) average pixels Y in rect. of image:horizontal pixels  &
			1.2) average pixels Y in rect. of image:vertical pixels =====*/
			//1.1)
	int avgY;
	img_vec.assign(image.datastart, image.dataend);
	for (int i_row = 0; i_row < image.rows; i_row++)
	{
		for (int i_col = 0; i_col < image.cols; i_col++)
		{
			if ((i_col + rect[1] - 1) >= image.cols)
				avgY = accumulate(img_vec.begin() + i_row * image.cols + i_col,
					img_vec.begin() + (i_row + 1) * image.cols, 0) / (image.cols - i_col);

			else
				avgY = accumulate(img_vec.begin() + i_row * image.cols + i_col,
					img_vec.begin() + i_row * image.cols + i_col + rect[1], 0) / rect[1];

			tmp_img.at<uchar>(i_col, i_row) = avgY;
		}
	}

	//1.2)
	img_tmp_vec.assign(tmp_img.datastart, tmp_img.dataend);
	for (int i_row = 0; i_row < tmp_img.rows; i_row++)
	{
		for (int i_col = 0; i_col < tmp_img.cols; i_col++)
		{
			if ((i_col + rect[0] - 1) >= tmp_img.cols)
				avgY = accumulate(img_tmp_vec.begin() + i_row * tmp_img.cols + i_col,
					img_tmp_vec.begin() + (i_row + 1) * tmp_img.cols, 0) / (tmp_img.cols - i_col);
			else
				avgY = accumulate(img_tmp_vec.begin() + i_row * tmp_img.cols + i_col,
					img_tmp_vec.begin() + i_row * tmp_img.cols + i_col + rect[0], 0) / rect[0];

			avg_img.at<uchar>(i_col, i_row) = avgY;
		}
	}

	uniform_res = DoTest(avg_img, cen_w, cen_h);

	while (uniform_res.size())
	{
		vector<int> d_rect = uniform_res[uniform_res.size() - 1];
		uniform_res.pop_back();

		//cols turn red for defected grids
		for (int col = d_rect[0]; col < (d_rect[0] + d_rect[2]); col++)
			res_img.at<uchar>(d_rect[1], col) = res_img.at<uchar>(d_rect[1] + d_rect[3] - 1, col) = 255;

		//cols turn red for defected grids
		for (int row = d_rect[1]; row < (d_rect[1] + d_rect[3]); row++)
			res_img.at<uchar>(row, d_rect[0]) = res_img.at<uchar>(row, d_rect[0] + d_rect[2] - 1) = 255;
	}

	return res_img;
}