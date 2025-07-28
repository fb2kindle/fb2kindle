@echo off

rmdir /s /q .vs
rmdir /s /q .idea

rmdir /s /q .\Fb2Kindle\bin
rmdir /s /q .\Fb2Kindle\obj

rmdir /s /q .\Fb2Thumbnails\bin
rmdir /s /q .\Fb2Thumbnails\obj

del /S ".\Fb2Kindle\FodyWeavers.xsd"
