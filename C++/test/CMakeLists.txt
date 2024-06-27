project(${PROJECT_NAME}_test)

file (GLOB SRC
	${CMAKE_CURRENT_SOURCE_DIR}/*.cpp
	${CMAKE_CURRENT_SOURCE_DIR}/*.c
	${CMAKE_CURRENT_SOURCE_DIR}/*.hpp
	${CMAKE_CURRENT_SOURCE_DIR}/*.h
)
source_group(src FILES ${SRC})

#
# Add Include Directory
#
include_directories(${CMAKE_CURRENT_SOURCE_DIR}/../include) 
include_directories(${cpprestsdk_INCLUDE_DIRS})
#
# Add Build Target
#
add_executable(
	${PROJECT_NAME} 
	${SRC}
	${CMAKE_SOURCE_DIR}/src/version.rc
)

#
# Add Linking Dependency
#
###link_directories(${CMAKE_BINARY_DIR}/Release)
target_link_libraries(
	${PROJECT_NAME} 
	${ROOT_PROJECT_NAME}
	${cpprestsdk_LIBS}
)

#
# Add Testing 
#
add_test(NAME ${PROJECT_NAME} COMMAND ${PROJECT_NAME})

#
# Add Install Targets
#
install(
	TARGETS ${PROJECT_NAME}
	RUNTIME DESTINATION bin/${platform}
	LIBRARY DESTINATION bin/${platform}
	ARCHIVE DESTINATION lib/${platform}
)