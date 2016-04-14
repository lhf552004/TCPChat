@ECHO OFF
if exist $(TargetDir)\Resources
	md $(TargetDir)\Resources
xcopy $(ProjectDir)\Resources\*.* $(TargetDir)