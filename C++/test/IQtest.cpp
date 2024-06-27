#include "Includes/IQtest.h"
#include "Includes/Blemish.h"
#include "Includes/Uniformity.h"

int main()
{
	IQtest iqtest;
	Blemish blemishtest;
	Uniformity uniformTest;

	int bgcolor;
	iqtest.res_img.release();

	string folder = "Images\\";
	bfs::path directory(folder);
	bfs::directory_iterator itr(directory), end_itr;

	char input;
	cout << "Enter 'B/b' for Blemish/ColorLeak Detection, or 'U/u' for Uniformity Detection !" << endl;
	cin >> input;

	for (; itr != end_itr; ++itr)
	{
		if (bfs::is_regular_file(itr->path()))
		{
			string current_file = itr->path().string();
			string cf = current_file;
			string figname;

			/*--- 
			1)read image file, tell color bg of images
			2)uniformity test 
			3)colorleak test 
			---*/
			// 1)
			cf = current_file.substr(cf.find("\\", 0) + 1, cf.length());
			iqtest.src_img = cv::imread(current_file);

			// 1.1)
			size_t found = current_file.find_last_of("_");
			string file = current_file.substr(found + 1);
			const char* bg = file.substr(0, file.find(".")).c_str();

			if (file.substr(0, file.find(".")) == "B")
				bgcolor = 1;
			else if (file.substr(0, file.find(".")) == "G")
				bgcolor = 2;
			else if (file.substr(0, file.find(".")) == "R")
				bgcolor = 3;
			else
				bgcolor = 0;


			if (iqtest.src_img.data)
			{
				if (input == 'u' || input == 'U') // 2) UNIFORMITY Detection
				{
					vector<cv::Mat> bgrChannels(3);
					cv::Mat image = iqtest.src_img.clone(), gray_img, res_img;

					cv::cvtColor(image, gray_img, CV_BGR2GRAY);
					bgrChannels[0] = bgrChannels[1] = gray_img;
					bgrChannels[2] = gray_img.clone();

					res_img = uniformTest.UniformityTest(gray_img, 0, 0);
					cv::addWeighted(bgrChannels[2], 0.0, res_img, 1.0, 0, bgrChannels[2]);
					cv::merge(bgrChannels, iqtest.res_img);
				}
				else // 3) ColorLeak Detection
				{
					iqtest.res_img = blemishtest.BlemishTest(iqtest.src_img, bgcolor);
					figname = cf.substr(0, cf.find("."));

				}

				figname = "Fig. " + cf.substr(0, cf.find("."));
				cv::namedWindow(figname, cv::WINDOW_NORMAL);
				cv::resizeWindow(figname, 640, 480);
				cv::imshow(figname, iqtest.res_img);
				cv::imwrite("./Result_" + figname + ".jpg", iqtest.res_img);
			}
			else
			{
				cerr << "Problem loading images!!!" << endl;
				return -1;
			}
		}
	}

	cv::waitKey(0); 
	return 0;
}
