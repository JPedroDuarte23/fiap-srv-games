# FiapSrvGames - API de Gerenciamento de Jogos

## 📖 Sobre o Projeto

**FiapSrvGames** é uma API RESTful desenvolvida em .NET 8 para gerenciar uma plataforma de jogos. Ela permite que *Publishers* gerenciem seus jogos e que *Players* gerenciem suas bibliotecas de jogos, além de oferecer funcionalidades de busca e recomendação.

O projeto foi construído seguindo princípios de arquitetura limpa, separando as responsabilidades em camadas de Domínio, Aplicação, Infraestrutura e API.

## ✨ Funcionalidades Principais

  - **Gerenciamento de Jogos**: CRUD completo para jogos, exclusivo para usuários com perfil de *Publisher*.
  - **Biblioteca do Jogador**: Adição, remoção e visualização de jogos na biblioteca pessoal de cada *Player*.
  - **Busca Avançada**: Endpoint de busca de jogos utilizando Elasticsearch, com suporte a *fuzziness* para tolerância a erros de digitação.
  - **Recomendações**: Sistema de recomendação de jogos baseado nos gêneros e tags dos jogos na biblioteca do usuário, também utilizando Elasticsearch.
  - **Autenticação e Autorização**: Sistema seguro baseado em JWT (JSON Web Tokens) com separação de papéis (*Roles*) para *Players* e *Publishers*.
  - **Mensageria**: Publicação de eventos (criação e atualização de jogos) em um tópico AWS SNS para integração com outros serviços.
  - **Auditoria**: Registro de eventos importantes (como criação e atualização de jogos) em uma coleção separada no MongoDB.

## 🚀 Tecnologias Utilizadas

  - **.NET 8**: Framework principal para a construção da API.
  - **ASP.NET Core**: Para a criação da API RESTful.
  - **MongoDB**: Banco de dados NoSQL para persistência dos dados.
  - **Elasticsearch**: Para funcionalidades de busca e recomendação.
  - **AWS (Amazon Web Services)**:
      - **SNS (Simple Notification Service)**: Para publicação de eventos.
      - **Parameter Store**: Para gerenciamento de segredos em ambiente de produção.
      - **S3 (Simple Storage Service)**: Para persistência de chaves de proteção de dados (Data Protection).
      - **ECS (Elastic Container Service)**: Para orquestração dos contêineres em produção.
  - **Docker**: Para containerização da aplicação.
  - **Serilog**: Para logging estruturado.
  - **Swagger (OpenAPI)**: Para documentação e teste interativo da API.
  - **xUnit & Moq**: Para a escrita de testes unitários.

## 🏗️ Arquitetura

O projeto está estruturado em uma arquitetura de 4 camadas, buscando baixo acoplamento e alta coesão:

  - **`FiapSrvGames.Domain`**: Contém as entidades de negócio e enums. É o núcleo do projeto, sem dependências externas.
  - **`FiapSrvGames.Application`**: Contém a lógica de negócio, DTOs, interfaces e os serviços que orquestram as operações.
  - **`FiapSrvGames.Infrastructure`**: Implementa as interfaces da camada de Aplicação. É responsável pelo acesso a dados (repositórios MongoDB), configurações e comunicação com serviços externos (AWS, Elasticsearch).
  - **`FiapSrvGames.API`**: A camada de apresentação, responsável por expor os endpoints da API, lidar com requisições HTTP, autenticação e autorização.

## ⚙️ CI/CD - Integração e Implantação Contínua

O projeto possui um pipeline de CI/CD configurado com **GitHub Actions** que automatiza todo o processo de build, teste, análise de código e deploy.

1.  **Orquestrador (`ci-cd.yml`)**: Inicia o pipeline em push/merge na branch `main`.
2.  **CI (`ci.yml`)**:
      - Realiza o build da solução.
      - Executa os testes unitários e gera um relatório de cobertura.
      - Envia os resultados para análise de qualidade de código no **SonarCloud**.
3.  **CD (`cd.yml`)**:
      - Faz o login no Docker Hub.
      - Gera a imagem Docker da aplicação.
      - Envia a imagem para o **Docker Hub**.
4.  **Deploy (`deploy-aws.yml`)**:
      - Faz o deploy da nova imagem Docker no **AWS ECS**, atualizando o serviço sem downtime.

## Endpoints da API

Abaixo estão os principais endpoints disponíveis. Para detalhes sobre os `requests` e `responses`, consulte a documentação do Swagger (`/swagger`).

### Games (`/api/games`)

  - `GET /`: Retorna todos os jogos.
  - `GET /{id}`: Retorna um jogo específico pelo ID.
  - `POST /`: Cria um novo jogo (requer role `Publisher`).
  - `PUT /{id}`: Atualiza um jogo existente (requer role `Publisher`).
  - `GET /search?query={texto}`: Busca jogos por título e descrição.
  - `GET /popular?count={numero}`: Retorna os jogos mais populares.

### Library (`/api/library`)

  - `GET /`: Retorna os jogos na biblioteca do usuário autenticado (requer role `Player`).
  - `POST /`: Adiciona um ou mais jogos à biblioteca (requer role `Player`).
  - `DELETE /{id}`: Remove um jogo da biblioteca (requer role `Player`).

### Recommendations (`/api/recommendations`)

  - `GET /user`: Retorna uma lista de jogos recomendados para o usuário autenticado (requer role `Player`).

## 🏁 Como Executar Localmente

### Pré-requisitos

  - [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
  - [Docker Desktop](https://www.docker.com/products/docker-desktop)
  - Um editor de código de sua preferência (ex: VS Code, Visual Studio).

### 1\. Configuração do Ambiente

1.  **Clone o repositório:**

    ```bash
    git clone https://github.com/jpedroduarte23/fiap-srv-games.git
    cd fiap-srv-games
    ```

2.  **Inicie o MongoDB e Elasticsearch com Docker Compose:**
    (Você pode criar um arquivo `docker-compose.yml` para facilitar ou iniciá-los manualmente)

    ```yaml
    # docker-compose.yml (Exemplo)
    version: '3.8'
    services:
      mongo:
        image: mongo
        ports:
          - "27017:27017"
        volumes:
          - mongo-data:/data/db
      elasticsearch:
        image: elasticsearch:8.13.10 # Use a versão compatível
        ports:
          - "9200:9200"
        environment:
          - "discovery.type=single-node"
          - "xpack.security.enabled=false"

    volumes:
      mongo-data:
    ```

    Execute: `docker-compose up -d`

### 2\. Configuração da Aplicação

1.  **Configure a Connection String**:
    No arquivo `FiapSrvGames.API/appsettings.Development.json`, verifique se a connection string do MongoDB está correta:

    ```json
    "ConnectionStrings": {
      "MongoDbConnection": "mongodb://localhost:27017"
    }
    ```

2.  **Restaure as dependências e execute a aplicação**:
    Navegue até a pasta raiz do projeto e execute:

    ```bash
    dotnet run --project FiapSrvGames.API/FiapSrvGames.API.csproj
    ```

3.  **Acesse a API**:
    A aplicação estará disponível em `https://localhost:7180` ou `http://localhost:5161`.
    A documentação do Swagger pode ser acessada em `https://localhost:7180/swagger`.
