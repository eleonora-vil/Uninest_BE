pipeline {

    agent any

    
    stages {

        stage('Packaging') {

            steps {
                
                sh 'docker build --pull --rm -f Dockerfile -t uninestbe:latest .'
                
            }
        }

        stage('Push to DockerHub') {

            steps {
                withDockerRegistry(credentialsId: 'dockerhub', url: 'https://index.docker.io/v1/') {
                    sh 'docker tag uninestbe:latest chalsfptu/uninestbe:latest'
                    sh 'docker push chalsfptu/uninestbe:latest'
                }
            }
        }

        stage('Deploy BE to DEV') {
            steps {
                withCredentials([string(credentialsId: 'SECRET_KEY', variable: 'SECRET_KEY'), string(credentialsId: 'DB_SERVER', variable: 'DB_SERVER'), string(credentialsId: 'DB_NAME', variable: 'DB_NAME'), string(credentialsId: 'DB_USER', variable: 'DB_USER'), string(credentialsId: 'DB_PASSWORD', variable: 'DB_PASSWORD'), string(credentialsId: 'DB_TRUST_SERVER_CERTIFICATE', variable: 'DB_TRUST_SERVER_CERTIFICATE'), string(credentialsId: 'DB_MULTIPLE_ACTIVE_RESULT_SETS', variable: 'DB_MULTIPLE_ACTIVE_RESULT_SETS')]) {
                    echo 'Deploying and cleaning'
                    sh 'docker container stop uninestbe || echo "this container does not exist" '
                    sh 'echo y | docker system prune '
                    sh '''docker container run  -d --rm --name uninestbe -p 90:80  chalsfptu/uninestbe '''
                }
            }
        }
        
 
    }
    post {
        always {
            cleanWs()
        }
    }
}