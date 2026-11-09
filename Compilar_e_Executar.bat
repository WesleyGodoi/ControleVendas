@echo off
chcp 65001 >nul
title Compilação Rápida - Controle de Vendas de Lajes
echo ========================================================
echo    Compilando Controle de Vendas de Lajes (Windows)
echo ========================================================
echo.

echo [1/3] Encerrando instâncias abertas para liberar o .exe...
taskkill /f /im ControleVendasLajes.exe >nul 2>&1
timeout /t 1 /nobreak >nul

echo [2/3] Compilando rapidamente (.exe)...
dotnet build "%~dp0ControleVendasLajes\ControleVendasLajes.csproj" -f net9.0-windows10.0.19041.0 -p:TargetFrameworks=net9.0-windows10.0.19041.0 --no-restore -c Debug

if %ERRORLEVEL% equ 0 (
    echo.
    echo [3/3] Sucesso! Iniciando aplicativo...
    cd /d "%~dp0ControleVendasLajes\bin\Debug\net9.0-windows10.0.19041.0\win10-x64"
    start "" "ControleVendasLajes.exe"
    exit
) else (
    echo.
    echo [ERRO] Falha ao compilar. Verifique as mensagens acima.
    pause
)
