# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /sample

COPY ./src ./src/
COPY Sample.Project.Template.sln .
COPY .editorconfig .

RUN dotnet restore

RUN dotnet publish -c Release -o build

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /sample
COPY --from=build-env /sample/build .
ENTRYPOINT ["dotnet"]
CMD ["Sample.WebApi.dll"]
