# Телеграм-бот для организации очередей

## Используемые технологии
Для реализации использовались следующие технологии:
- Microsoft.NET Core
- Telegram API
- Entity Framework Core
- PostgreSQL
- Docker
___

## Применение
Для управления очередями для сдачи лабораторных работ в&nbsp;разных учебных группах.

___

## Функциональность
Новый пользователь регистрируется, указывая свои фамилию и&nbsp;имя, номер курса и&nbsp;группы. Зарегистрированный пользователь может:
- записаться в&nbsp;очередь добавленного в&nbsp;бота предмета
- добавить новый предмет
- посмотреть свой номер в&nbsp;очереди для конкретного предмета или для всех предметов
- выйти из&nbsp;очереди, в&nbsp;которой он&nbsp;находится
- пропустить человека вперёд себя
- сменить фамилию и(или) имя
- сменить курс и(или) номер группы
- отписаться от&nbsp;рассылки
- отписаться от&nbsp;бота

___

## Описание алгоритма
При добавлении в&nbsp;очередь пользователь записывается в&nbsp;список ожидающих. Каждый день в&nbsp;определённое время список ожидающих случайным образом перемешивается и&nbsp;добавляется в&nbsp;конец соответствующей очереди. Тем, кто подписан на&nbsp;рассылку, приходит уведомление с&nbsp;его местами в&nbsp;очередях, в&nbsp;которые он&nbsp;записан. Пользователи с&nbsp;админскими правами соответствующей командой могут вызвать генерацию очередей для своей группы в&nbsp;любой момент времени. Для использования админских команд необходимо владеть паролем, который при запуске бота генерируется и&nbsp;присылается владельцу бота.

___

## В разработке участвовали
- https://github.com/KanatnikovMax
- https://github.com/HromBromIod
- https://github.com/Diploj
