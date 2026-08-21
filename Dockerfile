FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# ääÓÎ ßá Ôí: ÇáÜ solution + ÇáãÔÇÑíÚ ÇáÃÑÈÚÉ
COPY . .

# äÚãá restore Úáì ãÓÊæì ÇáÜ solution ßÇãá
RUN dotnet restore ./HospitalManagement.sln

# ääÔÑ ÈÓ ãÔÑæÚ ÇáÜ API (İíå äŞØÉ ÇáÊÔÛíá Program.cs)
RUN dotnet publish ./HosbitalManagement.Presentation/HosbitalManagement.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "HosbitalManagement.API.dll"]