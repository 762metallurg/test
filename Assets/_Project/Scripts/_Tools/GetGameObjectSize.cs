using UnityEngine;
using UnityEditor;

public class GetGameObjecSize : Editor
{
    // %g означает, что скрипт сработает по Ctrl+G (Windows) или Cmd+G (Mac)
    [MenuItem("Tools/📏 Measure Selected Object %g")]
    public static void MeasureSize()
    {
        GameObject selectedGO = Selection.activeGameObject;

        if (selectedGO == null)
        {
            Debug.LogWarning("Сначала выбери объект!");
            return;
        }

        // Собираем все рендереры (меши) объекта и его детей
        Renderer[] renderers = selectedGO.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            EditorUtility.DisplayDialog("Размер объекта", 
                $"У объекта '{selectedGO.name}' нет визуальной части (MeshRenderer).", "ОК");
            return;
        }

        // Создаем границы, начиная с первого найденного меша
        Bounds bounds = renderers[0].bounds;

        // Расширяем границы, чтобы включить всех детей
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        // Форматируем вывод
        string message = $"Объект: {selectedGO.name}\n\n" +
                         $"X (Ширина): {bounds.size.x:F2}\n" +
                         $"Y (Высота): {bounds.size.y:F2}\n" +
                         $"Z (Длина):  {bounds.size.z:F2}";

        // Выводим в консоль (чтобы можно было скопировать)
        Debug.Log(message.Replace("\n", ", "));

        // Показываем удобное окошко
        EditorUtility.DisplayDialog("Размеры (World Space)", message, "Понял");
    }
}
