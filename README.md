# TfsSdkConsoleApp

Este projeto � um aplicativo console criado em C# para se conectar a um servidor TFS (Team Foundation Server), obter **Test Cases** espec�ficos, e salvar seus passos e detalhes em um arquivo JSON.

## Funcionalidades

- Conex�o com um servidor **TFS** para obter **Test Cases** usando o **TFS SDK**.
- Leitura de IDs de **Test Cases** a partir de um arquivo de json.
- Salvamento dos dados de **Test Cases** (ID, t�tulo e passos) em um �nico arquivo JSON.
- Tratamento de **Shared Steps** dentro dos **Test Cases**.

## Configura��o

### Pr�-requisitos

- **.NET Framework 4.7**
- **TFS SDK**
- **Pacotes NuGet**:
  - `Microsoft.Extensions.Configuration`
  - `Microsoft.Extensions.Configuration.Json`
  - `Newtonsoft.Json`

### Estrutura de Arquivos

- **`appsettings.json`**: Configura��o para o caminho de sa�da dos arquivos JSON e a URL font no TFS.
- **`test-cases-source.json`**: Arquivo de json que cont�m os IDs dos **Test Cases** e a pasta onde ser�o salvos.