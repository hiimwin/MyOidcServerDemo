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
        		docker rm -f api || true
        		docker rm -f client || true

        		docker run -d -p 5000:5000 --name api ${REPO_API}:${env.BUILD_NUMBER}
        		docker run -d -p 5001:5001 --name client ${REPO_CLIENT}:${env.BUILD_NUMBER}

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
        failure {
            sh 'docker-compose down || true'
        }
    }
}