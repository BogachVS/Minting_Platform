FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src
ADD . .

RUN dotnet restore "PiggyGame/PiggyGame.csproj"
RUN dotnet build PiggyGame -c Release -o /app/build

FROM build AS publish
RUN dotnet publish PiggyGame -c Release -r linux-x64 --no-self-contained -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0

WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "PiggyGame.dll"]

EXPOSE 8080
