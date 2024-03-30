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
                
                    curl --version
                    jq --version
                '''
            }
        }

        stage('Deploy'){
            steps{
                sh 'docker-compose up -d --no-color --wait'
                sh 'docker-compose ps'
            }
        }

        stage('Test2'){
            steps{
                sh 'curl http://10.37.129.2:3000/param?query=demo |jq' 
            }
        }
    }
    post{
            always{
                sh 'docker-compose down --remove-orphans -v'
                sh 'docker-compose ps'
            }
        }
}