pipeline {
    agent any

    environment {
        // Tạo branch-safe tên image/network
        BRANCH_NAME_SAFE = "${env.BRANCH_NAME.replaceAll('/', '_')}"
        IMAGE_SERVER = "oidc-server-${BRANCH_NAME_SAFE}"
        IMAGE_CLIENT = "oidc-client-${BRANCH_NAME_SAFE}"
        COMPOSE_FILE = "docker-compose.branch.yml"
        NETWORK_NAME = "net-${BRANCH_NAME_SAFE}"
    }

    stages {

        stage('Checkout SCM') {
            steps {
                checkout scm
            }
        }

        stage('Check Docker & Compose') {
            steps {
                sh 'docker --version'
                sh 'docker-compose --version'
            }
        }

        stage('Build Docker Images') {
            steps {
                dir('.') {
                    sh "docker build -t ${IMAGE_CLIENT} -f Dockerfile.client ."
                    sh "docker build -t ${IMAGE_SERVER} -f Dockerfile.server ."
                }
            }
        }

        stage('Start Containers for Test') {
    steps {
        script {
            // tạo network an toàn (nếu chưa có)
            sh "docker network create net-fix_fix-cicd-v2 || true"
            
            // chạy docker-compose, up các service
            sh "docker-compose -f docker-compose.branch.yml up -d"

            // chờ container server sẵn sàng
            echo "Waiting for server to be ready..."
            sh "sleep 10"
        }
    }
}

stage('Smoke Test Containers') {
    steps {
        script {
            echo "Running basic smoke tests..."

            // dùng network của docker-compose
            sh """
            docker run --rm --network emo-multi-branch_fix_fix-cicd-v2_branch_net \\
                curlimages/curl:latest -f http://oidc-server:80/.well-known/openid-configuration
            """
        }
    }
}
    }

    post {
        always {
            echo "Cleaning up containers, network, and dangling images..."
            sh "docker-compose -f ${COMPOSE_FILE} down -v"
            sh "docker network rm ${NETWORK_NAME} || true"
            sh "docker system prune -f"
        }
    }
}