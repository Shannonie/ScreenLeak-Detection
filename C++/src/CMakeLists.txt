project(${PROJECT_NAME})

file (GLOB SRC
	${CMAKE_CURRENT_SOURCE_DIR}/*.cpp
	${CMAKE_CURRENT_SOURCE_DIR}/*.c
	${CMAKE_CURRENT_SOURCE_DIR}/*.hpp
	${CMAKE_CURRENT_SOURCE_DIR}/*.h
)

file (GLOB HEADER
	${CMAKE_CURRENT_SOURCE_DIR}/../include/*.hpp
	${CMAKE_CURRENT_SOURCE_DIR}/../include/*.h
)

source_group(src FILES ${SRC})
source_group(include FILES ${HEADER})

#
# Add Include Directory
#
include_directories(${CMAKE_CURRENT_SOURCE_DIR}/../include)
foreach(subdir ${SUBDIRS})
	include_directories("${${subdir}_INCLUDE_DIRS}")
endforeach()

#
# Add Build Target
#
add_library(
	${PROJECT_NAME} SHARED
	${SRC} ${HEADER}
	${CMAKE_SOURCE_DIR}/src/version.rc
)


ADD_EXECUTABLE( anyExecutable IQtest.cpp )

#
# Setup Output Name
#
if (WIN32)
	set_target_properties(${PROJECT_NAME} PROPERTIES OUTPUT_NAME ${PROJECT_NAME})
else ()
	set_target_properties(${PROJECT_NAME} PROPERTIES OUTPUT_NAME ${PROJECT_NAME})
endif ()

#
# Add Linking Dependency
#
foreach(subdir ${SUBDIRS})
	target_link_libraries(
		${PROJECT_NAME}
		debug "${${subdir}_LIBS}"
		optimized "${${subdir}_LIBS}"
	)
#endforeach()

#
# Add Install Targets
#
install(
	TARGETS ${PROJECT_NAME}
	RUNTIME DESTINATION bin/${platform}
	LIBRARY DESTINATION bin/${platform}
	ARCHIVE DESTINATION lib/${platform}
)