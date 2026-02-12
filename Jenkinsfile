pipeline {
  agent { label 'docker-agent-alpine' }

  options {
    timestamps()
    skipDefaultCheckout(true)
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    DOTNET_NOLOGO = '1'
    IMAGE_NAME = 'todolist-api'
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Build & Unit Tests') {
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:8.0'
          args '-u root:root'
          reuseNode true
        }
      }
      steps {
        sh '''
          set -eu

          dotnet restore ToDoList.sln
          dotnet build ToDoList.sln -c Release --no-restore
          dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release --no-build --logger "trx;LogFileName=unit-tests.trx" --results-directory TestResults
        '''
      }
      post {
        always {
          archiveArtifacts artifacts: 'TestResults/**/*.trx', fingerprint: true, allowEmptyArchive: true
        }
      }
    }

    stage('Docker Build') {
      steps {
        sh '''
          set -eu

          if [ -z "${DOCKER_HOST:-}" ]; then
            export DOCKER_HOST=unix:///var/run/docker.sock
          fi

          docker version
          docker info
          docker build -f Api/Dockerfile -t ${IMAGE_NAME}:${BUILD_NUMBER} .
        '''
      }
    }
  }

  post {
    always {
      cleanWs()
    }
  }
}
