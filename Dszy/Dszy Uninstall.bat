@echo.请稍等，服务停止......
@echo off

@sc stop Dszy

@echo.请稍等，服务删除......
@echo off

@sc delete Dszy
@echo off
@echo.服务删除完毕！
@pause