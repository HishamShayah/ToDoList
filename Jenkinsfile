pipeline {
  agent { label 'docker-agent-alpine' }

  options {
    timestamps()
  }

  environment {
    COMPOSE_PROJECT = "todolist-${BUILD_NUMBER}"
    IMAGE_NAME = "todolist-api"
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Build Image') {
      steps {
        sh '''
          set -eu
          docker build -f Api/Dockerfile \
            -t "$IMAGE_NAME:${BUILD_NUMBER}" \
            -t "$IMAGE_NAME:latest" .
          docker image inspect "$IMAGE_NAME:${BUILD_NUMBER}" >/dev/null
          docker image inspect "$IMAGE_NAME:latest" >/dev/null
          docker images "$IMAGE_NAME" --format "table {{.Repository}}\\t{{.Tag}}\\t{{.ID}}\\t{{.CreatedSince}}"
        '''
      }
    }

    stage('Start DB for Tests') {
      steps {
        sh '''
          set -eu
          cat > docker-compose.ci.yml <<'YAML'
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "P@ssw0rd"
      ACCEPT_EULA: "Y"
YAML

          docker compose -f docker-compose.ci.yml -p "$COMPOSE_PROJECT" up -d db
          DB_CONTAINER="$(docker compose -f docker-compose.ci.yml -p "$COMPOSE_PROJECT" ps -q db)"

          READY=0
          for i in $(seq 1 30); do
            if docker exec "$DB_CONTAINER" /bin/bash -lc '(/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" -C || /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1") > /dev/null 2>&1'; then
              READY=1
              break
            fi
            sleep 2
          done

          [ "$READY" = "1" ]
          docker compose -f docker-compose.ci.yml -p "$COMPOSE_PROJECT" ps
        '''
      }
    }

    stage('Unit Tests') {
      steps {
        sh '''
          set -eu
          docker run --rm \
            -v "$PWD:/src" -w /src \
            mcr.microsoft.com/dotnet/sdk:8.0 \
            sh -lc "dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release"
        '''
      }
    }

    stage('Integration Tests') {
      steps {
        sh '''
          set -eu

          # Run integration tests in compose network to reach SQL container by host "db".
          NET="${COMPOSE_PROJECT}_default"

          docker run --rm --network "$NET" \
            -v "$PWD:/src" -w /src \
            -e ConnectionStrings__DefaultConnection="Server=db;Database=ToDoListDB;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True;" \
            mcr.microsoft.com/dotnet/sdk:8.0 \
            sh -lc "dotnet test Api.IntegrationTests/Api.IntegrationTests.csproj -c Release"
        '''
      }
    }
  }

  post {
    always {
      sh '''
        set +e
        docker compose -f docker-compose.ci.yml -p "$COMPOSE_PROJECT" down -v
        docker compose -p "$COMPOSE_PROJECT" down -v
      '''
      cleanWs()
    }
  }
}
