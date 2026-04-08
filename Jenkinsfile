pipeline {
    agent any

    options {
        skipDefaultCheckout()
        timeout(time: 30, unit: 'MINUTES')
    }

    environment {
        DOCKER_REGISTRY = 'docker.io'
        DOCKER_CREDENTIALS = 'dockerhub-creds'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Check Docker') {
            steps {
                sh 'docker --version'
                sh 'docker-compose --version'
            }
        }

        stage('Build & Start Docker Compose') {
            steps {
                dir('MyOidcServerDemo') {
                    sh '''
                        echo "Stopping old containers..."
                        docker-compose down -v
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
                    echo "Waiting for API to be ready..."
                    sleep 10
                '''
            }
        }

        stage('Test Client') {
            steps {
                sh 'curl -f http://localhost:5000 || exit 1'
            }
        }

        stage('Push Docker Images (Master Only)') {
            when {
                branch 'master'
            }
            steps {
                withDockerRegistry([ credentialsId: env.DOCKER_CREDENTIALS, url: env.DOCKER_REGISTRY ]) {
                    sh 'docker-compose push'
                }
            }
        }
    }

    post {
        always {
            echo 'Cleaning up containers...'
            dir('MyOidcServerDemo') {
                sh 'docker-compose down -v || true'
                sh 'docker-compose logs || true'
            }
        }
    }
}