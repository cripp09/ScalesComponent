import sys
import time
import pythoncom
import win32com.client


class Scales:
    """
    Обёртка над COM-объектом ScalesComponent для работы с весами.
    """

    PROG_ID = "ScalesComponent.Scales.1"

    def __init__(self):
        # Инициализируем COM для текущего потока
        pythoncom.CoInitialize()
        self._obj = None

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        self.disconnect()
        pythoncom.CoUninitialize()

    def _ensure_connected(self):
        if self._obj is None:
            raise RuntimeError("COM-объект не создан. Сначала вызовите connect().")

    def create(self):
        """Создать COM-объект."""
        try:
            self._obj = win32com.client.Dispatch(self.PROG_ID)
            print(f"[OK] COM-объект {self.PROG_ID} создан")
            return True
        except pythoncom.com_error as e:
            print(f"[ERROR] Не удалось создать COM-объект: {e}")
            print("Убедитесь, что DLL зарегистрирована через regasm /codebase")
            return False

    def connect(self, port: str, baud_rate: int = 9600, scales_type: str = "Metrisoft") -> bool:
        """
        Подключиться к весам.
        
        :param port: Имя COM-порта, например "COM3"
        :param baud_rate: Скорость (обычно 9600)
        :param scales_type: Тип весов (Metrisoft, CAS, Auto)
        :return: True если успешно
        """
        self._ensure_connected()
        try:
            result = self._obj.Connect(port, int(baud_rate), scales_type)
            if result:
                print(f"[OK] Подключено к {port} ({scales_type}, {baud_rate} бод)")
            else:
                print(f"[ERROR] Ошибка подключения: {self._obj.GetLastError()}")
            return bool(result)
        except pythoncom.com_error as e:
            print(f"[ERROR] Исключение при подключении: {e}")
            return False

    def disconnect(self):
        """Отключиться от весов."""
        if self._obj is not None:
            try:
                self._obj.Disconnect()
                print("[OK] Отключено от весов")
            except pythoncom.com_error as e:
                print(f"[ERROR] Ошибка отключения: {e}")

    def get_current_weight(self) -> float:
        """Получить текущий вес (мгновенно, последнее значение из буфера)."""
        self._ensure_connected()
        try:
            return float(self._obj.GetCurrentWeight())
        except pythoncom.com_error as e:
            print(f"[ERROR] Ошибка чтения веса: {e}")
            return 0.0

    def get_stable_weight(self, timeout_ms: int = 5000) -> float:
        """
        Получить стабильный вес с ожиданием.
        Блокирует поток до появления флага стабильности или таймаута.
        """
        self._ensure_connected()
        try:
            weight = float(self._obj.GetStableWeight(int(timeout_ms)))
            if weight > 0:
                return weight
            else:
                print(f"[WARN] Стабильный вес не получен: {self._obj.GetLastError()}")
                return 0.0
        except pythoncom.com_error as e:
            print(f"[ERROR] Ошибка получения стабильного веса: {e}")
            return 0.0

    def is_connected(self) -> bool:
        """Проверить, подключены ли весы."""
        if self._obj is None:
            return False
        try:
            return bool(self._obj.IsConnected())
        except pythoncom.com_error:
            return False

    def is_stable(self) -> bool:
        """Проверить, стабилен ли текущий вес."""
        self._ensure_connected()
        try:
            return bool(self._obj.IsStable())
        except pythoncom.com_error:
            return False

    def tare(self):
        """Тарировать весы."""
        self._ensure_connected()
        try:
            self._obj.Tare()
            print("[OK] Тарирование выполнено")
        except pythoncom.com_error as e:
            print(f"[ERROR] Ошибка тарирования: {e}")

    def get_last_error(self) -> str:
        """Получить текст последней ошибки."""
        if self._obj is None:
            return ""
        try:
            return str(self._obj.GetLastError())
        except pythoncom.com_error:
            return ""

    def get_available_types(self) -> list:
        """Получить список поддерживаемых типов весов."""
        self._ensure_connected()
        try:
            # COM возвращает tuple, превращаем в список
            return list(self._obj.GetAvailableScalesTypes())
        except pythoncom.com_error as e:
            print(f"[ERROR] Ошибка получения списка типов: {e}")
            return []


def demo_interactive():
    """Интерактивная демонстрация работы с весами."""
    print("=" * 60)
    print("  Демонстрация работы с весами через Python + COM")
    print("=" * 60)

    with Scales() as scales:
        if not scales.create():
            print("\nНе удалось создать COM-объект. Выход.")
            sys.exit(1)

        # Показываем доступные типы
        types = scales.get_available_types()
        print(f"\nДоступные типы весов: {', '.join(types)}")

        # Параметры подключения
        port = input("\nВведите COM-порт (например COM3): ").strip() or "COM3"
        baud = input("Скорость (Enter = 9600): ").strip() or "9600"
        scale_type = input("Тип весов (Enter = Metrisoft): ").strip() or "Metrisoft"

        if not scales.connect(port, int(baud), scale_type):
            print("\nНе удалось подключиться. Выход.")
            sys.exit(1)

        print("\n--- Меню ---")
        print("1 - Получить текущий вес (быстро)")
        print("2 - Получить стабильный вес (с ожиданием)")
        print("3 - Тарировать")
        print("4 - Проверить стабильность")
        print("5 - Циклический опрос (Ctrl+C для остановки)")
        print("0 - Выход")

        while True:
            try:
                choice = input("\nВаш выбор: ").strip()

                if choice == "1":
                    weight = scales.get_current_weight()
                    stable = " [СТАБИЛЕН]" if scales.is_stable() else ""
                    print(f"  → Вес: {weight} кг{stable}")

                elif choice == "2":
                    print("  Жду стабильный вес (до 5 сек)...")
                    weight = scales.get_stable_weight(5000)
                    if weight > 0:
                        print(f"  → Стабильный вес: {weight} кг")
                    else:
                        print(f"  → Не удалось: {scales.get_last_error()}")

                elif choice == "3":
                    scales.tare()

                elif choice == "4":
                    stable = scales.is_stable()
                    print(f"  → Стабильность: {'Да' if stable else 'Нет'}")

                elif choice == "5":
                    print("  Циклический опрос. Ctrl+C для остановки.\n")
                    try:
                        while True:
                            weight = scales.get_current_weight()
                            stable = "S" if scales.is_stable() else " "
                            print(f"\r  [{stable}] {weight:.3f} кг  ", end="", flush=True)
                            time.sleep(0.3)
                    except KeyboardInterrupt:
                        print("\n  Остановлено.")

                elif choice == "0":
                    print("\nВыход.")
                    break

                else:
                    print("  Неизвестная команда.")

            except KeyboardInterrupt:
                print("\nПрервано.")
                break


def demo_simple():
    """Простой пример: подключился → взвесил → отключился."""
    with Scales() as scales:
        if not scales.create():
            return

        if not scales.connect("COM3", 9600, "Metrisoft"):
            return

        # Ждём стабильный вес
        weight = scales.get_stable_weight(5000)
        print(f"Результат взвешивания: {weight} кг")

        # Тарирование
        scales.tare()
        time.sleep(1)

        # Ещё одно взвешивание
        weight2 = scales.get_stable_weight(5000)
        print(f"После тарирования: {weight2} кг")


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--simple":
        demo_simple()
    else:
        demo_interactive()