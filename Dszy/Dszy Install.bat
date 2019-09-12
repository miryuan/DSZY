@echo.请稍等，服务启动......
@echo off
@sc create Dszy binPath= %cd%\Dszy.exe
@net start Dszy 
@sc config Dszy  start= AUTO
@sc description Dszy 这是一个测试服务
@echo off
@echo.启动完毕！
@pause