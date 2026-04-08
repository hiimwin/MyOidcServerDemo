pipeline {
    agent any
    environment {
        DOCKER_NETWORK = "oidc-net"
        API_PORT = "5000"
        CLIENT_PORT = "8080"
    }
    options {
        skipDefaultCheckout()
        timeout(time: 30, unit: 'MINUTES')
        ansiColor('xterm')
    }
    stages {
        stage('Checkout SCM') {
            steps {
                checkout scm
            }
        }

        stage('Check Docker') {
            steps {
                script {
                    sh 'docker --version'
                    sh 'docker-compose --version'
                }
            }
        }

        stage('Build & Start Docker Compose') {
            steps {
                dir("${WORKSPACE}") {
                    script {
                        echo "\033[34mStopping old containers...\033[0m"
                        sh "docker-compose down -v || true"

                        echo "\033[34mBuilding containers...\033[0m"
                        sh "docker-compose build"

                        echo "\033[34mStarting containers...\033[0m"
                        sh "docker-compose up -d"
                    }
                }
            }
        }

        stage('Wait for API') {
            steps {
                script {
                    echo "Waiting for API on port ${API_PORT}..."
                    retry(10) {
                        sh """
                        curl --fail http://localhost:${API_PORT}/health || exit 1
                        """
                    }
                }
            }
        }

        stage('Test Client') {
            steps {
                script {
                    echo "Testing client..."
                    retry(5) {
                        sh """
                        curl --fail http://localhost:${CLIENT_PORT}/ || exit 1
                        """
                    }
                }
            }
        }

        stage('Push Docker Images') {
            when {
                expression { env.BRANCH_NAME == 'master' || env.BRANCH_NAME == 'dev' }
            }
            steps {
                script {
                    docker.withRegistry('https://docker.io', 'dockerhub-creds') {
                        sh "docker build -t myuser/oidc-api:${BRANCH_NAME} -f Dockerfile.server ."
                        sh "docker push myuser/oidc-api:${BRANCH_NAME}"

                        sh "docker build -t myuser/oidc-client:${BRANCH_NAME} -f Dockerfile.client ."
                        sh "docker push myuser/oidc-client:${BRANCH_NAME}"
                    }
                }
            }
        }
    }

    post {
        always {
            echo "\033[34mCleaning up containers...\033[0m"
            dir("${WORKSPACE}") {
                sh "docker-compose down -v || true"
                sh "docker-compose logs || true"
            }
        }
        success {
            echo "\033[32mCI/CD SUCCESS!\033[0m"
        }
        failure {
            echo "\033[31mCI/CD FAILED!\033[0m"
        }
    }
}