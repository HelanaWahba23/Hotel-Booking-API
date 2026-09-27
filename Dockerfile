FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY NuGet.Config HotelBooking.sln ./
COPY HotelBooking.Api/HotelBooking.Api.csproj HotelBooking.Api/
RUN dotnet restore HotelBooking.sln --configfile NuGet.Config
COPY . .
RUN dotnet publish HotelBooking.Api/HotelBooking.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HotelBooking.Api.dll"]
