cmake_minimum_required(VERSION 3.9)
project(ctest C)

set(CMAKE_C_STANDARD 99)

add_executable(open62541.c open62541.h cgoua_api.h cgoua_api.c)