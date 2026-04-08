pipeline {
    agent any
    environment {
        DOCKER_REGISTRY = 'your-docker-registry.com'
        // Tên image sẽ thêm hậu tố branch để không xung đột
        BRANCH_SUFFIX = "${env.BRANCH_NAME.replaceAll('/', '_')}"
    }
    options {
        timeout(time: 40, unit: 'MINUTES')
        ansiColor('xterm')
    }
    stages {

        stage('Checkout') {
            steps {
                echo "Checking out branch: ${env.BRANCH_NAME}"
                checkout scm
            }
        }

        stage('Check Docker') {
            steps {
                sh 'docker --version'
                sh 'docker-compose --version'
            }
        }

        stage('Parse docker-compose.yml for images') {
            steps {
                script {
                    dir('MyOidcServerDemo') {
                        IMAGE_NAMES = sh(
                            script: "docker-compose config | grep 'image:' | awk '{print \$2}'",
                            returnStdout: true
                        ).trim().split("\n")
                        echo "Detected images: ${IMAGE_NAMES}"
                        // Thêm suffix branch
                        IMAGE_NAMES = IMAGE_NAMES.collect { it + "-${BRANCH_SUFFIX}" }
                        echo "Images with branch suffix: ${IMAGE_NAMES}"
                    }
                }
            }
        }

        stage('Build Docker Images') {
            steps {
                dir('MyOidcServerDemo') {
                    sh "docker-compose build"
                }
            }
        }

        stage('Start Containers for Test') {
            steps {
                dir('MyOidcServerDemo') {
                    sh 'docker-compose up -d'
                    sh 'sleep 5' // wait container ready
                }
            }
        }

        stage('Smoke Test Containers') {
            steps {
                dir('MyOidcServerDemo') {
                    echo 'Running basic smoke tests...'
                    sh '''
                    # Example: check oidc server is running
                    curl -f http://localhost:5000/.well-known/openid-configuration || exit 1
                    '''
                }
            }
        }

        stage('Push Docker Images (Master Only)') {
            when {
                branch 'master'
            }
            steps {
                dir('MyOidcServerDemo') {
                    script {
                        IMAGE_NAMES.each { img ->
                            def baseName = img.replaceAll("-${BRANCH_SUFFIX}\$", "")
                            def target = "${DOCKER_REGISTRY}/${baseName}:latest"
                            echo "Tag & push ${img} -> ${target}"
                            sh "docker tag ${img} ${target}"
                            sh "docker push ${target}"
                        }
                    }
                }
            }
        }

    }

    post {
        always {
            dir('MyOidcServerDemo') {
                echo 'Cleaning up containers and dangling images...'
                sh 'docker-compose down -v || true'
                sh 'docker system prune -f || true'
            }
        }
        success {
            echo 'Build and test completed successfully!'
        }
        failure {
            echo 'Build or tests failed!'
        }
    }
}