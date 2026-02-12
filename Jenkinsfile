pipeline {
  agent { label 'docker-agent-alpine' }

  options {
    timestamps()
  }

  environment {
    COMPOSE_PROJECT = "todolist-${BUILD_NUMBER}"
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Compose Up') {
      steps {
        sh '''
          set -eu
          docker compose -p "$COMPOSE_PROJECT" up -d --build
          docker compose -p "$COMPOSE_PROJECT" ps
        '''
      }
    }

    stage('Unit Tests') {
      steps {
        sh '''
          set -eu
          # تشغيل unit tests داخل كونتينر dotnet (بدون ما تحتاج dotnet على الـ agent)
          docker run --rm \
            -v "$PWD:/src" -w /src \
            mcr.microsoft.com/dotnet/sdk:8.0 \
            sh -lc "dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release"
        '''
      }
    }

    // إذا عندك Integration Tests وبدها DB "db" ضمن شبكة compose:
    stage('Integration Tests') {
      steps {
        sh '''
          set -eu

          # شغّل التكامل ضمن نفس شبكة compose ليقدر يوصل لـ db بالاسم "db"
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
        docker compose -p "$COMPOSE_PROJECT" down -v
      '''
      cleanWs()
    }
  }
}
