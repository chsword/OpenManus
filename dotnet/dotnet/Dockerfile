FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/OpenManus.Core/OpenManus.Core.csproj", "OpenManus.Core/"]
COPY ["src/OpenManus.Agent/OpenManus.Agent.csproj", "OpenManus.Agent/"]
COPY ["src/OpenManus.Flow/OpenManus.Flow.csproj", "OpenManus.Flow/"]
COPY ["src/OpenManus.Llm/OpenManus.Llm.csproj", "OpenManus.Llm/"]
COPY ["src/OpenManus.Mcp/OpenManus.Mcp.csproj", "OpenManus.Mcp/"]
COPY ["src/OpenManus.Tools/OpenManus.Tools.csproj", "OpenManus.Tools/"]
COPY ["src/OpenManus.Sandbox/OpenManus.Sandbox.csproj", "OpenManus.Sandbox/"]
COPY ["src/OpenManus.Prompt/OpenManus.Prompt.csproj", "OpenManus.Prompt/"]
COPY ["src/OpenManus.Console/OpenManus.Console.csproj", "OpenManus.Console/"]
RUN dotnet restore "OpenManus.sln"

COPY . .
WORKDIR "/src/OpenManus.Console"
RUN dotnet build "OpenManus.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OpenManus.Console.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OpenManus.Console.dll"]