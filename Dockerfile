# compilar con el sdk de .net
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish InsecomCotizador.csproj -c Release -o /app

# imagen final solo con el runtime (mas liviana)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "InsecomCotizador.dll"]
