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

#include <ctype.h>
#include <stdio.h>
#include <stdlib.h>
#include <pwd.h>
#include <unistd.h>
#include <string>
#include <mutex>
#include <security/pam_appl.h>
#include <security/pam_misc.h>
#include <boost/endian/buffers.hpp>
#include "util.h"

#define RTVS_AUTH_OK          0
#define RTVS_AUTH_INIT_FAILED 200
#define RTVS_AUTH_BAD_INPUT   201
#define RTVS_AUTH_NO_INPUT    202


int rtvs_conv(int num_msg, const struct pam_message **msgm, struct pam_response **response, void *appdata_ptr)
{
    int count = 0;
    struct pam_response *reply;

    if (num_msg < 0) {
        return PAM_CONV_ERR;
    }

    reply = (struct pam_response*) calloc(num_msg, sizeof(struct pam_response));
    if (reply == nullptr) {
        return PAM_CONV_ERR;
    }

    for (count = 0; count < num_msg; ++count) {
        char *str = nullptr;
        switch (msgm[count]->msg_style) {
        case PAM_PROMPT_ECHO_OFF:
            printf("Prompt ECHO OFF \n");
            str = strdup((char*)appdata_ptr);
            break;
        case PAM_PROMPT_ECHO_ON:
            printf("Prompt ECHO ON \n");
            break;
        case PAM_ERROR_MSG:
            printf("Error: %s \n", msgm[count]->msg);
            break;
        case PAM_TEXT_INFO:
            printf("Info: %s \n", msgm[count]->msg);
            break;
        case PAM_BINARY_PROMPT:
            printf("Prompt ECHO OFF \n");
            str = strdup((char*)appdata_ptr);
            break;
        }

        if (str) {
            reply[count].resp_retcode = 0;
            reply[count].resp = str;
            str = nullptr;
        }
    }

    *response = reply;
    reply = nullptr;

    return PAM_SUCCESS;
}

int rtvs_authenticate(const char *user, const char* password) {
    pam_handle_t *pamh = nullptr;
    int retval = 0;
    struct pam_conv conv = {
        rtvs_conv,
        (void*)password
    };

    SCOPE_WARDEN(pam_end, {
        pam_end(pamh, retval);
    });

    retval = pam_start("rtvs_auth", user, &conv, &pamh);

    if (retval != PAM_SUCCESS || pamh == nullptr) {
        return RTVS_AUTH_INIT_FAILED;
    }

    if (retval == PAM_SUCCESS) {
        retval = pam_authenticate(pamh, 0);
    }

    return (retval == PAM_SUCCESS) ? RTVS_AUTH_OK : RTVS_AUTH_BAD_INPUT;
}

std::string read_string(FILE* input) {
    boost::endian::little_uint32_buf_t data_size;
    if (fread(&data_size, sizeof data_size, 1, input) != 1) {
        return std::string();
    }

    std::string str(data_size.value(), '\0');
    if (!str.empty()) {
        if (fread(&str[0], str.size(), 1, input) != 1) {
            return std::string();
        }
    }

    return str;
}

std::mutex output_lock;
void write_string(FILE* output, std::string &data) {
    boost::endian::little_uint32_buf_t data_size(static_cast<uint32_t>(data.size()));

    std::lock_guard<std::mutex> lock(output_lock);
    if (fwrite(&data_size, sizeof data_size, 1, output) == 1) {
        if (fwrite(data.data(), data.size(), 1, output) == 1) {
            fflush(output);
        }
    }
}

std::string get_user_home(std::string &username) {
    struct passwd *pw = getpwnam(username.c_str());
    if (pw && pw->pw_dir && pw->pw_dir[0] != '\0') {
        return std::string(strdup(pw->pw_dir));
    }
    return std::string();
}

int main(int argc, char **argv) {
    bool auth_flag = false;
    bool run_flag = false;
    int opt = 0;

    while ((opt = getopt(argc, argv, "ar")) != -1) {
        switch (opt) {
        case 'a':
            auth_flag = true;
            break;
        case 'r':
            run_flag = true;
            break;
        }
    }

    FILE *input = nullptr, *output = nullptr;

    int auth_result = -2;
    if (auth_flag || run_flag) {
        input = fdopen(dup(fileno(stdin)), "rb");
        setvbuf(input, NULL, _IONBF, 0);
        output = fdopen(dup(fileno(stdout)), "wb");
        setvbuf(output, NULL, _IONBF, 0);

        freopen("/dev/null", "rb", stdin);
        freopen("/dev/null", "wb", stdout);

        std::string username = read_string(input);
        std::string password = read_string(input);

        if (username.empty() || password.empty()) {
            return RTVS_AUTH_NO_INPUT;
        }

        auth_result = rtvs_authenticate(username.c_str(), password.c_str());

        if (!run_flag) {
            if (auth_result == PAM_SUCCESS) {
                std::string user_home = get_user_home(username);
                write_string(output, user_home);
            }
            return auth_result;
        }
    }

    if (run_flag) {
        // TODO: fork and exec here
    }

    return 0;
}
