#include <windows.h>
#include "Logger.h"

#include <vector>
#include <chrono>
#include <ctime> 
#include <iostream>
#include <time.h>

#define BLACK			0
#define BLUE			1
#define GREEN			2
#define CYAN			3
#define RED				4
#define MAGENTA			5
#define BROWN			6
#define LIGHTGRAY		7
#define DARKGRAY		8
#define LIGHTBLUE		9
#define LIGHTGREEN		10
#define LIGHTCYAN		11
#define LIGHTRED		12
#define LIGHTMAGENTA	13
#define YELLOW			14
#define WHITE			15


using namespace std;
Log_level Logger::logLevel=L_INFO;

void Logger::printLog(const Log_level level, const std::string & message)
{
	std::FILE* output = (level == L_ERROR ? stderr : stdout);
	if (level <= logLevel)
	{
		
		auto now = std::chrono::system_clock::now();
		auto in_time_t = std::chrono::system_clock::to_time_t(now);
		char str[26];
			
		ctime_s(str, sizeof str, &in_time_t);
			str[strlen(str) - 1] = '\0';


			HANDLE hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
	
			switch (level)
			{
				case (1): SetConsoleTextAttribute(hConsole, LIGHTRED);
					break;
				case (2): SetConsoleTextAttribute(hConsole, LIGHTGREEN);
					break;
				case (3): SetConsoleTextAttribute(hConsole, LIGHTCYAN);
					break;
			}
	
		fprintf(output, "%s ----> %s", str, std::data(message +"\n" ));
		SetConsoleTextAttribute(hConsole, WHITE);
	
	}
}

