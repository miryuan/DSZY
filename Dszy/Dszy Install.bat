@echo.���Եȣ���������......
@echo off
@sc create Dszy binPath= F:\Code\DSZY\Dszy\bin\Release\Dszy.exe
@net start Dszy 
@sc config Dszy  start= AUTO
@sc description Dszy ����һ�����Է���
@echo off
@echo.������ϣ�
@pause