%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe TCPService.exe
Net Start TCPService.exe
sc config TCPService.exe start= auto