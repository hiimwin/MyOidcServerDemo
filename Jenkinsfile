pipeline {
    agent any

    environment {
        COMPOSE_PROJECT_NAME = "oidc-app"
        REPO_URL = "https://github.com/hiimwin/MyOdicServerDemo.git"
        REPO_API_IMAGE = "hiimwin/oidc-api"
        REPO_CLIENT_IMAGE = "hiimwin/oidc-client"
    }

    stages {
        stage('Clone Repo') {
            steps {
                script {
                    // Xóa folder nếu tồn tại trước khi clone
                    sh '''
                    if [ -d "MyOdicServerDemo" ]; then
                        rm -rf MyOdicServerDemo
                    fi
                    git clone $REPO_URL
                    '''
                }
            }
        }

        stage('Build & Run Docker Compose') {
            steps {
                dir('MyOdicServerDemo') {
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

        stage('Wait for API') {
            steps {
                sh '''
                echo "Waiting for API on localhost:5000..."
                for i in {1..12}; do
                    curl -s http://localhost:5000 && break
                    echo "Waiting 5s..."
                    sleep 5
                done
                '''
            }
        }

        stage('Test Client') {
            steps {
                sh '''
                echo "Testing Client on localhost:5001..."
                curl -s http://localhost:5001 || exit 1
                '''
            }
        }

        stage('Push Docker Images (Master Only)') {
            when { branch 'master' }
            steps {
                dir('MyOdicServerDemo') {
                    script {
                        docker.withRegistry('https://docker.io', 'dockerhub-creds') {
                            sh '''
                            docker-compose build
                            docker tag oidc-app_api:latest $REPO_API_IMAGE:latest
                            docker tag oidc-app_client:latest $REPO_CLIENT_IMAGE:latest
                            docker push $REPO_API_IMAGE:latest
                            docker push $REPO_CLIENT_IMAGE:latest
                            '''
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
            echo "CI/CD FAILED - Showing logs..."
            dir('MyOdicServerDemo') {
                sh 'docker-compose logs || true'
            }
        }
        always {
            echo "Cleaning up containers..."
            dir('MyOdicServerDemo') {
                sh 'docker-compose down -v || true'
            }
        }
    }
}