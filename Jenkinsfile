pipeline {
  agent { label 'docker-agent-alpine' }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Unit Tests') {
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:8.0'
          args '-u root:root'
          reuseNode true
        }
      }
      steps {
        sh '''
          dotnet restore ToDoList.sln
          dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release
        '''
      }
    }

    stage('Compose Up') {
      steps {
        sh 'docker compose up -d --build'
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
