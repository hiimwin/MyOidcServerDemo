pipeline {
    agent any

    environment {
        REPO_API = "hiimwin/oidc-api"
        REPO_CLIENT = "hiimwin/oidc-client"
    }

    stages {

        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build API Image') {
            steps {
                script {
                    apiImage = docker.build("${REPO_API}:${env.BUILD_NUMBER}", "-f Dockerfile.server .")
                }
            }
        }

        stage('Build Client Image') {
            steps {
                script {
                    clientImage = docker.build("${REPO_CLIENT}:${env.BUILD_NUMBER}", "-f Dockerfile.client .")
                }
            }
        }

        stage('Run Integration Test') {
            steps {
                sh """
                docker-compose down || true
                docker-compose up -d
                sleep 10
                docker ps
                """
            }
        }

        stage('Push Images') {
            when { branch 'master' }
            steps {
                script {
                    docker.withRegistry('https://docker.io', 'dockerhub-creds') {
                        apiImage.push()
                        apiImage.push('latest')

                        clientImage.push()
                        clientImage.push('latest')
                    }
                }
            }
        }
    }

    post {
        always {
            sh 'docker-compose down || true'
        }
    }
}