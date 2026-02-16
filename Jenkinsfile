pipeline {
agent any

  environment {
    REGISTRY    = "localhost:5000"   
    IMAGE_NAME  = "todolist-api"
    IMAGE_TAG   = "build-${env.BUILD_NUMBER}"
    DOCKERFILE  = "Api/Dockerfile"
    COMPOSE_FILE = "docker-compose.yml" // 
  }

  stages {

    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Test') {
      steps {
        sh """
          docker run --rm -v "\$PWD:/src" -w /src mcr.microsoft.com/dotnet/sdk:8.0 \
          bash -lc "dotnet test -c Release"
        """
      }
    }

    stage('Build Image') {
      steps {
        sh """
          docker build -f ${DOCKERFILE} -t ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG} .
          docker tag ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG} ${REGISTRY}/${IMAGE_NAME}:latest
        """
      }
    }

    stage('Push Image') {
      steps {
        sh """
          docker push ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}
          docker push ${REGISTRY}/${IMAGE_NAME}:latest
        """
      }
    }

    stage('Deploy (Compose)') {
      steps {
        sh """
          export IMAGE_TAG=${IMAGE_TAG}
          export REGISTRY=${REGISTRY}
          export IMAGE_NAME=${IMAGE_NAME}

          docker compose -f ${COMPOSE_FILE} pull || true
          docker compose -f ${COMPOSE_FILE} up -d
        """
      }
    }
  }

  post {
    always {
      echo "Built & deployed: ${REGISTRY}/${IMAGE_NAME}:${IMAGE_TAG}"
      cleanWs()
    }
  }
}
