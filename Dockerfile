FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY . ./

WORKDIR /app/src
RUN dotnet restore --runtime linux-x64
RUN dotnet publish --no-restore -c Release -o out --runtime linux-x64
RUN ls /app/src/out

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS runtime

WORKDIR /app
COPY --from=build /app/src/out .

ENTRYPOINT [ "dotnet", "./PlayerCountBot.dll" ]
