FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TicTacToe.UI.CommandLine/TicTacToe.UI.CommandLine.csproj", "TicTacToe.UI.CommandLine/"]
RUN dotnet restore "TicTacToe.UI.CommandLine/TicTacToe.UI.CommandLine.csproj"
COPY . .
WORKDIR "/src/TicTacToe.UI.CommandLine"
RUN dotnet build "TicTacToe.UI.CommandLine.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TicTacToe.UI.CommandLine.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicTacToe.UI.CommandLine.dll"]
