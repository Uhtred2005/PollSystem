FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["RealtimeService/RealtimeService.csproj", "RealtimeService/"]
RUN dotnet restore "RealtimeService/RealtimeService.csproj"
COPY . .
WORKDIR "/src/RealtimeService"
RUN dotnet build "RealtimeService.csproj" -c Release -o /app/build
RUN dotnet publish "RealtimeService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RealtimeService.dll"]