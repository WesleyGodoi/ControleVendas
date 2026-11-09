@echo off
chcp 65001 >nul
title Controle de Vendas de Lajes
cd /d "%~dp0ControleVendasLajes\bin\Debug\net9.0-windows10.0.19041.0\win10-x64"
start "" "ControleVendasLajes.exe"
exit
