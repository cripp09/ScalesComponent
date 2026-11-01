# ⚖️ ScalesComponent

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2+-blue.svg)](https://dotnet.microsoft.com/)
[![1C Enterprise](https://img.shields.io/badge/1C-Enterprise-FFB900.svg)](https://1c.ru)

Универсальная COM-библиотека для работы с торговыми и промышленными весами через COM-порт (RS-232).

---

## 📥 Скачать готовую сборку
Если вы не хотите компилировать проект самостоятельно, скачайте готовую библиотеку:  
👉 **[Скачать ScalesComponent.dll (Версия 1.0.0)](https://github.com/ТВОЙ_НИК/ScalesComponent/releases/latest)**

---

## 🚀 Возможности
- ✅ **Поддержка 15+ брендов** весов (CAS, Mettler Toledo, Massa-K, A&D, Ohaus, Штрих-М и др.)
- ✅ **Автоматическое определение** типа весов по протоколу (режим `Auto`)
- ✅ **Асинхронное чтение** данных в фоновом потоке без блокировки интерфейса
- ✅ **Определение стабильности** веса по аппаратным флагам производителя
- ✅ **Кроссплатформенность**: работает из 1С, Python, C#, PowerShell, Delphi, Node.js

## 💻 Системные требования
- ОС: Windows 7/8/10/11
- .NET Framework 4.7.2 или выше
- COM-порт (физический или виртуальный USB-to-Serial)

---

## ⚡ Быстрый старт

### 1. Регистрация DLL
Запустите `cmd` **от имени администратора** и выполните:
```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe "путь\к\ScalesComponent.dll" /codebase
```
*(Для 64-битной 1С используйте путь `Framework64`)*

### 2. Использование в 1С:Предприятие
```bsl
Весы = Новый COMОбъект("ScalesComponent.Scales.1");
Если Весы.Connect("COM3", 9600, "Auto") Тогда
    Вес = Весы.GetStableWeight(5000); // Ждем стабильный вес до 5 сек
    Сообщить("Вес: " + Вес + " кг");
    Весы.Disconnect();
КонецЕсли;
```

### 3. Использование в Python
```bash
pip install pywin32
```
```python
import win32com.client
scales = win32com.client.Dispatch("ScalesComponent.Scales.1")
scales.Connect("COM3", 9600, "Auto")
print(f"Вес: {scales.GetStableWeight(5000)} кг")
scales.Disconnect()
```

---

## 📊 Поддерживаемые типы весов
| Бренд | Страна | Пример протокола |
| :--- | :--- | :--- |
| **CAS** | Южная Корея | `ST,GS,+ 1.234 kg` |
| **Mettler Toledo** | Швейцария | ` S 12.345 kg` |
| **Massa-K** | Россия | `S 001234` |
| **A&D** | Япония | `ST,GS,+0012.345kg` |
| **Штрих-М** | Россия | `S 12.345` |
| **Digi** | Япония | `@00W 12.345` |
| **Рысь / Веста / Меркурий** | Россия | `12.345` / `W:12.345` |
*(Полный список и детали протоколов см. в [Документации по терминалам](Docs/))*

---

## 🔌 Краткий API
| Метод | Описание |
| :--- | :--- |
| `Connect(port, baud, type)` | Подключиться к весам |
| `Disconnect()` | Отключиться и освободить порт |
| `GetCurrentWeight()` | Мгновенно получить последний вес из кэша |
| `GetStableWeight(timeout)` | Блокировать поток и ждать стабильный вес (мс) |
| `IsConnected()` / `IsStable()` | Проверка статуса соединения и стабильности |
| `Tare()` | Отправить команду тарирования |
| `GetLastError()` | Получить текст последней ошибки |

---

## 📚 Полная документация

Для глубокого погружения используйте материалы в папке `Docs/`:

- 📖 **[Полное API и примеры кода](Docs/API.md)** — подробные примеры на 1С, Python, C#, PowerShell, JS, Delphi, а также обработка ошибок и FAQ.
- 🔌 **[Документация по терминалам](Docs/)** — распиновка, схемы кабелей и инструкции по настройке меню конкретных моделей (например, [CAS CI-200A](Docs/CAS/CI-200A.md)).

---

## 🛠 Сборка из исходного кода
1. Откройте `ScalesComponent.slnx` в Visual Studio 2019/2022.
2. Выберите конфигурацию **Release**.
3. Выполните **Build → Rebuild Solution**.
4. Зарегистрируйте полученную `ScalesComponent.dll` через `regasm` (см. выше).

---

## 📄 Лицензия
Проект распространяется под лицензией **MIT**. Используйте свободно в коммерческих и некоммерческих проектах.
