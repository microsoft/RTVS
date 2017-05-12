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
#include <security/pam_appl.h>
#include <security/pam_misc.h>
#include <boost/endian/buffers.hpp>

#include "picojson.h"
#include "util.h"

FILE *input = nullptr, *output = nullptr, *error = nullptr;

static constexpr int RTVS_AUTH_OK           = 0;
static constexpr int  RTVS_AUTH_INIT_FAILED = 200;
static constexpr int  RTVS_AUTH_BAD_INPUT   = 201;
static constexpr int  RTVS_AUTH_NO_INPUT    = 202;

static constexpr char RTVS_JSON_MSG_NAME[] = "name";
static constexpr char  RTVS_JSON_MSG_USERNAME[] = "username";
static constexpr char  RTVS_JSON_MSG_PASSWORD[] = "password";

static constexpr char  RTVS_RESPONSE_TYPE_PAM_INFO[] = "pam-info";
static constexpr char  RTVS_RESPONSE_TYPE_PAM_ERROR[] = "pam-error";
static constexpr char  RTVS_RESPONSE_TYPE_JSON_ERROR[] = "json-error";
static constexpr char  RTVS_RESPONSE_TYPE_RTVS_RESULT[] = "rtvs-result";
static constexpr char  RTVS_RESPONSE_TYPE_RTVS_ERROR[] = "rtvs-error";

std::string read_string(FILE* stream) {
    boost::endian::little_uint32_buf_t data_size;
    if (fread(&data_size, sizeof data_size, 1, stream) != 1) {
        return std::string();
    }

    std::string str(data_size.value(), '\0');
    if (!str.empty()) {
        if (fread(&str[0], str.size(), 1, stream) != 1) {
            return std::string();
        }
    }

    return str;
}

void write_string(FILE* stream, const std::string &data) {
    boost::endian::little_uint32_buf_t data_size(static_cast<uint32_t>(data.size()));

    if (fwrite(&data_size, sizeof data_size, 1, stream) == 1) {
        if (fwrite(data.data(), data.size(), 1, stream) == 1) {
            fflush(stream);
        } else {
            std::terminate();
        }
    } else {
        std::terminate();
    }
}

template<class Arg, class... Args>
inline void write_json(Arg&& arg, Args&&... args) {
    picojson::array msg;
    msg.push_back(picojson::value(std::forward<Arg>(arg)));
    append_json(msg, std::forward<Args>(args)...);
    write_string(output, picojson::value(msg).serialize());
}

int rtvs_conv(int num_msg, const pam_message **msgm, pam_response **response, void *appdata_ptr) {
    int count = 0;
    pam_response *reply;

    if (num_msg < 0) {
        return PAM_CONV_ERR;
    }

    reply = (pam_response*) calloc(num_msg, sizeof(pam_response));
    if (reply == nullptr) {
        return PAM_CONV_ERR;
    }

    for (count = 0; count < num_msg; ++count) {
        char *str = nullptr;
        switch (msgm[count]->msg_style) {
        case PAM_PROMPT_ECHO_OFF:
            str = strdup((char*)appdata_ptr);
            break;
        case PAM_PROMPT_ECHO_ON:
            str = strdup((char*)appdata_ptr);
            break;
        case PAM_ERROR_MSG:
            write_json(RTVS_RESPONSE_TYPE_PAM_ERROR, msgm[count]->msg);
            break;
        case PAM_TEXT_INFO:
            write_json(RTVS_RESPONSE_TYPE_PAM_INFO, msgm[count]->msg);
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
    int pam_err = 0;
    struct pam_conv conv = {
        rtvs_conv,
        (void*)password
    };

    SCOPE_WARDEN(pam_end, {
        pam_end(pamh, pam_err);
    });

    if ((pam_err = pam_start("rtvs_auth", user, &conv, &pamh)) != PAM_SUCCESS || pamh == nullptr) {
        write_json(RTVS_RESPONSE_TYPE_PAM_ERROR, pam_strerror(pam_err));
        return RTVS_AUTH_INIT_FAILED;
    }

    if ((pam_err = pam_authenticate(pamh, 0)) != PAM_SUCCESS) {
        write_json(RTVS_RESPONSE_TYPE_PAM_ERROR, pam_strerror(pam_err));
        return RTVS_AUTH_BAD_INPUT;
    }

    if ((pam_err = pam_acct_mgmt(pamh, 0)) != PAM_SUCCESS) {
        // This can fail if the user's password has expired
        write_json(RTVS_RESPONSE_TYPE_PAM_ERROR, pam_strerror(pam_err));
        return RTVS_AUTH_BAD_INPUT;
    }
    return RTVS_AUTH_OK;
}

std::string get_user_home(const std::string &username) {
    struct passwd *pw = getpwnam(username.c_str());
    if (pw && pw->pw_dir && pw->pw_dir[0] != '\0') {
        return std::string(strdup(pw->pw_dir));
    }
    return std::string();
}

int authenticate_only(const picojson::object& json) {
    std::string username(json[RTVS_JSON_MSG_USERNAME].get<std::string>());
    std::string password(json[RTVS_JSON_MSG_PASSWORD].get<std::string>());

    if (username.empty() || password.empty()) {
        return RTVS_AUTH_NO_INPUT;
    }

    int auth_result = rtvs_authenticate(username.c_str(), password.c_str());

    if (auth_result == PAM_SUCCESS) {
        std::string user_home = get_user_home(username);
        write_json(RTVS_RESPONSE_TYPE_RTVS_RESULT, user_home);
    }
    return auth_result;
}

int main(int argc, char **argv) {
    input = fdopen(dup(fileno(stdin)), "rb");
    setvbuf(input, NULL, _IONBF, 0);
    output = fdopen(dup(fileno(stdout)), "wb");
    setvbuf(output, NULL, _IONBF, 0);
    error = fdopen(dup(fileno(stderr)), "wb");
    setvbuf(error, NULL, _IONBF, 0);

    freopen("/dev/null", "rb", stdin);
    freopen("/dev/null", "wb", stdout);

    picojson::value json_value;
    std::string json_err = picojson::parse(json_value, read_string(input));

    if (!json_err.empty()) {
        write_json(RTVS_RESPONSE_TYPE_JSON_ERROR, json_err);
        return RTVS_AUTH_BAD_INPUT;
    }

    if (!json_value.is<picojson::object>()) {
        write_json(RTVS_RESPONSE_TYPE_RTVS_ERROR, "Error_RunAsUser_InputFormatInvalid");
        return RTVS_AUTH_BAD_INPUT;
    }

    picojson::object json = json_value.get<picojson::object>();
    std::string msg_name = json[RTVS_JSON_MSG_NAME].get<std::string>();

    
    if (msg_name == "AuthOnly") {
        return authenticate_only(json);
    } else if (msg_name == "AuthAndRun") {
        return 0; //TODO : authenticate and run
    } else {
        write_json(RTVS_RESPONSE_TYPE_RTVS_ERROR, "Error_RunAsUser_MessageTypeInvalid");
        return RTVS_AUTH_BAD_INPUT;
    }
}

// g++ -std=c++14 -fexceptions -fpermissive -O0 -ggdb -I../src -I../lib/picojson -c ../src/*.c*
// g++ -g -o Microsoft.R.Host.RunAsUser.out ./*.o -lpthread -L/usr/lib/x86_64-linux-gnu -lpam
