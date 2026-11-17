using Lab6.Core.Materials;

namespace Lab6.Rendering;

/// <summary>
/// Пункт 1.2 — шейдинг Фонга (туншейдинг). ЗАГОТОВКА: реализуется отдельной задачей.
///
/// Всё необходимое уже приходит во Fragment:
///   • fragment.WorldNormal   — нормаль, проинтерполированная по грани, НЕ нормированная;
///   • fragment.WorldPosition — мировая позиция фрагмента;
///   • fragment.Has(VertexAttributes.WorldSpace) — признак, что данные доступны;
///   • LightingModel.ToLight / LightingModel.Lambert — общая математика света.
/// Нормировать интерполированную нормаль обязательно здесь, во фрагментном шейдере.
/// </summary>
internal sealed class PhongFragmentShader : IFragmentShader
{
    public int Shade(in Fragment fragment, Material material) =>
        throw new NotImplementedException(
            "Пункт 1.2: шейдинг Фонга (туншейдинг) ещё не реализован.");
}
