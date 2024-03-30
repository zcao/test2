pipeline{
    agent any
    stages{
        stage('Build'){
            steps{
                echo 'Building the project'
            }
        }
        stage('Test'){
            steps{
                sh '''
                    docker version
                    docker info
                    
                    docker compose version
                    curl --version
                    jq --version
                '''
            }
        }

    }
}