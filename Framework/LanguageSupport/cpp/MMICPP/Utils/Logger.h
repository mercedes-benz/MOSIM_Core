//experimental class for logging

#pragma once

#include <string>

enum Log_level
{
	L_SILENT,
	L_ERROR,
	L_INFO,
	L_DEBUG,
};


class Logger
{
public:
	static Log_level logLevel;
	static void printLog(const Log_level level, const std::string & message);
};

