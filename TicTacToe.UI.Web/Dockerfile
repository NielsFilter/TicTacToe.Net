FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["TicTacToe.UI.Web/TicTacToe.UI.Web.csproj", "TicTacToe.UI.Web/"]
RUN dotnet restore "TicTacToe.UI.Web/TicTacToe.UI.Web.csproj"
COPY . .
WORKDIR "/src/TicTacToe.UI.Web"
RUN dotnet build "TicTacToe.UI.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TicTacToe.UI.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TicTacToe.UI.Web.dll"]
