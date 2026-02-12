pipeline {
  agent { label 'docker-agent-alpine' }

  options { timestamps() }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Unit Tests') {
      agent {
        docker { image 'mcr.microsoft.com/dotnet/sdk:8.0' }
      }
      steps {
        sh '''
          set -eu
          dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release
        '''
      }
    }

    stage('Compose Up') {
      steps {
        sh '''
          set -eu
          docker compose up -d --build
          sleep 25
          docker compose ps
          docker compose logs --no-color --tail=120 db
        '''
      }
    }

    // (اختياري) إذا عندك IntegrationTests
    stage('Integration Tests') {
      agent {
        docker { image 'mcr.microsoft.com/dotnet/sdk:8.0' }
      }
      steps {
        sh '''
          set -eu
          dotnet test Api.IntegrationTests/Api.IntegrationTests.csproj -c Release
        '''
      }
    }
  }

  post {
    always {
      sh 'docker compose down -v || true'
      cleanWs()
    }
  }
}
