/* ****************************************************************************
*
* Copyright (c) Microsoft Corporation. All rights reserved.
*
*
* This file is part of Microsoft R Host.
*
* Microsoft R Host is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 2 of the License, or
* (at your option) any later version.
*
* Microsoft R Host is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with Microsoft R Host.  If not, see <http://www.gnu.org/licenses/>.
*
* ***************************************************************************/

#pragma once
#pragma warning(disable: 4996)

#define NOMINMAX

#include <ctype.h>
#include <cstdio>
#include <cstdlib>
#include <cstdarg>
#include <chrono>
#include <string>
#include <mutex>
#include <thread>
#include <vector>
#include <algorithm>
#include <sys/types.h>
#include <sys/wait.h>
#include <pwd.h>
#include <grp.h>
#include <unistd.h>
#include <signal.h>
#include <security/pam_appl.h>
#include <security/pam_misc.h>
#include <boost/endian/buffers.hpp>
#include <libexplain/execv.h>
#include <libexplain/fork.h>
#include <libexplain/waitpid.h>
#include <libexplain/execve.h>
#include <libexplain/execv.h>

#include "boost/filesystem.hpp"

namespace fs = boost::filesystem;
