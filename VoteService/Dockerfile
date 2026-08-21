FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["VoteService/VoteService.csproj", "VoteService/"]
RUN dotnet restore "VoteService/VoteService.csproj"
COPY . .
WORKDIR "/src/VoteService"
RUN dotnet build "VoteService.csproj" -c Release -o /app/build
RUN dotnet publish "VoteService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "VoteService.dll"]