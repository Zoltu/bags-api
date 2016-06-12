FROM zoltu/aspnetcore

COPY . /app
WORKDIR /app
RUN dotnet restore

EXPOSE 80

ENTRYPOINT ["dotnet", "run"]