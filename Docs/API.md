# Документация COM-объекта ScalesComponent

## Версия 1.0 | Ноябрь 2026

---

## 📋 Оглавление

1. [Обзор проекта](#обзор-проекта)
2. [Архитектура и принципы работы](#архитектура-и-принципы-работы)
3. [Поддерживаемые типы весов](#поддерживаемые-типы-весов)
4. [Установка и регистрация](#установка-и-регистрация)
5. [API COM-объекта](#api-com-объекта)
6. [Примеры использования](#примеры-использования)
   - 6.1. 1С:Предприятие
   - 6.2. Python
   - 6.3. C#
   - 6.4. PowerShell
   - 6.5. JavaScript/Node.js
   - 6.6. Delphi
7. [Обработка ошибок](#обработка-ошибок)
8. [Техническая поддержка и FAQ](#техническая-поддержка-и-faq)

---

## Обзор проекта

**ScalesComponent** — это универсальная COM-библиотека для работы с торговыми и промышленными весами различных производителей через последовательный порт (COM/RS-232).

### Ключевые возможности

✅ **Поддержка 15+ брендов весов** с автоматическим определением типа  
✅ **Асинхронное чтение данных** в фоновом потоке без блокировки интерфейса  
✅ **Буферизация и парсинг** данных с фильтрацией шумов  
✅ **Определение стабильности** веса по флагам производителя  
✅ **Команды управления**: тарирование, запрос веса  
✅ **Кроссплатформенность**: работает с любой средой, поддерживающей COM (1С, Python, C#, Delphi, PowerShell)

### Системные требования

- **ОС**: Windows 7/8/10/11 (32-bit или 64-bit)
- **.NET Framework**: 4.7.2 или выше
- **COM-порт**: физический или виртуальный (USB-to-Serial)
- **Права**: администратор для регистрации DLL

---

## Архитектура и принципы работы

### Структура компонентов

```
┌─────────────────────────────────────────────────────────────┐
│                    ScalesComponent.dll                        │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────────┐    ┌──────────────────────────────┐   │
│  │  Scales (COM)   │───▶│  SerialPortManager           │   │
│  │  - Connect()    │    │  - Фоновое чтение порта      │   │
│  │  - GetWeight()  │    │  - Буферизация данных        │   │
│  │  - Tare()       │    │  - Управление соединением    │   │
│  └─────────────────┘    └──────────────────────────────┘   │
│           │                          │                       │
│           ▼                          ▼                       │
│  ┌─────────────────┐    ┌──────────────────────────────┐   │
│  │  IScalesParser  │◀───│  Parsers (15+ реализаций)    │   │
│  │  - Parse()      │    │  - CAS, Mettler, Massa-K...  │   │
│  │  - Detect()     │    │  - AutoDetect (статистика)   │   │
│  └─────────────────┘    └──────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### Принцип работы

1. **Подключение**: COM-объект открывает последовательный порт с заданными параметрами (скорость, биты данных, стоп-биты, чётность).

2. **Фоновое чтение**: Отдельный поток постоянно читает данные из порта, накапливает их в буфер и обрабатывает построчно.

3. **Парсинг**: Каждая строка передаётся соответствующему парсеру, который извлекает вес и флаг стабильности.

4. **Кэширование**: Последнее валидное значение сохраняется в памяти и доступно по запросу.

5. **Ожидание стабильности**: Метод `GetStableWeight()` блокирует вызывающий поток до получения значения с флагом стабильности или до таймаута.

---

## Поддерживаемые типы весов

| Бренд | Страна | Протокол | Пример данных | Скорость |
|-------|--------|----------|---------------|----------|
| **CAS** | Южная Корея | ASCII | `ST,GS,+ 123.4 kg` | 9600 |
| **Mettler Toledo** | Швейцария | ASCII | ` S 12.345 kg` | 9600 |
| **Massa-K** | Россия | МК | `S 001234` | 2400/9600 |
| **A&D** | Япония | ASCII | `ST,GS,+0012.345kg` | 2400/9600 |
| **Ohaus** | США | ASCII | `ST,GS,+ 12.345 g` | 9600 |
| **Штрих-М** | Россия | ASCII | `S 12.345` | 9600 |
| **Digi** | Япония | ASCII | `@00W 12.345` | 9600 |
| **Рысь** | Россия | ASCII | `12.345` или `S12.345` | 9600 |
| **Веста** | Россия | ASCII | `W:12.345` | 9600 |
| **Bizerba** | Германия | ASCII | `S 12.345 kg` | 9600 |
| **Sartorius** | Германия | ASCII | `S S 12.345 g` | 9600 |
| **Radwag/Kern** | Польша/Германия | ASCII | `MASS: 12.345 g` | 9600 |
| **Меркурий** | Россия | ASCII | `12.345` | 9600 |
| **Тензо-М** | Россия | ASCII | `12.345 kg` | 9600 |
| **Metrisoft** | - | ASCII | `A/B=0.6S` | 9600 |

### Автоматическое определение типа

Парсер `AutoDetect` анализирует первые 3-5 строк данных и определяет тип весов по статистике совпадений с известными паттернами. Рекомендуется использовать, если тип весов неизвестен.

---

## Установка и регистрация

### Шаг 1: Компиляция DLL

1. Откройте проект в **Visual Studio 2019/2022**.
2. Выберите конфигурацию **Release**.
3. Выполните **Build → Rebuild Solution**.
4. DLL будет создана в папке `bin\Release\ScalesComponent.dll`.

### Шаг 2: Регистрация в реестре Windows

Откройте командную строку **от имени администратора** и выполните:

**Для 32-битной 1С:**
```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe "C:\Путь\К\ScalesComponent.dll" /codebase
```

**Для 64-битной 1С:**
```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe "C:\Путь\К\ScalesComponent.dll" /codebase
```

**Для поддержки обеих битностей:**
```cmd
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe "C:\Путь\К\ScalesComponent.dll" /codebase
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\regasm.exe "C:\Путь\К\ScalesComponent.dll" /codebase
```

После успешной регистрации вы увидите сообщение:
```
Типы зарегистрированы успешно
```

### Шаг 3: Проверка регистрации

Откройте **regedit** и найдите ключ:
```
HKEY_CLASSES_ROOT\ScalesComponent.Scales.1
```

Если ключ существует — регистрация прошла успешно.

---

## API COM-объекта

### ProgID

```
ScalesComponent.Scales.1
```

### Методы

#### `Connect(portName: string, baudRate: int, scalesType: string): bool`

Подключается к весам через COM-порт.

**Параметры:**
- `portName` — имя порта (например, `"COM3"`, `"COM1"`)
- `baudRate` — скорость передачи данных (обычно `9600`)
- `scalesType` — тип весов (`"CAS"`, `"MettlerToledo"`, `"Auto"` и т.д.)

**Возвращает:** `true` при успешном подключении, `false` при ошибке.

**Пример:**
```python
result = scales.Connect("COM3", 9600, "CAS")
```

---

#### `Disconnect(): void`

Закрывает соединение с COM-портом и освобождает ресурсы.

**Пример:**
```python
scales.Disconnect()
```

---

#### `GetCurrentWeight(): double`

Мгновенно возвращает последнее известное значение веса из кэша. Не блокирует выполнение.

**Возвращает:** вес в килограммах (тип `double`). Если данных нет — `0.0`.

**Пример:**
```python
weight = scales.GetCurrentWeight()
print(f"Текущий вес: {weight} кг")
```

---

#### `GetStableWeight(timeoutMs: int): double`

Ожидает получения стабильного веса в течение указанного времени. Блокирует вызывающий поток.

**Параметры:**
- `timeoutMs` — максимальное время ожидания в миллисекундах (например, `5000` = 5 секунд)

**Возвращает:** стабильный вес в килограммах. Если не удалось получить — `0.0`.

**Пример:**
```python
weight = scales.GetStableWeight(5000)  # Ждём до 5 секунд
if weight > 0:
    print(f"Стабильный вес: {weight} кг")
else:
    print("Не удалось получить стабильный вес")
```

---

#### `IsConnected(): bool`

Проверяет, установлено ли соединение с весами.

**Возвращает:** `true` если подключено, `false` если нет.

**Пример:**
```python
if scales.IsConnected():
    print("Весы подключены")
```

---

#### `IsStable(): bool`

Проверяет, стабилен ли текущий вес (по флагу от весов).

**Возвращает:** `true` если вес стабилен, `false` если нет.

**Пример:**
```python
if scales.IsStable():
    print("Вес стабилен, можно записывать")
```

---

#### `Tare(): void`

Отправляет команду тарирования (обнуления) на весы.

**Пример:**
```python
scales.Tare()
print("Весы обнулены")
```

---

#### `GetLastError(): string`

Возвращает текст последней ошибки.

**Возвращает:** строку с описанием ошибки или пустую строку, если ошибок не было.

**Пример:**
```python
error = scales.GetLastError()
if error:
    print(f"Ошибка: {error}")
```

---

#### `GetAvailableScalesTypes(): string[]`

Возвращает массив строк с названиями всех поддерживаемых типов весов.

**Возвращает:** массив строк (например, `["CAS", "MettlerToledo", "Auto", ...]`).

**Пример:**
```python
types = scales.GetAvailableScalesTypes()
for t in types:
    print(t)
```

---

## Примеры использования

### 6.1. 1С:Предприятие (управляемые формы)

#### Подключение и взвешивание

```bsl
&НаКлиенте
Перем ВесыCOM;

&НаКлиенте
Процедура ПодключитьВесы(Команда)
    
    Попытка
        // Создаём COM-объект через реестр Windows
        ВесыCOM = Новый COMОбъект("ScalesComponent.Scales.1");
        
        // Подключаемся к COM3, скорость 9600, тип CAS
        Если ВесыCOM.Connect("COM3", 9600, "CAS") Тогда
            Сообщить("✅ Весы подключены");
        Иначе
            Сообщить("❌ Ошибка: " + ВесыCOM.GetLastError());
        КонецЕсли;
        
    Исключение
        Сообщить("❌ Ошибка создания объекта: " + ОписаниеОшибки());
    КонецПопытки;
    
КонецПроцедуры

&НаКлиенте
Процедура ВзвеситьТовар(Команда)
    
    Если ВесыCOM = Неопределено Тогда
        Сообщить("Сначала подключите весы!");
        Возврат;
    КонецЕсли;
    
    Попытка
        // Ждём стабильный вес до 5 секунд
        Вес = ВесыCOM.GetStableWeight(5000);
        
        Если Вес > 0 Тогда
            Сообщить("✅ Вес: " + Вес + " кг");
            // Записываем в документ
            Объект.Вес = Вес;
        Иначе
            Сообщить("❌ Не удалось получить вес: " + ВесыCOM.GetLastError());
        КонецЕсли;
        
    Исключение
        Сообщить("❌ Ошибка: " + ОписаниеОшибки());
    КонецПопытки;
    
КонецПроцедуры

&НаКлиенте
Процедура Тарировать(Команда)
    
    Если ВесыCOM <> Неопределено Тогда
        ВесыCOM.Tare();
        Сообщить("✅ Тарирование выполнено");
    КонецЕсли;
    
КонецПроцедуры

&НаКлиенте
Процедура ОтключитьВесы(Команда)
    
    Если ВесыCOM <> Неопределено Тогда
        ВесыCOM.Disconnect();
        ВесыCOM = Неопределено;
        Сообщить("✅ Отключено от весов");
    КонецЕсли;
    
КонецПроцедуры
```

#### Серийное взвешивание в цикле

```bsl
&НаКлиенте
Процедура СерийноеВзвешивание(Команда)
    
    Если ВесыCOM = Неопределено Тогда
        Сообщить("Сначала подключите весы!");
        Возврат;
    КонецЕсли;
    
    Для Каждого Товар Из Объект.Товары Цикл
        
        Сообщить("Положите на весы: " + Товар.Наименование);
        
        // Ждём стабильный вес до 10 секунд
        Вес = ВесыCOM.GetStableWeight(10000);
        
        Если Вес > 0 Тогда
            Товар.Вес = Вес;
            Сообщить("  ✓ " + Товар.Наименование + ": " + Вес + " кг");
            
            // Тарирование для следующего товара
            ВесыCOM.Tare();
            Sleep(500); // Пауза 0.5 секунды
        Иначе
            Сообщить("  ✗ Ошибка взвешивания: " + Товар.Наименование);
        КонецЕсли;
        
    КонецЦикла;
    
КонецПроцедуры
```

---

### 6.2. Python

#### Установка зависимостей

```bash
pip install pywin32
```

#### Базовый пример

```python
import pythoncom
import win32com.client
import time

# Инициализация COM
pythoncom.CoInitialize()

# Создание объекта
scales = win32com.client.Dispatch("ScalesComponent.Scales.1")

# Подключение
if scales.Connect("COM3", 9600, "CAS"):
    print("✅ Весы подключены")
else:
    print(f"❌ Ошибка: {scales.GetLastError()}")
    exit(1)

# Получение стабильного веса
weight = scales.GetStableWeight(5000)
if weight > 0:
    print(f"✅ Вес: {weight} кг")
else:
    print(f"❌ Ошибка: {scales.GetLastError()}")

# Тарирование
scales.Tare()
print("✅ Тарирование выполнено")

# Отключение
scales.Disconnect()
pythoncom.CoUninitialize()
```

#### Класс-обёртка для удобной работы

```python
class Scales:
    """Обёртка над COM-объектом для работы с весами."""
    
    def __init__(self, port="COM3", baud_rate=9600, scales_type="CAS"):
        pythoncom.CoInitialize()
        self._obj = win32com.client.Dispatch("ScalesComponent.Scales.1")
        self.port = port
        self.baud_rate = baud_rate
        self.scales_type = scales_type
        self._connected = False
    
    def connect(self):
        """Подключиться к весам."""
        if self._obj.Connect(self.port, self.baud_rate, self.scales_type):
            self._connected = True
            return True
        return False
    
    def get_weight(self, stable=True, timeout=5000):
        """Получить вес (stable=True - ждать стабильный)."""
        if stable:
            return self._obj.GetStableWeight(timeout)
        return self._obj.GetCurrentWeight()
    
    def tare(self):
        """Тарировать весы."""
        self._obj.Tare()
    
    def disconnect(self):
        """Отключиться от весов."""
        if self._connected:
            self._obj.Disconnect()
            self._connected = False
        pythoncom.CoUninitialize()
    
    def __enter__(self):
        self.connect()
        return self
    
    def __exit__(self, exc_type, exc_val, exc_tb):
        self.disconnect()


# Использование с контекстным менеджером
with Scales("COM3", 9600, "CAS") as scales:
    weight = scales.get_weight(stable=True, timeout=5000)
    print(f"Вес: {weight} кг")
    
    scales.tare()
    time.sleep(1)
    
    weight2 = scales.get_weight(stable=True)
    print(f"После тарирования: {weight2} кг")
```

#### Мониторинг в реальном времени с графиком

```python
import matplotlib.pyplot as plt
from collections import deque
import time

with Scales("COM3", 9600, "Auto") as scales:
    weights = deque(maxlen=100)
    times = deque(maxlen=100)
    
    fig, ax = plt.subplots()
    line, = ax.plot([], [])
    ax.set_ylim(0, 10)
    ax.set_title("Вес в реальном времени")
    
    start = time.time()
    
    try:
        while True:
            w = scales.get_weight(stable=False)
            t = time.time() - start
            
            weights.append(w)
            times.append(t)
            
            line.set_data(times, weights)
            ax.relim()
            ax.autoscale_view()
            fig.canvas.draw()
            fig.canvas.flush_events()
            
            time.sleep(0.2)
    except KeyboardInterrupt:
        plt.show()
```

---

### 6.3. C#

#### Подключение через COM Interop

```csharp
using System;
using System.Runtime.InteropServices;

class Program
{
    static void Main()
    {
        // Создание COM-объекта
        dynamic scales = Activator.CreateInstance(
            Type.GetTypeFromProgID("ScalesComponent.Scales.1")
        );
        
        // Подключение
        bool connected = scales.Connect("COM3", 9600, "CAS");
        
        if (connected)
        {
            Console.WriteLine("✅ Весы подключены");
            
            // Получение стабильного веса
            double weight = scales.GetStableWeight(5000);
            
            if (weight > 0)
            {
                Console.WriteLine($"✅ Вес: {weight} кг");
            }
            else
            {
                Console.WriteLine($"❌ Ошибка: {scales.GetLastError()}");
            }
            
            // Тарирование
            scales.Tare();
            Console.WriteLine("✅ Тарирование выполнено");
            
            // Отключение
            scales.Disconnect();
        }
        else
        {
            Console.WriteLine($"❌ Ошибка подключения: {scales.GetLastError()}");
        }
    }
}
```

#### Строгая типизация через TLB

Если вы сгенерировали TLB-файл при регистрации, можно добавить ссылку на него в проект и использовать строгую типизацию:

```csharp
using ScalesComponent;

class Program
{
    static void Main()
    {
        IScales scales = new Scales();
        
        if (scales.Connect("COM3", 9600, "CAS"))
        {
            double weight = scales.GetStableWeight(5000);
            Console.WriteLine($"Вес: {weight} кг");
            
            scales.Disconnect();
        }
    }
}
```

---

### 6.4. PowerShell

```powershell
# Создание COM-объекта
$scales = New-Object -ComObject "ScalesComponent.Scales.1"

# Подключение
$connected = $scales.Connect("COM3", 9600, "CAS")

if ($connected) {
    Write-Host "✅ Весы подключены" -ForegroundColor Green
    
    # Получение веса
    $weight = $scales.GetStableWeight(5000)
    
    if ($weight -gt 0) {
        Write-Host "✅ Вес: $weight кг" -ForegroundColor Green
    } else {
        Write-Host "❌ Ошибка: $($scales.GetLastError())" -ForegroundColor Red
    }
    
    # Тарирование
    $scales.Tare()
    Write-Host "✅ Тарирование выполнено" -ForegroundColor Green
    
    # Отключение
    $scales.Disconnect()
} else {
    Write-Host "❌ Ошибка подключения: $($scales.GetLastError())" -ForegroundColor Red
}

# Освобождение COM-объекта
[System.Runtime.InteropServices.Marshal]::ReleaseComObject($scales) | Out-Null
```

---

### 6.5. JavaScript/Node.js

#### Установка зависимостей

```bash
npm install node-activex
```

#### Пример использования

```javascript
const ActiveX = require('node-activex');

// Создание COM-объекта
const scales = new ActiveX.Object('ScalesComponent.Scales.1');

// Подключение
const connected = scales.Connect('COM3', 9600, 'CAS');

if (connected) {
    console.log('✅ Весы подключены');
    
    // Получение веса
    const weight = scales.GetStableWeight(5000);
    
    if (weight > 0) {
        console.log(`✅ Вес: ${weight} кг`);
    } else {
        console.log(`❌ Ошибка: ${scales.GetLastError()}`);
    }
    
    // Тарирование
    scales.Tare();
    console.log('✅ Тарирование выполнено');
    
    // Отключение
    scales.Disconnect();
} else {
    console.log(`❌ Ошибка подключения: ${scales.GetLastError()}`);
}
```

---

### 6.6. Delphi

```pascal
uses
  ComObj, ActiveX;

procedure TForm1.Button1Click(Sender: TObject);
var
  Scales: OleVariant;
  Weight: Double;
  Connected: Boolean;
begin
  try
    // Создание COM-объекта
    Scales := CreateOleObject('ScalesComponent.Scales.1');
    
    // Подключение
    Connected := Scales.Connect('COM3', 9600, 'CAS');
    
    if Connected then
    begin
      ShowMessage('✅ Весы подключены');
      
      // Получение веса
      Weight := Scales.GetStableWeight(5000);
      
      if Weight > 0 then
        ShowMessage(Format('✅ Вес: %.3f кг', [Weight]))
      else
        ShowMessage('❌ Ошибка: ' + Scales.GetLastError);
      
      // Тарирование
      Scales.Tare;
      ShowMessage('✅ Тарирование выполнено');
      
      // Отключение
      Scales.Disconnect;
    end
    else
      ShowMessage('❌ Ошибка подключения: ' + Scales.GetLastError);
      
  except
    on E: Exception do
      ShowMessage('❌ Ошибка: ' + E.Message);
  end;
end;
```

---

## Обработка ошибок

### Типичные ошибки и их решения

| Код ошибки | Описание | Решение |
|-----------|----------|---------|
| `0x800401F3` | Недопустимая строка с указанием класса | DLL не зарегистрирована. Выполните `regasm` от администратора |
| `0x80070005` | Отказано в доступе | Запустите приложение от имени администратора |
| `0x80040154` | Класс не зарегистрирован | Несоответствие битности. Зарегистрируйте DLL через правильный `regasm` (32 или 64 бит) |
| `0x800700AA` | Ресурс занят | COM-порт занят другой программой. Закройте Putty, терминал или другое приложение |
| `0x80070002` | Файл не найден | Неверное имя COM-порта. Проверьте в Диспетчере устройств |

### Пример обработки ошибок в 1С

```bsl
&НаКлиенте
Процедура ПодключитьСОбработкойОшибок(Команда)
    
    Попытка
        ВесыCOM = Новый COMОбъект("ScalesComponent.Scales.1");
        
        Если Не ВесыCOM.Connect("COM3", 9600, "CAS") Тогда
            ВызватьИсключение "Не удалось подключиться: " + ВесыCOM.GetLastError();
        КонецЕсли;
        
        Вес = ВесыCOM.GetStableWeight(5000);
        
        Если Вес = 0 Тогда
            ВызватьИсключение "Не удалось получить вес: " + ВесыCOM.GetLastError();
        КонецЕсли;
        
        Сообщить("Вес: " + Вес + " кг");
        
    Исключение
        ТекстОшибки = ОписаниеОшибки();
        
        Если Найти(ТекстОшибки, "0x800401F3") > 0 Тогда
            Сообщить("❌ DLL не зарегистрирована. Выполните regasm от администратора.");
        ИначеЕсли Найти(ТекстОшибки, "0x800700AA") > 0 Тогда
            Сообщить("❌ COM-порт занят. Закройте другие программы.");
        Иначе
            Сообщить("❌ Ошибка: " + ТекстОшибки);
        КонецЕсли;
        
    КонецПопытки;
    
КонецПроцедуры
```

---

## Техническая поддержка и FAQ

### Q: Как определить, какой тип весов использовать?

**A:** Используйте тип `"Auto"` — библиотека автоматически определит протокол по первым строкам данных.

### Q: Вес скачет, показания нестабильны. Что делать?

**A:** 
1. Убедитесь, что весы установлены на ровной поверхности без вибраций.
2. Используйте метод `GetStableWeight()` вместо `GetCurrentWeight()` — он ждёт флаг стабильности от весов.
3. Если весы не поддерживают флаг стабильности, реализуйте усреднение на стороне клиента (медианный фильтр по 5-10 измерениям).

### Q: Можно ли работать с USB-весами?

**A:** Да, если производитель предоставляет виртуальный COM-порт (USB-to-Serial драйвер). Проверьте в Диспетчере устройств, какой COM-порт назначен весам.

### Q: Как изменить параметры COM-порта (биты данных, стоп-биты, чётность)?

**A:** В текущей версии параметры жёстко заданы: 8 бит данных, 1 стоп-бит, без чётности (8N1). Это стандарт для 99% весов. Если ваши весы используют другие параметры, свяжитесь с разработчиком для добавления настройки.

### Q: Работает ли библиотека с весами, подключёнными по Ethernet/Wi-Fi?

**A:** Нет, библиотека работает только с последовательными портами (COM/RS-232). Для сетевых весов нужен другой подход (TCP/IP клиент).

### Q: Как добавить поддержку нового типа весов?

**A:** 
1. Создайте новый класс-парсер, реализующий интерфейс `IScalesParser`.
2. Добавьте его в словарь `_parsers` в классе `AutoDetectParser`.
3. Добавьте кейс в метод `CreateParser` класса `Scales`.
4. Перекомпилируйте и зарегистрируйте DLL.

### Q: Можно ли использовать библиотеку в веб-приложении?

**A:** COM-объекты работают только в Windows. Для веб-приложений нужен промежуточный сервис (например, Python-скрипт, который читает вес и отдаёт его через REST API).

---

## Лицензия и контакты

**ScalesComponent** распространяется по лицензии MIT. Используйте свободно в коммерческих и некоммерческих проектах.

**Автор**: [Ваше имя/компания]  
**Email**: [your@email.com]  
**GitHub**: [https://github.com/yourusername/scales-component](https://github.com/yourusername/scales-component)

---

## История версий

### Версия 1.0 (Октябрь 2026)
- ✅ Первоначальный релиз
- ✅ Поддержка 15+ типов весов
- ✅ Автоматическое определение типа
- ✅ Асинхронное чтение в фоновом потоке
- ✅ Примеры для 1С, Python, C#, PowerShell, JavaScript, Delphi

---

**Документация обновлена**: 1 ноября 2026  
**Версия библиотеки**: 1.0.0