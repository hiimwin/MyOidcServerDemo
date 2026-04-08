pipeline {
    agent any
    options {
        skipDefaultCheckout()
        timeout(time: 30, unit: 'MINUTES')
    }

    environment {
        REGISTRY = "your-docker-registry.com" // ví dụ: docker.io/hiimwin
        SERVER_IMAGE = "${REGISTRY}/oidc-server"
        CLIENT_IMAGE = "${REGISTRY}/oidc-client"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm // Multi-branch tự chọn nhánh
            }
        }

        stage('Check Docker') {
            steps {
                sh 'docker --version'
                sh 'docker-compose --version'
            }
        }

        stage('Build Docker Images') {
            steps {
                dir('MyOidcServerDemo') {
                    sh 'docker-compose build'
                }
            }
        }

        stage('Start Containers for Test') {
            steps {
                dir('MyOidcServerDemo') {
                    sh 'docker-compose up -d'
                }
            }
        }

        stage('Test Containers') {
            steps {
                echo 'Optional: Add smoke tests here, e.g., curl http://localhost:5000/.well-known/openid-configuration'
            }
        }

        stage('Push Docker Images (Master Only)') {
            when {
                branch 'master'
            }
            steps {
                dir('MyOidcServerDemo') {
                    sh "docker tag oidc-server:latest ${SERVER_IMAGE}:latest"
                    sh "docker tag oidc-client:latest ${CLIENT_IMAGE}:latest"
                    sh "docker push ${SERVER_IMAGE}:latest"
                    sh "docker push ${CLIENT_IMAGE}:latest"
                }
            }
        }
    }

    post {
        always {
            echo 'Cleaning up containers...'
            dir('MyOidcServerDemo') {
                sh 'docker-compose down -v'
                sh 'docker-compose logs'
            }
        }
    }
}