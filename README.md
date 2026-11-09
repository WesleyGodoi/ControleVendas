# 🏗️ Controle de Vendas de Lajes Pré-Moldadas

Aplicativo **Offline-First** multiplataforma (.NET MAUI 9 / C#) projetado para o gerenciamento comercial completo de vendas de lajes pré-moldadas, clientes, entregas e fluxo de recebimentos financeiros.

---

## 📱 Tecnologias Utilizadas

- **Framework:** .NET MAUI 9 (Multi-platform App UI)
- **Linguagem:** C# 13 / .NET 9
- **Arquitetura:** MVVM (Model-View-ViewModel) com `CommunityToolkit.Mvvm`
- **Banco de Dados Local:** SQLite (`sqlite-net-pcl`) com cache compartilhado e suporte a migrações automáticas
- **Plataformas Alvo:** Windows Machine (Desktop WinUI 3 Unpackaged), iOS (iPhone/iPad) e Android
- **Testes Automatizados:** xUnit com 31 testes de unidade cobrindo máscaras, filtros e integridade de backup

---

## ✨ Principais Funcionalidades

1. **Dashboard Operacional e Financeiro:**
   - Métricas em tempo real do mês atual: Total de m² vendidos e Receita efetivamente recebida.
   - Alertas rápidos de entregas pendentes na obra e pagamentos pendentes/parciais.
   - Listagem dos pedidos recentes com duplo status visual (Entrega e Pagamento).
   - Recarregamento com 1 clique (botão no Windows e gesto pull-to-refresh em dispositivos móveis).

2. **Gestão de Vendas & Histórico Financeiro Parcelado:**
   - Criação de novos pedidos com busca rápida de cliente e seleção de tipos de laje pré-moldada.
   - Cálculo automático de valor total em tempo real com base na área (m²) e valor unitário.
   - **Recebimento Parcial e Quitação:** Registro de pagamentos parciais no histórico com data, forma de pagamento (PIX, Dinheiro, Cartão, Boleto, Cheque) e observações, até a quitação total do pedido.
   - Painel de filtros avançado (busca por cliente, status de entrega, status de pagamento e intervalo de datas).

3. **Catálogo de Tipos de Laje:**
   - Cadastro e edição de produtos com preço padrão por m² (ex: Laje Treliçada c/ EPS H8, H12, Cerâmica, Painel Maciço).

4. **Gestão de Clientes:**
   - Cadastro completo com telefone, endereço de entrega detalhado (rua, número, bairro, cidade) e observações para entrega na obra.

5. **Backup & Restauração Segura:**
   - **Exportar Backup:** Gera cópias de segurança consolidadas do banco de dados SQLite com data e hora. Salva automaticamente na pasta Downloads (Windows) e integra com o compartilhamento nativo (`Share`).
   - **Restaurar Backup:** Importação de arquivos de backup prévios com validação prévia de integridade e rollback de segurança em caso de falha.
   - Painel com resumo dos registros e tamanho da base em disco.

6. **Interface Moderna e Alto Contraste:**
   - Menu lateral (gaveta / flyout) elegante com navegação intuitiva.
   - Máscaras brasileiras automáticas de moeda (`R$ 0,00`) e área (`m²`).
   - Ícone personalizado e tela de carregamento animada ao iniciar o aplicativo.

---

## 🚀 Como Executar o Projeto

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) instalado
- Carga de trabalho .NET MAUI: `dotnet workload install maui`

### Execução no Windows Desktop (Rápida)
Execute o script utilitário na raiz do projeto:
```bat
Compilar_e_Executar.bat
```

Ou execute via CLI:
```bash
dotnet build ControleVendasLajes\ControleVendasLajes.csproj -f net9.0-windows10.0.19041.0
dotnet run --project ControleVendasLajes\ControleVendasLajes.csproj -f net9.0-windows10.0.19041.0
```

### Executar Testes Automatizados
```bash
dotnet test ControleVendasLajes.Tests\ControleVendasLajes.Tests.csproj
```

---

## 👤 Autor

Desenvolvido por **Wesley Godoi**  
Versão 1.0 • Offline First
