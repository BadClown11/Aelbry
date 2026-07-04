# Build multi-etapa de Aelbry.Web. No incluye Aelbry.Tests (no hace falta en la imagen final).
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Aelbry.sln .
COPY Aelbry/Aelbry.csproj Aelbry/
COPY Engine/Engine.csproj Engine/
COPY DataService/DataService.csproj DataService/
COPY Aelbry.Tests/Aelbry.Tests.csproj Aelbry.Tests/
RUN dotnet restore Aelbry/Aelbry.csproj

COPY . .
RUN dotnet publish Aelbry/Aelbry.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# DejaVu Sans: fuente libre usada por PdfReportExporter (Engine/BL/Export/SystemFontResolver.cs)
# para generar PDFs en Linux, donde no hay fuentes Microsoft (Arial) instaladas por defecto.
RUN apt-get update \
    && apt-get install -y --no-install-recommends fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Aelbry.dll"]
