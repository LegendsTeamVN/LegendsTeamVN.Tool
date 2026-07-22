FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["LegendsTeamVN.Tool/LegendsTeamVN.Tool.csproj", "LegendsTeamVN.Tool/"]
RUN dotnet restore "LegendsTeamVN.Tool/LegendsTeamVN.Tool.csproj"

COPY . .
WORKDIR "/src/LegendsTeamVN.Tool"
RUN dotnet build "LegendsTeamVN.Tool.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LegendsTeamVN.Tool.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LegendsTeamVN.Tool.dll"]
