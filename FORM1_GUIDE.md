# Разбор Form1 и подключение решений

## Структура файлов

Все файлы `Form1.*.cs` содержат части одного класса `Form1`. Ключевое слово `partial` позволяет разделить класс по файлам. Поэтому приватные методы решений видят элементы формы и могут напрямую вызывать методы вывода результатов.

| Файл | Назначение |
|---|---|
| [Program.cs](Program.cs) | Настройка WinForms и запуск главного окна. |
| [Form1.Designer.cs](Form1.Designer.cs) | Создание интерфейса, расположение элементов, подписки на события кнопок и ползунков. |
| [Form1.cs](Form1.cs) | Загрузка, очистка, общие обработчики и вывод результатов. |
| [Form1.Grayscale.cs](Form1.Grayscale.cs) | Место для реализации первого задания — `SolveGrayscale()`. |
| [Form1.Channels.cs](Form1.Channels.cs) | Место для реализации второго задания — `SolveChannels()`. |
| [Form1.Hsv.cs](Form1.Hsv.cs) | Место для реализации HSV и сохранения — `SolveHsv()`, `SaveHsvResult()`. |
| [FastBitmap.cs](FastBitmap.cs) | Доступ к пикселям через LockBits, методы Select и ForEach. |
| [UiControls.cs](UiControls.cs) | Гистограмма и ползунок со сбросом двойным кликом. |

Интерфейса `ISolution` и промежуточных событий `...Requested` нет. Форма напрямую вызывает методы решений.

**Методы решений пока содержат только TODO-комментарии. Обработка и запись изображения в файл ещё не реализованы.**

## 1. Состояние формы

```csharp
private Bitmap? sourceBitmap;
private bool updatingHsvControls;
```

`sourceBitmap` хранит исходное изображение. Пока ничего не загружено, там `null`. Знак `?` означает, что отсутствие объекта допустимо.

`updatingHsvControls` — флаг группового изменения ползунков. Он подавляет промежуточные пересчёты при сбросе всех трёх значений.

## 2. Конструктор

```csharp
public Form1()
{
    InitializeComponent();
}
```

Конструктор вызывается при создании `new Form1()`. Метод `InitializeComponent()` находится в `Form1.Designer.cs`: он создаёт окно, элементы и подключает обработчики.

## 3. Где подключены обработчики

| Действие | Где подписка в Form1.Designer.cs | Обработчик в Form1.cs | Метод решения |
|---|---|---|---|
| «Открыть» | CreateSourcePanel | OpenImageButton_Click | При автообновлении вызывает UpdateHsvPreview. |
| «Очистить» | CreateSourcePanel | ClearImageButton_Click | Не вызывает решения. |
| «Рассчитать» на первой вкладке | CreateGrayscaleTab | RunGrayscaleButton_Click | SolveGrayscale |
| «Рассчитать» на второй вкладке | CreateChannelsTab | RunChannelsButton_Click | SolveChannels |
| Изменение H/S/V | CreateHsvControls | HsvTrackBar_ValueChanged | При автообновлении: UpdateHsvPreview → SolveHsv. |
| «Обновить» | CreateHsvControls | UpdateHsvButton_Click | UpdateHsvPreview → SolveHsv |
| «Сброс» | CreateHsvControls | ResetHsvButton_Click | Обнуляет параметры; при автообновлении пересчитывает HSV. |
| «Сохранить» | CreateHsvTab | SaveHsvButton_Click | SaveHsvResult |

Пример подписки:

```csharp
runGrayscaleButton.Click += RunGrayscaleButton_Click;
```

При событии `Click` будет вызван указанный метод. Само название метода его к кнопке не подключает: для этого нужна подписка через `+=`.

Параметры обычного обработчика:

```csharp
object? sender, EventArgs e
```

- `sender` — объект, вызвавший событие;
- `e` — сведения о событии.

У чекбокса «Автообновление» отдельного обработчика нет. Его `Checked` проверяется при загрузке исходника и изменении ползунков. Включение галочки само по себе пересчёт не запускает.

## 4. Доступ к исходнику и параметрам

```csharp
public Bitmap? SourceImage => sourceBitmap;
public int HueOffset => hueTrackBar.Value;
public int SaturationOffset => saturationTrackBar.Value;
public int ValueOffset => valueTrackBar.Value;
```

Это свойства только для чтения. Они возвращают исходник и текущие значения ползунков. H имеет диапазон −180..180 градусов, S/V — −100..100.

`SourceImage` возвращает ссылку, а не копию. Исходником управляет форма: внутри решений его не следует изменять или освобождать.

Текущие обработчики передают исходник и параметры в решения аргументами, чтобы входные данные были явно видны в сигнатуре.

## 5. Вывод готовых результатов

| Метод | Что принимает |
|---|---|
| SetGrayscaleResults | Два полутоновых изображения, разность и две гистограммы. |
| SetChannelResults | Изображения каналов R/G/B и три гистограммы. |
| SetHsvResult | Готовый RGB-результат после HSV-коррекции. |

Например:

```csharp
grayscaleFirstPreview.Image = first;
grayscaleSecondPreview.Image = second;
differencePreview.Image = difference;
grayscaleFirstHistogram.SetValues(firstHistogram);
grayscaleSecondHistogram.SetValues(secondHistogram);
```

Изображения отображаются стандартными `PictureBox` с `SizeMode = Zoom`. Гистограммы отображает `HistogramView`. Эти компоненты сами перерисовываются после передачи данных.

Гистограмма обычно представлена массивом `int[256]`: индекс — интенсивность, значение — число пикселей.

`SetHsvResult()` также включает кнопку сохранения, если результат не `null`.

Методы вывода не вычисляют результаты и не освобождают прежние изображения. При реализации решений нужно предусмотреть освобождение старых результатов после отсоединения от PictureBox. Изображение, которое ещё показывается, освобождать нельзя.

## 6. OpenImageButton_Click — загрузка

Обработчик открывает `OpenFileDialog`. Если пользователь отменяет выбор, метод заканчивается.

После подтверждения:

```csharp
using var loadedImage = new Bitmap(dialog.FileName);
var newBitmap = loadedImage.Select(color => color);
```

Метод `Select()` из пространства имён `FastBitmap` создаёт отдельную копию изображения. Функция `color => color` оставляет каждый цвет без изменений. Внутри библиотека использует LockBits и разблокирует изображения перед возвратом результата.

Отдельная копия позволяет освободить `loadedImage` и не держать файл заблокированным. `using` освобождает ресурс при выходе из области использования.

Замена старого исходника:

```csharp
sourcePreview.Image = null;
sourceBitmap?.Dispose();
sourceBitmap = newBitmap;
sourcePreview.Image = sourceBitmap;
```

Сначала старое изображение отсоединяется от просмотра, затем освобождается. После этого форма сохраняет и показывает новый исходник.

Обработчик включает кнопки, которым нужен исходник, очищает старые результаты и при включённом автообновлении вызывает `UpdateHsvPreview()`.

Исключения внутри `try` показываются через сообщение «Ошибка загрузки». Сейчас в этот блок также входит вызов HSV, поэтому исключение будущего решения при загрузке попадёт в то же сообщение.

## 7. ClearImageButton_Click — очистка

Метод отсоединяет исходник от PictureBox, освобождает его и записывает `null`. Затем отключает кнопки расчёта и обновления, очищает результаты.

Файл на диске не удаляется. Значения HSV-ползунков сохраняются.

## 8. Вызов первых двух решений

```csharp
private void RunGrayscaleButton_Click(object? sender, EventArgs e)
{
    if (sourceBitmap is not null)
        SolveGrayscale(sourceBitmap, grayscaleFirstHistogram, grayscaleSecondHistogram);
}
```

Проверка не позволяет вызвать решение без исходника. Метод `SolveGrayscale(Bitmap source, HistogramView firstHistogramView, HistogramView secondHistogramView)` находится в `Form1.Grayscale.cs`. Он получает исходник и два экранных компонента гистограмм. В нём нужно реализовать две формулы, разность и подсчёт частот, затем вывести результаты в указанном порядке (вызов SetGrayscaleResults без массивов очищает прежние гистограммы):

```csharp
// Имена переменных ниже обозначают результаты вашего алгоритма.
SetGrayscaleResults(first, second, difference);
firstHistogramView.SetValues(firstHistogram);
secondHistogramView.SetValues(secondHistogram);
```

`RunChannelsButton_Click()` аналогично вызывает `SolveChannels(sourceBitmap)` из `Form1.Channels.cs`. После выделения каналов и подсчёта гистограмм:

```csharp
SetChannelResults(red, green, blue, redHistogram, greenHistogram, blueHistogram);
```

Дополнительные подписки для подключения решений больше не нужны: достаточно заполнить тела этих методов.

## 9. HSV и ползунки

`HsvTrackBar_ValueChanged()` обновляет числовые подписи с помощью `FormatSignedValue()`.

Затем проверяет:

```csharp
if (!updatingHsvControls &&
    sourceBitmap is not null &&
    autoPreviewCheckBox.Checked)
{
    UpdateHsvPreview();
}
```

То есть не идёт групповой сброс, исходник существует и автообновление включено.

Общая точка вызова решения:

```csharp
private void UpdateHsvPreview()
{
    if (sourceBitmap is not null)
        SolveHsv(sourceBitmap, HueOffset, SaturationOffset, ValueOffset);
}
```

Её вызывают кнопка «Обновить», автообновление ползунков и загрузка исходника при включённом автообновлении.

В `Form1.Hsv.cs` метод `SolveHsv()` получает исходник и три смещения. Каждый пересчёт должен начинаться с исходника, чтобы изменения не накапливались от предыдущего результата. После RGB → HSV → изменение параметров → RGB нужно вызвать `SetHsvResult(result)`.

`ValueChanged` срабатывает при изменении значения мышью или программно. Распознавание двойного клика реализовано в `ResettableTrackBar` в `UiControls.cs`. Он обнуляет `Value`, после чего срабатывает обычный обработчик формы. Если значение уже нулевое и не изменилось, событие не возникает.

## 10. ResetHsvButton_Click — общий сброс

```csharp
updatingHsvControls = true;
hueTrackBar.Value = 0;
saturationTrackBar.Value = 0;
valueTrackBar.Value = 0;
updatingHsvControls = false;
HsvTrackBar_ValueChanged(sender, e);
```

Флаг подавляет промежуточные пересчёты. После сброса обработчик вызывается ещё раз для одного общего обновления.

При выключенном автообновлении меняются только значения и подписи. Для расчёта нужно нажать «Обновить».

## 11. SaveHsvButton_Click — сохранение

Сначала проверяет, есть ли изображение результата. Если его нет — выходит. Затем открывает SaveFileDialog с PNG, JPEG и BMP.

После подтверждения:

```csharp
SaveHsvResult(result, dialog.FileName);
```

Метод `SaveHsvResult(Image result, string fileName)` находится в `Form1.Hsv.cs`. Сейчас там TODO: нужно записать готовое RGB-изображение в файл, выбрав формат по расширению. Сам диалог файл не записывает.

Переданное изображение ещё отображается: в методе сохранения его освобождать нельзя.

Класс `SaveResultEventArgs` удалён: путь передаётся обычным аргументом метода.

## 12. Служебные методы

```csharp
private void ClearResultViews()
{
    SetGrayscaleResults(null, null, null);
    SetChannelResults(null, null, null);
    SetHsvResult(null);
}
```

Очищает изображения и гистограммы, а также отключает сохранение.

`FormatSignedValue()` формирует подписи: `15, "°"` → `+15°`, `-20, "%"` → `-20%`, `0, "%"` → `0%`.

`Dispose()` в `Form1.Designer.cs` освобождает контейнер компонентов и исходник, затем вызывает освобождение базовой формы.

## 13. Последовательность работы

```text
«Рассчитать» на первой вкладке
    → RunGrayscaleButton_Click
    → проверка исходника
    → SolveGrayscale в Form1.Grayscale.cs
    → ваш алгоритм
    → SetGrayscaleResults
    → PictureBox и HistogramView показывают результаты
```

Все текущие вызовы синхронные и выполняются в потоке интерфейса. Долгие вычисления внутри решения будут задерживать реакцию окна. Разделение по файлам само по себе не переносит обработку в фон.
