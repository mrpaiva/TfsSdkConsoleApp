# TfsSdkConsoleApp

Este projeto é um aplicativo console criado em C# para se conectar a um servidor TFS (Team Foundation Server), obter **Test Cases** específicos, e salvar seus passos e detalhes em um arquivo JSON.

## Funcionalidades

- Conexão com um servidor **TFS** para obter **Test Cases** usando o **TFS SDK**.
- Leitura de IDs de **Test Cases** a partir de um arquivo de json.
- Salvamento dos dados de **Test Cases** (ID, título e passos) em um único arquivo JSON.
- Tratamento de **Shared Steps** dentro dos **Test Cases**.

## Configuração

### Pré-requisitos

- **.NET Framework 4.7**
- **TFS SDK**
- **Pacotes NuGet**:
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.Configuration.Json`
  - `Newtonsoft.Json`

### Estrutura de Arquivos

- **`appsettings.json`**: Configuração para o caminho de saída dos arquivos JSON e a URL font no TFS.
- **`test-cases-source.json`**: Arquivo de json que contém os IDs dos **Test Cases** e a pasta onde serão salvos.