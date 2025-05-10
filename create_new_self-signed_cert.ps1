# Create a new SelfSignedCertificates
New-SelfSignedCertificate -Type Custom -KeyUsage DigitalSignature -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}") -Subject "CN=LittleBitBig" -FriendlyName "LittleBitBig Code Signing Certificate"
# Export the SelfSignedCertificate to a PFX
$passwordString = Read-Host "What password do you want for your LittleBitBig Code Signing Certificate?"
$password = ConvertTo-SecureString -String $passwordString -Force -AsPlainText 
Export-PfxCertificate -cert "Cert:\CurrentUser\My\<Certificate Thumbprint>" -FilePath ~\DisplayMagicianCodeSigning.pfx -Password $password
Import-PfxCertificate -CertStoreLocation "Cert:\LocalMachine\TrustedPeople" -Password $password -FilePath -FilePath ~\DisplayMagicianCodeSigning.pfx