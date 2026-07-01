FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY ["FlightBookingAPI.csproj", "./"]
RUN dotnet restore "./FlightBookingAPI.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet publish "FlightBookingAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generar la imagen final de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "FlightBookingAPI.dll"]