# Notes on Encryption for a GRPC service

## Tipos de certificados:
- **Certificate files**: .crt, .cer, .ca-bundle, .p7b, .p7c, .p7s, .pem
- **Keystore Files**: .key, .keystore, .jks
- **Combined certificate and key files**: .p12, .pfx, .pem

## Creando una autenticación a nivel de Canal:
- Configurar certificados: https://docs.microsoft.com/en-us/aspnet/core/security/authentication/certauth?view=aspnetcore-6.0#optional-client-certificates
- Ejemplo ( [Seguridad en aplicaciones gRPC](https://docs.microsoft.com/es-es/dotnet/architecture/grpc-for-wcf-developers/encryption) ): https://github.com/dotnet-architecture/grpc-for-wcf-developers/tree/main/FullStockTickerSample/grpc/FullStockTickerAuth/FullStockTicker

En cuanto al uso de certificados válidos, comentan que no es seguro usar certificados firmados por el propio servidor (Self-signed). En la [documentación](https://docs.microsoft.com/es-es/dotnet/architecture/grpc-for-wcf-developers/encryption) se comenta que se puede usar [Let's Encrypt](https://letsencrypt.org) como solución gratuita.
