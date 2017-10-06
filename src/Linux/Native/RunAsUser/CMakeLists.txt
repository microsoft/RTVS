cmake_minimum_required(VERSION 2.7)
project(RunAsUser)

if(${CMAKE_SOURCE_DIR} STREQUAL ${CMAKE_BINARY_DIR})
    message(FATAL_ERROR "In-source builds are not allowed. Please use the ./build.sh helper script.")
endif()

if(NOT CMAKE_RUNTIME_OUTPUT_DIRECTORY)
    set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_HOME_DIRECTORY}/bin/${CMAKE_BUILD_TYPE})
endif()

# TODO: add -fstack-protector
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++14 -fpermissive")

file(GLOB src "src/*.h" "src/*.cpp" )

add_executable(Microsoft.R.Host.RunAsUser ${src})

if(NOT APPLE)
set_target_properties(Microsoft.R.Host.RunAsUser PROPERTIES LINK_FLAGS "-static-libgcc -static-libstdc++")
else()
set_target_properties(Microsoft.R.Host.RunAsUser PROPERTIES COMPILE_DEFINITIONS _APPLE)
endif()

if("${TARGET_ARCH}" STREQUAL "x86")
    set_target_properties(Microsoft.R.Host.RunAsUser PROPERTIES COMPILE_FLAGS "-m32" LINK_FLAGS "-m32")
endif()

include_directories("${CMAKE_SOURCE_DIR}/../../Lib/picojson")

set(Boost_USE_STATIC_LIBS ON)
find_package(Boost 1.58.0 REQUIRED COMPONENTS filesystem system)
include_directories(${Boost_INCLUDE_DIRS})
target_link_libraries(Microsoft.R.Host.RunAsUser ${Boost_LIBRARIES})

find_library(PAM_LIBRARY pam)
if(NOT PAM_LIBRARY)
    message(FATAL_ERROR "pam not found")
endif()

if(NOT APPLE)
    find_library(EXPLAIN_LIBRARY explain)
    if(NOT EXPLAIN_LIBRARY)
        message(FATAL_ERROR "explain not found")
    endif()
    target_link_libraries(Microsoft.R.Host.RunAsUser pthread ${EXPLAIN_LIBRARY} ${PAM_LIBRARY})
else()
target_link_libraries(Microsoft.R.Host.RunAsUser pthread ${PAM_LIBRARY})
endif()
