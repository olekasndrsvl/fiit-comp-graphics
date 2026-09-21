# Гайд по `Form1` — обработка изображения в HSV

## Назначение

Класс реализует изменение цвета изображения через цветовую модель **HSV**:

- **H (Hue)** — оттенок, `0..360°`;
- **S (Saturation)** — насыщенность, `0..1`;
- **V (Value)** — яркость, `0..1`.

Общий алгоритм:

```text
RGB → HSV → изменение H/S/V → RGB
```

---

## `SolveHsv`

```csharp
private void SolveHsv(
    Bitmap source,
    int hueOffset,
    int saturationOffset,
    int valueOffset)
```

Основной метод обработки изображения.

Для каждого пикселя:

```csharp
var result = source.Select(color =>
{
    RgbToHsv(color, out double h, out double s, out double v);

    h += hueOffset;
    h = ((h % 360) + 360) % 360;

    s = Math.Clamp(s + saturationOffset / 100.0, 0.0, 1.0);
    v = Math.Clamp(v + valueOffset / 100.0, 0.0, 1.0);

    var (r, g, b) = HsvToRgb(h, s, v);

    return Color.FromArgb(color.A, r, g, b);
});
```

Сначала RGB преобразуется в HSV. Затем применяются смещения:

```text
H += hueOffset
S += saturationOffset / 100
V += valueOffset / 100
```

`S` и `V` ограничиваются диапазоном `[0, 1]` с помощью `Math.Clamp`.

`H` циклический, поэтому используется:

```csharp
((h % 360) + 360) % 360
```

Это позволяет корректно обработать значения вроде `370° → 10°` и `-20° → 340°`.

После изменения HSV цвет преобразуется обратно в RGB.

Альфа-канал исходного пикселя сохраняется:

```csharp
Color.FromArgb(color.A, r, g, b)
```

---

## `RgbToHsv`

```csharp
private static void RgbToHsv(
    Color color,
    out double hue,
    out double saturation,
    out double value)
```

Преобразует цвет из RGB в HSV.

### 1. Нормализация RGB

```csharp
var (r, g, b) = (
    color.R / 255.0,
    color.G / 255.0,
    color.B / 255.0);
```

RGB переводится из диапазона `[0, 255]` в `[0, 1]`.

### 2. Основные значения

```csharp
double max = Math.Max(r, Math.Max(g, b));
double min = Math.Min(r, Math.Min(g, b));
double delta = max - min;
```

`max` — максимальный RGB-канал.

`min` — минимальный RGB-канал.

`delta` — разница между ними.

### 3. Value

```csharp
value = max;
```

В HSV:

```text
V = max(R, G, B)
```

### 4. Saturation

```csharp
saturation = (max == 0) ? 0 : delta / max;
```

Формула:

```text
S = Δ / max
```

Для чёрного цвета (`max = 0`) насыщенность принимается равной `0`.

### 5. Hue

Если:

```csharp
delta == 0
```

цвет серый, поэтому оттенок условно принимается за `0`.

Иначе формула зависит от максимального канала:

```csharp
max == r:
H = 60 * (((g - b) / delta) % 6)

max == g:
H = 60 * ((b - r) / delta + 2)

max == b:
H = 60 * ((r - g) / delta + 4)
```

После этого отрицательный угол исправляется:

```csharp
if (hue < 0)
    hue += 360;
```

---

## `HsvToRgb`

```csharp
private static (byte R, byte G, byte B) HsvToRgb(
    double hue,
    double saturation,
    double value)
```

Обратное преобразование:

```text
HSV → RGB
```

Сначала `H` приводится к `[0, 360)`.

### Chroma

```csharp
double chroma = value * saturation;
```

Формула:

```text
C = V × S
```

Это величина цветовой составляющей.

### X

```csharp
double x =
    chroma * (1 - Math.Abs((hue / 60.0) % 2 - 1));
```

`X` используется для вычисления промежуточного RGB-канала внутри текущего сектора Hue.

### M

```csharp
double m = value - chroma;
```

`m` — серая составляющая, которая затем добавляется ко всем RGB-каналам.

---

## Сектора Hue

Цветовой круг разбит на шесть секторов по `60°`:

| Hue | `(R1, G1, B1)` |
|---|---|
| `0..60` | `(C, X, 0)` |
| `60..120` | `(X, C, 0)` |
| `120..180` | `(0, C, X)` |
| `180..240` | `(0, X, C)` |
| `240..300` | `(X, 0, C)` |
| `300..360` | `(C, 0, X)` |

После выбора сектора:

```text
R = R1 + M
G = G1 + M
B = B1 + M
```

Затем значения переводятся обратно в `[0, 255]`:

```csharp
(byte)Math.Clamp(
    Math.Round((r1 + m) * 255),
    0,
    255)
```

---

## `SaveHsvResult`

```csharp
private void SaveHsvResult(Image result, string fileName)
```

Сохраняет обработанное изображение.

Сначала определяется расширение:

```csharp
var ext = Path.GetExtension(fileName).ToLowerInvariant();
```

Затем выбирается соответствующий `ImageFormat`:

```text
.png  → PNG
.jpg  → JPEG
.bmp  → BMP
.gif  → GIF
.tif  → TIFF
```

Если расширение неизвестно, используется PNG.

Создаётся копия изображения:

```csharp
using var copy = new Bitmap(result.Width, result.Height);
```

После чего исходный результат рисуется на ней через `Graphics` и сохраняется:

```csharp
copy.Save(fileName, format);
```

`using` автоматически освобождает ресурсы `Bitmap` и `Graphics`.

---

## Главное для понимания

Вся программа сводится к четырём этапам:

```text
1. Получить пиксель RGB
          ↓
2. RGB → HSV
          ↓
3. Изменить H, S и V
          ↓
4. HSV → RGB
```

При этом:

- `H` изменяет **оттенок**;
- `S` изменяет **насыщенность**;
- `V` изменяет **яркость**;
- `Clamp` не позволяет `S` и `V` выйти за `[0, 1]`;
- `H` зациклен в диапазоне `[0, 360)`.
