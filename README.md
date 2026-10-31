
# ScalesComponent

Универсальная COM-библиотека для работы с торговыми и промышленными весами через COM-порт (RS-232).

## 📥 Скачать готовую сборку

Если вы не хотите компилировать проект самостоятельно, скачайте готовую библиотеку:
👉 **[Скачать ScalesComponent.dll (Версия 1.0.0)](https://github.com/cripp09/ScalesComponent/releases/latest)**


## Возможности

- Поддержка **15+ брендов** весов (CAS, Mettler Toledo, Massa-K, A&D, Ohaus, Штрих-М и др.)
- **Автоматическое определение** типа весов по протоколу
- Асинхронное чтение данных в фоновом потоке
- Определение стабильности веса
- Работа из 1С, Python, C#, PowerShell, Delphi, Node.js

## Системные требования

- Windows 7/8/10/11
- .NET Framework 4.7.2+
- COM-порт (физический или USB-to-Serial)

## Быстрый старт

### 1. Сборка

Откройте проект в Visual Studio и выполните **Build → Rebuild Solution** (Release).

### 2. Регистрация DLL

Запустите `cmd` **от имени администратора**:

```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe "путь\к\ScalesComponent.dll" /codebase
```

### 3. Использование в 1С

```bsl
Весы = Новый COMОбъект("ScalesComponent.Scales.1");
Весы.Connect("COM3", 9600, "CAS");
Вес = Весы.GetStableWeight(5000);
Весы.Disconnect();
```

### 4. Использование в Python

```bash
pip install pywin32
```

```python
import win32com.client
scales = win32com.client.Dispatch("ScalesComponent.Scales.1")
scales.Connect("COM3", 9600, "Auto")
weight = scales.GetStableWeight(5000)
print(f"Вес: {weight} кг")
```

## Поддерживаемые типы весов

| Бренд | Страна | Пример протокола |
|-------|--------|------------------|
| CAS | Южная Корея | `ST,GS,+ 1.234 kg` |
| Mettler Toledo | Швейцария | ` S 12.345 kg` |
| Massa-K | Россия | `S 001234` |
| A&D | Япония | `ST,GS,+0012.345kg` |
| Ohaus | США | `ST,GS,+ 12.345 g` |
| Штрих-М | Россия | `S 12.345` |
| Digi | Япония | `@00W 12.345` |
| Рысь | Россия | `12.345` |
| Веста | Россия | `W:12.345` |
| Bizerba | Германия | `S 12.345 kg` |
| Sartorius | Германия | `S S 12.345 g` |
| Radwag/Kern | Польша | `MASS: 12.345 g` |
| Меркурий | Россия | `12.345` |
| Тензо-М | Россия | `12.345 kg` |
| Metrisoft | - | `A/B=0.6S` |

## API

| Метод | Описание |
|-------|----------|
| `Connect(port, baud, type)` | Подключиться к весам |
| `Disconnect()` | Отключиться |
| `GetCurrentWeight()` | Мгновенно получить вес |
| `GetStableWeight(timeout)` | Ждать стабильный вес |
| `IsConnected()` | Проверить соединение |
| `IsStable()` | Проверить стабильность |
| `Tare()` | Тарирование |
| `GetLastError()` | Последняя ошибка |
| `GetAvailableScalesTypes()` | Список типов |

## Лицензия

MIT
