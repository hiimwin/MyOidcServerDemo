pipeline {
    agent any

    environment {
        IMAGE_NAMES = []
        IMAGE_NAMES_BRANCH = []
        BRANCH_SUFFIX = "${env.BRANCH_NAME.replaceAll('/', '_')}"
    }

    stages {
        stage('Declarative: Checkout SCM') {
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
                    IMAGE_NAMES = sh(script: "docker-compose config | grep image: | awk '{print \$2}'", returnStdout: true)
                                    .trim()
                                    .split('\n')
                    IMAGE_NAMES_BRANCH = IMAGE_NAMES.collect { "${it}-${BRANCH_SUFFIX}" }
                    echo "Detected images: ${IMAGE_NAMES}"
                    echo "Images with branch suffix: ${IMAGE_NAMES_BRANCH}"
                }
            }
        }

        stage('Build Docker Images') {
            steps {
                dir("${env.WORKSPACE}") {
                    script {
                        IMAGE_NAMES_BRANCH.eachWithIndex { imageName, idx ->
                            def dockerfile = (idx == 0) ? 'Dockerfile.client' : 'Dockerfile.server'
                            sh "docker build -t ${imageName} -f ${dockerfile} ."
                        }
                    }
                }
            }
        }

        stage('Start Containers for Test') {
            steps {
                script {
                    sh "docker network create net-${BRANCH_SUFFIX} || true"
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
                    docker run --rm --network net-${BRANCH_SUFFIX} curlimages/curl:latest -f http://oidc-server:80/.well-known/openid-configuration
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
                sh "docker network rm net-${BRANCH_SUFFIX} || true"
                sh "docker system prune -f"
            }
        }
    }
}