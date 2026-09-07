FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SimasTurbo.csproj", "./"]
RUN dotnet restore "SimasTurbo.csproj"
COPY . .
RUN dotnet publish "SimasTurbo.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SimasTurbo.dll"]