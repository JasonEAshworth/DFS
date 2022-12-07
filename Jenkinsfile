#!groovy
pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/core/sdk:3.1'
        }
    }
    environment {
        HOME = "/tmp"
        NUGET_SERVER = "https://www.myget.org/F/valid-nuget-feed/api/v2/package"
        NUGET_API_KEY = credentials('jenkins-myget-api-key')
        NUGET_SYMBOLS_SERVER = "https://www.myget.org/F/valid-nuget-feed/symbols/api/v2/package"
        PROJECT_NAME="Valid-DynamicFilterSort.csproj"
        CSPROJ_PATH="./Valid-DynamicFilterSort/"
        BUILD_DIR="./build"
        TEST_PROJECT="./UnitTests/UnitTests.csproj"
        NOTIFY_NAME="Valid Dynamic Filter Sort"
        SLACK_CHANNEL="platform-ci-cd"
    }
    options {
        buildDiscarder(logRotator(numToKeepStr: '10'))
    }
    stages {
        stage('Deploy') {
            steps {
                notify("Building and Publishing Nuget Package", ":pencil2:")
                sh "./build.sh"
            }
        }
    }
    post {
        success {
            notify("Build Successful", ":pencil2:")
        }
        failure {
            notify("Build Failed", ":x:")
        }
        always {
            deleteDir()
        }
    }
}

