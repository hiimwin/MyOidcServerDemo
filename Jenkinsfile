pipeline {
    agent any

    environment {
        BRANCH_SUFFIX = "${env.BRANCH_NAME.replaceAll('/', '_')}"
    }

    stages {
        stage('Checkout SCM') {
            steps {
                checkout([$class: 'GitSCM',
                    branches: [[name: env.BRANCH_NAME]],
                    doGenerateSubmoduleConfigurations: false,
                    extensions: [],
                    userRemoteConfigs: [[
                        url: 'https://github.com/hiimwin/MyOidcServerDemo.git',
                        credentialsId: 'github-token'
                    ]]
                ])
            }
        }

        stage('Check Docker & Compose') {
            steps {
                sh 'docker --version'
                sh 'docker-compose --version'
            }
        }

        stage('Parse docker-compose.yml') {
            steps {
                script {
                    def IMAGE_NAMES = sh(script: "docker-compose config | grep image: | awk '{print \$2}'", returnStdout: true)
                                        .trim()
                                        .split('\n')
                    def IMAGE_NAMES_BRANCH = IMAGE_NAMES.collect { "${it}-${BRANCH_SUFFIX}" }
                    echo "Detected images: ${IMAGE_NAMES}"
                    echo "Images with branch suffix: ${IMAGE_NAMES_BRANCH}"
                }
            }
        }

        stage('Build Docker Images') {
            steps {
                dir("${env.WORKSPACE}") {
                    script {
                        sh "docker build -t oidc-client-${BRANCH_SUFFIX} -f Dockerfile.client ."
                        sh "docker build -t oidc-server-${BRANCH_SUFFIX} -f Dockerfile.server ."
                    }
                }
            }
        }

        stage('Create Network') {
            steps {
                script {
                    def networkName = "branch_net-${BRANCH_SUFFIX}"
                    sh "docker network create ${networkName} || true"
                    env.NETWORK_NAME = networkName
                    echo "Network created: ${networkName}"
                }
            }
        }

        stage('Start Containers for Test') {
            steps {
                script {
                    sh "docker-compose -f docker-compose.yml up -d"
                    sh "sleep 5"
                }
            }
        }

        stage('Smoke Test Containers') {
            steps {
                script {
                    echo "Running basic smoke tests..."
                    sh """
                    docker run --rm --network ${env.NETWORK_NAME} curlimages/curl:latest -f http://oidc-server:80/.well-known/openid-configuration
                    """
                }
            }
        }
    }

    post {
        always {
            dir("${env.WORKSPACE}") {
                echo "Cleaning up containers, network, and dangling images..."
                sh "docker-compose -f docker-compose.yml down -v"
                sh "docker network rm ${env.NETWORK_NAME} || true"
                sh "docker system prune -f"
            }
        }
    }
}