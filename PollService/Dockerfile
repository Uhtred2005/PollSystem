FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PollService/PollService.csproj", "PollService/"]
RUN dotnet restore "PollService/PollService.csproj"
COPY . .
WORKDIR "/src/PollService"
RUN dotnet build "PollService.csproj" -c Release -o /app/build
RUN dotnet publish "PollService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PollService.dll"]