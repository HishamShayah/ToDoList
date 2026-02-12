pipeline {
  agent any

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Unit Tests') {
      steps {
        sh '''
          dotnet restore ToDoList.sln
          dotnet test Application.UnitTests/Application.UnitTests.csproj -c Release
        '''
      }
    }

    stage('Compose Up') {
      steps {
        sh '''
          docker compose up -d --build
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
