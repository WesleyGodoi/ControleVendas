# 🏗️ Controle de Vendas

Sistema comercial completo para gestão de vendas e orçamentos de lajes pré-moldadas, clientes, entregas e fluxo de recebimentos financeiros.

Disponível para acesso via web/celular e aplicativo desktop Windows (.NET 9).

---

## 📱 Tecnologias Utilizadas

- **Web App:** Blazor WebAssembly (.NET 9 C#)
- **Desktop (Windows):** .NET MAUI 9 (WinUI 3 Unpackaged)
- **Biblioteca Compartilhada:** `ControleVendasLajes.Core` (.NET 9)
- **Banco de Dados:** Armazenamento local com suporte a backup e restauração
- **Hospedagem & CI/CD:** GitHub Actions com deploy automático no **GitHub Pages**
- **Testes Automatizados:** xUnit com 31 testes de unidade cobrindo cálculos, regras de negócio e validações

---

## ✨ Principais Funcionalidades

1. **Dashboard Operacional e Financeiro:**
   - Métricas em tempo real do mês atual: Área total vendida (m²), Faturamento recebido, Entregas pendentes na obra e Pagamentos pendentes/parciais.
   - Listagem dos pedidos recentes com duplo status visual (Entrega e Pagamento).
   - Botão de atualização rápida em tempo real.

2. **Gestão de Vendas & Orçamentos:**
   - Criação ágil de orçamentos com busca rápida de cliente e seleção de tipos de laje pré-moldada.
   - Cálculo automático de valor total em tempo real com base na área (m²) e no preço unitário por m².
   - **Envio Direto no WhatsApp:** Botão com 1 clique para formatar a proposta comercial completa e abrir a conversa do cliente no WhatsApp.
   - Compartilhamento nativo da proposta comercial.

3. **Controle Financeiro e Parcelamento:**
   - Registro de pagamentos e entradas com data, método de recebimento (PIX, Dinheiro, Cartão, Transferência, Boleto) e observações.
   - Cálculo automático do saldo devedor restante e atualização do status (Pendente, Parcial ou Quitado).

4. **Controle de Entregas:**
   - Gerenciamento independente do status da entrega (Pendente, Em Transporte ou Entregue na Obra).

5. **Catálogo de Tipos de Laje e Preços:**
   - Cadastro e edição de produtos com preço padrão por m² (ex: Laje Treliçada c/ EPS H8, H12, Cerâmica H8/H12, EPS H16).

6. **Gestão de Clientes:**
   - Cadastro completo com nome, telefone com máscara automática `(XX) XXXXX-XXXX`, endereço de entrega detalhado (rua, número, bairro, cidade) e observações da obra.

7. **Backup e Restauração Segura:**
   - **Exportar Backup:** Baixa um arquivo `.json` consolidado com todos os dados salvos para você guardar no Google Drive, iCloud ou enviar por e-mail/WhatsApp.
   - **Restaurar Backup:** Importa arquivos de backup anteriores com validação de integridade.

---

## 🌐 Como Acessar o Aplicativo

O aplicativo pode ser acessado diretamente pelo navegador em qualquer dispositivo:

👉 **Link de Acesso:** [https://wesleygodoi.github.io/ControleVendas/](https://wesleygodoi.github.io/ControleVendas/)

### Como Adicionar à Tela Inicial do Celular:
1. Abra o link acima no navegador do celular (Safari no iPhone ou Chrome no Android).
2. Toque no botão de menu ou compartilhar do navegador.
3. Selecione **"Adicionar à Tela de Início"** (*Add to Home Screen*).
4. O ícone do app aparecerá na tela inicial e abrirá em tela cheia como um aplicativo.

---

## 💻 Como Executar Localmente

### Pré-requisitos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) instalado

### Executar a versão Web
```bash
dotnet run --project ControleVendasLajes.PWA/ControleVendasLajes.PWA.csproj
```
Abra o navegador no endereço exibido (ex: `http://localhost:5000`).

### Executar a versão Windows Desktop
Execute o script utilitário:
```bat
Compilar_e_Executar.bat
```

### Executar os Testes de Unidade
```bash
dotnet test ControleVendasLajes.Tests/ControleVendasLajes.Tests.csproj
```

---

## 👤 Autor

Desenvolvido por **Wesley Godoi**  
Versão 1.0
