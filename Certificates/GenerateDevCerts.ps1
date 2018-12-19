$array = @("Health.Monitor", "Svc.One", "Svc.Two", "Svc.Three")

$pfxFile = "NullPtrDev.pfx"
$crtFile = "NullPtrDev.crt"
$keyFile = "NullPtrDev.key"
$certPemFile = "NullPtrDevCert.pem"
$keyPemFile = "NullPtrDevKey.pem"

$pword = New-Guid

$env:path = $env:path + ";C:\Program Files\Git\usr\bin"

openssl req -x509 -newkey rsa:4096 -keyout $keyPemFile -out $certPemFile -nodes -subj "/C=US/ST=TX/L=DFW/O=NullPtrLtd/OU=AppDev/CN=nullptrltd.com" -reqexts "v3_req" -config .\OpenSSLConfig.cnf
openssl pkcs12 -export -out $pfxFile -inkey $keyPemFile -in $certPemFile -password pass:$pword
openssl pkcs12 -in $pfxFile -nocerts -nodes -out $keyFile -password pass:$pword
openssl pkcs12 -in $pfxFile -clcerts -nokeys -out $crtFile -password pass:$pword

$winPath = "$env:APPDATA\ASP.NET\https\$pfxFile"
$path = "/root/.aspnet/https/$pfxFile"

cp .\$pfxFile $winPath
foreach ($element in $array) {
	$proj = $element		
	cd ..\$proj
	dotnet user-secrets set Kestrel:Certificates:Development:Password $pword
	dotnet user-secrets set Kestrel:Certificates:Development:Path $path
	dotnet user-secrets set Kestrel:Certificates:Development:WinPath $winPath
	cd $PSScriptRoot
}
