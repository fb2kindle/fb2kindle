@echo off

rmdir /s /q .vs
rmdir /s /q .idea

rmdir /s /q .\Fb2Kindle\bin
rmdir /s /q .\Fb2Kindle\obj

rmdir /s /q .\LibraryCleaner\bin
rmdir /s /q .\LibraryCleaner\obj

rmdir /s /q .\other\PrettyXml\bin
rmdir /s /q .\other\PrettyXml\obj

rmdir /s /q .\jail\obj
del /S ".\jail\bin\*.pdb"
del /S ".\jail\bin\*.xml"
del /S ".\Fb2Kindle\FodyWeavers.xsd"
del /S ".\LibraryCleaner\FodyWeavers.xsd"
