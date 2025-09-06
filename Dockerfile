FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TvShop.csproj", "./"]
RUN dotnet restore "TvShop.csproj"
COPY . .
RUN dotnet build "TvShop.csproj" -c Release -o /app/build
RUN dotnet publish "TvShop.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Create necessary directories with proper permissions
RUN mkdir -p /home/appuser/.aspnet/DataProtection-Keys && \
    chown -R appuser:appuser /home/appuser/.aspnet

COPY --from=build /app/publish .
RUN chown -R appuser:appuser /app

USER appuser
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TvShop.dll"]
