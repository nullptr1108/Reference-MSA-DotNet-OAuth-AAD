$array = @("Health.Monitor", "Svc.Target")

foreach ($element in $array) {
	$pword = New-Guid
	$proj = $element	
	$path = "$env:APPDATA\ASP.NET\https\$proj.pfx"
	dotnet dev-certs https --trust --check
	dotnet dev-certs https --trust -ep $path -p $pword	
	cd $proj
	dotnet user-secrets set Kestrel:Certificates:Development:Password $pword
	dotnet user-secrets set Kestrel:Certificates:Development:Path "/root/.aspnet/https/$proj.pfx"
	dotnet user-secrets set Kestrel:Certificates:Development:WinPath $path
	cd $PSScriptRoot
}
