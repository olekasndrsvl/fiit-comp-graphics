# Разбор Form1.cs

В [Form1.cs](Form1.cs) находится **поведение окна**: загрузка изображения, очистка, реакция на ползунки, запросы на обработку и вывод готовых результатов. Расположение кнопок и остальных элементов задаётся в [Form1.Designer.cs](Form1.Designer.cs).

Эти два файла содержат части одного класса `Form1`. Ключевое слово `partial` позволяет разделить класс между файлами, поэтому обработчики свободно обращаются к элементам интерфейса, объявленным в другой части класса.

## 1. Состояние формы

```csharp
private Bitmap? sourceBitmap;
private bool updatingHsvControls;
```

`sourceBitmap` хранит исходное изображение. Пока ничего не загружено, там `null`. Знак `?` указывает, что отсутствие объекта допустимо.

`updatingHsvControls` — флаг группового изменения ползунков. Он нужен при сбросе всех трёх значений, чтобы не запускать обработку после каждого отдельного изменения.

## 2. Конструктор

```csharp
public Form1()
{
    InitializeComponent();
}
```

Вызывается при создании окна через `new Form1()`.

`InitializeComponent()` создаёт интерфейс и подписывает обработчики на кнопки и ползунки. Этот метод находится во второй части того же класса — в `Form1.Designer.cs`.

## 3. События для ваших алгоритмов

```csharp
public event EventHandler? SourceImageChanged;
public event EventHandler? GrayscaleRequested;
public event EventHandler? ChannelsRequested;
public event EventHandler? HsvPreviewRequested;
public event EventHandler<SaveResultEventArgs>? SaveHsvRequested;
```

Через эти события форма сообщает внешнему коду, что произошло действие:

| Событие | Значение |
|---|---|
| `SourceImageChanged` | Исходник загружен, заменён или очищен. |
| `GrayscaleRequested` | Нажата кнопка расчёта оттенков серого. |
| `ChannelsRequested` | Нажата кнопка выделения каналов RGB. |
| `HsvPreviewRequested` | Нужно обновить результат HSV. |
| `SaveHsvRequested` | Выбран путь для сохранения результата. |

**Событие само ничего не вычисляет.** Нужно подписать на него метод:

```csharp
GrayscaleRequested += HandleGrayscale;
```

Тогда при вызове события выполнится `HandleGrayscale()`.

На момент подготовки этого гайда подписчиков на эти события в проекте нет: алгоритмы ещё предстоит подключить.

## 4. Доступ к исходнику и значениям ползунков

```csharp
public Bitmap? SourceImage => sourceBitmap;
public int HueOffset => hueTrackBar.Value;
public int SaturationOffset => saturationTrackBar.Value;
public int ValueOffset => valueTrackBar.Value;
```

Это свойства только для чтения:

- `SourceImage` возвращает исходную картинку;
- `HueOffset` — текущее значение H, от −180 до +180;
- `SaturationOffset` — S, от −100 до +100;
- `ValueOffset` — V, от −100 до +100.

Например, ваш алгоритм может получить параметры так:

```csharp
var source = SourceImage;
var hue = HueOffset;
```

`SourceImage` возвращает ссылку на исходник формы, а не копию. Освобождением этого изображения управляет форма.

## 5. Методы показа результатов

```csharp
SetGrayscaleResults(...)
SetChannelResults(...)
SetHsvResult(...)
```

Они принимают **уже вычисленные данные** и передают их элементам интерфейса.

Например, внутри `SetGrayscaleResults()`:

```csharp
grayscaleFirstPreview.Image = first;
grayscaleSecondPreview.Image = second;
differencePreview.Image = difference;

grayscaleFirstHistogram.SetValues(firstHistogram);
grayscaleSecondHistogram.SetValues(secondHistogram);
```

Картинки передаются областям просмотра, массивы частот — гистограммам. Эти элементы сами запрашивают перерисовку.

Для гистограммы интенсивности обычно передаётся массив `int[256]`: индекс — интенсивность, значение — количество пикселей с этой интенсивностью.

`SetChannelResults()` делает то же самое для R, G и B.

```csharp
public void SetHsvResult(Image? result)
{
    hsvResultPreview.Image = result;
    saveHsvButton.Enabled = result is not null;
}
```

Этот метод показывает HSV-результат и включает кнопку сохранения, если изображение существует.

Методы отображения не освобождают предыдущие изображения результатов. При подключении вычислений нужно определить, кто хранит эти изображения и вызывает для них `Dispose()`.

## 6. OpenImageButton_Click() — открытие изображения

Вызывается кнопкой «Открыть».

Сначала создаётся диалог:

```csharp
using var dialog = new OpenFileDialog
{
    CheckFileExists = true,
    Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff|PNG|*.png|JPEG|*.jpg;*.jpeg|Bitmap|*.bmp|Все файлы|*.*",
    Title = "Выберите исходное изображение"
};
```

`Filter` определяет, какие файлы показывать. `CheckFileExists` требует, чтобы выбранный файл существовал.

```csharp
if (dialog.ShowDialog(this) != DialogResult.OK)
{
    return;
}
```

Если пользователь отменил выбор, обработчик заканчивается.

Далее читается изображение:

```csharp
using var loadedImage = Image.FromFile(dialog.FileName);
var newBitmap = new Bitmap(loadedImage.Width, loadedImage.Height);
```

Создаётся отдельный `Bitmap`, и в него копируется исходная картинка:

```csharp
using (var graphics = Graphics.FromImage(newBitmap))
{
    graphics.DrawImageUnscaled(loadedImage, 0, 0);
}
```

Здесь `Graphics` рисует **внутрь `newBitmap`**, а не на экран. `DrawImageUnscaled()` копирует изображение без масштабирования.

Отдельная копия позволяет затем освободить `loadedImage` и не держать открытый файл заблокированным. `using` обеспечивает освобождение соответствующего ресурса при выходе из области его использования.

После этого заменяется старый исходник:

```csharp
sourcePreview.Image = null;
sourceBitmap?.Dispose();
sourceBitmap = newBitmap;
sourcePreview.Image = sourceBitmap;
```

По шагам:

1. Отсоединяем старое изображение от области просмотра.
2. Освобождаем его ресурсы, если оно существовало.
3. Сохраняем новый `Bitmap`.
4. Передаём его для отображения.

Дальше включаются кнопки, которым нужен исходник, и очищаются старые результаты:

```csharp
ClearResultViews();
SourceImageChanged?.Invoke(this, EventArgs.Empty);
```

`?.Invoke()` вызывает подписчиков, если они есть. Если подписчиков нет, ничего не происходит.

При включённом автообновлении также запрашивается обработка HSV:

```csharp
if (autoPreviewCheckBox.Checked)
{
    HsvPreviewRequested?.Invoke(this, EventArgs.Empty);
}
```

Блок `catch` показывает сообщение, если внутри `try` возникло исключение. В текущем коде в этот `try` входят и вызовы подписчиков событий, поэтому исключение из будущего алгоритма также может попасть в сообщение «Ошибка загрузки».

## 7. ClearImageButton_Click() — очистка

Вызывается кнопкой «Очистить».

Он:

- убирает исходник из просмотра;
- освобождает `sourceBitmap`;
- записывает в него `null`;
- отключает кнопки расчёта и обновления;
- очищает результаты;
- вызывает `SourceImageChanged`.

Файл на диске не удаляется. Очищается только состояние приложения. Значения HSV-ползунков сохраняются.

## 8. HsvTrackBar_ValueChanged() — изменение ползунков

Один обработчик используется для всех трёх ползунков.

Сначала обновляет подписи:

```csharp
hueValueLabel.Text = FormatSignedValue(hueTrackBar.Value, "°");
saturationValueLabel.Text = FormatSignedValue(saturationTrackBar.Value, "%");
valueValueLabel.Text = FormatSignedValue(valueTrackBar.Value, "%");
```

Затем решает, нужно ли запрашивать обработку:

```csharp
if (!updatingHsvControls &&
    sourceBitmap is not null &&
    autoPreviewCheckBox.Checked)
{
    HsvPreviewRequested?.Invoke(this, EventArgs.Empty);
}
```

Обработка запрашивается, если одновременно:

- не выполняется групповой сброс;
- есть исходное изображение;
- включено автообновление.

Этот обработчик срабатывает и при движении мышью, и при программном изменении `Value`, в том числе после двойного клика со сбросом. Если значение уже равно нулю и остаётся нулевым, само присваивание не вызывает `ValueChanged`.

Распознавание двойного клика находится в `ResettableTrackBar` в [UiControls.cs](UiControls.cs), а не в форме.

## 9. ResetHsvButton_Click() — сброс всех параметров

```csharp
updatingHsvControls = true;

hueTrackBar.Value = 0;
saturationTrackBar.Value = 0;
valueTrackBar.Value = 0;

updatingHsvControls = false;
HsvTrackBar_ValueChanged(sender, e);
```

Пока флаг равен `true`, изменения значений обновляют подписи, но не вызывают обработку.

После обнуления всех ползунков обработчик вызывается ещё раз — теперь он может отправить **один общий запрос** на пересчёт.

Если автообновление выключено, пересчёт нужно запросить кнопкой «Обновить».

## 10. SaveHsvButton_Click() — выбор места сохранения

Создаёт `SaveFileDialog` с вариантами PNG, JPEG и BMP.

После подтверждения:

```csharp
SaveHsvRequested?.Invoke(
    this,
    new SaveResultEventArgs(dialog.FileName));
```

Передаёт выбранный путь подписчику события.

**Записи файла в этом методе пока нет.** Её должен выполнить ваш обработчик `SaveHsvRequested`, выбрав формат изображения в соответствии с расширением файла.

Для передачи пути в конце файла объявлен небольшой класс:

```csharp
public sealed class SaveResultEventArgs(string fileName) : EventArgs
{
    public string FileName { get; } = fileName;
}
```

В обработчике выбранный путь будет доступен как `e.FileName`.

## 11. Два вспомогательных метода

```csharp
private void ClearResultViews()
{
    SetGrayscaleResults(null, null, null);
    SetChannelResults(null, null, null);
    SetHsvResult(null);
}
```

Очищает все результаты через уже существующие методы отображения. Передача `null` убирает картинки и данные гистограмм.

```csharp
private static string FormatSignedValue(int value, string suffix)
    => $"{(value > 0 ? "+" : string.Empty)}{value}{suffix}";
```

Формирует подпись числа:

```text
15, "°"  → "+15°"
-20, "%" → "-20%"
0, "%"   → "0%"
```

## 12. Где подписаны обработчики

Подписки находятся в `Form1.Designer.cs`, рядом с созданием элементов:

| Элемент | Где создаётся подписка | Обработчик или событие |
|---|---|---|
| «Открыть» | `CreateSourcePanel()` | `OpenImageButton_Click` |
| «Очистить» | `CreateSourcePanel()` | `ClearImageButton_Click` |
| «Рассчитать» для оттенков серого | `CreateGrayscaleTab()` | Лямбда вызывает `GrayscaleRequested` |
| «Рассчитать» для каналов RGB | `CreateChannelsTab()` | Лямбда вызывает `ChannelsRequested` |
| «Сохранить» | `CreateHsvTab()` | `SaveHsvButton_Click` |
| Все три HSV-ползунка | `CreateHsvControls()` | `HsvTrackBar_ValueChanged` |
| «Обновить» | `CreateHsvControls()` | Лямбда вызывает `HsvPreviewRequested` |
| «Сброс» | `CreateHsvControls()` | `ResetHsvButton_Click` |

Например:

```csharp
openImageButton.Click += OpenImageButton_Click;
```

Означает: при событии `Click` у этой кнопки вызвать `OpenImageButton_Click`. Само имя метода не подключает его к кнопке: нужна подписка через `+=`.

Обычный обработчик имеет параметры:

```csharp
object? sender, EventArgs e
```

- `sender` — объект, вызвавший событие;
- `e` — сведения о событии.

Для кнопки расчёта используется короткий обработчик без отдельного имени:

```csharp
runGrayscaleButton.Click +=
    (_, _) => GrayscaleRequested?.Invoke(this, EventArgs.Empty);
```

`(_, _)` означает, что параметры нажатия здесь не используются. Обработчик только передаёт запрос подписчикам формы.

У чекбокса «Автообновление» отдельной подписки нет. Его состояние проверяется при загрузке исходника и изменении ползунков. Само включение галочки немедленный пересчёт не запускает.

## 13. Как подключить решение

Пример ниже показывает место подключения. Сам алгоритм в нём намеренно не реализован.

В конструкторе после создания интерфейса:

```csharp
public Form1()
{
    InitializeComponent();
    GrayscaleRequested += HandleGrayscale;
}
```

В том же классе:

```csharp
private void HandleGrayscale(object? sender, EventArgs e)
{
    if (SourceImage is null)
        return;

    // Вызвать ваш алгоритм для SourceImage.
    // Затем передать вычисленные данные:
    // SetGrayscaleResults(gray1, gray2, difference, hist1, hist2);
}
```

Последовательность работы:

```text
Кнопка «Рассчитать»
    → событие Click
    → вызов GrayscaleRequested
    → ваш обработчик HandleGrayscale
    → ваш алгоритм получает SourceImage
    → SetGrayscaleResults получает результат
    → элементы просмотра запрашивают перерисовку
```

Аналогично подключаются `ChannelsRequested`, `HsvPreviewRequested` и `SaveHsvRequested`.

События вызываются синхронно в потоке интерфейса. Если ваш обработчик долго считает результат, окно на это время перестаёт отвечать на действия пользователя. Само использование событий не переносит вычисления в фон.
