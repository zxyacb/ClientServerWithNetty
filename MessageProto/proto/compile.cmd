@echo off
rem 将proto文件转换成.cs文件
set SRC_DIR=./
set DST_DIR=../generated
If Not Exist "%DST_DIR%" MD "%DST_DIR%"
protoc -I=%SRC_DIR% --csharp_out=%DST_DIR% message.proto
echo 转换成功
pause