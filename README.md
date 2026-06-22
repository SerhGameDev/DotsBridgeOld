# DotsBridge 🚀

[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity&logoColor=white)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Cross--platform-blue)](#)

[cite_start]**DotsBridge** — это кроссплатформенный открытый архитектурный фреймворк и инструментарий для разработки высокопроизводительных имитационных моделей промышленных установок и цифровых двойников (Digital Twins) в реальном времени[cite: 43, 51]. 

[cite_start]Фреймворк эффективно устраняет технологический разрыв между сложным, низкоуровневым стеком **Unity DOTS (Data-Oriented Technology Stack)** и классической объектно-ориентированной (ООП) бизнес-логикой верхнего уровня[cite: 161, 162].

---

## 💡 Решаемые проблемы

[cite_start]При создании масштабных индустриальных симуляторов (с тысячами труб, датчиков и задвижек) классический подход `MonoBehaviour` (ООП) сталкивается с аппаратными ограничениями: избыточным выделением памяти в куче, постоянными промахами кэша процессора (Cache Misses) и фризами из-за сборщика мусора (Garbage Collector)[cite: 40, 113].

[cite_start]`DotsBridge` переводит вычисления на рельсы архитектурного паттерна **ECS**, **C# Job System** и компилятора **Burst**[cite: 41, 132]. [cite_start]Он упаковывает данные в последовательные блоки памяти (Chunks), позволяя процессору обрабатывать их на максимальной физической скорости шины с использованием векторных инструкций SIMD[cite: 135, 148, 154].

### Сравнение характеристик производительности

| Критерий сравнения | Классический подход (MonoBehaviour / ООП) | Data-Oriented Technology Stack (DOTS) + `DotsBridge` |
| :--- | :--- | :--- |
| **Организация данных** | [cite_start]Ссылочные типы, рассредоточенные в управляемой куче (Heap)[cite: 139]. | [cite_start]Значимые типы, упакованные в последовательные блоки (Chunks)[cite: 139]. |
| **Эффективность кэша ЦП** | [cite_start]Низкая (высокая частота промахов кэша Cache Misses)[cite: 139]. | [cite_start]Максимальная (оптимальная загрузка строк кэша Cache Lines)[cite: 139]. |
| **Многопоточность** | [cite_start]Затруднена из-за Race Conditions и Mutex-блокировок[cite: 139, 150]. | [cite_start]Нативная, безопасная параллелизация на все доступные ядра ЦП[cite: 139]. |
| **Сборка мусора (GC)** | [cite_start]Высокая нагрузка на Garbage Collector, риск микрофризов[cite: 141]. | [cite_start]Линейная структура (Unmanaged Memory), отсутствие нагрузки на GC[cite: 141]. |
| **Оптимизация кода** | [cite_start]Стандартная JIT-компиляция со средним уровнем оптимизации[cite: 141]. | [cite_start]Глубокая низкоуровневая оптимизация под архитектуру процессора[cite: 141]. |

---

## 🛠️ Архитектура и ключевые модули

[cite_start]Фреймворк предлагает гибридную программную архитектуру, совмещающую жестко структурированный параллельный конвейер обработки данных с гибким, абстрактным событийно-ориентированным интерфейсом верхнего уровня[cite: 57].

### 1. Ядро фреймворка (Core Architecture)
[cite_start]Роль единой точки входа выполняют статические фасады `ClientBridge` и `ServerBridge`, инкапсулирующие доступ к `EntityManager`[cite: 164]. [cite_start]Вместо работы с сырыми идентификаторами `Entity` фреймворк предоставляет декларативный **Fluent API** и две строго типизированные структуры[cite: 165, 170]:
* [cite_start]`SingleEntity` — легкий фасад (`readonly struct`) для управления одной сущностью в главном потоке[cite: 166].
* [cite_start]`ListEntity` — объектно-ориентированный фасад для массовых, пакетных (batch) операций над группами сущностей за одно структурное изменение памяти (Structural Change)[cite: 168, 169, 232].

[cite_start]Дополнительно интегрирована подсистема **IdMapSystem**, которая индексирует объекты в `NativeParallelMultiHashMap`, обеспечивая мгновенный поиск технологических узлов по строковым ID за константное время $O(1)$[cite: 75, 174, 180, 184].

### Жизненный цикл сущности в DotsBridge
1. [cite_start]**Инициализация (`SpawnerBuilder`)**: Накопление конфигурации в ООП-контексте[cite: 188, 190].
2. [cite_start]**Аллокация (Спавн)**: Мгновенное тиражирование сущностей в ECS-архетипы[cite: 191, 192].
3. [cite_start]**Симуляция**: Сущность обрабатывается внутри Чанков последовательно, без участия главного потока[cite: 195, 196].
4. [cite_start]**Регистрация триггера уничтожения (`DeathEvent`)**: Перевод маркера в состояние `true` для исключения из расчетов[cite: 198].
5. [cite_start]**Деаллокация и вызов обратных связей (`OnDestroyEvents`)**: Синхронный вызов управляемых делегатов (например, для UI или звуков) и полная очистка памяти[cite: 200, 202, 203].

### 2. Модуль перемещения (Movement Module)
[cite_start]Обеспечивает групповое пространственное изменение состояний механизмов (задвижек, заслонок)[cite: 502]. [cite_start]Использует интерфейс `IEnableableComponent` для компонента-маркера `IsTransformMoving`, что позволяет включать/выключать движение через битовую маску активности с нулевыми накладными расходами на перераспределение памяти[cite: 213, 233, 235].

### 3. Модуль физического взаимодействия (Raycast Module)
[cite_start]Организует двусторонний шлюз данных, преобразующий физические пересечения лучей в асинхронном пространстве `Unity.Physics` в классические управляемые C# делегаты (`Action<T>`) верхнего уровня[cite: 239, 241, 242].
* [cite_start]**Эмиттеры**: Поддержка `PointerRayEmitter` (курсор/тач), `ScreenCenterRayEmitter` (центр экрана/взгляд в VR) и `CustomRayEmitter` (произвольные векторы)[cite: 246, 247, 248].
* [cite_start]**Маркерная система состояний**: Объекты размечаются легкими компонентами `RaycastHovered`, `RaycastPressed`, `RaycastReleased`[cite: 264, 265, 267, 268].
* [cite_start]**Атомарность**: Класс `RaycastEventListener` фильтрует ежекадровый спам, гарантируя строго однократное срабатывание события при пересечении границы объекта[cite: 271, 274, 281].

### 4. Промышленный шлюз (ModbusBridge)
[cite_start]Асинхронный сетевой драйвер протокола **Modbus TCP** для двустороннего обмена данными с реальными ПЛК и SCADA-системами без блокировки главного графического потока Unity[cite: 55, 81].
* [cite_start]**Оптимизация трафика**: Метод `TryGetPollingRange` автоматически вычисляет минимальный диапазонHolding-регистров ПЛК для сканирования одной непрерывной посылкой[cite: 82, 292].
* [cite_start]**Потокобезопасность**: Защита от Race Conditions реализована через неблокирующие очереди `ConcurrentQueue`, флаги `volatile` и паттерн `Main Thread Dispatcher`[cite: 83, 305, 306, 308, 310].
* [cite_start]**Конвертер данных**: `ModbusDataConverter` поддерживает аппаратный byte-сваппинг различных порядков следования байт и слов в памяти (`ABCD`, `CDAB`, `BADC`, `DCBA`)[cite: 84, 323].

### 5. Математическое ядро процессов (RefineryBridge)
[cite_start]Заменяет тяжелые сеточные CFD-методы альтернативной гидравлической моделью сообщающихся сосудов на основе градиентов активных и унаследованных потенциалов давлений[cite: 86, 331, 332].
* [cite_start]Рассчитывает многокомпонентный перенос масс с сохранением процентного фракционного состава сред[cite: 87, 355].
* [cite_start]Моделирует термодинамику средневзвешенного смешивания температур и естественные теплопотери резервуаров по закону Ньютона[cite: 87, 359, 361].
* [cite_start]Распараллеливает вычисления в `Unity Job System` с использованием `Burst Compiler` на 4 независимых стадии: `PropagatePotentialJob` $\rightarrow$ `CalculateFlowJob` $\rightarrow$ `ApplyTransfersJob` $\rightarrow$ `DistributeMassJob`[cite: 88, 365].

---

## 💻 Примеры использования (Code Snippets)

### Использование фасада `SingleEntity` (Исходный код ядра)
[cite_start]Ниже представлен пример реализации структуры `SingleEntity`, инкапсулирующей низкоуровневые манипуляции с памятью чанков ECS[cite: 73]:

```csharp
public readonly struct SingleEntity
{
    public readonly Entity Entity;
    public readonly BridgeWorld Bridge;
    public EntityManager Manager => Bridge.Manager;

    public SingleEntity(Entity entity, BridgeWorld bridge)
    {
        Entity = entity;
        Bridge = bridge;
    }

    /// <summary>
    /// Включает или выключает конкретный компонент на сущности.
    /// </summary>
    public SingleEntity SetEnabled<T>(bool enabled) where T : unmanaged, IEnableableComponent
    {
        if (!Manager.Exists(Entity)) return this;
        
        if (Manager.HasComponent<T>(Entity))
        {
            Manager.SetComponentEnabled<T>(Entity, enabled);
        }
        return this;
    }

    /// <summary>
    /// Конвертирует одиночную сущность в ListEntity (группу из одного элемента).
    /// Выделяет NativeList, поэтому результат требует вызова Dispose!
    /// </summary>
    public ListEntity ToListEntity(Allocator allocator = Allocator.Temp)
    {
        var list = new NativeList<Entity>(1, allocator);
        if (Manager.Exists(Entity))
        {
            list.Add(Entity);
        }
        return new ListEntity(Bridge, list, isOwner: true);
    }
}
