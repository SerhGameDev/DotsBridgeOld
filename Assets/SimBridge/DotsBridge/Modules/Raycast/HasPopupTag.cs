using Unity.Collections;
using Unity.Entities;

namespace DotsBridge
{
    /// <summary>
    /// Вешается на сущности, у которых есть данные для всплывающего окна.
    /// </summary>
    public struct HasPopupTag : IComponentData { }
    /// <summary>
    /// Хранит локализованное или базовое имя сущности для отображения в UI.
    /// </summary>
    public struct PopupTitleComponent : IComponentData
    {
        // Используем фиксированную строку. 64 байт обычно хватает для названий.
        public FixedString64Bytes Title;
    }
}