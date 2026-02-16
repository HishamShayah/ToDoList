pipeline {
  agent { label 'docker-agent-alpine' }

  environment {
    IMAGE_NAME = "todolist-api"
    IMAGE_TAG  = "build-${BUILD_NUMBER}"
    COMPOSE_PROJECT = "todolist-${BUILD_NUMBER}"
    // SA_PASSWORD: خليها Credentials (مو هون)
  }

  stages {
    stage('Checkout') {
      steps {
        echo "Checking out source code..."
        checkout scm
      }
    }

    stage('Build Docker Image') {
      steps {
        echo "Building Docker image..."
        sh """
          docker version
          docker build -f Api/Dockerfile -t "${IMAGE_NAME}:${IMAGE_TAG}" .
        """
      }
    }
  }

  post {
    always {
      echo "Pipeline execution completed."
      echo "Built image: ${IMAGE_NAME}:${IMAGE_TAG}"
    }
    failure {
      echo "❌ Pipeline failed! Check logs for details."
    }
    cleanup {
      cleanWs()
    }
  }
}
