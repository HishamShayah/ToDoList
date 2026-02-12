pipeline {
  agent { label 'docker-agent-alpine' }

  options { timestamps() }

  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    DOTNET_NOLOGO = '1'
    IMAGE_NAME = 'todolist-api'
  }

  stages {
    stage('Build & Unit Tests') {
      steps {
        sh '''
          docker version
          docker info || true

          docker run --rm -u root:root \
            -e DOTNET_CLI_TELEMETRY_OPTOUT=$DOTNET_CLI_TELEMETRY_OPTOUT \
            -e DOTNET_NOLOGO=$DOTNET_NOLOGO \
            -v "$PWD":/workspace -w /workspace \
            mcr.microsoft.com/dotnet/sdk:8.0 \
            sh -lc "dotnet restore ToDoList.sln && dotnet build ToDoList.sln -c Release --no-restore && dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release --no-build --logger \\"trx;LogFileName=unit-tests.trx\\" --results-directory TestResults"
        '''
      }
      post {
        always {
          archiveArtifacts artifacts: 'TestResults/**/*.trx', fingerprint: true
        }
      }
    }

    stage('Docker Build') {
      steps {
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
