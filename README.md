

# ![](/assets/microservice.png) TesteService

> Este é um projeto de 2 microserviços (ReceiverService e ConsumerService) desenvolvidos em C# que se comunicam com o Azure Cloud, usando os Comos DB para persistência dos dados e o Service Bus como serviço de mensageria. 

## Tecnologias e Frameworks Utilizados
- .NET/C#
- Azure Cosmos 
- Azure Service Bus
- xUnit
- AutoMapper
- Newtonsoft - Json.NET

## Configuração das Variáveis de Ambiente
Para o correto funcionamento das aplicações é necessário configurar as seguintes variáveis de ambiente no sistema operacional:

- `AZURE_SERVICE_BUS_CS` - Conection String do Service bus a ser utilizado

- `COSMOSDB_END_POINT_URI` - Endpoint do Cosmos DB a ser utilizado

- `COSMOSDB_PRIMARY_KEY` - Primary Key de acesso do Cosmos DB

- `JWT_SECRET_KEY` - Secret Key para geração do token JWT necessário para acessar os endpoints

## Funcionalidades
### ReceiverService
- Serviço que faz a persistência dos dados no Cosmos DB e envia o dado persistido para a fila do Service Bus. 
- Possui um endpoint de login que retorna um token JWT para validação nos demais endpoints.
- Todos os endpoints, exceto login, precisam de um token para validação do usuário.
- Possui um filtro de erros que lança uma mensagem genérica para qualquer erro que ocorra no sistema
  
### ConsumerService
- Serviço que faz o consumo da fila do Service Bus, anteriormente alimentado pelo ReceiverService.

## ReceiverService Endpoints

### **[POST]**
`/receiver/login` - endpoint de login de usuário  
    - Entrada: JSON de LoginDto  
    - Ação: Valida o valor de usuário recebido e gera um token JWT de acordo com a chave JWT_SECRET_KEY (Único usuário válido é o usuário *"root"*).  
    - Saída: Retorna um token JWT caso usuário seja válido, caso contrário, retorna status 401-Unauthorized.

`/receiver/error` - endpoint de teste do filtro de exceção  
    - Entrada: não possui.  
    - Ação: retorna um erro para teste do filtro de exceção.  
    - Saída: JSON com todos os receivers cadastrados. 

`/receiver` - endpoint para cadastro de um receiver  
    - Entrada: JSON de Receiver.  
    - Ação: Cadastra o Receiver recebido no Cosmos DB e envia um JSON do Receiver cadastrado para a fila do Service Bus.  
    - Saída: JSON com todos o receiver cadastrado e status 200. 

### **[GET]**
`/receiver` - endpoint para buscar todos os receivers cadastrados no banco  
    - Entrada: não possui.  
    - Ação: Busca no Cosmos DB os receivers cadastrados.  
    - Saída: JSON com todos os receivers cadastrados.  

`/receiver/{id}` - endpoint para buscar por id um receiver cadastrado no banco
    - Entrada: Parâmetro de ID na URI.  
    - Ação: Busca no Cosmos DB o receiver com o id passado.  
    - Saída: JSON com o receiver buscado.  

## ConsumerService Endpoints

### **[POST]**
`/receiver` - endpoint para consumir a fila do Service Bus  
    - Entrada: não possui.  
    - Ação: Acessa o Service Bus e processa a fila.   
    - Saída: não possui. 


## Exemplos de JSON

### LoginDto
```json
{
    "usuario": "root"
}
```

### Receiver
```json
{
    "id": "1",
    "Field1": "field_1",
    "Field2": "field_2",
    "Field3": "field_3",
    "Field4": "field_4"
}
```