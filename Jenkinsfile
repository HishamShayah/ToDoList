pipeline {
  agent none

  options {
    timestamps()
  }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    DOTNET_NOLOGO = '1'
    IMAGE_NAME = 'todolist-api'
  }

  stages {
    stage('Checkout') {
      agent any
      steps {
        checkout scm
        stash name: 'source', includes: '**/*', useDefaultExcludes: false
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
        unstash 'source'
        sh 'dotnet restore ToDoList.sln'
        sh 'dotnet build ToDoList.sln -c Release --no-restore'
        sh 'dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release --no-build --logger "trx;LogFileName=unit-tests.trx" --results-directory TestResults'
      }
      post {
        always {
          archiveArtifacts artifacts: 'TestResults/**/*.trx', fingerprint: true
        }
      }
    }

    stage('Docker Build') {
      agent any
      steps {
        unstash 'source'
        sh 'docker build -f Api/Dockerfile -t ${IMAGE_NAME}:${BUILD_NUMBER} .'
      }
    }
  }

  post {
    always {
      cleanWs()
    }
  }
}