pipeline {
  agent { label 'docker-agent-alpine' }
  options { timestamps() }

  environment {
    IMAGE_NAME = "todolist-api"
    COMPOSE_PROJECT = "todolist-${BUILD_NUMBER}"
    SA_PASSWORD = "P@ssw0rd"
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Build') {
      steps {
        sh '''
          set -eu
          docker build -f Api/Dockerfile -t "$IMAGE_NAME:${BUILD_NUMBER}" .
        '''
      }
    }

    stage('Unit Tests') {
      steps {
        sh '''
          set -eu
          dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release
        '''
      }
    }

    stage('Start DB') {
      steps {
        sh '''
          set -eu
          docker compose -p "$COMPOSE_PROJECT" -f docker-compose.yml up -d db
          docker compose -p "$COMPOSE_PROJECT" -f docker-compose.yml ps
        '''
      }
    }

    stage('Integration Tests') {
      steps {
        sh '''
          set -eu
          export ConnectionStrings__DefaultConnection="Server=localhost;Database=ToDoListDB;User Id=sa;Password=$SA_PASSWORD;TrustServerCertificate=True;"
          dotnet test Api.IntegrationTests/Api.IntegrationTests.csproj -c Release
        '''
      }
    }
  }

  post {
    always {
      sh '''
        set +e
        docker compose -p "$COMPOSE_PROJECT" -f docker-compose.ci.yml down -v
      '''
      cleanWs()
    }
  }
}
