FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY pt-capitole.sln ./
COPY src/Bookings.Domain/Bookings.Domain.csproj src/Bookings.Domain/
COPY src/Bookings.Application/Bookings.Application.csproj src/Bookings.Application/
COPY src/Bookings.Infrastructure/Bookings.Infrastructure.csproj src/Bookings.Infrastructure/
COPY src/Bookings.Contracts/Bookings.Contracts.csproj src/Bookings.Contracts/
COPY src/Bookings.Api/Bookings.Api.csproj src/Bookings.Api/
RUN dotnet restore src/Bookings.Api/Bookings.Api.csproj
COPY src/ ./src/
RUN dotnet publish src/Bookings.Api/Bookings.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Bookings.Api.dll"]
