FROM microsoft/dotnet:2.2-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src
COPY ["TobinTaxer/TobinTaxer.csproj", "TobinTaxer/"]
COPY ["TobinTaxer.IntegrationTests/TobinTaxer.IntegrationTests.csproj", "TobinTaxer.IntegrationTests/"]
RUN dotnet restore "TobinTaxer/TobinTaxer.csproj"
RUN dotnet restore "TobinTaxer.IntegrationTests/TobinTaxer.IntegrationTests.csproj"
COPY . .
WORKDIR "/src/TobinTaxer"
RUN dotnet build "TobinTaxer.csproj" -c Release -o /app
RUN dotnet test ..
FROM build AS publish
RUN dotnet publish "TobinTaxer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "TobinTaxer.dll"]