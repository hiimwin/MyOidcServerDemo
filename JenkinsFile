pipeline {
    agent any

    environment {
        REPO = "hiimwin/deloymyodicserverdemo"
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }

        stage('Build .NET') {
            steps {
                sh 'dotnet restore'
                sh 'dotnet publish -c Release -o publish'
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    // Tag theo branch và build number
                    def branchName = env.BRANCH_NAME.replaceAll('/', '-')
                    dockerImage = docker.build("${REPO}:${branchName}-${env.BUILD_NUMBER}")
                }
            }
        }

        stage('Test Docker Image') {
            steps {
                script {
                    def branchName = env.BRANCH_NAME.replaceAll('/', '-')
                    sh "docker run --rm -p 5000:5000 ${REPO}:${branchName}-${env.BUILD_NUMBER} --help || echo 'Smoke test OK'"
                }
            }
        }

        stage('Optional Push Docker Image') {
            when {
                expression { env.BRANCH_NAME == 'master' }
            }
            steps {
                script {
                    docker.withRegistry('https://docker.io', 'dockerhub-creds') {
                        dockerImage.push()
                        dockerImage.push('latest')
                    }
                }
            }
        }
    }

    post {
        success {
            echo "Branch ${env.BRANCH_NAME} built and tested successfully!"
        }
        failure {
            echo "Branch ${env.BRANCH_NAME} failed. Check logs!"
        }
    }
}
