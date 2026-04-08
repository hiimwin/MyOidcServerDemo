pipeline {
    agent any

    environment {
        COMPOSE_PROJECT_NAME = "oidc-app"
        REPO_URL = "https://github.com/hiimwin/MyOidcServerDemo.git"
        REPO_API_IMAGE = "hiimwin/oidc-api"
        REPO_CLIENT_IMAGE = "hiimwin/oidc-client"
        CLIENT_PORT = "5001"
        API_PORT = "5000"
    }

    stages {
        stage('Prepare') {
            steps {
                script {
                    // Kiểm tra Docker & Docker-compose có sẵn
                    sh 'docker --version || { echo "Docker not found"; exit 1; }'
                    sh 'docker-compose --version || { echo "docker-compose not found"; exit 1; }'
                }
            }
        }

        stage('Clone Repo') {
            steps {
                script {
                    // Xóa folder nếu tồn tại
                    if (fileExists('MyOidcServerDemo')) {
                        sh 'rm -rf MyOidcServerDemo'
                    }
                    // Clone repo
                    git branch: 'master',
                        url: "$REPO_URL",
                        credentialsId: 'github-token'
                }
            }
        }

        stage('Build & Run Docker Compose') {
            steps {
                dir('MyOidcServerDemo') {
                    script {
                        sh '''
                        echo "Stopping old containers..."
                        docker-compose down -v || true

                        echo "Building containers..."
                        docker-compose build

                        echo "Starting containers..."
                        docker-compose up -d
                        '''
                    }
                }
            }
        }

        stage('Wait for API') {
            steps {
                script {
                    sh '''
                    echo "Waiting for API on localhost:${API_PORT}..."
                    for i in {1..12}; do
                        curl -s http://localhost:${API_PORT} && break
                        echo "Waiting 5s..."
                        sleep 5
                    done
                    '''
                }
            }
        }

        stage('Test Client') {
            steps {
                script {
                    sh '''
                    echo "Testing Client on localhost:${CLIENT_PORT}..."
                    curl -s http://localhost:${CLIENT_PORT} || exit 1
                    '''
                }
            }
        }

        stage('Push Docker Images (Master Only)') {
            when { branch 'master' }
            steps {
                dir('MyOidcServerDemo') {
                    script {
                        docker.withRegistry('https://docker.io', 'dockerhub-creds') {
                            sh """
                            docker-compose build
                            docker tag oidc-app_api:latest $REPO_API_IMAGE:latest
                            docker tag oidc-app_client:latest $REPO_CLIENT_IMAGE:latest
                            docker push $REPO_API_IMAGE:latest
                            docker push $REPO_CLIENT_IMAGE:latest
                            """
                        }
                    }
                }
            }
        }
    }

    post {
        success {
            echo "CI/CD SUCCESS - App is running"
        }
        failure {
            node {
                echo "CI/CD FAILED - Showing logs..."
                dir('MyOidcServerDemo') {
                    sh 'docker-compose logs || true'
                }
            }
        }
        always {
            node {
                echo "Cleaning up containers..."
                dir('MyOidcServerDemo') {
                    sh 'docker-compose down -v || true'
                }
            }
        }
    }
}