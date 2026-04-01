# Stage 1 — build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY KanbanBoard.sln .
COPY KanbanBoard.Application/KanbanBoard.Application.csproj KanbanBoard.Application/
COPY KanbanBoard.Domain/KanbanBoard.Domain.csproj KanbanBoard.Domain/
COPY KanbanBoard.Host/KanbanBoard.Host.csproj KanbanBoard.Host/
COPY KanbanBoard.Infrastructure/KanbanBoard.Infrastructure.csproj KanbanBoard.Infrastructure/
COPY KanbanBoard.Test/KanbanBoard.Test.csproj KanbanBoard.Test/

RUN dotnet restore

COPY . .

RUN dotnet publish KanbanBoard.Host/KanbanBoard.Host.csproj -c Release -o /app/out

# Stage 2 — runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "KanbanBoard.Host.dll"]