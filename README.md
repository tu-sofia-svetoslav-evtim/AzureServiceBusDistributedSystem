# Distributed Student Enrollment System

Разпределена система за студентско записване, базирана на **Event-Driven Architecture** и **Microservices**. Проектът демонстрира използването на асинхронна комуникация чрез **Azure Service Bus** за разкачане (decoupling) на компонентите и управление на натоварването.

---

## Архитектура

Системата се състои от три независими микросървиса:

1. **Admin API (Producer) – Port 5000**
   - REST API за администратори.
   - Публикува събития за създаване на нови курсове в опашка `course-queue`.

2. **Student API (Producer) – Port 5100**
   - REST API за студенти.
   - Приема заявки за записване и ги изпраща към опашка `enrollment-queue`.

3. **Backend Worker (Consumer)**
   - Фонова услуга (BackgroundService).
   - Слуша и двете опашки едновременно.
   - Изпълнява бизнес логиката (проверка на капацитет, валидация за дубликати) и поддържа in-memory състояние на системата.

**Инфраструктура:** Локален **Azure Service Bus Emulator**, работещ в Docker контейнер.

---

## Изисквания и инсталация

- .NET 8 SDK  
- Docker Desktop  
- Postman  

---

## Конфигурация на емулатора (ConfigFile.json)

```json
{
  "UserConfig": {
    "Namespaces": [
      {
        "Name": "sbemulatorns",
        "Queues": [
          { "Name": "course-queue" },
          { "Name": "enrollment-queue" }
        ]
      }
    ]
    ...
  }
}
```

---

## Стартиране

```bash
cd Backend.Worker
dotnet run
```

```bash
cd Admin.Api
dotnet run
```

```bash
cd Student.Api
dotnet run
```

---

## API Endpoints

- POST http://localhost:5000/api/Admin?name=Math&capacity=2  
- POST http://localhost:5100/api/Student?email=ivan@test.com&courseName=Math  

---

## Структура

```plaintext
UniversitySystem/
├── Admin.Api/
├── Student.Api/
├── Backend.Worker/
├── docker-compose.yml
├── ConfigFile.json
└── UniversitySystem.sln
```
