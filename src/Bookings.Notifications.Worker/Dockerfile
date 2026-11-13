FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY pt-capitole.sln ./
COPY src/Bookings.Contracts/Bookings.Contracts.csproj src/Bookings.Contracts/
COPY src/Bookings.Notifications.Worker/Bookings.Notifications.Worker.csproj src/Bookings.Notifications.Worker/
RUN dotnet restore src/Bookings.Notifications.Worker/Bookings.Notifications.Worker.csproj
COPY src/Bookings.Contracts/ src/Bookings.Contracts/
COPY src/Bookings.Notifications.Worker/ src/Bookings.Notifications.Worker/
RUN dotnet publish src/Bookings.Notifications.Worker/Bookings.Notifications.Worker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Bookings.Notifications.Worker.dll"]
