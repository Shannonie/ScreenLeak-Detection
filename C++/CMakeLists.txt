cmake_minimum_required(VERSION 3.8)
get_filename_component(current_dirname ${CMAKE_CURRENT_LIST_DIR} NAME)
project(${current_dirname})
set(ROOT_PROJECT_NAME ${PROJECT_NAME})

#
# set C++ version
# 
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} /std:c++14")

#
# Define Macro
#
MACRO(SUBDIRLIST subdir curdir)
  file(GLOB children RELATIVE ${curdir} ${curdir}/*)
  set(dirlist "")
  foreach(child ${children})
    if(IS_DIRECTORY ${curdir}/${child})
      list(APPEND dirlist ${child})
    endif()
  endforeach()
  set(${subdir} ${dirlist})
ENDMACRO()

MACRO(PARSE_VERSION_FILE)
	file(READ ${CMAKE_SOURCE_DIR}/src/version.h VERSION_FILE)
	
	string(REGEX MATCH "DIT_VERSION_MAJ [0-9]*" VER_MAJOR ${VERSION_FILE})
	STRING(REGEX REPLACE "DIT_VERSION_MAJ " "" VER_MAJOR ${VER_MAJOR})
	message(VER_MAJOR: ${VER_MAJOR})

	string(REGEX MATCH "DIT_VERSION_MID [0-9]*" VER_MIDDLE ${VERSION_FILE})
	STRING(REGEX REPLACE "DIT_VERSION_MID " "" VER_MIDDLE ${VER_MIDDLE})
	message(VER_MIDDLE: ${VER_MIDDLE})

	string(REGEX MATCH "DIT_VERSION_MIN [0-9]*" VER_MINOR ${VERSION_FILE})
	STRING(REGEX REPLACE "DIT_VERSION_MIN " "" VER_MINOR ${VER_MINOR})
	message(VER_MINOR: ${VER_MINOR})

	string(REGEX MATCH "DIT_VERSION_BULD [0-9]*" VER_BUILD ${VERSION_FILE})
	STRING(REGEX REPLACE "DIT_VERSION_BULD " "" VER_BUILD ${VER_BUILD})
	message(VER_BUILD: ${VER_BUILD})

	string(REGEX MATCH "DIT_VERSION_HASH [0-9, a-z, A-Z]*" VER_HASH ${VERSION_FILE})
	STRING(REGEX REPLACE "DIT_VERSION_HASH " "" VER_HASH ${VER_HASH})
	message(VER_HASH: ${VER_HASH})

	string(REGEX MATCH "(2[0-9][0-9][0-9])-(0[1-9]|1[0-2])-(0[1-9]|1[0-9]|2[0-9]|3[0-1])" DATE ${VERSION_FILE})
	message(DATE: ${DATE})

	string(REGEX MATCH "(2[0-9][0-9][0-9])" DATE_YEAR ${DATE})

	configure_file(
		${CMAKE_SOURCE_DIR}/src/version.rc.in 
		${CMAKE_SOURCE_DIR}/src/version.rc
		@ONLY
	)
ENDMACRO()

#
# Setup INCLUDE_DIRS and LIBS for 3rdparty library
# 
if(EXISTS ${CMAKE_CURRENT_SOURCE_DIR}/3rdparty)
	SUBDIRLIST(SUBDIRS ${CMAKE_CURRENT_SOURCE_DIR}/3rdparty)
	foreach(subdir ${SUBDIRS})
		find_package(${subdir} PATHS ${CMAKE_CURRENT_SOURCE_DIR}/3rdparty/${subdir} REQUIRED)
		endforeach()
endif()

#
# Setup Project Output Paths
#
if(WIN32)
	set(platform win64)
else()
	set(platform linux64)
endif()
set(CMAKE_INSTALL_PREFIX ${CMAKE_CURRENT_SOURCE_DIR})
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/lib)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/bin)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_CURRENT_BINARY_DIR}/bin)

#
# Parse Revision and Release Date
#
PARSE_VERSION_FILE()

#
# Add Build Targets
#
add_subdirectory(src)
add_subdirectory(test)